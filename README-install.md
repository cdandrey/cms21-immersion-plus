# Build and install — CMS21 Immersion+

## Build
1. Restore local game/MelonLoader DLL references with `scripts\restore-libs.ps1 -GamePath <game>`.
2. Run the VS Code task `CMS21 Immersion+: Build Release` or `scripts\build.ps1 -Target Build -Configuration Release`.
3. `scripts\build-install.ps1` prepares the release payload and can install it into a detected/explicit CMS 2021 directory.

## Runtime layout
```text
Mods\
├─ CMS21ImmersionPlus.dll
└─ CMS21ImmersionPlus\
   ├─ CMS21ImmersionPlus.cfg
   └─ ...
```

The package also includes `AuthenticCarNames.cfg`, the UI-settings manifest, brand logos and texture replacements. User-generated profile/config state is not source-controlled.
