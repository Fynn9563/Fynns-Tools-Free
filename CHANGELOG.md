# Changelog

## Complete

### [1.0.0] - 2026-10-15

- Added Fynn's Tools Free Complete, an empty VPM package that installs every standalone tool through dependencies.

## AnimationHierarchyEditor

### [1.1.0] - 2026-10-15

#### Added
- Motion replacement across controller layers and nested BlendTrees, with scoped previews and Undo/Redo.
- Automatic hierarchy repathing and broken-binding repair with persistent object assignments and collision checks.

#### Fixed
- Keep unresolved bindings visible after applying repairs and retain picker choices between redraws.
- Report invalid animated properties and previews without path changes.

### [1.0.0] - 2026-10-09

Initial standalone source package.

#### Added
- Single and multiple clip path editing, text/regex replacement and hierarchy object assignment.
- Preflight collision checks across all selected clips and one Undo operation per edit.
- Float and object-reference curves, root paths, editable asset checks and Fynn9563 GitHub credit.
- One menu under **Tools > Fynn's Tools > Animation Hierarchy Editor**; no SDK dependency.

## AssetOrganizer

### [1.0.3] - 2026-10-09

#### Fixed
- Automatically selected Skip stays editable like other asset rules.

### [1.0.2] - 2026-10-09

#### Fixed
- Assets already in the destination use the existing Skip option. Changing the destination checks them again.

### [1.0.1] - 2026-10-09

#### Fixed
- Scene avatars and prefabs default to their own named destination folder instead of the Assets root or source folder.
- Empty selections have no default destination; custom destinations are kept when rescanning.

### [1.0.0] - 2026-10-09

Initial standalone source package.

#### Added
- Asset dependency and folder scanning with original type categories, exclusions and Move/Skip defaults.
- Destination preview, unique filenames, protected-asset checks and move failure reporting.
- Working Delete Empty Folders setting and session recovery through Revert Last Move.
- One menu under **Tools > Fynn's Tools > Asset Organizer**, with Fynn9563 GitHub credit and no SDK dependency.

## BuildSizeViewer

### [1.0.0] - 2026-10-09

Initial standalone source package.

#### Added
- Current and saved Editor log reading without an adjacent temporary copy.
- Separate bundle reports, VRChat report selection, failed/incomplete build warnings and malformed-row handling.
- Compressed size, uncompressed categories, asset filtering, safe Go selection and resizable categories.
- One menu under **Tools > Fynn's Tools > Build Size Viewer**, without an SDK dependency.
- Fynn9563 GitHub credit and original MunifiSense credit and MIT license.

## CameraUtility

### [1.0.0] - 2026-10-09

Initial standalone source package.

#### Added
- Direct Match Scene to Game and Match Game to Scene actions, automatic main camera/fallback selection and Undo/Redo.
- Correct Scene view positioning, optional perspective/orthographic lens matching and portrait view support.
- Prefab instance overrides and validation of missing, persistent, preview, locked and read-only camera targets.
- Quick actions and optional Settings under **Tools > Fynn's Tools > Camera Utility**, with Fynn9563 GitHub credit and no SDK dependency.

## ClippingTester

### [1.0.0] - 2026-10-09

Initial standalone source package.

#### Added
- Isolated humanoid/generic animation preview, all six original clips, scrubbing, frame stepping and speed presets.
- Loop sections, animation playlists and saved problem poses.
- Multiple body/clothing mesh selections before or during playback, per-piece material slots, temporary contrast colours and real mesh wireframe.
- Optional VRCFury/Modular Avatar preparation through the Avatar SDK build pipeline, enabled automatically for supported avatars.
- Optional VRChat SDK PhysBone simulation during playback, with isolated ownership and cleanup.
- One menu under **Tools > Fynn's Tools > Clipping Tester** with Fynn9563 GitHub credit; visual inspection without automatic clipping detection.

## ClientSimStorageCleaner

### [1.0.0] - 2026-09-30

First release.

#### Added
- Deletes the `ClientSimStorage` folder at the project root every time play mode ends, so each ClientSim test run starts as a new player. On by default, switchable from **Tools > Fynn's Tools > ClientSim Storage Cleaner > Settings**
- **Clear Now** in the same menu, for deleting the folder at any time outside play mode
- The delete waits for ClientSim to finish writing and retries for a few seconds, then warns in the Console instead of failing silently
- Nothing under `Assets`, `Packages` or `Library` is touched, and a project without the folder is left alone

## ControllersPlus

### [1.0.0] - 2026-10-15

#### Added
- Controllers+ under **Tools > Fynn's Tools**, with controller, state, transition, parameter and BlendTree editing.
- Thirteen bundled controller templates and their animation/mask resources; SDK-specific templates appear when the Avatar SDK is available.
- Fuzzy motion search, parameter usage and animation searches, persistent notes, coloured types/status icons, ordering and conflict diagnostics.
- BlendTree clipboard, internal/external creation, child reparenting, constant parameter clips, smoothing and grouped object/component toggles.
- Optional VRChat expression parameter actions, dynamics indicators, unused-blendshape and FX-only animation searches.
- Graph and Inspector shortcuts, transition labels, colour rules, and selected-state/transition arrow animation.
- Assets and VPM package builds, including an Editor-only Harmony dependency for projects without the SDK.

#### Fixed
- Redirect transitions without retaining the old connection; preserve settings and Undo/Redo.
- Use readable compact reference labels and constrain motion labels/icons within state nodes.
- Resolve filtered parameter rows correctly and add VRChat built-ins to the native dropdown.
- Copy BlendTrees through serialized data without Unity strong-pointer assertions; preserve shared motions, copied references and Undo/Redo.

## ControllerCleaner

### [1.0.0] - 2026-10-10

Initial standalone source package, tested and approved by the user.

#### Added
- Original controller scanner and cleanup with a refreshed window and Fynn9563 GitHub credit.
- One menu under **Tools > Fynn's Tools > Controller Cleaner**; no SDK dependency.

#### Fixed
- Preserve blend trees used by synced-layer motion overrides, including nested blend trees.
- Prevent failed or cancelled scans from cleaning incomplete results.

## CopyCutPaste

### [1.0.0] - 2026-10-10

Initial standalone source package, tested and approved by the user.

#### Added
- Original Assets/Copy, Assets/Cut and Assets/Paste commands for Project files and folders.
- Editor-only assembly with no SDK dependency and Fynn9563 GitHub credit in the guide.

#### Fixed
- Repeating cut/paste through a selected destination file no longer renames an asset already in that folder.

## DynamicsPlus

### [1.1.0] - 2026-10-11

#### Added
- Organize PhysBones and colliders with scoped selection, custom folders, naming and destination previews.
- Remove selected components with reference review, optional candidate highlighting and one-step Undo/Redo.
- Preserve serialized scene references, effective roots and activation when organizing; protect animation bindings, existing simulation chains and inherited prefab components.

#### Changed
- Open the working panel directly from the main menu, with Components, Organize and Remove visible across the top.
- Keep all settings in the same window, including the scene settings shortcut.

### [1.0.0] - 2026-10-09

Initial standalone source package.

#### Added
- Dynamics+ under **Tools > Fynn's Tools > Dynamics+**, with the Fynn9563 GitHub credit.
- PhysBone, collider and Contact inspectors, scene editing, curves, conversions and component browser.
- Isolated PhysBone testing with move/rotation handles, Restart, Apply and cleanup.
- Avatar and World SDK support, with separate Avatar Animator helpers.
- Readable C# source directly under `DynamicsPlus/Editor`, with no DLL or Source folder required.

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

## MatCapMaker

### [1.0.0] - 2026-10-11

#### Added
- One gallery of 31 editable presets with procedural stone, Julia fractals, crystal facets, shards, petals and other surfaces.
- Studio reflection panels, lighting, layer ordering with Undo/Redo, and an embedded texture generator.
- Bloom, PNG/EXR export up to 4096px, isolated material capture and reflection-probe capture.
- Saved presets with embedded generated/captured textures and retained layer order.
- One menu under **Tools > Fynn's Tools > MatCap Maker**, with Fynn9563 credit and no SDK dependency.

## NoAutoGame

### [1.0.0] - 2026-10-11

#### Added
- Keep the previous editor tab focused when Unity switches to Game at Play entry.
- Close newly opened Game tabs while preserving existing tabs and tabs opened later during Play.
- Independent settings enabled by default, reload-safe startup handling and callback cleanup.
- One menu under **Tools > Fynn's Tools > No Auto Game**, with Fynn9563 credit and no SDK dependency.

## PackageImporter

### [1.0.0] - 2026-10-11

#### Added
- Select and queue Unity packages from folders, individual files and drag and drop.
- Persistent queue with matching import events, reload recovery and explicit uncertain-result controls.
- Archive validation, byte-identical package skipping and native review for existing assets.
- Stop after the current package; pause on failure or cancellation and verify assets after automatic imports.
- One menu under **Tools > Fynn's Tools > Package Importer**, with Fynn9563 credit and no Harmony or SDK dependency.

## QuickToggle

### [1.0.0] - 2026-10-11

#### Added
- Standalone object/component and blendshape clips, direct VRChat, VRCFury and Modular Avatar setup.
- Native VRCFury object and renderer-specific blendshape actions without generated animation files; component-enabled bindings use one necessary clip.
- Shared targets across Objects & Components and Blendshapes, grouping and preserved shape selections/values.
- Saved/default states, optional integrations, preflight checks, unique output files and one-step setup Undo/Redo.
- Verified with 72 regression checks and 186 build/playback checks across 18 scenarios in Unity 2022.3.22f1, SDK 3.10.5, VRCFury 1.1431.0 and Modular Avatar 1.18.7.

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

### [Unreleased]

#### Changed
- Merged into VRCSDK+; the unsupported-shader selection fix is included in the VRCSDKPlus package.
- Move patch controls into the existing **VRChat SDK > Settings > SDK Patches** section and remove the separate VRC SDK Patches menu and window.
- Keep the patch enabled by default, independently switchable from the VRCSDK+ layout, and preserve existing patch preferences.

#### Fixed
- Cached build-error Select actions respect later patch preference changes.
- Match shader names containing apostrophes and fall back to the SDK selection action when no matching renderer remains.

### [1.0.0] - 2026-09-25

First release.

#### Added
- Optional fixes for the VRChat SDK's build panel, on by default and switchable individually from **Tools > Fynn's Tools > VRC SDK Patches > Settings**
- The **Select** button on the "unsupported shader" Quest build error now selects the objects actually using that shader, instead of the avatar root. All of them are selected at once when several share the shader, and the first is highlighted in the Hierarchy
- Patches are applied to the running editor only. The SDK's own files are never modified, and clearing a tick box restores the original behaviour straight away
- A patch reports itself as unavailable instead of failing when a future SDK version moves the part it relies on

## HierarchyPlus

### [1.0.1] - 2026-10-15

#### Fixed
- Hide empty collection folders in unrelated scenes, including collections whose objects belong to another scene.
- Keep collection tooltip bullets readable across source encodings.

### [1.0.0] - 2026-10-11

Initial standalone source package, tested and approved by the user.

#### Added
- Hierarchy+ settings, icons, guide lines, full layer labels and icon fading.
- Enhanced Mode with virtual collection folders, shared/focus controls and independent display ordering.
- Native object controls, parenting and an explicit Apply Order action for real sibling order.
- Compact collection badges and a normal-mode badge visibility option.
- Included icon assets, original GPL license and Fynn9563 credit.



## VRCSDKPlus

### [1.0.0] - 2026-10-13

#### Added
- Enhanced expression menu and parameter inspectors with active-avatar selection, parameter import, cleanup, memory usage and missing-controller-parameter creation.
- Inline Find Usages, Rename Everywhere, validation, parameter presets and branching Menu Tree panels, with Undo/Redo for asset edits.
- Rename previews identify existing uses and conflicts across the parameter file, connected menus and selected avatar controllers, with an explicit option to reconnect existing destination-name references.
- Themed SDK Authentication, Builder, Content Manager and Settings panels with rounded controls and scrollable layouts.
- Content Manager categories for All, Avatars, Worlds and Local Tests, name/content-ID search, visibility filters, name sorting and inline management actions.
- Scene capture options for ignored objects and their children, animation posing with time scrubbing, and image backgrounds.
- Optional outfit preparation for posed captures through the SDK build pipeline, including VRCFury and Modular Avatar, reused when changing animation clips.
- Default-on VRCSDK+ layout control and separate SDK patch controls in the existing SDK Settings tab.
- The merged unsupported-shader Select fix from VRCSdkPatches, retaining its existing preferences and availability reporting.
- Assets-based unitypackage and Packages-based VPM ZIP builds.

#### Changed
- Place inspector feature buttons below the normal menu/parameter controls; clicking an open feature button again closes its inline panel.
- Use compact parameter columns and reference rows, readable transition labels, and the consistent clickable **Created by Fynn9563** credit.
- Keep native SDK content-management and build actions behind the enhanced presentation without editing or bundling SDK files.

#### Fixed
- Restore standard SDK panels, capture controls and expression inspectors when the VRCSDK+ layout is disabled, while retaining independently enabled SDK patches.
- Prevent old/new Content Manager layout flicker during fetching and management, use the available window height, and align search and clear controls.
- Apply themed control styles without briefly displaying grey buttons when switching SDK tabs.
- Display nested submenu branches, missing submenu references and cycles in Menu Tree.
- Keep rename selections synchronized after parameter edits and show where conflicting names are used instead of rejecting every existing reference.
- Detect assigned playable-layer controllers and show Add to Controller only when parameters are missing.
- Keep the capture panel within the SDK window, with compact options and independently enabled object ignoring and posing.
- Make Fill Background and Image Background mutually exclusive, preserve avatar rendering during image compositing, and keep image backgrounds upright.
- Reuse prepared outfit captures when changing pose clips and restore temporary capture state when previewing ends.

## TextureTools

### [1.0.0] - 2026-10-13

- Standalone tool with Assets and VPM package builds.

## ResetHumanoidTPose

### [1.0.0] - 2026-10-13

- Standalone tool with Assets and VPM package builds.
