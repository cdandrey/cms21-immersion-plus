using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if NET6_0_OR_GREATER
using Il2Cpp;
using Il2CppCMS.Containers;
#else
using CMS;
using CMS.Containers;
#endif

namespace Cms21ImmersionPlus
{
    /// <summary>Loads brand logos supplied by car mods, with optional TK Aftermarket integration.</summary>
    public static class BrandLogoFeature
    {
        private sealed class BrandLogoCandidate
        {
            public string Name;
            public byte[] ImageData;
            public string Source;
        }

        public static bool Apply()
        {
            if (Main.SettingsEntry == null ||
                !Main.SettingsEntry.Value.loadBrandLogosFromMods)
                return true;
            if (GlobalState.GameManager == null ||
                GlobalState.GameManager.CarBundleLoader == null)
                return false;

            GameInventory inventory = Singleton<GameInventory>.Instance;
            if (inventory == null || inventory.partPropertyList == null)
                return false;

            try {
                HashSet<string> modCarIds =
                    new HashSet<string>(StringComparer.Ordinal);
                HashSet<string> candidateNames =
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                List<BrandLogoCandidate> candidates =
                    new List<BrandLogoCandidate>();

                foreach (CarConfigData car in
                    GlobalState.GameManager.CarBundleLoader.CarNamesData) {
                    if (car.FileType != CarFileType.MOD)
                        continue;

                    NormalizeCarBrands(car);
                    modCarIds.Add(car.CarID);
                    AddWorkshopLogo(car, candidates, candidateNames);
                }

                if (Main.SettingsEntry.Value.loadBrandLogosFromTKAftermarket)
                    AddTkAftermarketLogos(candidates, candidateNames);

                NormalizeModPartBrands(inventory, modCarIds);
                int added = InstallMissingLogos(inventory, candidates);
                if (added > 0) {
                    ModLogger.Log("[BrandLogos] Installed " + added +
                        " new logo(s) from supported mods.", Types.LoggingLevels.Normal);
                }
                return true;
            } catch (Exception exception) {
                ModLogger.Log("[BrandLogos] Failed to load mod brand logos." +
                    Environment.NewLine + exception, Types.LoggingLevels.Error);
                return false;
            }
        }

        private static void NormalizeCarBrands(CarConfigData car)
        {
            foreach (var config in car.CarConfigs) {
                if (!config.elements.ContainsKey("0_main.carBrand"))
                    continue;

                string configured = config.elements["0_main.carBrand"];
                config.elements["0_main.carBrand"] =
                    string.IsNullOrWhiteSpace(configured)
                        ? " "
                        : configured.ToLowerInvariant();
            }
        }

        private static void AddWorkshopLogo(CarConfigData car,
            List<BrandLogoCandidate> candidates, HashSet<string> candidateNames)
        {
            if (string.IsNullOrWhiteSpace(car.PathToFile))
                return;

            string carDirectory = Path.GetDirectoryName(car.PathToFile);
            if (string.IsNullOrWhiteSpace(carDirectory))
                return;

            string brandDirectory = Path.Combine(carDirectory, "Brand");
            if (!Directory.Exists(brandDirectory))
                return;

            string[] files = Directory.GetFiles(brandDirectory, "*.png")
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (files.Length > 1) {
                ModLogger.Log("[BrandLogos] More than one PNG exists in " +
                    brandDirectory + "; only the first file is used.",
                    Types.LoggingLevels.Warning);
            }
            if (files.Length == 0)
                return;

            AddCandidate(files[0], car.CarID, candidates, candidateNames);
        }

        private static void AddTkAftermarketLogos(
            List<BrandLogoCandidate> candidates, HashSet<string> candidateNames)
        {
            string directory = Path.GetFullPath(
                GlobalConfig.directoryTKAftermarketBrands);
            if (!Directory.Exists(directory)) {
                ModLogger.Debug("[BrandLogos] TK Aftermarket integration is enabled, " +
                    "but its brands directory is absent. Integration was skipped.");
                return;
            }

            foreach (string file in Directory.GetFiles(directory, "*.png")
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)) {
                AddCandidate(file, "TKAftermarket", candidates, candidateNames);
            }
        }

        private static void AddCandidate(string file, string source,
            List<BrandLogoCandidate> candidates, HashSet<string> candidateNames)
        {
            string name = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(name) || !candidateNames.Add(name))
                return;

            try {
                candidates.Add(new BrandLogoCandidate {
                    Name = name,
                    ImageData = File.ReadAllBytes(file),
                    Source = source
                });
            } catch (Exception exception) {
                candidateNames.Remove(name);
                ModLogger.Log("[BrandLogos] Failed to read '" + file + "'." +
                    Environment.NewLine + exception, Types.LoggingLevels.Warning);
            }
        }

        private static void NormalizeModPartBrands(GameInventory inventory,
            HashSet<string> modCarIds)
        {
            for (int i = 0; i < inventory.partPropertyList.entries.Count; i++) {
                var entry = inventory.partPropertyList.entries[i];
                if (entry == null || entry.value == null)
                    continue;

                PartProperty part = entry.value;
                if (part.ShopName == "AddonsShop" && part.Brand == "")
                    part.Brand = " ";

                if (part.Brand != "")
                    continue;

                foreach (string carId in modCarIds) {
                    if (!part.ID.StartsWith(carId, StringComparison.Ordinal))
                        continue;

                    part.Brand = " ";
                    break;
                }
            }
        }

        private static int InstallMissingLogos(GameInventory inventory,
            List<BrandLogoCandidate> candidates)
        {
            if (candidates.Count == 0)
                return 0;

            HashSet<string> installed =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (SpriteContainer container in inventory.Brands) {
                if (container != null && !string.IsNullOrEmpty(container.name))
                    installed.Add(container.name);
            }

            int added = 0;
            foreach (BrandLogoCandidate candidate in candidates) {
                if (installed.Contains(candidate.Name))
                    continue;

                Sprite sprite = TextureLoader.LoadSpriteFromBytes(candidate.ImageData);
                if (sprite == null)
                    continue;

                sprite.texture.name = candidate.Name;
                inventory.Brands.Add(new SpriteContainer {
                    name = candidate.Name,
                    sprite = sprite
                });
                installed.Add(candidate.Name);
                added++;
                ModLogger.Debug("[BrandLogos] Added '" +
                    candidate.Name.ToUpperInvariant() + "' from " +
                    candidate.Source + ".");
            }

            if (added > 0)
                inventory.ReloadBrands(inventory.Brands);
            return added;
        }
    }
}
