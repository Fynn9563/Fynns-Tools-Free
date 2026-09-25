#!/usr/bin/env python3
"""Build .unitypackage archives for the tools in this repository.

A .unitypackage is a gzip-compressed tar in which every asset is stored under a
directory named after its Unity GUID:

    <guid>/pathname     project-relative path the asset installs to
    <guid>/asset        the file's bytes (absent for folder assets)
    <guid>/asset.meta   the .meta file's bytes

Every asset here has a committed .meta file, so the GUIDs are already known and
the archives can be built without Unity. Reusing the committed GUIDs also means
upgrading a tool in place keeps existing references intact.

Tools are discovered rather than configured: a top-level directory is a tool when
a sibling <name>.meta exists, which is what marks it as a Unity asset folder.
Adding a tool needs no change to this script or to the workflow that calls it.

Usage:
    build-unitypackage.py                      build every tool into dist/
    build-unitypackage.py --tool UnityIDE      build one tool
    build-unitypackage.py --list               print tool metadata as JSON
"""

from __future__ import annotations

import argparse
import gzip
import io
import json
import re
import sys
import tarfile
import time
from dataclasses import dataclass
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
DEFAULT_INSTALL_ROOT = "Assets/Fynn's Tools"
DEFAULT_OUT_DIR = REPO_ROOT / "dist"
CHANGELOG = REPO_ROOT / "CHANGELOG.md"
UNVERSIONED = "0.0.0"


# Unity skips these on import, so they never belong in a package.
IGNORED_SUFFIXES = ("~", ".tmp", ".orig", ".rej")
IGNORED_NAMES = {"cvs", "thumbs.db", "desktop.ini"}

GUID_PATTERN = re.compile(r"^guid:\s*([0-9a-fA-F]{32})\s*$", re.MULTILINE)
TOOL_HEADING = re.compile(r"^##\s+(.+?)\s*$", re.MULTILINE)
VERSION_HEADING = re.compile(r"^###\s+\[(\d+\.\d+\.\d+[^\]]*)\]", re.MULTILINE)


class BuildError(Exception):
    """A problem that would make the resulting package untrustworthy."""


@dataclass(frozen=True)
class Asset:
    pathname: str          # where it installs to, e.g. Assets/Fynn's Tools/UnityIDE/Editor
    guid: str              # from the .meta file, so references survive upgrades
    meta: bytes
    contents: bytes | None  # None for folders, which Unity stores meta-only

    @property
    def is_folder(self) -> bool:
        return self.contents is None


def is_ignored(name: str) -> bool:
    return name.startswith(".") or name.lower() in IGNORED_NAMES or name.endswith(IGNORED_SUFFIXES)


def discover_tools() -> list[str]:
    """Top-level directories that Unity treats as asset folders."""
    return [
        entry.name
        for entry in sorted(REPO_ROOT.iterdir())
        if entry.is_dir()
        and not is_ignored(entry.name)
        and REPO_ROOT.joinpath(entry.name + ".meta").is_file()
    ]


def read_guid(meta_path: Path) -> str:
    match = GUID_PATTERN.search(meta_path.read_text(encoding="utf-8", errors="replace"))
    if not match:
        raise BuildError(f"no guid in {meta_path.relative_to(REPO_ROOT).as_posix()}")
    return match.group(1).lower()


def read_version(tool: str) -> str:
    """Newest version from the tool's section of the central changelog.

    The changelog stays the single source of truth for versions, so a release is
    written once. A tool with no section yet still builds, as UNVERSIONED.
    """
    if not CHANGELOG.is_file():
        return UNVERSIONED

    text = CHANGELOG.read_text(encoding="utf-8")
    headings = list(TOOL_HEADING.finditer(text))
    for index, heading in enumerate(headings):
        if heading.group(1) != tool:
            continue
        section_end = headings[index + 1].start() if index + 1 < len(headings) else len(text)
        version = VERSION_HEADING.search(text, heading.end(), section_end)
        return version.group(1) if version else UNVERSIONED
    return UNVERSIONED


def collect_assets(tool: str, install_root: str) -> list[Asset]:
    """The tool folder and everything inside it, paired with its .meta."""
    tool_dir = REPO_ROOT / tool
    install_base = f"{install_root}/{tool}"
    assets: list[Asset] = []
    missing_meta: list[str] = []

    def add(path: Path, pathname: str) -> None:
        meta_path = path.with_name(path.name + ".meta")
        if not meta_path.is_file():
            missing_meta.append(path.relative_to(REPO_ROOT).as_posix())
            return
        assets.append(Asset(
            pathname=pathname,
            guid=read_guid(meta_path),
            meta=meta_path.read_bytes(),
            contents=None if path.is_dir() else path.read_bytes(),
        ))

    add(tool_dir, install_base)

    for path in sorted(tool_dir.rglob("*")):
        relative = path.relative_to(tool_dir)
        if path.suffix == ".meta" or any(is_ignored(part) for part in relative.parts):
            continue
        add(path, f"{install_base}/{relative.as_posix()}")


    if missing_meta:
        listed = "\n  ".join(missing_meta)
        raise BuildError(
            f"{tool}: {len(missing_meta)} asset(s) have no .meta file. Unity would assign "
            f"new GUIDs on import and break references in existing projects:\n  {listed}"
        )
    return assets


def add_entry(tar: tarfile.TarFile, name: str, payload: bytes | None, mtime: int) -> None:
    """Add one entry.

    The mtime must be a real timestamp. Unity puts it on the files it extracts, so a
    zero mtime makes imported source look older than the assembly built from the
    previous import and nothing recompiles.
    """
    info = tarfile.TarInfo(name)
    info.uid = info.gid = 0
    info.uname = info.gname = ""
    info.mtime = mtime
    if payload is None:
        info.type = tarfile.DIRTYPE
        info.mode = 0o755
        tar.addfile(info)
    else:
        info.type = tarfile.REGTYPE
        info.mode = 0o644
        info.size = len(payload)
        tar.addfile(info, io.BytesIO(payload))


def build(tool: str, out_dir: Path, install_root: str) -> Path:
    assets = collect_assets(tool, install_root)
    version = read_version(tool)
    out_dir.mkdir(parents=True, exist_ok=True)
    out_path = out_dir / f"{tool}-{version}.unitypackage"

    # One timestamp for the whole archive, so every file in an import shares it.
    stamp = int(time.time())

    with open(out_path, "wb") as raw, \
            gzip.GzipFile(filename="", mode="wb", fileobj=raw, mtime=stamp) as gz, \
            tarfile.open(fileobj=gz, mode="w", format=tarfile.GNU_FORMAT) as tar:
        for asset in assets:
            add_entry(tar, asset.guid, None, stamp)
            add_entry(tar, f"{asset.guid}/pathname", asset.pathname.encode("utf-8"), stamp)
            add_entry(tar, f"{asset.guid}/asset.meta", asset.meta, stamp)
            if not asset.is_folder:
                add_entry(tar, f"{asset.guid}/asset", asset.contents, stamp)

    folders = sum(1 for asset in assets if asset.is_folder)
    files = len(assets) - folders
    size_kb = out_path.stat().st_size / 1024
    print(f"{tool} {version}: {files} file(s), {folders} folder(s), {size_kb:.0f} KB "
          f"-> {out_path.relative_to(REPO_ROOT).as_posix()}")
    return out_path


def main() -> int:
    parser = argparse.ArgumentParser(
        description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--tool", action="append", dest="tools", metavar="NAME",
                        help="build only this tool; repeatable (default: every tool found)")
    parser.add_argument("--out-dir", type=Path, default=DEFAULT_OUT_DIR,
                        help=f"where to write packages (default: {DEFAULT_OUT_DIR.name}/)")
    parser.add_argument("--install-root", default=DEFAULT_INSTALL_ROOT,
                        help=f"project folder the package installs into (default: {DEFAULT_INSTALL_ROOT!r})")
    parser.add_argument("--list", action="store_true",
                        help="print discovered tools as JSON and exit, for CI matrices")
    args = parser.parse_args()

    available = discover_tools()
    if not available:
        print("No tools found. A tool is a top-level folder with a sibling <name>.meta file.",
              file=sys.stderr)
        return 1

    if args.list:
        print(json.dumps([{"name": name, "version": read_version(name)} for name in available]))
        return 0

    unknown = [name for name in (args.tools or []) if name not in available]
    if unknown:
        print(f"Unknown tool(s): {', '.join(unknown)}. Available: {', '.join(available)}",
              file=sys.stderr)
        return 1

    try:
        for name in args.tools or available:
            build(name, args.out_dir, args.install_root)
    except BuildError as error:
        print(f"error: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
