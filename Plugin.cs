using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Dawn;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace NeatMenu
{
    [BepInPlugin("nikki.neatmenu", "NeatMenu", "1.0.0")]
    [BepInDependency("me.swipez.melonloader.morecompany", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("LethalPhones", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.github.teamxiaolan.dawnlib", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.github.teamxiaolan.dawnlib.dusk", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("TwitchChatAPI.LethalCompany", BepInDependency.DependencyFlags.SoftDependency)]

    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle Assets;

        public static Sprite phoneButtonSprite;
        public static Sprite configButtonSprite;
        public static Sprite catButtonSprite;
        public static Sprite achievementButtonSprite;
        public static Sprite twitchButtonSprite;

        public static ManualLogSource LogSource;

        public string spriteDirectory = "assets/neatmenu/cat/";

        public static List<Sprite> loadedCats = new List<Sprite>();

        private void Awake()
        {
            LogSource = Logger;

            Log.Info("          ███  ██ ▄▄▄▄▄  ▄▄▄ ▄▄▄▄▄▄   ██▄  ▄██ ▄▄▄▄▄ ▄▄  ▄▄ ▄▄ ▄▄           ");
            Log.Info("▄▀▀▄  █   ██ ▀▄██ ██▄▄  ██▀██  ██     ██ ▀▀ ██ ██▄▄  ███▄██ ██ ██   ▄▀▀▄  █ ");
            Log.Info("▀   ▀▀    ██   ██ ██▄▄▄ ██▀██  ██     ██    ██ ██▄▄▄ ██ ▀██ ▀███▀   ▀   ▀▀");
            Log.Debug("I've always wanted to do this and a menu mod seemed like a good time to.");

            // Loading assets
            var path = Path.Combine(Path.GetDirectoryName(Info.Location), "neatmenuassets");
            Assets = AssetBundle.LoadFromFile(path);
            phoneButtonSprite = Assets.LoadAsset<Sprite>("LethalPhonesIcon");
            configButtonSprite = Assets.LoadAsset<Sprite>("LethalConfigIcon");
            catButtonSprite = Assets.LoadAsset<Sprite>("CatButtonIcon");
            achievementButtonSprite = Assets.LoadAsset<Sprite>("AchievementsIcon");
            twitchButtonSprite = Assets.LoadAsset<Sprite>("TwitchAPIIcon");

            LoadCatSprites();

            // Checking what mods are active
            ModStateChecker.Init();

            if (ModStateChecker.MoreCompanyLoaded)
                Log.Info("Detected MoreCompany.");
            if (ModStateChecker.LethalConfigLoaded)
                Log.Info("Detected LethalConfig.");
            if (ModStateChecker.LethalPhonesLoaded)
                Log.Info("Detected LethalPhones.");
            if (ModStateChecker.DawnLibLoaded)
                Log.Info("Detected DawnLib.");
            if (ModStateChecker.TwitchChatAPILoaded)
                Log.Info("Detected TwitchChatAPI.");

            if (!ModStateChecker.MoreCompanyLoaded)
            {
                Log.Error("MoreCompany was not detected. NeatMenu currently requires MoreCompany for initialization. NeatMenu will not function.");
                return;
            }
            if (!ModStateChecker.LethalConfigLoaded && !ModStateChecker.LethalPhonesLoaded && !ModStateChecker.DawnLibLoaded && !ModStateChecker.TwitchChatAPILoaded)
            {
                Log.Error("No mods with implemented UI changes were found during NeatMenu initialization. Either something has gone wrong, or you have no relevant mods installed, and NeatMenu will not function.");
            }

            // Configs
            Configure();

            // Generate the list of active buttons based on compatible mods
            NeatMenuLayout.GenerateButtonList();

            // Harmony patch
            this.Patch();
        }

        private void LoadCatSprites()
        {
            loadedCats.Clear();

            if (Assets == null)
            {
                Log.Error("Assets not found.");
                return;
            }

            string[] assetNames = Assets.GetAllAssetNames();

            foreach (string assetName in assetNames)
            {
                Log.Debug(assetName);

                if (!assetName.StartsWith(spriteDirectory.ToLower()))
                    continue;

                Sprite sprite = Assets.LoadAsset<Sprite>(assetName);

                if (sprite != null)
                {
                    loadedCats.Add(sprite);
                }
            }

            Log.Debug($"Loaded {loadedCats.Count} cat images.");
        }

        private void Patch()
        {
            Harmony harmony = new Harmony("Nikki.NeatMenu");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void Configure()
        {
            Log.shouldPurgeLogs = Config.Bind<bool>(new ConfigDefinition("Debug", "Purge logs"), false,
                new ConfigDescription("Purges all logging from this mod except for critical errors.")).Value;

            NeatMenuLayout.catButtonEnabled = Config.Bind<bool>(new ConfigDefinition("Main", "Enable cat button"), true,
                new ConfigDescription("Enables the cat button.")).Value;
        }
    }
    public static class Log
    {
        public static bool shouldPurgeLogs;
        public static ManualLogSource Logger = Plugin.LogSource;

        public static void Debug(string msg)
        {
            if (!shouldPurgeLogs)
                Logger.LogDebug(msg);
        }

        public static void Info(string msg)
        {
            if (!shouldPurgeLogs)
                Logger.LogInfo(msg);
        }

        public static void Warning(string msg)
        {
            Logger.LogWarning(msg);
        }

        public static void Error(string msg)
        {
            Logger.LogError(msg);
        }
    }
}
