#!/usr/bin/env python3
"""Build .unitypackage archives, VPM ZIPs and VPM listing manifests.

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
import hashlib
import io
import json
import re
import sys
import tarfile
import time
import os
import subprocess
import zipfile
from urllib.parse import quote
from dataclasses import dataclass
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
DEFAULT_INSTALL_ROOT = "Assets/Fynn's Tools"
DEFAULT_OUT_DIR = REPO_ROOT / "dist"
CHANGELOG = REPO_ROOT / "CHANGELOG.md"
PACKAGE_SETTINGS = REPO_ROOT / "scripts" / "packages.json"
UNVERSIONED = "0.0.0"
COMPLETE = "Complete"


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
    if tool == COMPLETE:
        return []
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


def package_settings(tool: str) -> dict:
    settings = json.loads(PACKAGE_SETTINGS.read_text(encoding="utf-8")) if PACKAGE_SETTINGS.is_file() else {}
    entry = settings.get(tool, {})
    if tool == COMPLETE:
        entry = dict(entry, vpmDependencies={
            package_settings(name)["name"]: ">=" + read_version(name)
            for name in discover_tools()
        })
        if any(version == ">=" + UNVERSIONED for version in entry["vpmDependencies"].values()):
            raise BuildError("Complete: every dependency needs a release version.")
    name = entry.get("name", "com.fynn.tools." + tool.lower())
    if not re.fullmatch(r"[a-z0-9]+(?:[.-][a-z0-9]+)+", name):
        raise BuildError(f"{tool}: invalid package ID {name!r}")
    title = re.sub(r"(?<=[a-z])(?=[A-Z])", " ", tool)
    return {"name": name, "displayName": entry.get("displayName", title),
            "description": entry.get("description", title),
            "vpmDependencies": entry.get("vpmDependencies", {}),
            "dependencies": entry.get("dependencies", {})}


def repository_name(value: str | None) -> str:
    repository = value or os.environ.get("GITHUB_REPOSITORY")
    if not repository:
        remote = subprocess.run(["git", "remote", "get-url", "origin"], cwd=REPO_ROOT,
                                text=True, capture_output=True, check=False).stdout.strip()
        match = re.fullmatch(r"(?:https://github.com/|git@github.com:)([^/]+/[^/]+?)(?:\.git)?", remote)
        repository = match.group(1) if match else None
    if not repository or not re.fullmatch(r"[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+", repository):
        raise BuildError("Use --repository owner/repo to set the package download URLs.")
    return repository


def build_vpm(tool: str, out_dir: Path, repository: str, release_tag: str | None = None) -> Path:
    assets = collect_assets(tool, DEFAULT_INSTALL_ROOT)
    if any(a.pathname.endswith('.cs') for a in assets) and not any(a.pathname.endswith('.asmdef') for a in assets):
        raise BuildError(f"{tool}: scripts require an assembly definition to compile from Packages.")
    version = read_version(tool)
    if not re.fullmatch(r"\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?", version) or version == UNVERSIONED:
        raise BuildError(f"{tool}: add a release version to CHANGELOG.md before building VPM packages.")
    manifest = package_settings(tool)
    tag = release_tag or f"{tool}-v{version}"
    manifest.update({"version": version, "unity": "2022.3", "author": {"name": "Fynn9563"},
                     "url": f"https://github.com/{repository}/releases/download/{quote(tag, safe='')}/{tool}-{version}.zip"})
    out_dir.mkdir(parents=True, exist_ok=True)
    archive = out_dir / f"{tool}-{version}.zip"
    install_base = f"{DEFAULT_INSTALL_ROOT}/{tool}/"
    with zipfile.ZipFile(archive, "w", compression=zipfile.ZIP_DEFLATED) as output:
        output.writestr("package.json", json.dumps(manifest, indent=2) + "\n")
        for asset in assets:
            if not asset.pathname.startswith(install_base):
                continue
            relative = asset.pathname[len(install_base):]
            if relative == "package.json":
                raise BuildError(f"{tool}: package.json is generated by the builder; do not include a second manifest.")
            output.writestr(relative + ".meta", asset.meta)
            if asset.contents is not None:
                output.writestr(relative, asset.contents)
            else:
                output.writestr(relative + "/", b"")
    listing_entry = dict(manifest, zipSHA256=hashlib.sha256(archive.read_bytes()).hexdigest())
    (out_dir / f"{tool}-{version}.package.json").write_text(json.dumps(listing_entry, indent=2) + "\n", encoding="utf-8")
    verify_vpm(tool, archive)
    print(f"{tool} {version}: VPM ZIP and listing manifest -> {archive.name}")
    return archive


def verify_vpm(tool: str, archive: Path) -> None:
    with zipfile.ZipFile(archive) as package:
        if package.testzip() is not None:
            raise BuildError(f"{tool}: ZIP failed its integrity check.")
        names = package.namelist()
        if len(names) != len(set(names)) or "package.json" not in names:
            raise BuildError(f"{tool}: invalid ZIP structure.")
        for name in names:
            if name.startswith(("/", "Assets/", "Packages/")) or ".." in name.split("/"):
                raise BuildError(f"{tool}: invalid ZIP entry {name!r}")
        for asset in collect_assets(tool, DEFAULT_INSTALL_ROOT):
            prefix = f"{DEFAULT_INSTALL_ROOT}/{tool}/"
            if not asset.pathname.startswith(prefix):
                continue
            relative = asset.pathname[len(prefix):]
            if package.read(relative + ".meta") != asset.meta:
                raise BuildError(f"{tool}: metadata mismatch for {relative}")
            if asset.contents is not None and package.read(relative) != asset.contents:
                raise BuildError(f"{tool}: payload mismatch for {relative}")


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
    parser.add_argument("--format", choices=["both", "unitypackage", "vpm"], default="both")
    parser.add_argument("--repository", help="GitHub owner/repository for VPM download URLs")
    parser.add_argument("--release-tag", help="Download tag; defaults to <Tool>-v<version>")
    args = parser.parse_args()

    available = discover_tools()
    if available and args.format != "unitypackage":
        available.append(COMPLETE)
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
        repository = repository_name(args.repository) if args.format != "unitypackage" else None
        package_ids = [package_settings(name)["name"] for name in available]
        if len(package_ids) != len(set(package_ids)):
            raise BuildError("Package IDs must be unique.")
        for name in args.tools or available:
            if args.format != "vpm" and name != COMPLETE:
                build(name, args.out_dir, args.install_root)
            if args.format != "unitypackage":
                build_vpm(name, args.out_dir, repository, args.release_tag)
    except BuildError as error:
        print(f"error: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
