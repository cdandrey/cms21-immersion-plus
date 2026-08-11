using System.Collections;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

#if NET6_0_OR_GREATER
using Il2Cpp;
using Il2CppCMS.Providers;
#else
using CMS;
using CMS.Providers;
#endif

namespace Cms21ImmersionPlus
{
    /// <summary>Displays the current player name on showroom and car-salon licence plates.</summary>
    [HarmonyPatch]
    public static class ShowroomLicencePlateFeature
    {
        private static string currentUsername = string.Empty;

        public static void RefreshCurrentUsername()
        {
            GameManager manager = GlobalState.GameManager != null
                ? GlobalState.GameManager
                : Singleton<GameManager>.Instance;
            currentUsername = manager != null && manager.PlatformManager != null
                ? manager.PlatformManager.GetCurrentUserName()
                : string.Empty;
        }

        private static bool IsEnabled {
            get {
                return Main.SettingsEntry != null &&
                    Main.SettingsEntry.Value.showPlayerNameOnShowroomLicencePlates &&
                    !string.IsNullOrWhiteSpace(currentUsername);
            }
        }

        [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.LoadAndPrepareModel))]
        [HarmonyPrefix]
        public static void LoadAndPrepareModelPrefix(CarLoader __instance)
        {
            if (IsEnabled && __instance != null)
                MelonCoroutines.Start(ApplyAfterLoad(__instance));
        }

        private static IEnumerator ApplyAfterLoad(CarLoader loader)
        {
            const int maximumWaitFrames = 600;
            string scene = UnityEngine.SceneManagement.SceneManager
                .GetActiveScene().name;
            if (!IsSupportedScene(scene))
                yield break;

            int waitedFrames = 0;
            while (loader != null && (!loader.done || !loader.modelLoaded) &&
                waitedFrames < maximumWaitFrames) {
                if (!IsEnabled ||
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name !=
                        scene)
                    yield break;
                waitedFrames++;
                yield return new WaitForEndOfFrame();
            }

            if (!CanApply(loader, scene)) {
                if (loader != null && IsEnabled &&
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name ==
                        scene) {
                    ModLogger.Log("[ShowroomPlate] Car model was not ready within " +
                        "the wait limit.", Types.LoggingLevels.Warning);
                }
                yield break;
            }

            yield return new WaitForEndOfFrame();
            if (!CanApply(loader, scene))
                yield break;

            if (!loader.liveUpdate) {
                ApplyPlayerName(loader);
                if (scene == "Showroom_2") {
                    yield return new WaitForSeconds(1f);
                    if (!CanApply(loader, scene))
                        yield break;
                    ApplyPlayerName(loader);
                }
            }

            if (CanApply(loader, scene))
                ApplyConfiguredPlateTextures(loader);
        }

        private static bool IsSupportedScene(string scene)
        {
            return scene == "Showroom_2" || scene == "Auto_salon";
        }

        private static bool CanApply(CarLoader loader, string scene)
        {
            return IsEnabled && loader != null && loader.done && loader.modelLoaded &&
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == scene;
        }

        private static void ApplyPlayerName(CarLoader loader)
        {
            loader.SetNewLicensePlateNumber(currentUsername, false);
            loader.SetNewLicensePlateNumber(currentUsername, true);
        }

        private static void ApplyConfiguredPlateTextures(CarLoader loader)
        {
            GameInventory inventory = Singleton<GameInventory>.Instance;
            GameManager gameManager = Singleton<GameManager>.Instance;
            if (inventory == null || gameManager == null)
                return;

            LicensePlatesProvider provider = inventory.licensePlatesProvider;
            CarBundleLoader bundleLoader = gameManager.CarBundleLoader;
            if (provider == null || bundleLoader == null)
                return;

            string frontTexture = bundleLoader.GetCarPropertyString(
                loader.carToLoad, loader.ConfigVersion, "licensePlateFrontTex",
                "9_exterior", string.Empty);
            string rearTexture = bundleLoader.GetCarPropertyString(
                loader.carToLoad, loader.ConfigVersion, "licensePlateRearTex",
                "9_exterior", string.Empty);

            ApplyPlateTexture(loader, provider,
                loader.GetCarPart("license_plate_front"), frontTexture);
            ApplyPlateTexture(loader, provider,
                loader.GetCarPart("license_plate_rear"), rearTexture);
        }

        private static void ApplyPlateTexture(CarLoader loader,
            LicensePlatesProvider provider, CarPart part, string textureName)
        {
            if (part == null || string.IsNullOrWhiteSpace(textureName))
                return;

            try {
                var plate = provider.GetLicensePlate(textureName);
                if (plate.Value != null && plate.Value.texture != null)
                    loader.ChangeLicencePlateTexture(part, plate.Value.texture.name);
            } catch (System.Exception exception) {
                ModLogger.Log("[ShowroomPlate] Licence-plate texture '" +
                    textureName + "' was not applied." +
                    System.Environment.NewLine + exception,
                    Types.LoggingLevels.Warning);
            }
        }
    }
}
