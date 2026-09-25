#if NET6_0_OR_GREATER
using Il2Cpp;
#else
using CMS;
#endif

namespace Cms21ImmersionPlus
{
    public sealed class Settings
    {
        [Tomlet.Attributes.TomlInlineComment("Remove the dyno menu blur")]
        public bool removeDynoMenuBlur = true;
        [Tomlet.Attributes.TomlInlineComment("Use authentic vehicle names, brands and local visual replacements")]
        public bool useAuthenticCarNames = true;
        [Tomlet.Attributes.TomlInlineComment("Load brand logos supplied by workshop vehicle mods")]
        public bool loadBrandLogosFromMods = true;
        [Tomlet.Attributes.TomlInlineComment("Also import brand logos from Mods\\TKAftermarket\\brands when present")]
        public bool loadBrandLogosFromTKAftermarket = false;
        [Tomlet.Attributes.TomlInlineComment("Load garage advertising, calendars and related environment replacements")]
        public bool loadGarageAdvertising = true;
        [Tomlet.Attributes.TomlInlineComment("Replace shop-card branding with a unified style based on selected real-world brands")]
        public bool rebrandShops = true;
        [Tomlet.Attributes.TomlInlineComment("Replace fictional aftermarket part brands with real-world component manufacturers")]
        public bool rebrandParts = true;
        [Tomlet.Attributes.TomlInlineComment("Keep all ten vehicles visible in the current parking alley")]
        public bool preloadAllParkingSceneVehicles = true;
        [Tomlet.Attributes.TomlInlineComment("Show the current player name on showroom licence plates")]
        public bool showPlayerNameOnShowroomLicencePlates = true;
    }

    public static class GlobalConfig
    {
        public static readonly string cfgFile = @"Mods\CMS21ImmersionPlus\CMS21ImmersionPlus.cfg";
        public static readonly string cfgAuthCar = @"Mods\CMS21ImmersionPlus\AuthenticCarNames.cfg";
        public static readonly string directoryCarBrand = @"Mods\CMS21ImmersionPlus\CarBrand\";
        public static readonly string directoryTKAftermarketBrands = @"Mods\TKAftermarket\brands\";
        public static readonly string directoryTextureReplacements = @"Mods\CMS21ImmersionPlus\TextureReplacements\";
        public static readonly string directoryShopBrand = @"Mods\CMS21ImmersionPlus\ShopBrand\";
        public static readonly string directoryPartBrand = @"Mods\CMS21ImmersionPlus\PartBrand\";
    }

    public static class GlobalState
    {
        public static bool IsGarageSceneActive;
        public static GameManager GameManager;
    }

    public static class Types
    {
        public enum LoggingLevels { Normal, NormalClean, PlayerLog, Warning, Error }

        public sealed class AuthenticCarNamesConfig
        {
            public CarNameConfig[] Car;
        }

        public sealed class CarNameConfig
        {
            public string CarID;
            public string CarBrand;
            public string CarName;
            public string[] CarConfigSuffix;
        }
    }
}
