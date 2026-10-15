import importlib.util
import json
from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import patch

spec = importlib.util.spec_from_file_location('listing', Path(__file__).resolve().parents[1] / 'build-vpm-listing.py')
listing = importlib.util.module_from_spec(spec)
spec.loader.exec_module(listing)


class ListingTests(unittest.TestCase):
    def release(self, version='1.0.0', release_id=1):
        filename = f'Example-{version}'
        url = f'https://github.com/Fynn9563/Fynns-Tools-Free/releases/download/v{version}/{filename}.zip'
        manifest = {'name': 'com.fynn.tools.example', 'version': version, 'url': url,
                    'zipSHA256': 'a' * 64, 'vpmDependencies': {'com.vrchat.base': '>=3.10.5'}}
        return {'id': release_id, 'tag_name': f'v{version}', 'published_at': f'2026-10-{release_id:02d}',
                'assets': [{'name': filename + '.package.json', 'manifest': manifest},
                           {'name': filename + '.zip', 'browser_download_url': url}]}

    def build(self, releases):
        return listing.build_listing('Fynn9563/Fynns-Tools-Free', 'main', releases, lambda a: a['manifest'])

    def test_preserves_versions_and_manifest_fields(self):
        result = self.build([self.release(), self.release('1.1.0', 2)])
        versions = result['packages']['com.fynn.tools.example']['versions']
        self.assertEqual(set(versions), {'1.0.0', '1.1.0'})
        self.assertEqual(versions['1.0.0']['vpmDependencies'], {'com.vrchat.base': '>=3.10.5'})
        self.assertEqual(result['url'], 'https://raw.githubusercontent.com/Fynn9563/Fynns-Tools-Free/main/vpm.json')

    def test_newest_duplicate_wins_deterministically(self):
        old, new = self.release(), self.release(release_id=2)
        new['assets'][0]['manifest']['displayName'] = 'New'
        self.assertEqual(self.build([old, new]), self.build([new, old]))
        self.assertEqual(self.build([old, new])['packages']['com.fynn.tools.example']['versions']['1.0.0']['displayName'], 'New')

    def test_drafts_and_legacy_releases_excluded(self):
        draft = self.release()
        draft['draft'] = True
        self.assertEqual(self.build([draft, {'id': 2, 'assets': [{'name': 'old.unitypackage'}]}])['packages'], {})

    def test_prerelease_included(self):
        release = self.release('1.2.0-beta.1')
        release['prerelease'] = True
        self.assertIn('1.2.0-beta.1', self.build([release])['packages']['com.fynn.tools.example']['versions'])

    def test_invalid_manifest_or_missing_zip_fails(self):
        for field, value in [('name', '../invalid'), ('version', 'bad'), ('version', '2.0.0'),
                             ('url', 'https://example.com/wrong.zip'), ('zipSHA256', '')]:
            with self.subTest(field=field, value=value):
                release = self.release()
                release['assets'][0]['manifest'][field] = value
                with self.assertRaises(ValueError):
                    self.build([release])
        release = self.release()
        release['assets'].pop()
        with self.assertRaises(ValueError):
            self.build([release])

    def test_download_url_checked_against_release(self):
        release = self.release()
        release['assets'][1]['browser_download_url'] = 'https://example.com/file.zip'
        with self.assertRaises(ValueError):
            self.build([release])

    def test_paginated_api_request(self):
        with patch.object(listing.subprocess, 'check_output', return_value='[[{"id":1}],[]]') as call:
            self.assertEqual(listing.github_json('repos/owner/repo/releases?per_page=100', True), [[{'id': 1}], []])
            self.assertEqual(call.call_args.args[0][-2:], ['--paginate', '--slurp'])

    def test_main_writes_listing_and_keeps_output_on_failure(self):
        release = self.release()
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / 'vpm.json'
            argv = ['build-vpm-listing.py', '--repository', 'Fynn9563/Fynns-Tools-Free', '--output', str(output)]
            with patch.object(sys, 'argv', argv), patch.object(listing, 'github_json', side_effect=[[[release]], [release['assets']]]), patch.object(listing, 'read_manifest', return_value=release['assets'][0]['manifest']):
                listing.main()
            before = output.read_bytes()
            self.assertEqual(json.loads(before), self.build([release]))
            with patch.object(sys, 'argv', argv), patch.object(listing, 'github_json', side_effect=[[[release]], [[release['assets'][0]]]]), patch.object(listing, 'read_manifest', return_value=release['assets'][0]['manifest']):
                with self.assertRaises(ValueError):
                    listing.main()
            self.assertEqual(output.read_bytes(), before)


if __name__ == '__main__':
    unittest.main()
