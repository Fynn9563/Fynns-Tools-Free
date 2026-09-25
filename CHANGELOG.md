# Changelog

## FaceEmoPatches

### [1.0.0] - 2026-08-25

First release.

#### Added
- Optional fixes for [FaceEmo](https://suzuryg.github.io/face-emo/), on by default and switchable individually from **Tools > Fynn's Tools > FaceEmo Patches > Settings**
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

## UnityPetFramework

### [1.0.0] - 2026-09-14

First release.

#### Added
- An animated pet that walks around on top of the Unity Editor, switched on from **Tools > Fynn's Tools > Unity Pet Framework**
- Four pets included with the tool: Bun, Grim, Kino and Tiger
- Walking, turning, idling, thinking, jumping and sleeping, with the pet always playing a turn animation before it changes direction
- Smart Slice, which reads a sprite sheet's transparency and works out where the frames are, groups them into likely animations, and associates detached effects such as hearts or question marks with the frame they belong to. Strict, Normal and Loose control how eagerly it does that
- Correction tools for when detection gets it wrong: select, merge, split, delete, add a missed piece, resize, reorder, regroup, and undo
- Frames can be any size and any shape. An animation is not required to use one uniform grid, and animations can be any length
- Per-frame anchors, generated automatically, so differently cropped frames sit still instead of jittering
- An animated Pet Icon, shown in the pet list, the selected pet header and the import and export list
- Each animation is drawn once and mirrored to face the other way, so there is no second copy of a walk cycle to keep in step. Artwork that reads wrong backwards can opt out
- One turn animation covers both directions, played forwards or backwards
- Jumping carries the pet forwards, and stops cleanly at the edge rather than landing outside where it is allowed
- Optional pickup: drag a pet that has a carry pose and drop it where you want it. Petting still works the same way and is separate from dragging
- Behaviours are mapped onto animations rather than being tied to fixed names, so a pet can call its animations whatever it likes
- Reactions to a click, a double click, and to being stroked with the mouse held down
- Reactions to Unity compiling, to errors in the Console, and to entering and leaving play mode, switchable from Settings
- Energy that drains while the pet is busy and fills back up while it sleeps, with an optional bar in the corner of the editor. Nothing bad happens when it runs out
- Safe UI mode, which keeps the pet inside the Scene and Game views, and Free Roam, which lets it use the whole Unity window
- A Pet Creator for building a pet from any sprite sheet: click the frames in the order they play, mirror any of them, set the speed, and preview it
- Pet packages (`.fynnpet`) for sharing a pet as one file. Exporting packs the sheet down to only the frames the pet uses, cropped to the drawn area
- Optional sounds, one per animation
- The pet is editor only. Nothing it uses is included in a built game, and nothing is written into your scenes or project assets
- A pet package can only contain pictures, sounds and a description of its animations. It is never added to your project and nothing inside it is ever run

## VRCSdkPatches

### [1.0.0] - 2026-09-25

First release.

#### Added
- Optional fixes for the VRChat SDK's build panel, on by default and switchable individually from **Tools > Fynn's Tools > VRC SDK Patches > Settings**
- The **Select** button on the "unsupported shader" Quest build error now selects the objects actually using that shader, instead of the avatar root. All of them are selected at once when several share the shader, and the first is highlighted in the Hierarchy
- Patches are applied to the running editor only. The SDK's own files are never modified, and clearing a tick box restores the original behaviour straight away
- A patch reports itself as unavailable instead of failing when a future SDK version moves the part it relies on
