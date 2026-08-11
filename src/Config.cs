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
        [Tomlet.Attributes.TomlInlineComment("Rename vehicles and brands from AuthenticCarNames.cfg")]
        public bool useAuthenticCarNames = true;
        [Tomlet.Attributes.TomlInlineComment("Load brand logos supplied by workshop vehicle mods")]
        public bool loadBrandLogosFromMods = true;
        [Tomlet.Attributes.TomlInlineComment("Also import brand logos from Mods\\TKAftermarket\\brands when present")]
        public bool loadBrandLogosFromTKAftermarket = false;
        [Tomlet.Attributes.TomlInlineComment("Load local brand logos and scene/car texture replacements")]
        public bool loadTexturesFromFolder = true;
        [Tomlet.Attributes.TomlInlineComment("Keep all ten vehicles visible in the current parking alley")]
        public bool preloadAllParkingSceneVehicles = true;
        [Tomlet.Attributes.TomlInlineComment("Show the current player name on showroom licence plates")]
        public bool showPlayerNameOnShowroomLicencePlates = true;
    }

    public static class GlobalConfig
    {
        public static readonly string cfgFile = @"Mods\CMS21ImmersionPlus\CMS21ImmersionPlus.cfg";
        public static readonly string cfgAuthCar = @"Mods\CMS21ImmersionPlus\AuthenticCarNames.cfg";
        public static readonly string directoryBrandLogos = @"Mods\CMS21ImmersionPlus\BrandLogos\";
        public static readonly string directoryTKAftermarketBrands = @"Mods\TKAftermarket\brands\";
        public static readonly string directoryTextureReplacements = @"Mods\CMS21ImmersionPlus\TextureReplacements\";
    }

    public static class GlobalState
    {
        public static bool IsGarageSceneActive;
        public static GameManager GameManager;
    }

    public static class Types
    {
        public enum LoggingLevels { Normal, NormalClean, Debug, PlayerLog, Warning, Error }

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
