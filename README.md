# Fynn's Tools Free

Free tools for the Unity Editor.

| Tool | What it does |
|---|---|
| [FaceEmo Patches](#faceemo-patches) | Optional fixes for FaceEmo: transparent menu icons, and a blend shape picker that fits its contents |
| [GoGoLoco Pose Changer](#gogoloco-pose-changer) | Swap GoGo Loco's stand, crouch, prone, fall and AFK animations on your VRChat avatar |
| [Unity Pet Framework](#unity-pet-framework) | An animated pet that walks around on top of the Unity Editor |

---

## FaceEmo Patches

Optional fixes for [FaceEmo](https://suzuryg.github.io/face-emo/). Each one is a tick box, and
both are on as soon as you install it. Untick one to turn it off.

FaceEmo is never edited to do it. The fixes are applied to the Unity Editor while it is running, so
FaceEmo itself stays untouched and updating it changes nothing here.

### Requirements

Unity 2022.3 or newer, plus:

| Needed | Install |
|---|---|
| VRChat SDK (Avatars 3.0) | Already included with VCC |
| [FaceEmo](https://suzuryg.github.io/face-emo/) | [Add to VCC](https://suzuryg.github.io/face-emo/) |

### Install

Download the latest `.unitypackage` from
[Releases](https://github.com/Fynn9563/Fynns-Tools-Free/releases/latest), drag it into your open
Unity project, and click **Import**.

### How to use

Both fixes are already on, so there is nothing to do to start using them. To turn one off:

1. **Tools > Fynn's Tools > FaceEmo Patches > Settings**
2. Untick it

Ticking or unticking one takes effect straight away. There is nothing to save and no need to restart
Unity.

### What each one fixes

| Patch | What it changes |
|---|---|
| Transparent thumbnail backgrounds | FaceEmo renders its thumbnails onto an opaque background, and reuses those same images as your expression menu icons. This renders them with transparency instead, so the icons have no box behind the face. |
| Fit the blend shape picker to its contents | The **+** button under Excluded Blend Shapes opens a popup fixed at 200 by 200 pixels. Entries are listed as the mesh path followed by the blend shape name, so the name is usually the part cut off. This sizes the popup to its longest entry. |

### If a tick box is greyed out

The patch says why underneath it. That happens when FaceEmo is not installed, or when the installed
version of FaceEmo has moved the part the patch relies on. Your project is left alone either way.

---

## GoGoLoco Pose Changer

Replace [GoGo Loco](https://booth.pm/en/items/3290806)'s built-in poses with your own animations.

Nothing is edited to do it. The swap happens while your avatar is being built, so GoGo Loco itself
stays untouched and your changes keep working after you update it.

### Requirements

Unity 2022.3 or newer, plus:

| Needed | Install |
|---|---|
| VRChat SDK (Avatars 3.0) | Already included with VCC |
| [NDMF](https://github.com/bdunderscore/ndmf) | [Add to VCC](https://modular-avatar.nadena.dev/) |
| [GoGo Loco](https://booth.pm/en/items/3290806) | [Add to VCC](https://spokeek.github.io/goloco/) |

### Install

1. Download the latest `.unitypackage` from
   [Releases](https://github.com/Fynn9563/Fynns-Tools-Free/releases/latest)
2. Drag it into your open Unity project, or use **Assets > Import Package > Custom Package**
3. Click **Import**

### How to use

1. Select any object under your avatar
2. **GameObject > Fynn's Tools > GoGoLoco Pose Changer > Add Prefab**
3. Drop your animations into the fields on the object that appears

Any field left empty is left alone, so you can change a single pose without touching the others.

**AFK** takes either one looping animation, or tick **AFK motion has start and end** to use a
three-part Start / Loop / End sequence instead.

**Enable Debug Logging** under Settings prints every animation it replaced to the Console when you
build. That is the quickest way to confirm it worked.

### If nothing changes

A warning appears in the Console and your avatar is left untouched when:

- GoGo Loco is not in the project
- No animations have been set
- **AFK motion has start and end** is ticked but Start, Loop, or End is missing

---

## Unity Pet Framework

An animated pet that walks around on top of the Unity Editor. Four pets are included, and you can
build your own from any sprite sheet without writing anything.

The pet lives in the editor only. Nothing it uses is included in a built game, and nothing is
written into your scenes or project assets. The pet itself is shown on Windows; the rest of the
tool, including building and sharing pets, works the same everywhere.

### Requirements

Unity 2022.3 or newer. Nothing else: no SDK, no other packages.

The pet is drawn on Windows only.

### Install

Download the latest `.unitypackage` from
[Releases](https://github.com/Fynn9563/Fynns-Tools-Free/releases/latest), drag it into your open
Unity project, and click **Import**.

### How to use

1. **Tools > Fynn's Tools > Unity Pet Framework**
2. On the **Pet** tab, tick **Show the pet**

The pet walks along the bottom of whatever it is standing on, turns around when it reaches the end,
stops to think, sits down for a rest, jumps about, and goes to sleep when it runs out of energy.

| What you do | What it does |
|---|---|
| Click it | Looks pleased |
| Click and hold | Gets petted, for as long as you hold, and perks up |
| Click it again and again | Puts up with three, then gets annoyed |
| Double click it | Startles, and once that has played, walks somewhere else. Use this when it is in the way |
| Double click and hold | Picks it up, if the pet has a carry pose. Drop it where you like |

It also reacts to Unity: thinking while it compiles, startling at errors in the Console, and
celebrating when you enter play mode. All of that can be turned off under **Settings**, and the
**About** tab lists every interaction.

**Settings** has its size, speed, how calm it is, how long its energy lasts, and whether it stays
inside the Scene and Game views or roams the whole Unity window.

### Making your own pet

The **Pet Creator** tab builds a pet from a sprite sheet. Every frame lives on one PNG laid out on
an even grid.

1. Choose the sheet and set the frame size
2. Pick an animation from the list on the left
3. Click cells on the sheet in the order they play
4. Watch it in the preview underneath

**Smart Slice** reads the sheet's transparency and works out where the frames are, so the normal
job is correcting a detection rather than drawing a box around every sprite by hand. It groups
frames into likely animations and attaches detached effects, such as a heart or a question mark, to
the frame they belong with. **Strict**, **Normal** and **Loose** control how eagerly it does that;
Normal is the default.

When it gets something wrong you can select, merge, split, delete, add a missed piece, resize,
reorder, move a frame to another group, or undo.

Frames do not have to be the same size, and animations can be any length. Each frame carries its
own anchor so differently cropped frames sit still rather than jittering; anchors are generated for
you and only need correcting occasionally.

You only draw each animation once. The pet is drawn facing one way, and the framework mirrors it
when it walks the other way, so there is no second walk cycle to draw or to keep in step. Artwork
that reads wrong backwards can opt out with **Allow Mirroring**.

Turning is the exception, because a turn is a rotation rather than the same pose seen from the
other side. One **Turn Standing** animation covers both directions: it plays forwards to turn left
to right, and backwards to turn the other way. **Turn Sitting** is the same thing sat down, used
when the pet rolls over in its sleep.

If a sheet really is a clean grid, there is still a grid mode that takes either the frame size or
the number of columns and rows and works out the rest.

You can also assign frames as a **Pet Icon**. One frame gives a still icon, several give an
animated one, and it is used wherever the tool lists pets.

### Sharing pets

**Import / Export** writes a pet out as a single `.fynnpet` file. Exporting packs the sheet down to
only the frames the pet actually uses, cropped to the drawn area, which usually makes it a good
deal smaller.

A pet package can only contain pictures, sounds and a description of its animations. It is never
added to your project and nothing inside it is ever run. Anything unexpected inside one and the
whole package is refused.

Imported pets are kept outside your project, in your own app data folder, so they follow you rather
than the project you happened to import them in.

### If the pet does not appear

The **Pet** tab says why underneath the tick box. The usual reasons are that you are not on
Windows, or that part of the tool folder was moved and the pet that ships with it could not be
found.

---

## License

[MIT](LICENSE.md).