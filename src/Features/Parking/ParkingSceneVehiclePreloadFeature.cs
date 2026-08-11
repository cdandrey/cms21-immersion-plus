using System;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

#if NET6_0_OR_GREATER
using Il2Cpp;
using Il2CppCMS.UI;
using Il2CppCMS.UI.Logic;
using Il2CppCMS.UI.Logic.Parking;
using Il2CppCMS.UI.Windows;
#else
using CMS;
using CMS.UI;
using CMS.UI.Logic;
using CMS.UI.Logic.Parking;
using CMS.UI.Windows;
#endif

namespace Cms21ImmersionPlus
{
    /// <summary>
    /// Keeps all ten vehicles visible in the current parking alley. This does not move
    /// parking vehicles into the garage and does not change garage capacity.
    /// </summary>
    [HarmonyPatch]
    public static class ParkingSceneVehiclePreloadFeature
    {
        private const int MaximumWaitFrames = 600;
        private const int ParkingPlaceLimit = 10;

        private static int sceneGeneration;
        private static int reloadRequestVersion;
        private static int closeRequestVersion;
        private static int reloadWorkerGeneration = -1;
        private static int closeWorkerGeneration = -1;

        public static void OnSceneInitialized(string sceneName)
        {
            if (sceneName != "Parking")
                return;

            sceneGeneration++;
            if (IsEnabled())
                RequestReload();
        }

        public static void OnSceneUnloaded(string sceneName)
        {
            if (sceneName != "Parking")
                return;

            sceneGeneration++;
            reloadRequestVersion++;
            closeRequestVersion++;
        }

        [HarmonyPatch(typeof(ParkingWindow), nameof(ParkingWindow.LoadCar))]
        [HarmonyPrefix]
        private static void ParkingWindowLoadCarPrefix()
        {
            if (IsFeatureActive())
                RequestCloseLoadedCars();
        }

        [HarmonyPatch(typeof(ParkingManager), nameof(ParkingManager.HideCarAtPlace))]
        [HarmonyPrefix]
        private static bool ParkingManagerHideCarAtPlacePrefix(
            ref Il2CppSystem.Collections.IEnumerator __result)
        {
            if (!IsFeatureActive())
                return true;

            UIManager uiManager = UIManager.Get();
            if (uiManager == null || uiManager.InfoWindow == null ||
                uiManager.InfoWindow.isActiveAndEnabled)
                return true;

            try {
                ParkingWindow parkingWindow =
                    UnityEngine.Object.FindObjectOfType<ParkingWindow>();
                if (parkingWindow == null)
                    return true;

                if (!parkingWindow.submitInProgress)
                    parkingWindow.DeselectCurrentItem();

                parkingWindow.submitInProgress = false;
                parkingWindow.state = ParkingState.Idle;
                __result = null;
                return false;
            } catch (Exception exception) {
                ModLogger.Log("[ParkingPreload] Failed to keep the parking vehicle visible." +
                    Environment.NewLine + exception, Types.LoggingLevels.Warning);
                return true;
            }
        }

        [HarmonyPatch(typeof(ParkingWindow), nameof(ParkingWindow.OnParkingAlleyChange))]
        [HarmonyPostfix]
        private static void ParkingWindowOnParkingAlleyChangePostfix()
        {
            if (IsFeatureActive())
                RequestReload();
        }

        private static bool IsEnabled()
        {
            return Main.SettingsEntry != null &&
                Main.SettingsEntry.Value.preloadAllParkingSceneVehicles;
        }

        private static bool IsFeatureActive()
        {
            return IsEnabled() &&
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name ==
                    "Parking";
        }

        private static void RequestReload()
        {
            reloadRequestVersion++;
            int generation = sceneGeneration;
            if (reloadWorkerGeneration == generation)
                return;

            reloadWorkerGeneration = generation;
            MelonCoroutines.Start(ReloadWorker(generation));
        }

        private static System.Collections.IEnumerator ReloadWorker(int generation)
        {
            int processedVersion = -1;
            try {
                while (IsWorkerValid(generation) &&
                    processedVersion != reloadRequestVersion) {
                    processedVersion = reloadRequestVersion;
                    yield return ReloadCurrentAlley(generation);
                }
            } finally {
                if (reloadWorkerGeneration == generation)
                    reloadWorkerGeneration = -1;
                if (IsWorkerValid(generation) &&
                    processedVersion != reloadRequestVersion)
                    RequestReload();
            }
        }

        private static System.Collections.IEnumerator ReloadCurrentAlley(int generation)
        {
            ParkingWindow parkingWindow = null;
            int waitedFrames = 0;
            while (parkingWindow == null && waitedFrames < MaximumWaitFrames) {
                if (!IsWorkerValid(generation))
                    yield break;
                parkingWindow = UnityEngine.Object.FindObjectOfType<ParkingWindow>();
                if (parkingWindow == null) {
                    waitedFrames++;
                    yield return new WaitForFixedUpdate();
                }
            }

            if (!IsWorkerValid(generation) || parkingWindow == null) {
                if (IsWorkerValid(generation)) {
                    ModLogger.Log("[ParkingPreload] Parking window was not ready " +
                        "within the wait limit.", Types.LoggingLevels.Warning);
                }
                yield break;
            }

            yield return new WaitForFixedUpdate();
            if (!IsWorkerValid(generation))
                yield break;

            ParkingManager parkingManager =
                UnityEngine.Object.FindObjectOfType<ParkingManager>();
            parkingWindow = UnityEngine.Object.FindObjectOfType<ParkingWindow>();
            if (parkingManager == null || parkingWindow == null ||
                parkingManager.parkingSpaces == null ||
                parkingWindow.parkingButtons == null)
                yield break;

            int placeCount = Math.Min(ParkingPlaceLimit,
                parkingManager.parkingSpaces.Length);
            int buttonCount = Math.Min(placeCount, Math.Min(ParkingPlaceLimit,
                parkingWindow.parkingButtons.Length));

            ModLogger.Log("[ParkingPreload] Loading all vehicles in alley " +
                (parkingWindow.parkingLevel + 1) + ".", Types.LoggingLevels.Debug);

            for (int i = 0; i < placeCount; i++) {
                if (!IsWorkerValid(generation))
                    yield break;

                var space = parkingManager.parkingSpaces[i];
                if (space == null || space.carLoader == null ||
                    !space.carLoader.modelLoaded)
                    continue;

                space.carLoader.DeleteCar();
                space.CloseDoor();
            }

            GameScript gameScript = GameScript.Get();
            if (gameScript == null)
                yield break;

            int firstOccupiedButton = -1;
            for (int i = 0; i < buttonCount; i++) {
                if (!IsWorkerValid(generation) || parkingManager == null ||
                    parkingWindow == null)
                    yield break;

                var button = parkingWindow.parkingButtons[i];
                if (button == null || button.saveIndex == -1)
                    continue;

                if (firstOccupiedButton == -1)
                    firstOccupiedButton = i;

                yield return new WaitForFixedUpdate();
                if (!IsWorkerValid(generation))
                    yield break;

                gameScript.StartCoroutine(parkingManager.LoadCarAtPlace(
                    i, button.saveIndex));
            }

            if (firstOccupiedButton == -1 || !IsWorkerValid(generation) ||
                parkingWindow == null)
                yield break;

            var firstButton = parkingWindow.parkingButtons[firstOccupiedButton];
            if (firstButton == null)
                yield break;
            firstButton.OnMouseClick.Invoke(1);
            waitedFrames = 0;
            while (parkingWindow != null &&
                parkingWindow.state != ParkingState.CarInfoVisible &&
                waitedFrames < MaximumWaitFrames) {
                if (!IsWorkerValid(generation))
                    yield break;
                waitedFrames++;
                yield return new WaitForFixedUpdate();
            }

            if (parkingWindow != null &&
                parkingWindow.state != ParkingState.CarInfoVisible) {
                ModLogger.Log("[ParkingPreload] Car information did not become " +
                    "visible within the wait limit.", Types.LoggingLevels.Warning);
            }
        }

        private static void RequestCloseLoadedCars()
        {
            closeRequestVersion++;
            int generation = sceneGeneration;
            if (closeWorkerGeneration == generation)
                return;

            closeWorkerGeneration = generation;
            MelonCoroutines.Start(CloseLoadedCarsWorker(generation));
        }

        private static System.Collections.IEnumerator CloseLoadedCarsWorker(
            int generation)
        {
            int processedVersion = -1;
            try {
                do {
                    processedVersion = closeRequestVersion;
                    yield return new WaitForSeconds(2f);
                } while (IsWorkerValid(generation) &&
                    processedVersion != closeRequestVersion);

                if (!IsWorkerValid(generation))
                    yield break;

                for (int i = 0; i < ParkingPlaceLimit; i++) {
                    if (!IsWorkerValid(generation))
                        yield break;

                    GameObject loaderObject = GameObject.Find("#CarLoader" + i);
                    if (loaderObject == null)
                        continue;

                    CarLoader carLoader = loaderObject.GetComponent<CarLoader>();
                    if (carLoader == null || !carLoader.IsCarLoaded())
                        continue;

                    yield return new WaitForEndOfFrame();
                    if (IsWorkerValid(generation) && carLoader != null)
                        carLoader.CloseCar(true);
                }
            } finally {
                if (closeWorkerGeneration == generation)
                    closeWorkerGeneration = -1;
                if (IsWorkerValid(generation) &&
                    processedVersion != closeRequestVersion)
                    RequestCloseLoadedCars();
            }
        }

        private static bool IsWorkerValid(int generation)
        {
            return generation == sceneGeneration && IsFeatureActive();
        }
    }
}
