import hashlib
import importlib.util
import json
from pathlib import Path
import sys
import tarfile
import tempfile
import unittest
import zipfile

spec = importlib.util.spec_from_file_location('builder', Path(__file__).resolve().parents[1] / 'build-unitypackage.py')
builder = importlib.util.module_from_spec(spec)
sys.modules[spec.name] = builder
spec.loader.exec_module(builder)


class PackageTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        self.saved = builder.REPO_ROOT, builder.CHANGELOG, builder.PACKAGE_SETTINGS
        builder.REPO_ROOT = self.root
        builder.CHANGELOG = self.root / 'CHANGELOG.md'
        builder.PACKAGE_SETTINGS = self.root / 'packages.json'
        builder.CHANGELOG.write_text('## Example\n\n### [1.2.3]\n', encoding='utf-8')
        (self.root / 'Example/Editor').mkdir(parents=True)
        for index, name in enumerate(['Example', 'Example/Editor', 'Example/Editor/Tool.cs', 'Example/Editor/Test.asmdef']):
            path = self.root / name
            if path.suffix:
                path.write_text('payload', encoding='utf-8')
            path.with_name(path.name + '.meta').write_text(f'fileFormatVersion: 2\nguid: {index:032x}\n', encoding='utf-8')
        builder.PACKAGE_SETTINGS.write_text(json.dumps({'Example': {'name': 'com.fynn.tools.example', 'vpmDependencies': {'com.vrchat.base': '>=3.10.5'}, 'dependencies': {'com.unity.textmeshpro': '3.0.9'}}}), encoding='utf-8')

    def tearDown(self):
        builder.REPO_ROOT, builder.CHANGELOG, builder.PACKAGE_SETTINGS = self.saved
        self.temp.cleanup()

    def test_both_formats_preserve_assets_and_guids(self):
        unity = builder.build('Example', self.root / 'dist', builder.DEFAULT_INSTALL_ROOT)
        archive = builder.build_vpm('Example', self.root / 'dist', 'owner/repo')
        with tarfile.open(unity) as tar:
            paths = [tar.extractfile(entry).read().decode() for entry in tar if entry.name.endswith('/pathname')]
            self.assertIn("Assets/Fynn's Tools/Example/Editor/Tool.cs", paths)
        with zipfile.ZipFile(archive) as package:
            self.assertEqual(package.read('Editor/Tool.cs'), b'payload')
            self.assertEqual(package.read('Editor/Tool.cs.meta'), (self.root / 'Example/Editor/Tool.cs.meta').read_bytes())
            manifest = json.loads(package.read('package.json'))
            self.assertEqual(manifest['vpmDependencies'], {'com.vrchat.base': '>=3.10.5'})
            self.assertEqual(manifest['dependencies'], {'com.unity.textmeshpro': '3.0.9'})
            self.assertNotIn('legacyFolders', manifest)
            self.assertNotIn('zipSHA256', manifest)
            self.assertEqual(manifest['url'], 'https://github.com/owner/repo/releases/download/Example-v1.2.3/Example-1.2.3.zip')
        listing = json.loads((archive.parent / 'Example-1.2.3.package.json').read_text())
        self.assertEqual(listing['zipSHA256'], hashlib.sha256(archive.read_bytes()).hexdigest())

    def test_collection_release_url_uses_actual_tag(self):
        archive = builder.build_vpm('Example', self.root / 'dist', 'owner/repo', 'v2.0.0')
        with zipfile.ZipFile(archive) as package:
            self.assertIn('/download/v2.0.0/', json.loads(package.read('package.json'))['url'])

    def test_complete_is_manifest_only_and_depends_on_all_tools(self):
        with builder.CHANGELOG.open('a', encoding='utf-8') as changelog:
            changelog.write('\n## Complete\n\n### [1.0.0]\n')
        archive = builder.build_vpm(builder.COMPLETE, self.root / 'dist', 'owner/repo', 'v2.0.0')
        with zipfile.ZipFile(archive) as package:
            self.assertEqual(package.namelist(), ['package.json'])
            manifest = json.loads(package.read('package.json'))
        self.assertEqual(manifest['vpmDependencies'], {'com.vrchat.avatars': '>=3.10.5', 'com.fynn.tools.example': '>=1.2.3'})
        self.assertEqual(manifest['name'], 'com.fynn.tools.complete')
        self.assertIn('/download/v2.0.0/Complete-1.0.0.zip', manifest['url'])
        listing = json.loads(archive.with_name('Complete-1.0.0.package.json').read_text())
        self.assertEqual(listing['zipSHA256'], hashlib.sha256(archive.read_bytes()).hexdigest())

    def test_complete_rejects_unversioned_dependencies(self):
        builder.CHANGELOG.write_text('## Complete\n\n### [1.0.0]\n', encoding='utf-8')
        with self.assertRaises(builder.BuildError):
            builder.build_vpm(builder.COMPLETE, self.root / 'dist', 'owner/repo')

    def test_complete_packages_exclude_opposite_sdk_tools(self):
        settings = json.loads(builder.PACKAGE_SETTINGS.read_text())
        for name, sdk in [('AvatarOnly', 'com.vrchat.avatars'), ('WorldOnly', 'com.vrchat.worlds')]:
            (self.root / name).mkdir()
            (self.root / (name + '.meta')).write_text('fileFormatVersion: 2\nguid: ' + 'a' * 32)
            settings[name] = {'name': 'com.fynn.tools.' + name.lower(), 'vpmDependencies': {sdk: '>=3.10.5'}}
            with builder.CHANGELOG.open('a') as log:
                log.write('\n## ' + name + '\n\n### [1.0.0]\n')
        builder.PACKAGE_SETTINGS.write_text(json.dumps(settings))
        for bundle, sdk, included, excluded in [
            ('Complete', 'com.vrchat.avatars', 'avataronly', 'worldonly'),
            ('CompleteWorlds', 'com.vrchat.worlds', 'worldonly', 'avataronly')
        ]:
            dependencies = builder.package_settings(bundle)['vpmDependencies']
            self.assertIn(sdk, dependencies)
            self.assertIn('com.fynn.tools.example', dependencies)
            self.assertIn('com.fynn.tools.' + included, dependencies)
            self.assertNotIn('com.fynn.tools.' + excluded, dependencies)

    def test_missing_metadata_rejected(self):
        (self.root / 'Example/Editor/Tool.cs.meta').unlink()
        with self.assertRaises(builder.BuildError):
            builder.build_vpm('Example', self.root / 'dist', 'owner/repo')

    def test_unversioned_package_rejected(self):
        builder.CHANGELOG.write_text('', encoding='utf-8')
        with self.assertRaises(builder.BuildError):
            builder.build_vpm('Example', self.root / 'dist', 'owner/repo')

    def test_scripts_without_assembly_definition_rejected(self):
        (self.root / 'Example/Editor/Test.asmdef').unlink()
        (self.root / 'Example/Editor/Test.asmdef.meta').unlink()
        with self.assertRaises(builder.BuildError):
            builder.build_vpm('Example', self.root / 'dist', 'owner/repo')

    def test_corrupt_payload_detected(self):
        archive = builder.build_vpm('Example', self.root / 'dist', 'owner/repo')
        (self.root / 'Example/Editor/Tool.cs').write_text('changed', encoding='utf-8')
        with self.assertRaises(builder.BuildError):
            builder.verify_vpm('Example', archive)


if __name__ == '__main__':
    unittest.main()
