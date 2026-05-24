using HarmonyLib;

namespace NeatMenu
{
    public static class ModStateChecker
    {
        // the MoreCompany check is currently unnecessary however I may decouple this mod from MoreCompany at a later date
        public static bool MoreCompanyLoaded;
        public static bool LethalConfigLoaded;
        public static bool LethalPhonesLoaded;
        public static bool DawnLibLoaded;
        public static bool TwitchChatAPILoaded;

        public static void Init()
        {
            MoreCompanyLoaded =
                BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("me.swipez.melonloader.morecompany");
            LethalConfigLoaded =
                BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig");
            LethalPhonesLoaded =
                BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("LethalPhones");
            DawnLibLoaded =
                BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.github.teamxiaolan.dawnlib");
            TwitchChatAPILoaded =
                BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("TwitchChatAPI.LethalCompany");
        }

        public static bool CheckAchievementStatus()
        {
            var modContentType = AccessTools.TypeByName("Dusk.DuskModContent");
            var achievementsField = AccessTools.Field(modContentType, "Achievements");
            var achievements = achievementsField.GetValue(null);

            int count = (int)AccessTools.Property(achievements.GetType(), "Count").GetValue(achievements);
            bool noAchievements = count == 0;

            var type = AccessTools.TypeByName("Dawn.Internal.DawnConfig");
            var field = AccessTools.Field(type, "DisableAchievementsButton");
            var entry = field.GetValue(null) as BepInEx.Configuration.ConfigEntry<bool>;

            bool disabled = entry?.Value ?? false;

            if (disabled || noAchievements)
            {
                Log.Debug("Removing DawnLib Achievements button due to either config or Achievement Count.");
                return false;
            }

            return true;
        }
    }
}
