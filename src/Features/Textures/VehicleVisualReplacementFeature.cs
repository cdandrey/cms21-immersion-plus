using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

#if NET6_0_OR_GREATER
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppCMS.Containers;
using Il2CppCMS.SceneLoaders;
#else
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using CMS;
using CMS.Containers;
using CMS.SceneLoaders;
#endif

namespace Cms21ImmersionPlus
{
    /// <summary>Loads local visual replacements for vehicle brands, models and related thumbnails from module-owned folders.</summary>
    [HarmonyPatch]
    public static class VehicleVisualReplacementFeature
    {
        private const int MaximumWaitFrames = 600;

        private static readonly string[] ByteSizeSuffixes = {
            "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"
        };

        private static readonly Dictionary<string, string> RuntimeTextureNames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "CallopeInterior", "int_callope" },
                { "LavetinoInteriorDiffuse", "lavetino_int_d" }
            };

        private static readonly Dictionary<string, string> RuntimeBrandNames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "FiatLegacy", "fiat_old" },
                { "FordLegacy", "ford_old" }
            };

        private static Dictionary<string, byte[]> carLoaderTextures;
        private static bool carLoaderCacheComplete;

        private static bool IsEnabled {
            get {
                return Main.SettingsEntry != null &&
                    Main.SettingsEntry.Value.loadVehicleVisualReplacements;
            }
        }

        public static void OnSceneLoaded(string sceneName)
        {
            if (IsEnabled && sceneName == "LoadResources")
                LoadBrandLogosFromFolder();
        }

        public static bool OnGameDataReady()
        {
            if (!IsEnabled)
                return true;

            string directory = Path.Combine(
                Path.GetFullPath(GlobalConfig.directoryTextureReplacements),
                "CarLoader");
            if (!EnsureCarLoaderCache(directory))
                return false;

            ReplacePartThumbnails();
            return true;
        }

        [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.LoadAndPrepareModel))]
        [HarmonyPrefix]
        public static void LoadAndPrepareModelPrefix(CarLoader __instance)
        {
            if (IsEnabled && __instance != null)
                MelonCoroutines.Start(ApplyCarLoaderTextures(__instance));
        }

        private static IEnumerator ApplyCarLoaderTextures(CarLoader loader)
        {
            int waitedFrames = 0;
            while (loader != null && (!loader.done || !loader.modelLoaded) &&
                waitedFrames < MaximumWaitFrames) {
                waitedFrames++;
                yield return new WaitForEndOfFrame();
            }

            if (loader == null)
                yield break;
            if (!loader.done || !loader.modelLoaded)
                yield break;

            yield return ReplaceCarLoaderTextures(loader);
        }

        private static IEnumerator ReplaceCarLoaderTextures(CarLoader loader)
        {
            string directory = Path.Combine(
                Path.GetFullPath(GlobalConfig.directoryTextureReplacements),
                "CarLoader");
            if (!EnsureCarLoaderCache(directory) || carLoaderTextures.Count == 0 ||
                loader == null)
                yield break;

            int waitedFrames = 0;
            while (SceneLoader.blockProgress && waitedFrames < MaximumWaitFrames) {
                if (loader == null)
                    yield break;
                waitedFrames++;
                yield return new WaitForEndOfFrame();
            }
            if (loader == null || SceneLoader.blockProgress)
                yield break;

            GameObject model = loader.GetModel();
            if (model == null)
                yield break;

            ReplaceTexturesOnObject(model, carLoaderTextures);
        }

        private static bool EnsureCarLoaderCache(string directory)
        {
            if (carLoaderCacheComplete)
                return true;
            if (!Directory.Exists(directory)) {
                ModLogger.Log("[Textures] CarLoader texture directory is absent: " +
                    directory, Types.LoggingLevels.Warning);
                carLoaderCacheComplete = true;
                carLoaderTextures = new Dictionary<string, byte[]>(
                    StringComparer.Ordinal);
                return true;
            }

            Dictionary<string, byte[]> loaded =
                LoadTextureFiles(directory, "[Textures] Failed to cache");
            if (Directory.GetFiles(directory, "*.png").Length > 0 && loaded.Count == 0)
                return false;

            carLoaderTextures = loaded;
            carLoaderCacheComplete = true;
            if (loaded.Count > 0) {
                long bytes = loaded.Sum(item => (long)item.Value.Length);
                ModLogger.Log("[Textures] Cached " + loaded.Count +
                    " CarLoader texture(s), " + FormatByteSize(bytes, 2) + ".",
                    Types.LoggingLevels.Normal);
            }
            return true;
        }

        private static Dictionary<string, byte[]> LoadTextureFiles(string directory,
            string errorPrefix)
        {
            Dictionary<string, byte[]> result =
                new Dictionary<string, byte[]>(StringComparer.Ordinal);
            foreach (string file in Directory.GetFiles(directory, "*.png")) {
                try {
                    result[GetRuntimeTextureName(file)] = File.ReadAllBytes(file);
                } catch (Exception exception) {
                    ModLogger.Log(errorPrefix + " '" + file + "'." +
                        Environment.NewLine + exception,
                        Types.LoggingLevels.Warning);
                }
            }
            return result;
        }

        private static void ReplacePartThumbnails()
        {
            GameInventory inventory = Singleton<GameInventory>.Instance;
            if (inventory == null)
                return;

            int replaced = 0;
            foreach (KeyValuePair<string, byte[]> data in carLoaderTextures) {
                if (!inventory.Thumbnails.ContainsKey(data.Key))
                    continue;

                Sprite sprite = TextureLoader.LoadSpriteFromBytes(data.Value);
                if (sprite == null)
                    continue;
                sprite.name = data.Key + "_cms21immersionplus";
                sprite.texture.name = sprite.name;
                inventory.Thumbnails[data.Key] = sprite;
                replaced++;
            }

            if (replaced > 0)
                ModLogger.Log("[Textures] Replaced " + replaced +
                    " part thumbnail(s).", Types.LoggingLevels.Normal);
        }

        private static int ReplaceTexturesOnObject(GameObject gameObject,
            Dictionary<string, byte[]> replacements)
        {
            if (gameObject == null || replacements == null ||
                replacements.Count == 0)
                return 0;

            int replaced = 0;
            foreach (Renderer renderer in
                gameObject.GetComponentsInChildren<Renderer>(true)) {
                foreach (Material material in renderer.sharedMaterials)
                    replaced += ReplaceTexturesOnMaterial(material, replacements);
            }
            return replaced;
        }

        private static int ReplaceTexturesOnMaterial(Material material,
            Dictionary<string, byte[]> replacements)
        {
            if (material == null || replacements == null)
                return 0;

            int replaced = 0;
            foreach (int propertyId in material.GetTexturePropertyNameIDs()) {
                Texture texture = material.GetTexture(propertyId);
                if (texture == null)
                    continue;

                byte[] replacement;
                if (!replacements.TryGetValue(texture.name, out replacement))
                    continue;

                Texture2D texture2D = texture.TryCast<Texture2D>();
                if (texture2D != null &&
                    ImageConversion.LoadImage(texture2D, replacement)) {
                    texture.name += "_cms21immersionplus";
                    replaced++;
                }
            }
            return replaced;
        }

        private static void LoadBrandLogosFromFolder()
        {
            string directory = Path.GetFullPath(GlobalConfig.directoryCarBrand);
            if (!Directory.Exists(directory)) {
                ModLogger.Debug("[Textures] Brand-logo directory is absent: " + directory);
                return;
            }

            Dictionary<string, byte[]> logos =
                new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            foreach (string file in Directory.GetFiles(directory, "*.png")) {
                try {
                    logos[GetRuntimeBrandName(file)] = File.ReadAllBytes(file);
                } catch (Exception exception) {
                    ModLogger.Log("[Textures] Failed to read brand logo '" + file +
                        "'." + Environment.NewLine + exception,
                        Types.LoggingLevels.Warning);
                }
            }
            if (logos.Count == 0)
                return;

            Il2CppReferenceArray<UnityEngine.Object> inventories =
                Resources.FindObjectsOfTypeAll(Il2CppType.Of<GameInventory>());
            foreach (UnityEngine.Object loaded in inventories) {
                GameInventory inventory = loaded.TryCast<GameInventory>();
                if (inventory == null)
                    continue;

                int replaced = 0;
                int added = 0;
                HashSet<string> used =
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                for (int i = 0; i < inventory.Brands.Count; i++) {
                    SpriteContainer existing = inventory.Brands[i];
                    if (existing == null)
                        continue;

                    byte[] imageData;
                    if (!logos.TryGetValue(existing.name, out imageData))
                        continue;

                    Sprite sprite = TextureLoader.LoadSpriteFromBytes(imageData);
                    if (sprite == null)
                        continue;
                    sprite.texture.name = existing.name;
                    inventory.Brands.RemoveAt(i);
                    inventory.Brands.Insert(i, new SpriteContainer {
                        name = existing.name,
                        sprite = sprite
                    });
                    used.Add(existing.name);
                    replaced++;
                }

                foreach (KeyValuePair<string, byte[]> logo in logos) {
                    if (used.Contains(logo.Key))
                        continue;
                    Sprite sprite = TextureLoader.LoadSpriteFromBytes(logo.Value);
                    if (sprite == null)
                        continue;
                    sprite.texture.name = logo.Key;
                    inventory.Brands.Add(new SpriteContainer {
                        name = logo.Key,
                        sprite = sprite
                    });
                    added++;
                }

                inventory.ReloadBrands(inventory.Brands);
                if (loaded.name != "GameInventory(Clone)") {
                    ModLogger.Log("[Textures] Brand logos: replaced=" + replaced +
                        ", added=" + added + ".", Types.LoggingLevels.Normal);
                }
            }
        }

        private static string GetRuntimeTextureName(string file)
        {
            string projectName = Path.GetFileNameWithoutExtension(file);
            if (projectName.StartsWith("CB_", StringComparison.OrdinalIgnoreCase))
                projectName = projectName.Substring(3);

            string runtimeName;
            return RuntimeTextureNames.TryGetValue(projectName, out runtimeName)
                ? runtimeName
                : projectName;
        }

        private static string GetRuntimeBrandName(string file)
        {
            string projectName = Path.GetFileNameWithoutExtension(file);
            string runtimeName;
            return RuntimeBrandNames.TryGetValue(projectName, out runtimeName)
                ? runtimeName
                : projectName.ToLowerInvariant();
        }

        private static string FormatByteSize(long value, int decimalPlaces)
        {
            if (value < 0)
                throw new ArgumentException("Bytes should not be negative", "value");
            if (value == 0)
                return "0 bytes";

            int magnitude = (int)Math.Min(ByteSizeSuffixes.Length - 1,
                Math.Floor(Math.Log(value, 1024)));
            double adjusted = Math.Round(value / Math.Pow(1024, magnitude),
                decimalPlaces);
            return adjusted + " " + ByteSizeSuffixes[magnitude];
        }
    }
}
