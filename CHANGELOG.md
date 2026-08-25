# Changelog

## FaceEmoPatches

### [1.0.0] - 2026-08-25

First release.

#### Added
- Optional fixes for [FaceEmo](https://suzuryg.github.io/face-emo/), each turned on individually from **Tools > Fynn's Tools > FaceEmo Patches > Settings**
- Transparent thumbnail backgrounds, so the expression menu icons FaceEmo generates keep their alpha instead of arriving with a solid backdrop behind the face
- Blend shape picker sized to its contents, so long names stay readable in the popup opened by the **+** button under Excluded Blend Shapes
- Patches are applied to the running editor only. FaceEmo's own files are never modified, and clearing a tick box restores the original behaviour straight away
- A patch reports itself as unavailable instead of failing when FaceEmo is missing, or when a future version of it moves what the patch relies on

## GoGoLocoPoseChanger

### [1.0.0] - 2026-08-04

First release.

#### Added
- Replaces GoGo Loco's stand, crouch, prone, fall and AFK animations on a VRChat avatar while the avatar is built, leaving the installed GoGo Loco asset untouched
- Avatar Thumbnail pose replacement
- AFK as either one looping motion or a three-part Start / Loop / End sequence, with the inspector showing only the fields for the selected mode
- Fields left empty are skipped, so a single pose can be replaced without affecting the others
- Added from **GameObject > Fynn's Tools > GoGoLoco Pose Changer > Add Prefab**, or the same path under **Tools > Fynn's Tools**. The object is placed under the avatar root and the inspector reports which avatar it found
- Adding it when the avatar already has one selects the existing object instead of adding a second, since only the first is used
- Optional debug logging that lists every replaced animation in the Console during the build
- Warnings instead of build failures when GoGo Loco is absent, no animations are set, or the three-part AFK sequence is missing a motion
