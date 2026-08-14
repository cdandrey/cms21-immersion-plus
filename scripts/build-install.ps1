[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$Destination
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRevision = "r018"
$configuration = "Release"
$repoRoot = Split-Path -Parent $PSScriptRoot
$projectFile = Join-Path $repoRoot "CMS21ImmersionPlus.csproj"
$configsRoot = Join-Path $repoRoot "configs"
$resourcesRoot = Join-Path $repoRoot "resources"
$releaseBinRoot = Join-Path $repoRoot "bin\Release"
$releaseObjRoot = Join-Path $repoRoot "obj\Release"
$compiledDll = Join-Path $releaseBinRoot "CMS21ImmersionPlus.dll"
$installRoot = Join-Path $repoRoot "install"
$installDataRoot = Join-Path $installRoot "CMS21ImmersionPlus"
$automaticDestinationSearchDepth = 2
$steamGameRelativePath = "steamapps\common\Car Mechanic Simulator 2021"

# Only runtime files listed in this release manifest are packaged and installed.
# Repository-only, source and work-in-progress resources are intentionally excluded.
# Paths are relative to both resources\ and install\CMS21ImmersionPlus\.
$releaseConfigFiles = @(
    "AuthenticCarNames.cfg",
    "CMS21ImmersionPlus.cfg"
)

$releaseUiSettingsFiles = @(
    "CMS21ImmersionPlus.ui-settings.json"
)

$releaseResourceFiles = @(
    "CarBrand/CB_AC.png",
    "CarBrand/CB_AlfaRomeo.png",
    "CarBrand/CB_Audi.png",
    "CarBrand/CB_Bugatti.png",
    "CarBrand/CB_Buick.png",
    "CarBrand/CB_Cadillac.png",
    "CarBrand/CB_Checker.png",
    "CarBrand/CB_Chevrolet.png",
    "CarBrand/CB_Chrysler.png",
    "CarBrand/CB_Dacia.png",
    "CarBrand/CB_Dodge.png",
    "CarBrand/CB_Dongfeng.png",
    "CarBrand/CB_Duesenberg.png",
    "CarBrand/CB_FactoryFive.png",
    "CarBrand/CB_Ferrari.png",
    "CarBrand/CB_Fiat.png",
    "CarBrand/CB_FiatLegacy.png",
    "CarBrand/CB_FordLegacy.png",
    "CarBrand/CB_GMC.png",
    "CarBrand/CB_Holden.png",
    "CarBrand/CB_Honda.png",
    "CarBrand/CB_Hongqi.png",
    "CarBrand/CB_HotWheels.png",
    "CarBrand/CB_Hudson.png",
    "CarBrand/CB_Hyundai.png",
    "CarBrand/CB_Italdesign.png",
    "CarBrand/CB_Jeep.png",
    "CarBrand/CB_Koenigsegg.png",
    "CarBrand/CB_LTI.png",
    "CarBrand/CB_Lamborghini.png",
    "CarBrand/CB_Lincoln.png",
    "CarBrand/CB_McLaren.png",
    "CarBrand/CB_Mercury.png",
    "CarBrand/CB_Minecraft.png",
    "CarBrand/CB_Mini.png",
    "CarBrand/CB_Mitsubishi.png",
    "CarBrand/CB_Oldsmobile.png",
    "CarBrand/CB_Opel.png",
    "CarBrand/CB_Peugeot.png",
    "CarBrand/CB_Plymouth.png",
    "CarBrand/CB_Pontiac.png",
    "CarBrand/CB_Porsche.png",
    "CarBrand/CB_Rayfield.png",
    "CarBrand/CB_Renault.png",
    "CarBrand/CB_Scion.png",
    "CarBrand/CB_Shelby.png",
    "CarBrand/CB_Skoda.png",
    "CarBrand/CB_Subaru.png",
    "CarBrand/CB_Suzuki.png",
    "CarBrand/CB_Tesla.png",
    "CarBrand/CB_Toyota.png",
    "CarBrand/CB_VUHL.png",
    "CarBrand/CB_Volkswagen.png",
    "CarBrand/CB_Volvo.png",
    "CarBrand/CB_WayneTech.png",
    "CarBrand/CB_Yamaha.png",
    "PartBrand/PB_BBS.png",
    "PartBrand/PB_BFGoodrich.png",
    "PartBrand/PB_Bilstein.png",
    "PartBrand/PB_Borla.png",
    "PartBrand/PB_Bosch.png",
    "PartBrand/PB_Brembo.png",
    "PartBrand/PB_CompCams.png",
    "PartBrand/PB_Continental.png",
    "PartBrand/PB_Dorman.png",
    "PartBrand/PB_Edelbrock.png",
    "PartBrand/PB_Eibach.png",
    "PartBrand/PB_Enkei.png",
    "PartBrand/PB_Garrett.png",
    "PartBrand/PB_Gates.png",
    "PartBrand/PB_GKN.png",
    "PartBrand/PB_KN.png",
    "PartBrand/PB_Lemforder.png",
    "PartBrand/PB_MagnaFlow.png",
    "PartBrand/PB_MAHLE.png",
    "PartBrand/PB_MANNFilter.png",
    "PartBrand/PB_MOMO.png",
    "PartBrand/PB_NGK.png",
    "PartBrand/PB_OZRacing.png",
    "PartBrand/PB_Pirelli.png",
    "PartBrand/PB_RAYS.png",
    "PartBrand/PB_RECARO.png",
    "PartBrand/PB_SACHS.png",
    "PartBrand/PB_SKF.png",
    "PartBrand/PB_TREMEC.png",
    "PartBrand/PB_VARTA.png",
    "PartBrand/PB_Wiseco.png",
    "PartBrand/PB_ZF.png",
    "ShopBrand/SB_BBS.png",
    "ShopBrand/SB_Bosch.png",
    "ShopBrand/SB_LKQ.png",
    "ShopBrand/SB_LicensePlatesTV.png",
    "ShopBrand/SB_MaxtonDesign.png",
    "ShopBrand/SB_NAPA.png",
    "ShopBrand/SB_NexusMods.png",
    "ShopBrand/SB_MOMO.png",
    "ShopBrand/SB_SteamWorkshop.png",
    "ShopBrand/SB_SummitRacing.png",
    "ShopBrand/SB_BFGoodrich.png",
    "ShopBrand/SB_TREMEC.png",
    "TextureReplacements/CarLoader/CallopeInterior.png",
    "TextureReplacements/CarLoader/LavetinoInteriorDiffuse.png"
)

function Get-ProjectLibraryReferences {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectFile
    )

    [xml]$projectXml = Get-Content -LiteralPath $ProjectFile -Raw
    $namespaceUri = $projectXml.DocumentElement.NamespaceURI

    if ([string]::IsNullOrEmpty($namespaceUri)) {
        $hintPathNodes = $projectXml.SelectNodes("//Reference/HintPath")
    } else {
        $namespaceManager = New-Object System.Xml.XmlNamespaceManager($projectXml.NameTable)
        $namespaceManager.AddNamespace("msbuild", $namespaceUri)
        $hintPathNodes = $projectXml.SelectNodes("//msbuild:Reference/msbuild:HintPath", $namespaceManager)
    }

    $projectDirectory = Split-Path -Parent $ProjectFile
    $references = New-Object 'System.Collections.Generic.List[object]'

    foreach ($hintPathNode in $hintPathNodes) {
        $relativePath = ([string]$hintPathNode.InnerText).Trim()
        if ([string]::IsNullOrEmpty($relativePath)) {
            continue
        }

        $normalizedPath = $relativePath.Replace('/', '\')
        if (-not $normalizedPath.StartsWith("libs\", [System.StringComparison]::OrdinalIgnoreCase)) {
            continue
        }

        $fullPath = [System.IO.Path]::GetFullPath((Join-Path $projectDirectory $relativePath))
        $references.Add([PSCustomObject]@{
            Name = [System.IO.Path]::GetFileName($fullPath)
            RelativePath = $normalizedPath
            FullPath = $fullPath
        })
    }

    return @($references | Sort-Object RelativePath -Unique)
}

function Assert-ProjectLibraries {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectFile
    )

    $projectDirectory = Split-Path -Parent $ProjectFile
    $libraryDirectory = Join-Path $projectDirectory "libs"
    $requiredLibraries = @(Get-ProjectLibraryReferences -ProjectFile $ProjectFile)

    if ($requiredLibraries.Count -eq 0) {
        throw "No library references under 'libs' were found in '$ProjectFile'."
    }

    $missingLibraries = @(
        $requiredLibraries |
            Where-Object { -not (Test-Path -LiteralPath $_.FullPath -PathType Leaf) }
    )

    if ((Test-Path -LiteralPath $libraryDirectory -PathType Container) -and
        $missingLibraries.Count -eq 0) {
        return
    }

    Write-Host ""
    Write-Host "Build was not started because required project libraries are unavailable." -ForegroundColor Red

    if (-not (Test-Path -LiteralPath $libraryDirectory -PathType Container)) {
        Write-Host "Library directory is missing: $libraryDirectory" -ForegroundColor Red
    } else {
        Write-Host "Missing libraries:" -ForegroundColor Red
        foreach ($library in $missingLibraries) {
            Write-Host "  - $($library.Name)"
        }
    }

    Write-Host ""
    Write-Host "All required libraries:"
    foreach ($library in $requiredLibraries) {
        Write-Host "  - $($library.Name)"
    }

    $restoreScript = Join-Path $repoRoot "scripts\restore-libs.ps1"
    $readmeFile = Join-Path $repoRoot "README.md"

    Write-Host ""
    Write-Host "The library restore helper must be present in the scripts directory:"
    Write-Host "  $restoreScript"
    Write-Host "Detailed project setup and library restoration instructions are in:"
    Write-Host "  $readmeFile"
    Write-Host ""
    throw "Restore the required DLL files under '$libraryDirectory' and run the script again."
}

function Find-MSBuild {
    $vswhere = Join-Path ([Environment]::GetFolderPath("ProgramFilesX86")) "Microsoft Visual Studio\Installer\vswhere.exe"

    if (Test-Path -LiteralPath $vswhere -PathType Leaf) {
        $candidates = @(
            & $vswhere `
                -latest `
                -products * `
                -requires Microsoft.Component.MSBuild `
                -find "MSBuild\**\Bin\MSBuild.exe"
        )

        if ($candidates.Length -gt 0) {
            return $candidates[0]
        }
    }

    $command = Get-Command "MSBuild.exe" -ErrorAction SilentlyContinue
    if ($null -ne $command) {
        return $command.Source
    }

    throw "MSBuild.exe was not found. Install Visual Studio Build Tools or add MSBuild.exe to PATH."
}

function ConvertTo-NativeRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return $Path.Replace('/', [System.IO.Path]::DirectorySeparatorChar)
}

function Get-SearchableDriveRoots {
    $driveRoots = New-Object 'System.Collections.Generic.List[string]'

    foreach ($drive in [System.IO.DriveInfo]::GetDrives()) {
        try {
            if (-not $drive.IsReady) {
                continue
            }

            if (($drive.DriveType -ne [System.IO.DriveType]::Fixed) -and
                ($drive.DriveType -ne [System.IO.DriveType]::Removable)) {
                continue
            }

            $driveRoots.Add($drive.RootDirectory.FullName)
        } catch {
            continue
        }
    }

    return @($driveRoots | Sort-Object -Unique)
}

function Find-SteamGameDirectoryFromBase {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BasePath,

        [Parameter(Mandatory = $true)]
        [int]$CurrentDepth,

        [Parameter(Mandatory = $true)]
        [int]$MaximumDepth
    )

    $candidate = Join-Path $BasePath $steamGameRelativePath
    if (Test-Path -LiteralPath $candidate -PathType Container -ErrorAction SilentlyContinue) {
        try {
            return (Resolve-Path -LiteralPath $candidate -ErrorAction Stop).Path
        } catch {
            # Continue the bounded search when the matching path cannot be resolved.
        }
    }

    if ($CurrentDepth -ge $MaximumDepth) {
        return $null
    }

    $childDirectories = @(
        Get-ChildItem -LiteralPath $BasePath -Directory -Force -ErrorAction SilentlyContinue |
            Where-Object {
                ($_.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -eq 0
            } |
            Sort-Object Name
    )

    foreach ($childDirectory in $childDirectories) {
        $found = Find-SteamGameDirectoryFromBase `
            -BasePath $childDirectory.FullName `
            -CurrentDepth ($CurrentDepth + 1) `
            -MaximumDepth $MaximumDepth

        if (-not [string]::IsNullOrWhiteSpace($found)) {
            return $found
        }
    }

    return $null
}

function Find-SteamGameDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateRange(0, 8)]
        [int]$MaximumBaseDepth
    )

    foreach ($driveRoot in @(Get-SearchableDriveRoots)) {
        Write-Host "  Searching $driveRoot"

        $found = Find-SteamGameDirectoryFromBase `
            -BasePath $driveRoot `
            -CurrentDepth 0 `
            -MaximumDepth $MaximumBaseDepth

        if (-not [string]::IsNullOrWhiteSpace($found)) {
            return $found
        }
    }

    return $null
}

function Assert-ReleaseSources {
    $missing = New-Object 'System.Collections.Generic.List[string]'

    if (-not (Test-Path -LiteralPath $projectFile -PathType Leaf)) {
        $missing.Add($projectFile)
    }

    foreach ($fileName in $releaseConfigFiles) {
        $sourcePath = Join-Path $configsRoot $fileName
        if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
            $missing.Add($sourcePath)
        }
    }

    foreach ($fileName in $releaseUiSettingsFiles) {
        $sourcePath = Join-Path $configsRoot $fileName
        if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
            $missing.Add($sourcePath)
        }
    }

    foreach ($relativePath in $releaseResourceFiles) {
        $nativeRelativePath = ConvertTo-NativeRelativePath -Path $relativePath
        $sourcePath = Join-Path $resourcesRoot $nativeRelativePath
        if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
            $missing.Add($sourcePath)
        }
    }

    if ($missing.Count -gt 0) {
        throw "Release sources are incomplete:`n$($missing -join [Environment]::NewLine)"
    }
}

function Copy-ReleaseFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Source,

        [Parameter(Mandatory = $true)]
        [string]$Destination
    )

    $destinationDirectory = Split-Path -Parent $Destination
    if (-not (Test-Path -LiteralPath $destinationDirectory -PathType Container)) {
        New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
    }

    Copy-Item -LiteralPath $Source -Destination $Destination -Force
}

function Assert-InstallPayload {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PayloadRoot
    )

    $payloadDataRoot = Join-Path $PayloadRoot "CMS21ImmersionPlus"
    $missing = New-Object 'System.Collections.Generic.List[string]'

    $packagedDll = Join-Path $PayloadRoot "CMS21ImmersionPlus.dll"
    if (-not (Test-Path -LiteralPath $packagedDll -PathType Leaf)) {
        $missing.Add($packagedDll)
    }

    foreach ($fileName in $releaseConfigFiles) {
        $destinationPath = Join-Path $payloadDataRoot $fileName
        if (-not (Test-Path -LiteralPath $destinationPath -PathType Leaf)) {
            $missing.Add($destinationPath)
        }
    }

    foreach ($fileName in $releaseUiSettingsFiles) {
        $destinationPath = Join-Path $payloadDataRoot $fileName
        if (-not (Test-Path -LiteralPath $destinationPath -PathType Leaf)) {
            $missing.Add($destinationPath)
        }
    }

    foreach ($relativePath in $releaseResourceFiles) {
        $nativeRelativePath = ConvertTo-NativeRelativePath -Path $relativePath
        $destinationPath = Join-Path $payloadDataRoot $nativeRelativePath
        if (-not (Test-Path -LiteralPath $destinationPath -PathType Leaf)) {
            $missing.Add($destinationPath)
        }
    }

    if ($missing.Count -gt 0) {
        throw "Install payload is incomplete:`n$($missing -join [Environment]::NewLine)"
    }
}

function Resolve-ModsDirectory {
    param(
        [AllowNull()]
        [string]$Path
    )

    $candidate = $Path
    while ($true) {
        if ([string]::IsNullOrWhiteSpace($candidate)) {
            $candidate = Read-Host "Path to Car Mechanic Simulator 2021 or its Mods directory"
        }

        $candidate = $candidate.Trim().Trim('"').Trim("'")
        if (-not (Test-Path -LiteralPath $candidate -PathType Container)) {
            Write-Warning "Directory was not found: $candidate"
            $candidate = $null
            continue
        }

        $resolved = (Resolve-Path -LiteralPath $candidate).Path
        $leaf = Split-Path $resolved -Leaf

        if ($leaf -ieq "CMS21ImmersionPlus") {
            $parent = Split-Path $resolved -Parent
            if ((Split-Path $parent -Leaf) -ieq "Mods") {
                return $parent
            }
        }

        if ($leaf -ieq "Mods") {
            return $resolved
        }

        return (Join-Path $resolved "Mods")
    }
}

Assert-ReleaseSources
Assert-ProjectLibraries -ProjectFile $projectFile
$msbuild = Find-MSBuild

Write-Host "build-install $scriptRevision"
Write-Host "Repository:    $repoRoot"
Write-Host "Project:       $projectFile"
Write-Host "MSBuild:       $msbuild"
Write-Host "Configuration: $configuration"
Write-Host ""
Write-Host "Removing previous Release build and install payload..."

Remove-Item -LiteralPath $releaseBinRoot -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $releaseObjRoot -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $installRoot -Recurse -Force -ErrorAction SilentlyContinue

$buildArguments = @(
    $projectFile,
    "/t:Build",
    "/p:Configuration=$configuration",
    "/m"
)

Write-Host "Building Release..."
& $msbuild @buildArguments
if ($LASTEXITCODE -ne 0) {
    throw "Release build failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path -LiteralPath $compiledDll -PathType Leaf)) {
    throw "Release build completed, but CMS21ImmersionPlus.dll was not found: $compiledDll"
}

Write-Host ""
Write-Host "Creating install payload from the release manifest..."

New-Item -ItemType Directory -Path $installDataRoot -Force | Out-Null
Copy-ReleaseFile -Source $compiledDll -Destination (Join-Path $installRoot "CMS21ImmersionPlus.dll")

foreach ($fileName in $releaseConfigFiles) {
    Copy-ReleaseFile `
        -Source (Join-Path $configsRoot $fileName) `
        -Destination (Join-Path $installDataRoot $fileName)
}

foreach ($fileName in $releaseUiSettingsFiles) {
    Copy-ReleaseFile `
        -Source (Join-Path $configsRoot $fileName) `
        -Destination (Join-Path $installDataRoot $fileName)
}

foreach ($relativePath in $releaseResourceFiles) {
    $nativeRelativePath = ConvertTo-NativeRelativePath -Path $relativePath
    Copy-ReleaseFile `
        -Source (Join-Path $resourcesRoot $nativeRelativePath) `
        -Destination (Join-Path $installDataRoot $nativeRelativePath)
}

Assert-InstallPayload -PayloadRoot $installRoot

Write-Host ""
Write-Host "Install payload created: $installRoot"
Write-Host "Packaged configs:               $($releaseConfigFiles.Length)"
Write-Host "Packaged UI settings manifests: $($releaseUiSettingsFiles.Length)"
Write-Host "Packaged resources:             $($releaseResourceFiles.Length)"
Write-Host ""

$resolvedDestination = $Destination
if ([string]::IsNullOrWhiteSpace($resolvedDestination)) {
    Write-Host "Destination was not supplied. Searching local fixed and removable drives..."

    $searchTimer = [System.Diagnostics.Stopwatch]::StartNew()
    $resolvedDestination = Find-SteamGameDirectory `
        -MaximumBaseDepth $automaticDestinationSearchDepth
    $searchTimer.Stop()
    $elapsedSeconds = [Math]::Round($searchTimer.Elapsed.TotalSeconds, 1)

    if ([string]::IsNullOrWhiteSpace($resolvedDestination)) {
        Write-Warning "Car Mechanic Simulator 2021 was not found automatically within $automaticDestinationSearchDepth directory levels. Search completed in $elapsedSeconds second(s)."
    } else {
        Write-Host "Detected game directory: $resolvedDestination" -ForegroundColor Green
        Write-Host "Automatic search completed in $elapsedSeconds second(s)."
    }

    Write-Host ""
}

$modsDirectory = Resolve-ModsDirectory -Path $resolvedDestination
New-Item -ItemType Directory -Path $modsDirectory -Force | Out-Null

Write-Host "Installing the prepared payload to: $modsDirectory"
foreach ($item in Get-ChildItem -LiteralPath $installRoot -Force) {
    Copy-Item -LiteralPath $item.FullName -Destination $modsDirectory -Recurse -Force
}

Assert-InstallPayload -PayloadRoot $modsDirectory

Write-Host ""
Write-Host "CMS21 Immersion+ build and installation completed."
Write-Host "DLL:       $(Join-Path $modsDirectory 'CMS21ImmersionPlus.dll')"
Write-Host "Resources:   $(Join-Path $modsDirectory 'CMS21ImmersionPlus')"
Write-Host "UI settings: $(Join-Path $modsDirectory 'CMS21ImmersionPlus\CMS21ImmersionPlus.ui-settings.json')"
