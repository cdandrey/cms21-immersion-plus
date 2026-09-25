# CMS21 Immersion+

**CMS21 Immersion+** is a Car Mechanic Simulator 2021 mod focused on vehicle authenticity,
visual presentation and immersion improvements.

- Display name: **CMS21 Immersion+**
- Short name: **CMS21 Immersion+**
- Technical name and assembly: `CMS21ImmersionPlus`
- DLL: `CMS21ImmersionPlus.dll`
- Runtime directory: `Mods\CMS21ImmersionPlus\`
- Main configuration: `Mods\CMS21ImmersionPlus\CMS21ImmersionPlus.cfg`
- Repository name: `cms21-immersion-plus`
- Questions: `cdandrey@gmail.com` — include `CMS21Immersion+` in the subject line

## Relationship to QoLmod

CMS21 Immersion+ is a substantially refactored and reduced derivative of **QoLmod** by
**Meitzi**, originally published at <https://www.nexusmods.com/carmechanicsimulator2021/mods/105>
and licensed under GNU GPL v3. Selected QoLmod feature ideas are retained; the retained and
removed feature lists are summarized in [QoLmod origin](#qolmod-origin).

## Features and settings

All switches below are stored under `[CMS21ImmersionPlus.Settings]` in
`CMS21ImmersionPlus.cfg`. Displayed names are taken from the in-game settings manifest; every
listed switch currently uses `restartGame` apply mode.

### Authenticity

| In-game setting | Config flag | Default | Detailed behavior |
|---|---|---:|---|
| **Authentic vehicles** | `useAuthenticCarNames` | `true` | Applies configured real-world vehicle and brand names from `AuthenticCarNames.cfg` and enables the matching local vehicle visual replacements: brand logos, vehicle/interior textures and thumbnails. Unmatched or missing assets are left unchanged. |
| **Authentic auto-part brands** | `rebrandParts` | `true` | Replaces fictional aftermarket brands on part cards with real-world component manufacturers while preserving vehicle-specific OEM brands. |
| **Authentic auto-shop brands** | `rebrandShops` | `true` | Replaces shop-card avatars with the configured real-world automotive brands. |
| **Authentic banners** | `loadGarageAdvertising` | `true` | Loads garage banners, calendars and related advertising replacements from `TextureReplacements\Environment\GarageAds`. Missing files are ignored. |
| **Vehicle brand logos from Workshop mods** | `loadBrandLogosFromMods` | `true` | Discovers supported brand-logo PNG files supplied by installed Workshop vehicle mods and adds missing matching brand images to the game's logo data. Existing logos are left unchanged. |
| **TK Aftermarket logos** | `loadBrandLogosFromTKAftermarket` | `false` | When Workshop logo loading is enabled, additionally reads `Mods\TKAftermarket\brands`. A missing directory is a no-op. |

### Effects and improvements

| In-game setting | Config flag | Default | Detailed behavior |
|---|---|---:|---|
| **Dyno window without blur** | `removeDynoMenuBlur` | `true` | Disables the dyno window's menu blur without changing confirmation behavior. |
| **Player name on showroom plates** | `showPlayerNameOnShowroomLicencePlates` | `true` | Writes the current profile/player name to licence plates in Showroom and Auto Salon only. |
| **Show all parking vehicles** | `preloadAllParkingSceneVehicles` | `true` | Keeps all ten vehicles in the selected parking alley loaded and visible instead of only the currently focused subset. |

## In-game mod settings

The mod provides `configs/CMS21ImmersionPlus.ui-settings.json` for the shared in-game mod
settings interface supplied by CMS21 UI+. Integration is declarative: CMS21 Immersion+ does not
reference `CMS21UIPlus.dll`, implement an interface or expose a provider class. CMS21 UI+ reads
the manifest and the declared TOML configuration file without calling into the Immersion+ DLL.

The manifest contains its own English and Russian setting names and descriptions. If CMS21 UI+
is not installed, CMS21 Immersion+ continues to use `CMS21ImmersionPlus.cfg` normally.

`applyMode` is descriptive: CMS21 UI+ writes the configuration but does not notify, reload or
invoke CMS21 Immersion+.

## Configuration and runtime files

Current templates and UI manifest:

- `configs/CMS21ImmersionPlus.cfg` — primary feature switches;
- `configs/CMS21ImmersionPlus.ui-settings.json` — in-game settings groups, labels and metadata;
- `configs/AuthenticCarNames.cfg` — vehicle, brand and version-name mappings;
- `resources/CarBrand` — local vehicle brand-logo replacements;
- `resources/PartBrand` — real-world aftermarket brand logos used on part cards;
- `resources/ShopBrand` — local shop-branding replacements;
- `resources/TextureReplacements` — vehicle, interior and garage advertising texture replacements.

At runtime they are installed under:

```text
<Game>\Mods\CMS21ImmersionPlus\
```

`CMS21ImmersionPlus.cfg.bak` can exist temporarily when the configuration is saved through the
CMS21 UI+ Mods menu and contains the previous configuration. It is deleted when the Mods menu
closes, but can remain after an abnormal termination. Do not commit or package the generated
file.

## QoLmod origin

CMS21 Immersion+ retains the following feature concepts from QoLmod by **Meitzi**:

- removal of the dyno menu blur, split from the former streamlined dyno feature:
  `removeDynoMenuBlur`;
- authentic vehicle names, Workshop logos and local vehicle visual replacement:
  `useAuthenticCarNames`, `loadBrandLogosFromMods`;
- parking-scene vehicle preloading and showroom licence plates:
  `preloadAllParkingSceneVehicles`, `showPlayerNameOnShowroomLicencePlates`.

Garage advertising replacements, shop rebranding and part branding are CMS21 Immersion+ additions.

TK Aftermarket logo loading is a CMS21 Immersion+ extension rather than a retained QoLmod
feature.

## Repository layout

```text
cms21-immersion-plus/
├─ configs/
│  ├─ AuthenticCarNames.cfg
│  ├─ CMS21ImmersionPlus.cfg
│  └─ CMS21ImmersionPlus.ui-settings.json
├─ resources/
│  ├─ CarBrand/
│  ├─ PartBrand/
│  ├─ ShopBrand/
│  └─ TextureReplacements/
├─ libs/                    # local reference DLLs, not tracked by Git
├─ scripts/
│  ├─ build.ps1
│  ├─ build-install.ps1
│  └─ restore-libs.ps1
├─ src/
│  ├─ Features/
│  ├─ Infrastructure/
│  ├─ Config.cs
│  └─ Main.cs
├─ CMS21ImmersionPlus.csproj
├─ LICENSE.md
├─ README-install.md
└─ README.md
```

## Build

Requirements:

- Windows;
- .NET Framework 4.7.2 Developer Pack;
- Visual Studio Build Tools/MSBuild;
- game, Unity, MelonLoader, Tomlet and Harmony assemblies in `libs`.

### Restoring reference libraries

The DLL files under `libs` are local development dependencies and are not tracked by Git.
Restore them from the installed game and MelonLoader directories:

```powershell
.\scripts\restore-libs.ps1 `
    -GamePath "D:\SteamLibrary\steamapps\common\Car Mechanic Simulator 2021"
```

The script reads the required `libs\*.dll` entries from `CMS21ImmersionPlus.csproj`, creates the
`libs` directory when necessary and preserves existing DLLs unless `-Force` is used. It also
validates the game layout and reports the selected source path and assembly version for each
restored DLL.

### Compiling and installing

From the repository root:

```powershell
.\scripts\build.ps1 -Target Rebuild -Configuration Release
```

Build, create the explicit install payload and install it:

```powershell
.\scripts\build-install.ps1
```

A destination can be supplied directly:

```powershell
.\scripts\build-install.ps1 `
    -Destination "D:\SteamLibrary\steamapps\common\Car Mechanic Simulator 2021"
```

See `README-install.md` for accepted destination paths and installation behavior.

## Licence

GNU General Public License v3.0. See `LICENSE.md`.
