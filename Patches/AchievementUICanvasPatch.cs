using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace NeatMenu.Patches
{
    // This patch will be unnecessary once there's a null check in DawnLib here

    [HarmonyPatch]
    public class AchievementUICanvasPatch
    {

       public static bool Prepare()
        {
            return AccessTools.TypeByName("Dusk.AchievementUICanvas") != null;
        }

        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("Dusk.AchievementUICanvas");
            return AccessTools.Method(type, "BackButtonOnClick");
        }

        [HarmonyPrefix]
        public static bool Prefix(object __instance)
        {
            var type = __instance.GetType();

            var menuManagerField = AccessTools.Field(type, "_menuManager");
            var mainButtonsField = AccessTools.Field(type, "_mainButtons");
            var backgroundField = AccessTools.Field(type, "_background");

            var menuManager = menuManagerField?.GetValue(__instance);
            var mainButtons = mainButtonsField?.GetValue(__instance);
            var background = backgroundField?.GetValue(__instance) as GameObject;

            if (menuManager != null && mainButtons != null)
            {
                var enableMethod = AccessTools.Method(menuManager.GetType(), "EnableUIPanel");
                enableMethod?.Invoke(menuManager, new object[] { mainButtons });
            }

            background?.SetActive(false);
            
            return false;
        }
    }
}
