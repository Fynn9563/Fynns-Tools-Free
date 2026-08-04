# Fynn's Tools Free

Free tools for the Unity Editor.

| Tool | What it does |
|---|---|
| [GoGoLoco Pose Changer](#gogoloco-pose-changer) | Swap GoGo Loco's stand, crouch, prone, fall and AFK animations on your VRChat avatar |

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