# ClientSim Storage Cleaner

Reset ClientSim between Play sessions in VRChat world projects.

- **Delay script reloads until ClientSim stops** is on by default. Script changes take effect after Play Mode ends so ClientSim can shut down before assemblies reload.
- **Reset runtime caches before play (Reload Domain off)** is on by default. Clears stale runtime references between Play sessions while keeping saved progress.
- **Delete ClientSimStorage after play mode** separately resets saved player data and saved player objects. Turn this off when testing quest progress across sessions.

Open **Tools > Fynn's Tools > ClientSim Storage Cleaner > Settings** to change these options. **Clear Now** deletes saved data; **Reset Runtime Caches Now** only clears the known memory caches. Both manual actions are available outside Play Mode.

Created by [Fynn9563](https://github.com/Fynn9563).
