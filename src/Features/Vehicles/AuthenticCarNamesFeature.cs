using System;
using System.Collections.Generic;
using System.IO;
using Tomlet;

#if NET6_0_OR_GREATER
using Il2Cpp;
using Il2CppCMS.UI.Logic;
#else
using CMS;
using CMS.UI.Logic;
#endif

namespace Cms21ImmersionPlus
{
    /// <summary>Applies configured real-world car, brand, and version names once game data is ready.</summary>
    public static class AuthenticCarNamesFeature
    {
        public static bool Apply()
        {
            if (Main.SettingsEntry == null || !Main.SettingsEntry.Value.useAuthenticCarNames)
                return true;
            if (GlobalState.GameManager == null ||
                GlobalState.GameManager.CarBundleLoader == null)
                return false;
            if (!File.Exists(GlobalConfig.cfgAuthCar)) {
                ModLogger.Log("[AuthenticCarNames] Config file not found: " +
                    GlobalConfig.cfgAuthCar, Types.LoggingLevels.Warning);
                return true;
            }

            try {
                Types.AuthenticCarNamesConfig configuredNames = TomletMain.To<Types.AuthenticCarNamesConfig>(
                    TomlParser.ParseFile(GlobalConfig.cfgAuthCar));
                Types.CarNameConfig[] entries = configuredNames != null
                    ? configuredNames.Car
                    : null;
                if (entries == null || entries.Length == 0) {
                    ModLogger.Log("[AuthenticCarNames] No car entries were configured.",
                        Types.LoggingLevels.Warning);
                    return true;
                }

                Dictionary<string, Types.CarNameConfig> byCarId =
                    new Dictionary<string, Types.CarNameConfig>(StringComparer.Ordinal);
                foreach (Types.CarNameConfig entry in entries) {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.CarID))
                        continue;
                    if (byCarId.ContainsKey(entry.CarID)) {
                        ModLogger.Log("[AuthenticCarNames] Duplicate CarID '" +
                            entry.CarID + "' was ignored.", Types.LoggingLevels.Warning);
                        continue;
                    }
                    byCarId.Add(entry.CarID, entry);
                }

                Dictionary<string, string> changedBrands =
                    new Dictionary<string, string>(StringComparer.Ordinal);
                int renamedCars = 0;
                int renamedVersions = 0;
                int matchedCars = 0;
                int vanillaCars = 0;
                int dlcCars = 0;
                int workshopCars = 0;
                int localModCars = 0;

                foreach (CarConfigData car in
                    GlobalState.GameManager.CarBundleLoader.CarNamesData) {
                    CountSource(car, ref vanillaCars, ref dlcCars,
                        ref workshopCars, ref localModCars);

                    Types.CarNameConfig configured;
                    if (!byCarId.TryGetValue(car.CarID, out configured))
                        continue;
                    matchedCars++;

                    if (string.IsNullOrWhiteSpace(configured.CarName) ||
                        string.IsNullOrWhiteSpace(configured.CarBrand))
                        continue;

                    string originalBrand = GetConfigValue(car, 0, "0_main.carBrand");
                    if (car.CarName != configured.CarName ||
                        originalBrand != configured.CarBrand)
                        renamedCars++;

                    car.CarName = configured.CarName;
                    if (originalBrand != configured.CarBrand)
                        changedBrands[car.CarID] = configured.CarBrand;

                    string[] suffixes = configured.CarConfigSuffix ?? new string[0];
                    if (suffixes.Length != car.CarConfigs.Count)
                        LogVersionNameMismatch(car, suffixes);

                    int suffixCount = Math.Min(suffixes.Length, car.CarConfigs.Count);
                    for (int i = 0; i < car.CarConfigs.Count; i++) {
                        car.CarConfigs[i].elements["0_main.carBrand"] =
                            configured.CarBrand;
                        if (i >= suffixCount || suffixes[i] == null)
                            continue;

                        if (GetConfigValue(car, i, "0_main.carVersionName") !=
                            suffixes[i])
                            renamedVersions++;
                        car.CarConfigs[i].elements["0_main.carVersionName"] =
                            suffixes[i];
                    }

                }

                GameInventory inventory = Singleton<GameInventory>.Instance;
                if (inventory == null)
                    return false;

                UpdateBodyPartBrands(inventory, changedBrands);
                inventory.UpdateLocalizations();
                ModLogger.Log("[AuthenticCarNames] Loaded " + entries.Length +
                    " entries; matched=" + matchedCars + ", renamedCars=" +
                    renamedCars + ", renamedVersions=" + renamedVersions + "." +
                    Environment.NewLine + "Game cars: vanilla=" + vanillaCars +
                    ", DLC=" + dlcCars + ", workshop=" + workshopCars +
                    ", localMods=" + localModCars + ".",
                    Types.LoggingLevels.Normal);
                return true;
            } catch (Exception exception) {
                ModLogger.Log("[AuthenticCarNames] Failed to apply " +
                    GlobalConfig.cfgAuthCar + Environment.NewLine + exception,
                    Types.LoggingLevels.Error);
                return false;
            }
        }

        private static void LogVersionNameMismatch(CarConfigData car,
            string[] configuredSuffixes)
        {
            ModLogger.Log("[AuthenticCarNames] " + car.CarID +
                " suffix count " + configuredSuffixes.Length +
                " does not match game config count " +
                car.CarConfigs.Count + ".", Types.LoggingLevels.Warning);
        }

        private static void UpdateBodyPartBrands(GameInventory inventory,
            Dictionary<string, string> changedBrands)
        {
            if (changedBrands.Count == 0)
                return;

            UpdateBodyPartBrandList(inventory.GetItems(ShopType.Body), changedBrands);
            UpdateBodyPartBrandList(inventory.GetItems(ShopType.BodyTuning), changedBrands);
        }

        private static void UpdateBodyPartBrandList(
            Il2CppSystem.Collections.Generic.List<PartProperty> items,
            Dictionary<string, string> changedBrands)
        {
            if (items == null)
                return;

            foreach (PartProperty item in items) {
                if (item == null)
                    continue;
                string brand;
                if (changedBrands.TryGetValue(item.CarID, out brand))
                    item.Brand = brand;
            }
        }

        private static string GetConfigValue(CarConfigData car, int index,
            string key)
        {
            if (index < 0 || index >= car.CarConfigs.Count ||
                !car.CarConfigs[index].elements.ContainsKey(key))
                return string.Empty;
            return car.CarConfigs[index].elements[key];
        }

        private static void CountSource(CarConfigData car, ref int vanilla,
            ref int dlc, ref int workshop, ref int localMod)
        {
            switch (car.FileType) {
                case CarFileType.Standard:
                    vanilla++;
                    break;
                case CarFileType.DLC:
                    dlc++;
                    break;
                case CarFileType.MOD:
                    if (!string.IsNullOrEmpty(car.PathToFile) &&
                        car.PathToFile.IndexOf("StreamingAssets",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        localMod++;
                    else
                        workshop++;
                    break;
            }
        }
    }
}
