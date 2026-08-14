using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

#if NET6_0_OR_GREATER
using Il2CppCMS.UI.Logic;
using Il2CppCMS.UI.Logic.Shop;
#else
using CMS.UI.Logic;
using CMS.UI.Logic.Shop;
#endif

namespace Cms21ImmersionPlus
{
    internal static class ShopRebrandingFeature
    {
        private static readonly Dictionary<ShopType, string> LogoFiles =
            new Dictionary<ShopType, string> {
                { ShopType.Main, "SB_NAPA.png" },
                { ShopType.Body, "SB_LKQ.png" },
                { ShopType.Interior, "SB_MOMO.png" },
                { ShopType.Tire, "SB_BFGoodrich.png" },
                { ShopType.LicensePlate, "SB_LicensePlatesTV.png" },
                { ShopType.Tuning, "SB_SummitRacing.png" },
                { ShopType.BodyTuning, "SB_MaxtonDesign.png" },
                { ShopType.Rims, "SB_BBS.png" },
                { ShopType.Gearbox, "SB_TREMEC.png" },
                { ShopType.Electronics, "SB_Bosch.png" },
                { ShopType.Community, "SB_SteamWorkshop.png" },
                { ShopType.Addons, "SB_NexusMods.png" },
            };

        private static readonly Dictionary<ShopType, Sprite> LogoSprites =
            new Dictionary<ShopType, Sprite>();
        private static bool logosLoaded;

        private static void Apply(HomePage homePage)
        {
            if (!Main.SettingsEntry.Value.rebrandShops || homePage == null ||
                homePage.shopAvatars == null)
                return;

            EnsureLogosLoaded();
            if (LogoSprites.Count == 0)
                return;

            foreach (ShopAvatar avatar in homePage.shopAvatars) {
                if (avatar == null || !LogoSprites.TryGetValue(avatar.ShopType, out Sprite sprite))
                    continue;

                Image background = avatar.GetComponent<Image>();
                if (background != null) {
                    Color backgroundColor = background.color;
                    backgroundColor.a = 0f;
                    background.color = backgroundColor;
                }

                Transform imageTransform = avatar.transform.Find("Img");
                Image image = imageTransform != null ? imageTransform.GetComponent<Image>() : null;
                if (image != null) {
                    image.sprite = sprite;
                    image.preserveAspect = true;
                }
            }
        }

        private static void EnsureLogosLoaded()
        {
            if (logosLoaded)
                return;

            logosLoaded = true;
            foreach (KeyValuePair<ShopType, string> entry in LogoFiles) {
                string path = Path.Combine(GlobalConfig.directoryShopBrand, entry.Value);
                Sprite sprite = LoadLogoSprite(path);
                if (sprite != null)
                    LogoSprites.Add(entry.Key, sprite);
            }
        }

        private static Sprite LoadLogoSprite(string path)
        {
            try {
                if (!File.Exists(path)) {
                    ModLogger.Log("[ShopRebranding] Logo file is absent: " + path,
                        Types.LoggingLevels.Warning);
                    return null;
                }

                Texture2D texture = new Texture2D(2, 2);
                texture.name = Path.GetFileNameWithoutExtension(path) + "_cms21immersionplus";
                if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(path))) {
                    UnityEngine.Object.Destroy(texture);
                    ModLogger.Log("[ShopRebranding] Failed to decode logo: " + path,
                        Types.LoggingLevels.Warning);
                    return null;
                }

                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = FilterMode.Bilinear;
                Sprite sprite = Sprite.Create(texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.FullRect);
                sprite.name = texture.name;
                return sprite;
            } catch (Exception exception) {
                ModLogger.Log("[ShopRebranding] Failed to load logo '" + path + "'." +
                    Environment.NewLine + exception, Types.LoggingLevels.Warning);
                return null;
            }
        }

        [HarmonyPatch]
        private static class HomePageOpenPatch
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(HomePage), "Open", Type.EmptyTypes);
            }

            [HarmonyPostfix]
            private static void Postfix(HomePage __instance)
            {
                Apply(__instance);
            }
        }
    }
}
