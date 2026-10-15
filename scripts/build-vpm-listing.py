import argparse
import json
from pathlib import Path
import re
import subprocess
from urllib.parse import quote


def github_json(endpoint, paginate=False):
    args = ['gh', 'api', endpoint]
    if paginate:
        args += ['--paginate', '--slurp']
    return json.loads(subprocess.check_output(args, text=True, encoding='utf-8'))


def read_manifest(repository, asset):
    asset_id = int(asset['id'])
    data = subprocess.check_output([
        'gh', 'api', f'repos/{repository}/releases/assets/{asset_id}',
        '-H', 'Accept: application/octet-stream',
    ])
    return json.loads(data)


def build_listing(repository, branch, releases, load_manifest):
    packages = {}
    # Newest publication wins when the same version appears in multiple releases.
    releases = sorted(releases, key=lambda r: (r.get('published_at') or '', r['id']), reverse=True)
    for release in releases:
        if release.get('draft'):
            continue
        assets = {asset['name']: asset for asset in release['assets']}
        for filename, asset in sorted(assets.items()):
            if not filename.endswith('.package.json'):
                continue
            manifest = load_manifest(asset)
            name, version = manifest.get('name', ''), manifest.get('version', '')
            if not re.fullmatch(r'com\.fynn\.tools\.[a-z0-9][a-z0-9.-]*', name):
                raise ValueError(f'{filename}: invalid package ID')
            if not re.fullmatch(r'(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?', version):
                raise ValueError(f'{filename}: invalid version')
            if not filename.endswith(f'-{version}.package.json'):
                raise ValueError(f'{filename}: version does not match filename')
            zip_name = filename.removesuffix('.package.json') + '.zip'
            archive = assets.get(zip_name)
            if archive is None:
                raise ValueError(f'{filename}: missing ZIP asset')
            expected = f'https://github.com/{repository}/releases/download/{quote(release["tag_name"], safe="")}/{quote(zip_name, safe="")}'
            if archive.get('browser_download_url') != expected or manifest.get('url') != expected:
                raise ValueError(f'{filename}: download URL does not match release ZIP')
            if not re.fullmatch(r'[0-9a-fA-F]{64}', manifest.get('zipSHA256', '')):
                raise ValueError(f'{filename}: missing or invalid ZIP checksum')
            versions = packages.setdefault(name, {'versions': {}})['versions']
            versions.setdefault(version, manifest)
    return {
        'name': "Fynn's Tools Free",
        'id': 'com.fynn.tools.free',
        'url': f'https://raw.githubusercontent.com/{repository}/{quote(branch, safe="")}/vpm.json',
        'author': 'Fynn9563',
        'packages': packages,
    }


def main():
    parser = argparse.ArgumentParser(description='Build the VPM listing from published release manifests.')
    parser.add_argument('--repository', required=True)
    parser.add_argument('--branch', default='main')
    parser.add_argument('--output', type=Path, default=Path('vpm.json'))
    args = parser.parse_args()
    if not re.fullmatch(r'[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+', args.repository):
        parser.error('repository must be owner/repo')
    pages = github_json(f'repos/{args.repository}/releases?per_page=100', paginate=True)
    releases = [release for page in pages for release in page]
    for release in releases:
        if not release.get('draft'):
            asset_pages = github_json(f'repos/{args.repository}/releases/{int(release["id"])}/assets?per_page=100', paginate=True)
            release['assets'] = [asset for page in asset_pages for asset in page]
    listing = build_listing(args.repository, args.branch, releases, lambda asset: read_manifest(args.repository, asset))
    args.output.write_text(json.dumps(listing, indent=2, sort_keys=True) + '\n', encoding='utf-8')
    print(f'Wrote {args.output}: {len(listing["packages"])} packages, '
          f'{sum(len(p["versions"]) for p in listing["packages"].values())} versions')


if __name__ == '__main__':
    main()
