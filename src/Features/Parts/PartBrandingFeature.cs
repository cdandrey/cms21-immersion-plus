using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if NET6_0_OR_GREATER
using Il2Cpp;
using Il2CppCMS.Containers;
using Il2CppCMS.UI.Logic;
#else
using CMS;
using CMS.Containers;
using CMS.UI.Logic;
#endif

namespace Cms21ImmersionPlus
{
    public static class PartBrandingFeature
    {
        private static readonly Dictionary<string, string> LogoFiles =
            new Dictionary<string, string>(StringComparer.Ordinal) {
                { "bbs", "PB_BBS.png" },
                { "bfgoodrich", "PB_BFGoodrich.png" },
                { "bilstein", "PB_Bilstein.png" },
                { "borla", "PB_Borla.png" },
                { "bosch", "PB_Bosch.png" },
                { "brembo", "PB_Brembo.png" },
                { "compcams", "PB_CompCams.png" },
                { "continental", "PB_Continental.png" },
                { "dorman", "PB_Dorman.png" },
                { "edelbrock", "PB_Edelbrock.png" },
                { "eibach", "PB_Eibach.png" },
                { "enkei", "PB_Enkei.png" },
                { "garrett", "PB_Garrett.png" },
                { "gates", "PB_Gates.png" },
                { "gkn", "PB_GKN.png" },
                { "kn", "PB_KN.png" },
                { "lemforder", "PB_Lemforder.png" },
                { "magnaflow", "PB_MagnaFlow.png" },
                { "mahle", "PB_MAHLE.png" },
                { "mannfilter", "PB_MANNFilter.png" },
                { "momo", "PB_MOMO.png" },
                { "ngk", "PB_NGK.png" },
                { "ozracing", "PB_OZRacing.png" },
                { "pirelli", "PB_Pirelli.png" },
                { "rays", "PB_RAYS.png" },
                { "recaro", "PB_RECARO.png" },
                { "sachs", "PB_SACHS.png" },
                { "skf", "PB_SKF.png" },
                { "tremec", "PB_TREMEC.png" },
                { "varta", "PB_VARTA.png" },
                { "wiseco", "PB_Wiseco.png" },
                { "zf", "PB_ZF.png" }
            };

        private static readonly Dictionary<string, float> LogoScales =
            new Dictionary<string, float>(StringComparer.Ordinal) {
                { "dorman", 0.50f },
                { "mahle", 0.50f },
                { "tremec", 0.50f },
                { "bfgoodrich", 0.50f },
                { "recaro", 0.50f },
                { "momo", 0.50f },
                { "bosch", 0.50f },
                { "varta", 0.50f },
                { "sachs", 0.60f },
                { "wiseco", 0.40f },
                { "compcams", 0.50f },
                { "garrett", 0.60f },
                { "brembo", 0.60f },
                { "continental", 0.60f },
                { "bbs", 0.60f },
                { "enkei", 0.60f },
                { "rays", 0.60f },
                { "eibach", 0.60f },
                { "pirelli", 0.60f },
                { "bilstein", 0.60f },
                { "mannfilter", 0.70f },
                { "ozracing", 0.70f },
                { "gkn", 0.80f },
                { "kn", 0.70f },
                { "ngk", 0.70f },
                { "borla", 0.80f },
                { "magnaflow", 0.80f },
                { "edelbrock", 0.80f }
            };

        private static readonly HashSet<string> GenericPartBrands =
            new HashSet<string>(StringComparer.Ordinal) {
                "bardogh", "bostonawy", "evanor", "fierte", "octarion",
                "shushutri", "trando", "voiz", "wralthz"
            };

        private static readonly HashSet<string> GenericInteriorBrands =
            new HashSet<string>(StringComparer.Ordinal) {
                "bostonawy", "reichshof", "trando", "wralthz"
            };

        private static readonly Dictionary<string, string> GenericRimBrands =
            new Dictionary<string, string>(StringComparer.Ordinal) {
                { "voiz", "bbs" },
                { "evanor", "ozracing" },
                { "wralthz", "enkei" },
                { "trando", "rays" }
            };

        private static readonly string[] BatteryTerms = { "akumulator", "battery" };
        private static readonly string[] IgnitionTerms = { "swiec", "cewk", "kable", "rozdzielacz", "kopulka", "zaplon" };
        private static readonly string[] TuningIgnitionTerms = { "swiec", "cewk", "kable", "cable", "rozdzielacz", "kopulka", "zaplon" };
        private static readonly string[] FilterTerms = { "filtr" };
        private static readonly string[] TuningFilterTerms = { "filtr", "rura_dolot", "ruradolot" };
        private static readonly string[] TurboTerms = { "turbo", "kompresor", "sprezarka", "supercharger" };
        private static readonly string[] TuningTurboTerms = { "turbo", "kompresor", "sprezarka", "supercharger", "intercooler" };
        private static readonly string[] TimingTerms = { "pasek", "lancuch", "rolka", "napinacz", "pompa_wody", "pompawody" };
        private static readonly string[] TuningTimingTerms = { "termostat", "pasek", "lancuch", "rolka", "napinacz" };
        private static readonly string[] PowerSteeringTerms = { "wspomag", "power_steering" };
        private static readonly string[] MiscReplacementTerms = { "bak_", "chlodnic", "wentylator", "fan", "zbiornik", "reservoir", "klips" };
        private static readonly string[] MainElectricalTerms = {
            "alternator", "rozrusz", "starter", "ecu", "kontroler", "controller", "inverter", "rectifier",
            "fuse", "abs", "wtrysk", "inject", "pompa_paliwa", "pompapaliwa", "fuelpump", "czujnik", "sensor",
            "przepustnic", "listwa_wtrysk"
        };
        private static readonly string[] TuningElectricalTerms = {
            "alternator", "rozrusz", "starter", "ecu", "kontroler", "controller", "inverter", "rectifier",
            "fuse", "abs", "wtrysk", "inject", "pompa_1", "pompa_paliwa", "pompapaliwa", "fuelpump",
            "przepustnic", "listwa_wtrysk", "charger", "converter", "inductor", "capacitor", "junction",
            "electronic", "electonics"
        };
        private static readonly string[] ClutchTerms = { "sprzeg", "clutch", "kolo_zamach", "kolazamach", "flywheel", "docisk" };
        private static readonly string[] DriveTerms = { "walnaped", "polos", "przegub", "axle", "driveshaft" };
        private static readonly string[] AbsorberTerms = { "amortyz" };
        private static readonly string[] SpringTerms = { "sprezyn", "spring" };
        private static readonly string[] BearingTerms = { "lozysk", "piasta", "hub", "bearing" };
        private static readonly string[] CamshaftTerms = { "walek", "camshaft" };
        private static readonly string[] PistonTerms = { "tlok", "piston" };
        private static readonly string[] ExhaustManifoldTerms = { "kolektor_wydech" };
        private static readonly string[] IntakeTerms = { "kolektor_dolot", "kolektordolot", "gaznik", "glowica", "kolektor" };
        private static readonly string[] VintageTireTerms = { "vintage", "4x4" };
        private static readonly string[] PerformanceTireTerms = { "sport", "race", "slick" };

        public static bool Apply()
        {
            if (Main.SettingsEntry == null || !Main.SettingsEntry.Value.rebrandParts)
                return true;

            GameInventory inventory = Singleton<GameInventory>.Instance;
            if (inventory == null || inventory.partPropertyList == null)
                return false;

            try {
                HashSet<string> availableBrands = InstallMissingLogos(inventory);
                if (availableBrands.Count == 0) {
                    ModLogger.Log("[PartBranding] No part-brand logos are available; part branding was skipped.",
                        Types.LoggingLevels.Warning);
                    return true;
                }

                int changed = 0;
                int availableTargetBrands = CountAvailableTargetBrands(availableBrands);
                changed += ApplyMainShop(inventory.GetItems(ShopType.Main), availableBrands);
                changed += ApplyElectronicsShop(inventory.GetItems(ShopType.Electronics), availableBrands);
                changed += ApplyTuningShop(inventory.GetItems(ShopType.Tuning), availableBrands);
                changed += ApplyGearboxShop(inventory.GetItems(ShopType.Gearbox), availableBrands);
                changed += ApplyInteriorShop(inventory.GetItems(ShopType.Interior), availableBrands);
                changed += ApplyTireShop(inventory.GetItems(ShopType.Tire), availableBrands);
                changed += ApplyRimsShop(inventory.GetItems(ShopType.Rims), availableBrands);
                changed += ApplyAddonsShop(inventory.GetItems(ShopType.Addons), availableBrands);

                ModLogger.Log("[PartBranding] Rebranded " + changed +
                    " aftermarket part(s) using " + availableTargetBrands + " real-world brand logo(s).",
                    Types.LoggingLevels.Normal);
                return true;
            } catch (Exception exception) {
                ModLogger.Log("[PartBranding] Failed to apply part branding." +
                    Environment.NewLine + exception, Types.LoggingLevels.Error);
                return false;
            }
        }

        private static HashSet<string> InstallMissingLogos(GameInventory inventory)
        {
            HashSet<string> available = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (SpriteContainer container in inventory.Brands) {
                if (container != null && !string.IsNullOrEmpty(container.name))
                    available.Add(container.name);
            }

            string directory = Path.GetFullPath(GlobalConfig.directoryPartBrand);
            if (!Directory.Exists(directory)) {
                ModLogger.Log("[PartBranding] Part-brand logo directory is missing: " + directory,
                    Types.LoggingLevels.Warning);
                return available;
            }

            int added = 0;
            foreach (KeyValuePair<string, string> logo in LogoFiles) {
                if (available.Contains(logo.Key))
                    continue;

                string path = Path.Combine(directory, logo.Value);
                if (!File.Exists(path)) {
                    ModLogger.Log("[PartBranding] Logo file is missing: " + path,
                        Types.LoggingLevels.Warning);
                    continue;
                }

                Sprite sprite = TextureLoader.LoadSpriteFromFile(path, true, false);
                if (sprite == null)
                    continue;

                sprite.texture.name = logo.Key;
                Sprite displaySprite = TextureLoader.CreateScaledTrimmedSprite(
                    sprite, GetLogoScale(logo.Key));
                if (displaySprite == null)
                    displaySprite = sprite;
                else if (displaySprite != sprite) {
                    UnityEngine.Object.Destroy(sprite.texture);
                    UnityEngine.Object.Destroy(sprite);
                }

                inventory.Brands.Add(new SpriteContainer {
                    name = logo.Key,
                    sprite = displaySprite
                });
                available.Add(logo.Key);
                added++;
            }

            if (added > 0)
                inventory.ReloadBrands(inventory.Brands);
            return available;
        }

        private static int CountAvailableTargetBrands(HashSet<string> availableBrands)
        {
            int count = 0;
            foreach (string brand in LogoFiles.Keys) {
                if (availableBrands.Contains(brand))
                    count++;
            }
            return count;
        }

        private static int ApplyMainShop(Il2CppSystem.Collections.Generic.List<PartProperty> parts,
            HashSet<string> availableBrands)
        {
            if (parts == null)
                return 0;

            int changed = 0;
            foreach (PartProperty part in parts) {
                if (part == null || !GenericPartBrands.Contains(part.Brand ?? string.Empty))
                    continue;

                string target = GetMainBrand(part);
                if (AssignBrand(part, target, availableBrands))
                    changed++;
            }
            return changed;
        }

        private static int ApplyElectronicsShop(Il2CppSystem.Collections.Generic.List<PartProperty> parts,
            HashSet<string> availableBrands)
        {
            if (parts == null)
                return 0;

            int changed = 0;
            foreach (PartProperty part in parts) {
                if (part == null || !GenericPartBrands.Contains(part.Brand ?? string.Empty))
                    continue;

                string id = part.ID ?? string.Empty;
                string target = ContainsAny(id, BatteryTerms)
                    ? "varta"
                    : ContainsAny(id, IgnitionTerms)
                        ? "ngk"
                        : "bosch";
                if (AssignBrand(part, target, availableBrands))
                    changed++;
            }
            return changed;
        }

        private static int ApplyTuningShop(Il2CppSystem.Collections.Generic.List<PartProperty> parts,
            HashSet<string> availableBrands)
        {
            if (parts == null)
                return 0;

            int changed = 0;
            foreach (PartProperty part in parts) {
                if (part == null)
                    continue;

                string target = GetTuningBrand(part);
                if (AssignBrand(part, target, availableBrands))
                    changed++;
            }
            return changed;
        }

        private static int ApplyGearboxShop(Il2CppSystem.Collections.Generic.List<PartProperty> parts,
            HashSet<string> availableBrands)
        {
            if (parts == null)
                return 0;

            int changed = 0;
            foreach (PartProperty part in parts) {
                if (part != null && AssignBrand(part, "tremec", availableBrands))
                    changed++;
            }
            return changed;
        }

        private static int ApplyInteriorShop(Il2CppSystem.Collections.Generic.List<PartProperty> parts,
            HashSet<string> availableBrands)
        {
            if (parts == null)
                return 0;

            int changed = 0;
            foreach (PartProperty part in parts) {
                if (part == null || !GenericInteriorBrands.Contains(part.Brand ?? string.Empty))
                    continue;

                string target = string.Equals(part.ShopGroup, "SteeringWheels", StringComparison.Ordinal)
                    ? "momo"
                    : string.Equals(part.ShopGroup, "Seats", StringComparison.Ordinal) ||
                      string.Equals(part.ShopGroup, "Benches", StringComparison.Ordinal)
                        ? "recaro"
                        : null;
                if (AssignBrand(part, target, availableBrands))
                    changed++;
            }
            return changed;
        }

        private static int ApplyTireShop(Il2CppSystem.Collections.Generic.List<PartProperty> parts,
            HashSet<string> availableBrands)
        {
            if (parts == null)
                return 0;

            int changed = 0;
            foreach (PartProperty part in parts) {
                if (part == null)
                    continue;

                string id = part.ID ?? string.Empty;
                string target = ContainsAny(id, VintageTireTerms)
                    ? "bfgoodrich"
                    : ContainsAny(id, PerformanceTireTerms)
                        ? "pirelli"
                        : "continental";
                if (AssignBrand(part, target, availableBrands))
                    changed++;
            }
            return changed;
        }

        private static int ApplyRimsShop(Il2CppSystem.Collections.Generic.List<PartProperty> parts,
            HashSet<string> availableBrands)
        {
            if (parts == null)
                return 0;

            int changed = 0;
            foreach (PartProperty part in parts) {
                if (part == null)
                    continue;

                string target;
                if (!GenericRimBrands.TryGetValue(part.Brand ?? string.Empty, out target))
                    continue;
                if (AssignBrand(part, target, availableBrands))
                    changed++;
            }
            return changed;
        }

        private static int ApplyAddonsShop(Il2CppSystem.Collections.Generic.List<PartProperty> parts,
            HashSet<string> availableBrands)
        {
            if (parts == null)
                return 0;

            int changed = 0;
            foreach (PartProperty part in parts) {
                if (part == null || !string.IsNullOrWhiteSpace(part.Brand))
                    continue;

                string id = part.ID ?? string.Empty;
                string target = id.IndexOf("hood_scoop", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "edelbrock"
                    : id.IndexOf("policeSiren", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      id.IndexOf("policeLights", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      id.IndexOf("ledLights", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      id.IndexOf("taxiSign", StringComparison.OrdinalIgnoreCase) >= 0
                        ? "bosch"
                        : "dorman";
                if (AssignBrand(part, target, availableBrands))
                    changed++;
            }
            return changed;
        }

        private static string GetMainBrand(PartProperty part)
        {
            string id = part.ID ?? string.Empty;
            switch (part.ShopGroup) {
                case "Brakes":
                    return "brembo";
                case "Exhaust":
                    return "magnaflow";
                case "Gearbox":
                    if (ContainsAny(id, ClutchTerms))
                        return "sachs";
                    if (ContainsAny(id, DriveTerms))
                        return "gkn";
                    return "zf";
                case "Suspension":
                    if (ContainsAny(id, AbsorberTerms))
                        return "bilstein";
                    if (ContainsAny(id, SpringTerms))
                        return "eibach";
                    if (ContainsAny(id, BearingTerms))
                        return "skf";
                    if (ContainsAny(id, DriveTerms))
                        return "gkn";
                    return "lemforder";
                case "Engine":
                    if (ContainsAny(id, BatteryTerms))
                        return "varta";
                    if (ContainsAny(id, FilterTerms))
                        return "mannfilter";
                    if (ContainsAny(id, MiscReplacementTerms))
                        return "dorman";
                    if (ContainsAny(id, TimingTerms))
                        return "gates";
                    if (ContainsAny(id, TurboTerms))
                        return "garrett";
                    if (ContainsAny(id, PowerSteeringTerms))
                        return "zf";
                    if (ContainsAny(id, MainElectricalTerms))
                        return "bosch";
                    return "mahle";
                default:
                    return null;
            }
        }

        private static string GetTuningBrand(PartProperty part)
        {
            string id = part.ID ?? string.Empty;
            switch (part.ShopGroup) {
                case "Brakes":
                    return "brembo";
                case "Gearbox":
                    return ContainsAny(id, ClutchTerms) ? "sachs" : "tremec";
                case "Exhaust":
                    return "borla";
                case "Engine":
                    if (ContainsAny(id, BatteryTerms))
                        return "varta";
                    if (ContainsAny(id, TuningIgnitionTerms))
                        return "ngk";
                    if (ContainsAny(id, TuningFilterTerms))
                        return "kn";
                    if (ContainsAny(id, TuningTurboTerms))
                        return "garrett";
                    if (ContainsAny(id, CamshaftTerms))
                        return "compcams";
                    if (ContainsAny(id, PistonTerms))
                        return "wiseco";
                    if (ContainsAny(id, TuningTimingTerms))
                        return "gates";
                    if (ContainsAny(id, PowerSteeringTerms))
                        return "zf";
                    if (ContainsAny(id, TuningElectricalTerms))
                        return "bosch";
                    if (ContainsAny(id, ExhaustManifoldTerms))
                        return "borla";
                    if (ContainsAny(id, IntakeTerms))
                        return "edelbrock";
                    return "mahle";
                default:
                    return null;
            }
        }

        private static float GetLogoScale(string brand)
        {
            float scale;
            return LogoScales.TryGetValue(brand ?? string.Empty, out scale)
                ? scale
                : 1f;
        }

        private static bool AssignBrand(PartProperty part, string target,
            HashSet<string> availableBrands)
        {
            if (string.IsNullOrEmpty(target) || !availableBrands.Contains(target) ||
                string.Equals(part.Brand, target, StringComparison.Ordinal))
                return false;

            part.Brand = target;
            return true;
        }

        private static bool ContainsAny(string value, string[] terms)
        {
            for (int i = 0; i < terms.Length; i++) {
                if (value.IndexOf(terms[i], StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }
    }



}