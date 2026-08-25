# Fynn's Tools Free

Free tools for the Unity Editor.

| Tool | What it does |
|---|---|
| [FaceEmo Patches](#faceemo-patches) | Optional fixes for FaceEmo: transparent menu icons, and a blend shape picker that fits its contents |
| [GoGoLoco Pose Changer](#gogoloco-pose-changer) | Swap GoGo Loco's stand, crouch, prone, fall and AFK animations on your VRChat avatar |

---

## FaceEmo Patches

Optional fixes for [FaceEmo](https://suzuryg.github.io/face-emo/). Each one is a tick box, and
nothing is on until you tick it.

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

1. **Tools > Fynn's Tools > FaceEmo Patches > Settings**
2. Tick the fixes you want

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

## License

[MIT](LICENSE.md).