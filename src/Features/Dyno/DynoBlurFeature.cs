using HarmonyLib;
using UnityEngine;

#if NET6_0_OR_GREATER
using Il2CppCMS.UI.Windows;
#else
using CMS.UI.Windows;
#endif

namespace Cms21ImmersionPlus
{
    [HarmonyPatch]
    public static class DynoBlurFeature
    {
        private static bool IsEnabled {
            get {
                return Main.SettingsEntry != null &&
                    Main.SettingsEntry.Value.removeDynoMenuBlur &&
                    GlobalState.IsGarageSceneActive;
            }
        }

        [HarmonyPatch(typeof(DynoWindow), nameof(DynoWindow.Show))]
        [HarmonyPostfix]
        public static void DynoWindowShowPostfix()
        {
            if (!IsEnabled)
                return;

            GameObject background = GameObject.Find("BGMenu");
            if (background != null)
                background.SetActive(false);
        }
    }
}
