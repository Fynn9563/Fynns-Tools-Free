# MultiUploader

Build, test or publish multiple VRChat avatars for PC, Quest, or both. Includes
optional Quest material conversion and a PC/Quest material preview.

## Requirements

- Unity 2022.3 with the build modules for your chosen platforms.
- VRChat Avatars SDK 3.10.5 or later and NDMF 1.14.8 or later.
- Poiyomi's decal baker when converting supported Poiyomi decals.

## Quick start

1. Remove any previous embedded uploader package before installing this version
   through VCC or importing the standalone `.unitypackage` to avoid duplicate scripts.
2. Open **Tools > Fynn's Tools > MultiUploader** and select avatars from the open
   scenes. Save their scenes before starting.
   Use **Ignore uploads** to drag in avatars or their hierarchy children, or
   choose **Ignore hierarchy selection**. This skips the whole avatar for builds,
   tests and uploads. Remove it from the list or tick its checkbox to include it again.
3. Choose PC, Quest, or PC + Quest. Review Quest conversion settings and use the
   material preview before building.
   Drag hierarchy objects into **Ignore Quest material conversion**, or use
   **Add hierarchy selection**, to keep their materials unchanged. Children are
   included. Each avatar saves its own list, with Remove buttons to undo exclusions.
4. Choose Build Only All, Build & Test All, or Build & Publish All.

Publishing updates existing avatar listings. Upload new avatars through the
regular SDK once to obtain their blueprint IDs. The queue returns to its starting
platform by default. Cancel stops later jobs and requests cancellation of an
upload already running.

Quest conversion works on build copies and uses VRChat/Mobile/Toon Standard.
It does not reduce meshes or guarantee Quest performance limits. Opaque
approximation is enabled by default and changes transparent surfaces; inspect
the preview or supply prepared Quest material overrides. Generated textures are
cached in `Assets/FynnMultiUploaderGenerated`.

Ignored objects stay in the avatar and keep their animated material swaps.
Their materials must still pass VRChat's Quest validation.

Report bugs or propose changes through [issues](https://github.com/Fynn9563/Fynns-Tools-Free/issues) and pull requests.

Created by [Fynn9563](https://github.com/Fynn9563). [MIT license](LICENSE.md).
