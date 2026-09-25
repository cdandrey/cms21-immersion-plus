using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cms21ImmersionPlus
{
    internal static class GarageAdvertisingFeature
    {
        private const string SceneName = "garage";
        private const string ResourceSubdirectory = "Environment/GarageAds";

        private static readonly BannerReplacement[] Replacements = {
            new BannerReplacement(
                "!Garage/Garage/Garage_Banners_1/",
                "Garage_Banner_1",
                "Garage_Banner_1_color",
                "GA_WallAtlas_2_4_Dodge_Chevrolet.png"),
            new BannerReplacement(
                "!Garage/Garage/Garage_Banners_1/",
                "Garage_Banner_2",
                "Garage_Banner_2_color",
                "GA_WallAtlas_1_3_MANN_NGK.png"),
            new BannerReplacement(
                "!Garage/Garage/#LifterOn/Garage_Banners_2/",
                "Garage_Banner_4",
                "Garage_Banner_4_color",
                "GA_LifterAtlas_1_Jaguar_2_2_Huayra_1.png"),
            new BannerReplacement(
                "!Garage/Garage/#LifterOn/Garage_Banners_2/",
                "Garage_Banner_3",
                "Garage_Banner_5_color",
                "GA_Lifter_3_Continental_1.png"),
            new BannerReplacement(
                "!Garage/Garage/#LifterOn/Garage_Banners_3",
                "Garage_Banner_6",
                "Garage_Banner_6_color",
                "GA_GaragePaint_4_BuickRiviera71_1_Porsche.png"),
            new BannerReplacement(
                "!Garage/Paintshop/Paintshop_Banner_1",
                "Paintshop_Banner_1",
                "Garage_Banner_6_color",
                "GA_GaragePaint_4_BuickRiviera71_1_Porsche.png"),
            new BannerReplacement(
                "!Garage/EngineRoom/Engine_Room_Banner_1",
                "Engine_Room_Banner_1",
                "Engine_Room_Banner_1_color",
                "GA_EngineRoom_Bosch_ZF.png"),
            new BannerReplacement(
                "!Garage/Garage/Garage_Stuff_33/",
                "Paper_4",
                "Paper_4_color",
                "GA_Garage_Calendars_MagnaFlow_Cadillac_2026.png"),
            new BannerReplacement(
                "!Garage/PathTest/Pathtest_Banner_1",
                "Pathtest_Banner_1",
                "Pathtest_Banner_1",
                "GA_PathTest_Lemforder_GKN.png"),
            new BannerReplacement(
                "!Garage/EngineRoom/Engine_Room_Stuff_5/",
                "Paper_4",
                "Paper_4_color",
                "GA_Garage_Calendars_MagnaFlow_Cadillac_2026.png"),
            new BannerReplacement(
                "!Garage/Exterior/Garage_Exterior_Banner_Front_2",
                "Garage_Exterior_Banner_Front_2",
                "Garage_Exterior_Banner_Front_2_color",
                "GA_Exterior_BFGoodrich_Enkei.png")
        };

        private static readonly Dictionary<string, Texture2D> Textures =
            new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        public static void OnSceneInitialized(string sceneName)
        {
            if (!string.Equals(sceneName, SceneName, StringComparison.Ordinal))
                return;

            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded)
                return;

            if (Main.SettingsEntry == null ||
                !Main.SettingsEntry.Value.loadGarageAdvertising)
                return;

            GameObject root = FindGarageRoot(scene);
            if (root == null)
                return;

            ReloadTextures();
            if (Textures.Count == 0)
                return;

            EnsurePathTestSecondBanner(root);
            int replaced = ApplyBannerReplacements(root);
            EnsureEngineRoomRecaroBanner(root);
            EnsureGaragePirelliBanner(root);
            if (replaced > 0) {
                ModLogger.Log("[GarageAdvertising] Replaced " + replaced +
                    " garage-banner material binding(s).",
                    Types.LoggingLevels.Normal);
            }
        }

        private static GameObject FindGarageRoot(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects()) {
                if (root != null && root.name == "!Garage")
                    return root;
            }
            return null;
        }

        private static void ReloadTextures()
        {
            foreach (Texture2D texture in Textures.Values) {
                if (texture != null)
                    UnityEngine.Object.Destroy(texture);
            }
            Textures.Clear();

            string directory = Path.Combine(
                Path.GetFullPath(GlobalConfig.directoryTextureReplacements),
                ResourceSubdirectory);
            if (!Directory.Exists(directory))
                return;

            HashSet<string> loadedFiles =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (BannerReplacement replacement in Replacements) {
                if (!loadedFiles.Add(replacement.TextureFile))
                    continue;

                string file = Path.Combine(directory, replacement.TextureFile);
                if (!File.Exists(file))
                    continue;

                try {
                    Texture2D texture = new Texture2D(2, 2);
                    if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(file))) {
                        UnityEngine.Object.Destroy(texture);
                        continue;
                    }

                    texture.name = Path.GetFileNameWithoutExtension(file);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.filterMode = FilterMode.Bilinear;
                    Textures[replacement.TextureFile] = texture;
                } catch (Exception exception) {
                    ModLogger.Log("[GarageAdvertising] Failed to load '" + file + "'." +
                        Environment.NewLine + exception,
                        Types.LoggingLevels.Warning);
                }
            }
        }

        private static void EnsurePathTestSecondBanner(GameObject root)
        {
            Transform pathTest = root.transform.Find("PathTest");
            if (pathTest == null || pathTest.Find("CMS21_PathTest_Banner_2") != null)
                return;

            Transform source = pathTest.Find("Pathtest_Banner_1");
            Transform anchor = pathTest.Find("Pathtest_Stuff_4/Pathtest_Stuff_4_LOD0");
            if (source == null || anchor == null)
                return;

            MeshRenderer sourceRenderer = source.GetComponent<MeshRenderer>();
            Renderer anchorRenderer = anchor.GetComponent<Renderer>();
            if (sourceRenderer == null || sourceRenderer.sharedMaterial == null ||
                anchorRenderer == null)
                return;

            Texture2D texture = LoadPathTestTexture("GA_PathTest_Sachs_SKF.png");
            if (texture == null)
                return;

            int propertyId = FindTextureProperty(sourceRenderer.sharedMaterial,
                "Pathtest_Banner_1");
            if (propertyId < 0)
                return;

            Bounds bounds = sourceRenderer.bounds;
            float pixelToWorld = bounds.size.z / 2048.0f;
            float rightOffset = 650.0f * pixelToWorld;

            GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Quad);
            banner.name = "CMS21_PathTest_Banner_2";
            banner.layer = source.gameObject.layer;
            banner.transform.SetParent(pathTest, true);
            banner.transform.position = new Vector3(
                bounds.center.x,
                bounds.center.y,
                anchorRenderer.bounds.center.z - rightOffset);
            banner.transform.rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
            banner.transform.localScale = new Vector3(
                bounds.size.z,
                bounds.size.y,
                1.0f);

            Collider collider = banner.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.Destroy(collider);

            MeshRenderer renderer = banner.GetComponent<MeshRenderer>();
            Material material = new Material(sourceRenderer.sharedMaterial);
            material.name = "CMS21_PathTest_Banner_2_cms21immersionplus";
            material.SetTexture(propertyId, texture);
            if (material.HasProperty("_Cull"))
                material.SetInt("_Cull", 0);
            renderer.sharedMaterial = material;
            renderer.receiveShadows = sourceRenderer.receiveShadows;

            CreatePathTestWallBanner(pathTest, sourceRenderer, propertyId,
                "CMS21_PathTest_Brembo_1", "GA_PathTest_Brembo_1.png",
                new Vector3(-20.30f, 4.41f, -43.45f),
                Quaternion.Euler(0.0f, 180.0f, 0.0f), 4.32f);
            CreatePathTestWallBanner(pathTest, sourceRenderer, propertyId,
                "CMS21_PathTest_Brembo_2", "GA_PathTest_Brembo_2.png",
                new Vector3(-24.95f, 4.41f, -39.37f),
                Quaternion.Euler(0.0f, 270.0f, 0.0f), 4.32f);
            CreatePathTestWallBanner(pathTest, sourceRenderer, propertyId,
                "CMS21_PathTest_SKF_2", "GA_PathTest_SKF_2.png",
                new Vector3(-24.95f, 4.41f, -31.58f),
                Quaternion.Euler(0.0f, 270.0f, 0.0f), 4.32f);
            CreatePathTestWallBanner(pathTest, sourceRenderer, propertyId,
                "CMS21_PathTest_Bilstein", "GA_PathTest_Bilstein.png",
                new Vector3(-21.02f, 5.30f, -26.55f),
                Quaternion.identity, 4.3f);
        }

        private static void CreatePathTestWallBanner(Transform parent,
            MeshRenderer sourceRenderer, int propertyId, string name,
            string textureFile, Vector3 position, Quaternion rotation, float width)
        {
            if (parent.Find(name) != null)
                return;

            Texture2D texture = LoadPathTestTexture(textureFile);
            if (texture == null)
                return;

            GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Quad);
            banner.name = name;
            banner.layer = sourceRenderer.gameObject.layer;
            banner.transform.SetParent(parent, true);
            banner.transform.position = position;
            banner.transform.rotation = rotation;
            banner.transform.localScale = new Vector3(
                width, width * texture.height / texture.width, 1.0f);

            Collider collider = banner.GetComponent<Collider>();
            if (collider != null) {
                collider.enabled = false;
                UnityEngine.Object.Destroy(collider);
            }

            MeshRenderer renderer = banner.GetComponent<MeshRenderer>();
            Material material = new Material(sourceRenderer.sharedMaterial);
            material.name = name + "_cms21immersionplus";
            material.SetTexture(propertyId, texture);
            if (material.HasProperty("_Cull"))
                material.SetInt("_Cull", 0);
            renderer.sharedMaterial = material;
            renderer.receiveShadows = sourceRenderer.receiveShadows;
        }

        private static void EnsureEngineRoomRecaroBanner(GameObject root)
        {
            Transform engineRoom = root.transform.Find("EngineRoom");
            if (engineRoom == null ||
                engineRoom.Find("CMS21_EngineRoom_RECARO") != null)
                return;

            Transform source = engineRoom.Find("Engine_Room_Banner_1");
            if (source == null)
                return;

            MeshRenderer sourceRenderer = source.GetComponent<MeshRenderer>();
            if (sourceRenderer == null || sourceRenderer.sharedMaterial == null)
                return;

            Texture2D texture = LoadPathTestTexture("GA_EngineRoom_RECARO.png");
            if (texture == null)
                return;

            int propertyId = FindTextureProperty(sourceRenderer.sharedMaterial,
                "GA_EngineRoom_Bosch_ZF");
            if (propertyId < 0)
                return;

            GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Quad);
            banner.name = "CMS21_EngineRoom_RECARO";
            banner.layer = source.gameObject.layer;
            banner.transform.SetParent(engineRoom, true);
            banner.transform.position = new Vector3(-19.29f, 2.24f, -10.98f);
            banner.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            banner.transform.localScale = new Vector3(
                1.575f, 1.575f * texture.height / texture.width, 1.0f);

            Collider collider = banner.GetComponent<Collider>();
            if (collider != null) {
                collider.enabled = false;
                UnityEngine.Object.Destroy(collider);
            }

            MeshRenderer renderer = banner.GetComponent<MeshRenderer>();
            Material material = new Material(sourceRenderer.sharedMaterial);
            material.name = "CMS21_EngineRoom_RECARO_cms21immersionplus";
            material.SetTexture(propertyId, texture);
            if (material.HasProperty("_Cull"))
                material.SetInt("_Cull", 0);
            renderer.sharedMaterial = material;
            renderer.receiveShadows = sourceRenderer.receiveShadows;
        }

        private static void EnsureGaragePirelliBanner(GameObject root)
        {
            Transform parent =
                root.transform.Find("Garage/#LifterOn/Garage_Banners_2");
            if (parent == null ||
                parent.Find("CMS21_Garage_Pirelli_Vertical") != null)
                return;

            Transform source = parent.Find("Garage_Banners_2_LOD0");
            if (source == null)
                return;

            MeshRenderer sourceRenderer = source.GetComponent<MeshRenderer>();
            if (sourceRenderer == null || sourceRenderer.sharedMaterials.Length == 0 ||
                sourceRenderer.sharedMaterials[0] == null)
                return;

            Texture2D texture = LoadPathTestTexture("GA_Garage_Pirelli_Vertical.png");
            if (texture == null)
                return;

            Material sourceMaterial = sourceRenderer.sharedMaterials[0];
            int propertyId = FindTextureProperty(sourceMaterial,
                "GA_LifterAtlas_1_Jaguar_2_2_Huayra_1");
            if (propertyId < 0)
                return;

            const float width = 0.99f;
            const float height = width * 2.0f;
            GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Quad);
            banner.name = "CMS21_Garage_Pirelli_Vertical";
            banner.layer = source.gameObject.layer;
            banner.transform.SetParent(parent, true);
            banner.transform.position = new Vector3(-3.21f, 4.74f, 7.72f);
            banner.transform.rotation = Quaternion.identity;
            banner.transform.localScale = new Vector3(width, height, 1.0f);

            Collider collider = banner.GetComponent<Collider>();
            if (collider != null) {
                collider.enabled = false;
                UnityEngine.Object.Destroy(collider);
            }

            MeshRenderer renderer = banner.GetComponent<MeshRenderer>();
            Material material = new Material(sourceMaterial);
            material.name = "CMS21_Garage_Pirelli_Vertical_cms21immersionplus";
            material.SetTexture(propertyId, texture);
            if (material.HasProperty("_Cull"))
                material.SetInt("_Cull", 0);
            renderer.sharedMaterial = material;
            renderer.receiveShadows = sourceRenderer.receiveShadows;
        }

        private static void EnsureEngineRoomCalendarBanner(GameObject root)
        {
            Transform engineRoom = root.transform.Find("EngineRoom");
            if (engineRoom == null ||
                engineRoom.Find("CMS21_EngineRoom_Calendar") != null)
                return;

            Transform source = engineRoom.Find(
                "Engine_Room_Workbench_1_Stuff_B/Engine_Room_Workbench_1_Stuff_B_LOD0");
            if (source == null)
                return;

            MeshRenderer sourceRenderer = source.GetComponent<MeshRenderer>();
            if (sourceRenderer == null)
                return;

            Material sourceMaterial = null;
            foreach (Material material in sourceRenderer.sharedMaterials) {
                if (material != null && material.name == "Paper_2") {
                    sourceMaterial = material;
                    break;
                }
            }
            if (sourceMaterial == null)
                return;

            Texture2D texture = LoadPathTestTexture(
                "GA_Garage_Calendars_MagnaFlow_Cadillac_2026.png");
            if (texture == null)
                return;

            int propertyId = FindTextureProperty(sourceMaterial, "Paper_2_color");
            if (propertyId < 0)
                return;

            GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Quad);
            banner.name = "CMS21_EngineRoom_Calendar";
            banner.layer = source.gameObject.layer;
            banner.transform.SetParent(engineRoom, true);
            banner.transform.position = new Vector3(-24.90f, 1.60f, -5.85f);
            banner.transform.rotation = Quaternion.Euler(0.0f, 270.0f, 0.0f);
            banner.transform.localScale = new Vector3(
                1.62f, 1.62f * texture.height / texture.width, 1.0f);

            Collider collider = banner.GetComponent<Collider>();
            if (collider != null) {
                collider.enabled = false;
                UnityEngine.Object.Destroy(collider);
            }

            MeshRenderer renderer = banner.GetComponent<MeshRenderer>();
            Material replacementMaterial = new Material(sourceMaterial);
            replacementMaterial.name = "CMS21_EngineRoom_Calendar_cms21immersionplus";
            replacementMaterial.SetTexture(propertyId, texture);
            if (replacementMaterial.HasProperty("_Cull"))
                replacementMaterial.SetInt("_Cull", 0);
            renderer.sharedMaterial = replacementMaterial;
            renderer.receiveShadows = sourceRenderer.receiveShadows;
        }

        public static void UpdateDiagnostics()
        {
            if (!Input.GetKeyDown(KeyCode.F8))
                return;

            Scene scene = SceneManager.GetSceneByName(SceneName);
            if (!scene.isLoaded)
                return;

            GameObject root = FindGarageRoot(scene);
            if (root == null)
                return;

            WriteGarageGeometrySnapshot(root);
        }

        private static void WriteGarageGeometrySnapshot(GameObject root)
        {
            try {
                Camera camera = Camera.main;
                if (camera == null)
                    return;

                Vector3 cameraPosition = camera.transform.position;
                string path = GetNextGarageGeometrySnapshotPath();
                using (StreamWriter writer = new StreamWriter(path, false)) {
                    writer.WriteLine("=== Garage geometry snapshot ===");
                    writer.WriteLine("time=" + DateTime.Now.ToString("O"));
                    writer.WriteLine("camera.position=" +
                        PathTestVector(camera.transform.position) +
                        "; rotation=" +
                        PathTestVector(camera.transform.rotation.eulerAngles) +
                        "; forward=" + PathTestVector(camera.transform.forward) +
                        "; right=" + PathTestVector(camera.transform.right) +
                        "; up=" + PathTestVector(camera.transform.up));

                    WriteGarageGeometryRaycast(writer, root, camera.transform,
                        "forward", camera.transform.forward);
                    WriteGarageGeometryRaycast(writer, root, camera.transform,
                        "back", -camera.transform.forward);
                    WriteGarageGeometryRaycast(writer, root, camera.transform,
                        "right", camera.transform.right);
                    WriteGarageGeometryRaycast(writer, root, camera.transform,
                        "left", -camera.transform.right);
                    WriteGarageGeometryRaycast(writer, root, camera.transform,
                        "up", camera.transform.up);
                    WriteGarageGeometryRaycast(writer, root, camera.transform,
                        "down", -camera.transform.up);

                    writer.WriteLine("--- nearby colliders ---");
                    foreach (Collider collider in
                        root.GetComponentsInChildren<Collider>(true)) {
                        if (collider == null ||
                            Vector3.Distance(cameraPosition,
                                collider.bounds.center) > 20.0f)
                            continue;

                        writer.WriteLine(GetTransformPath(collider.transform,
                            root.transform) +
                            "; type=" + collider.GetType().Name +
                            "; layer=" + collider.gameObject.layer +
                            "; enabled=" + collider.enabled +
                            "; trigger=" + collider.isTrigger +
                            "; center=" + PathTestVector(collider.bounds.center) +
                            "; size=" + PathTestVector(collider.bounds.size));
                    }

                    writer.WriteLine("--- nearby renderers ---");
                    foreach (Renderer renderer in
                        root.GetComponentsInChildren<Renderer>(true)) {
                        if (renderer == null ||
                            Vector3.Distance(cameraPosition,
                                renderer.bounds.center) > 20.0f)
                            continue;

                        WriteGarageGeometryRenderer(writer, root.transform, renderer);
                    }
                }

                string textureDirectory = Path.Combine(
                    Path.GetDirectoryName(path),
                    Path.GetFileNameWithoutExtension(path) + "_textures");
                WriteGarageTextureSnapshot(root, cameraPosition, textureDirectory);

                ModLogger.Log("[GarageAdvertising] Geometry snapshot written: " + path +
                    Environment.NewLine +
                    "[GarageAdvertising] Nearby textures written: " +
                    textureDirectory,
                    Types.LoggingLevels.Normal);
            } catch (Exception exception) {
                ModLogger.Log("[GarageAdvertising] Geometry snapshot failed." +
                    Environment.NewLine + exception,
                    Types.LoggingLevels.Warning);
            }
        }

        private static string GetNextGarageGeometrySnapshotPath()
        {
            string directory = Path.GetFullPath(@"Mods\CMS21ImmersionPlus");
            for (int index = 1; ; index++) {
                string path = Path.Combine(directory,
                    "GarageGeometry_" + index.ToString("D3") + ".log");
                if (!File.Exists(path))
                    return path;
            }
        }

        private static void WriteGarageGeometryRaycast(StreamWriter writer,
            GameObject root, Transform camera, string name, Vector3 direction)
        {
            RaycastHit hit;
            if (!Physics.Raycast(camera.position, direction, out hit, 50.0f)) {
                writer.WriteLine("raycast." + name + "=no hit");
                return;
            }

            writer.WriteLine("raycast." + name +
                "; point=" + PathTestVector(hit.point) +
                "; normal=" + PathTestVector(hit.normal) +
                "; distance=" + hit.distance.ToString("F4") +
                "; collider=" + GetTransformPath(hit.transform, root.transform));
        }

        private static void WriteGarageGeometryRenderer(StreamWriter writer,
            Transform root, Renderer renderer)
        {
            Transform transform = renderer.transform;
            Bounds bounds = renderer.bounds;
            writer.WriteLine(GetTransformPath(transform, root) +
                "; type=" + renderer.GetType().Name +
                "; layer=" + renderer.gameObject.layer +
                "; activeSelf=" + renderer.gameObject.activeSelf +
                "; activeInHierarchy=" + renderer.gameObject.activeInHierarchy +
                "; enabled=" + renderer.enabled +
                "; staticBatch=" + renderer.isPartOfStaticBatch +
                "; position=" + PathTestVector(transform.position) +
                "; rotation=" + PathTestVector(transform.rotation.eulerAngles) +
                "; scale=" + PathTestVector(transform.localScale) +
                "; boundsCenter=" + PathTestVector(bounds.center) +
                "; boundsSize=" + PathTestVector(bounds.size));

            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null) {
                writer.WriteLine("  mesh=" + filter.sharedMesh.name +
                    "; vertices=" + filter.sharedMesh.vertexCount +
                    "; subMeshes=" + filter.sharedMesh.subMeshCount);
            }

            Material[] materials = renderer.sharedMaterials;
            for (int materialIndex = 0; materialIndex < materials.Length;
                materialIndex++) {
                Material material = materials[materialIndex];
                if (material == null)
                    continue;

                writer.WriteLine("  material[" + materialIndex + "]=" +
                    material.name + "; shader=" +
                    (material.shader != null ? material.shader.name : "<null>"));
                foreach (int propertyId in material.GetTexturePropertyNameIDs()) {
                    Texture texture = material.GetTexture(propertyId);
                    if (texture == null)
                        continue;

                    writer.WriteLine("    texture propertyId=" + propertyId +
                        "; name=" + texture.name +
                        "; type=" + texture.GetType().Name +
                        "; size=" + texture.width + "x" + texture.height);
                }
            }
        }

        private static void WriteGarageTextureSnapshot(GameObject root,
            Vector3 cameraPosition, string directory)
        {
            Directory.CreateDirectory(directory);
            HashSet<int> savedTextures = new HashSet<int>();

            using (StreamWriter writer = new StreamWriter(
                Path.Combine(directory, "Textures.log"), false)) {
                foreach (Renderer renderer in
                    root.GetComponentsInChildren<Renderer>(true)) {
                    if (renderer == null ||
                        !renderer.gameObject.activeInHierarchy ||
                        Vector3.Distance(cameraPosition,
                            renderer.bounds.center) > 20.0f)
                        continue;

                    foreach (Material material in renderer.sharedMaterials) {
                        if (material == null || !material.HasProperty(4))
                            continue;

                        Texture texture = material.GetTexture(4);
                        if (texture == null ||
                            !savedTextures.Add(texture.GetInstanceID()))
                            continue;

                        string fileName =
                            SanitizeTextureFileName(texture.name) + ".png";
                        string filePath = Path.Combine(directory, fileName);
                        if (!SaveTextureAsPng(texture, filePath))
                            continue;

                        writer.WriteLine(fileName + "; texture=" + texture.name +
                            "; material=" + material.name +
                            "; renderer=" + GetTransformPath(renderer.transform,
                                root.transform));
                    }
                }
            }
        }

        private static bool SaveTextureAsPng(Texture texture, string path)
        {
            if (texture.width <= 0 || texture.height <= 0)
                return false;

            RenderTexture temporary = RenderTexture.GetTemporary(
                texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Texture2D copy = null;
            try {
                Graphics.Blit(texture, temporary);
                RenderTexture.active = temporary;
                copy = new Texture2D(texture.width, texture.height,
                    TextureFormat.RGBA32, false);
                copy.ReadPixels(new Rect(0.0f, 0.0f,
                    texture.width, texture.height), 0, 0);
                copy.Apply();
                File.WriteAllBytes(path, ImageConversion.EncodeToPNG(copy));
                return true;
            } catch {
                return false;
            } finally {
                RenderTexture.active = previous;
                if (copy != null)
                    UnityEngine.Object.Destroy(copy);
                RenderTexture.ReleaseTemporary(temporary);
            }
        }

        private static string SanitizeTextureFileName(string name)
        {
            foreach (char character in Path.GetInvalidFileNameChars())
                name = name.Replace(character, '_');
            return string.IsNullOrEmpty(name) ? "texture" : name;
        }

        private static string PathTestVector(Vector3 value)
        {
            return value.x.ToString("F4") + "," +
                value.y.ToString("F4") + "," +
                value.z.ToString("F4");
        }

        private static Texture2D LoadPathTestTexture(string textureFile)
        {
            Texture2D texture;
            if (Textures.TryGetValue(textureFile, out texture))
                return texture;

            string file = Path.Combine(
                Path.GetFullPath(GlobalConfig.directoryTextureReplacements),
                ResourceSubdirectory, textureFile);
            if (!File.Exists(file))
                return null;

            texture = new Texture2D(2, 2);
            if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(file))) {
                UnityEngine.Object.Destroy(texture);
                return null;
            }

            texture.name = Path.GetFileNameWithoutExtension(textureFile);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            Textures[textureFile] = texture;
            return texture;
        }

        private static int ApplyBannerReplacements(GameObject root)
        {
            int replaced = 0;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true)) {
                if (renderer == null)
                    continue;

                string path = GetTransformPath(renderer.transform, root.transform);
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int materialIndex = 0; materialIndex < materials.Length;
                    materialIndex++) {
                    Material material = materials[materialIndex];
                    if (material == null)
                        continue;

                    BannerReplacement replacement = FindReplacement(path, material.name);
                    if (replacement == null)
                        continue;

                    Texture2D texture;
                    if (!Textures.TryGetValue(replacement.TextureFile, out texture))
                        continue;

                    int propertyId = FindTextureProperty(material,
                        replacement.SourceTextureName);
                    if (propertyId < 0)
                        continue;

                    Material replacementMaterial = new Material(material);
                    replacementMaterial.name = material.name + "_cms21immersionplus";
                    replacementMaterial.SetTexture(propertyId, texture);
                    materials[materialIndex] = replacementMaterial;
                    changed = true;
                    replaced++;
                }

                if (changed)
                    renderer.sharedMaterials = materials;
            }

            return replaced;
        }

        private static BannerReplacement FindReplacement(string path,
            string materialName)
        {
            foreach (BannerReplacement replacement in Replacements) {
                if (!path.StartsWith(replacement.TargetPathPrefix,
                    StringComparison.Ordinal))
                    continue;
                if (string.Equals(replacement.MaterialName, materialName,
                    StringComparison.Ordinal))
                    return replacement;
            }
            return null;
        }

        private static int FindTextureProperty(Material material,
            string sourceTextureName)
        {
            foreach (int propertyId in material.GetTexturePropertyNameIDs()) {
                Texture texture = material.GetTexture(propertyId);
                if (texture != null && string.Equals(texture.name, sourceTextureName,
                    StringComparison.Ordinal))
                    return propertyId;
            }
            return -1;
        }

        private static string GetTransformPath(Transform transform, Transform root)
        {
            string path = transform.name;
            while (transform != root && transform.parent != null) {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        private sealed class BannerReplacement
        {
            public BannerReplacement(string targetPathPrefix, string materialName,
                string sourceTextureName, string textureFile)
            {
                TargetPathPrefix = targetPathPrefix;
                MaterialName = materialName;
                SourceTextureName = sourceTextureName;
                TextureFile = textureFile;
            }

            public string TargetPathPrefix { get; private set; }

            public string MaterialName { get; private set; }

            public string SourceTextureName { get; private set; }

            public string TextureFile { get; private set; }
        }
    }
}
