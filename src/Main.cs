using MelonLoader;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;

#if NET6_0_OR_GREATER
using Il2Cpp;
#else
using CMS;
#endif

namespace Cms21ImmersionPlus
{
    public static class BuildInfo
    {
        public const string Name = "CMS21 Immersion+";
        public const string ShortName = "CMS21 Immersion+";
        public const string TechnicalName = "CMS21ImmersionPlus";
        public const string Description = "Authenticity and visual immersion improvements for Car Mechanic Simulator 2021";
        public const string Version = "4.2";
        public const string Author = "CMS21 Immersion Plus contributors";
        public const string Company = "CMS21 Immersion Plus";
        public const string DownloadLink = "";
        public const string MelonGameCompany = "Red Dot Games";
        public const string MelonGameName = "Car Mechanic Simulator 2021";
    }

    public sealed class Main : MelonMod
    {
        private const int DataInitializationAttempts = 10;
        private static readonly string NewLine = Environment.NewLine;
        private static bool initialized;
        private static bool dataInitializationWorkerRunning;
        private static bool authenticCarNamesInitialized;
        private static bool brandLogosInitialized;
        private static bool vehicleVisualReplacementsInitialized;

        public static MelonPreferences_Entry<Settings> SettingsEntry;

        public override void OnLateInitializeMelon()
        {
            ModLogger.InitializeDebugFile();
            string startMessage = BuildInfo.Name + " v" + BuildInfo.Version +
                " initializing; Unity " + Application.unityVersion + ", game " +
                GameSettings.BuildVersion + ".";
            ModLogger.Log(startMessage, Types.LoggingLevels.NormalClean);
            ModLogger.Log(startMessage, Types.LoggingLevels.PlayerLog);
            GlobalState.GameManager = Singleton<GameManager>.Instance;
            DetectPlatform();
            ShowroomLicencePlateFeature.RefreshCurrentUsername();
            bool melonDebug = Environment.GetCommandLineArgs().Contains("--melonloader.debug");
            LoadSettings();
            ModLogger.ConfigureUnityLogForwarding(melonDebug);
            ApplyHarmonyPatches();
            initialized = true;
        }

        public override void OnDeinitializeMelon()
        {
            if (!initialized)
                return;
            ModLogger.Log(BuildInfo.ShortName + " stopped.", Types.LoggingLevels.Normal);
            ModLogger.Shutdown();
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            ParkingSceneVehiclePreloadFeature.OnSceneUnloaded(sceneName);
            GlobalState.IsGarageSceneActive = UnityEngine.SceneManagement.SceneManager
                .GetSceneByName("garage").isLoaded;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (!initialized || buildIndex == -1)
                return;
            VehicleVisualReplacementFeature.OnSceneLoaded(sceneName);
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (!initialized || buildIndex == -1)
                return;
            ParkingSceneVehiclePreloadFeature.OnSceneInitialized(sceneName);
            if (sceneName == "garage")
                GlobalState.IsGarageSceneActive = true;
            if (sceneName == "Menu") {
                GlobalState.GameManager = Singleton<GameManager>.Instance;
                ShowroomLicencePlateFeature.RefreshCurrentUsername();
                TryInitializeGameDataFeatures();
                if (!AreGameDataFeaturesInitialized() && !dataInitializationWorkerRunning)
                    MelonCoroutines.Start(InitializeGameDataFeatures());
            }
        }

        private static IEnumerator InitializeGameDataFeatures()
        {
            dataInitializationWorkerRunning = true;
            try {
                for (int attempt = 1; attempt <= DataInitializationAttempts; attempt++) {
                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Menu")
                        yield break;
                    TryInitializeGameDataFeatures();
                    if (AreGameDataFeaturesInitialized())
                        yield break;
                    yield return new WaitForSeconds(1f);
                }
                ModLogger.Log("[Startup] Immersion game-data features were not fully initialized after " +
                    DataInitializationAttempts + " attempts.", Types.LoggingLevels.Warning);
            } finally {
                dataInitializationWorkerRunning = false;
            }
        }

        private static void TryInitializeGameDataFeatures()
        {
            if (GlobalState.GameManager == null)
                GlobalState.GameManager = Singleton<GameManager>.Instance;
            if (!authenticCarNamesInitialized)
                authenticCarNamesInitialized = TryInitializeFeature("AuthenticCarNames", AuthenticCarNamesFeature.Apply);
            if (!brandLogosInitialized)
                brandLogosInitialized = TryInitializeFeature("BrandLogos", BrandLogoFeature.Apply);
            if (!vehicleVisualReplacementsInitialized)
                vehicleVisualReplacementsInitialized = TryInitializeFeature("VehicleVisualReplacements", VehicleVisualReplacementFeature.OnGameDataReady);
        }

        private static bool TryInitializeFeature(string name, Func<bool> initializer)
        {
            try { return initializer(); }
            catch (Exception exception) {
                ModLogger.Log("[Startup] " + name + " initialization failed and will be retried." +
                    NewLine + exception, Types.LoggingLevels.Error);
                return false;
            }
        }

        private static bool AreGameDataFeaturesInitialized()
        {
            return authenticCarNamesInitialized && brandLogosInitialized && vehicleVisualReplacementsInitialized;
        }

        private void ApplyHarmonyPatches()
        {
            Type[] patchTypes = typeof(Main).Assembly.GetTypes()
                .Where(type => type.GetCustomAttributes(typeof(HarmonyPatch), false).Length > 0)
                .OrderBy(type => type.FullName, StringComparer.Ordinal).ToArray();
            int applied = 0;
            int failed = 0;
            foreach (Type patchType in patchTypes) {
                try {
                    HarmonyInstance.CreateClassProcessor(patchType).Patch();
                    applied++;
                } catch (Exception exception) {
                    failed++;
                    ModLogger.Log("[Harmony] Patch class failed: " + patchType.FullName +
                        NewLine + exception, Types.LoggingLevels.Error);
                }
            }
            ModLogger.Log("Harmony patch classes: applied=" + applied + ", failed=" + failed + ".",
                failed == 0 ? Types.LoggingLevels.NormalClean : Types.LoggingLevels.Warning);
        }

        private static void DetectPlatform()
        {
            try {
                string platform = GlobalState.GameManager.PlatformManager.platform.ToString();
                ModLogger.Log("Platform " + platform, Types.LoggingLevels.NormalClean);
            } catch (Exception exception) {
                ModLogger.Log("[Startup] Platform detection failed." + NewLine + exception,
                    Types.LoggingLevels.Warning);
            }
        }

        private static void LoadSettings()
        {
            bool fileExisted = File.Exists(GlobalConfig.cfgFile);
            MelonPreferences_Category preferences = MelonPreferences.CreateCategory(BuildInfo.TechnicalName);
            preferences.SetFilePath(GlobalConfig.cfgFile, autoload: false);
            SettingsEntry = preferences.CreateEntry<Settings>("Settings", new Settings(), null,
                BuildInfo.ShortName + " feature switches");
            preferences.LoadFromFile();
            if (!fileExisted)
                preferences.SaveToFile(false);
        }
    }
}
