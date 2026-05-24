using HarmonyLib;
using NeatMenu.MonoBehaviours;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NeatMenu.Patches
{
    [HarmonyPatch(typeof(MenuManager), "Start")]
    [HarmonyPriority(Priority.Last)]
    public static class MenuPatches
    {
        public static GameObject cosmeticButton;

        public static GameObject cosmeticScreen;
        public static GameObject menuContainer;

        public static GameObject phoneRoot;
        public static GameObject phoneMenu;

        public static GameObject neatMenuButtonsContainer;

        public static bool achievementStatus;

        [HarmonyPostfix]
        public static void Postfix(MenuManager __instance)
        {
            menuContainer = GameObject.Find("Canvas/MenuContainer");
            if (menuContainer == null)
            {
                Log.Error("MenuContainer not found. Another mod may be interfering with the main menu.");
                return;
            }

            // handles MoreCompany's button to integrate it and deal with later layering issues
            if (ModStateChecker.MoreCompanyLoaded)
                HandleMoreCompany();
            // handle phones first so that phones can be rightmost button
            if (ModStateChecker.LethalPhonesLoaded)
                HandleLethalPhones(__instance);
            // handle config after
            if (ModStateChecker.LethalConfigLoaded)
                HandleLethalConfig(__instance);
            if (ModStateChecker.DawnLibLoaded)
                HandleDawnLib(__instance);
            if (ModStateChecker.TwitchChatAPILoaded)
                HandleTwitchChatAPI(__instance);

            __instance.StartCoroutine(GenerateButtonLayout());

            if (cosmeticButton == null)
            {
                return;
            }
            // just gonna slide the old button off-screen now
            cosmeticButton.transform.localPosition += new Vector3(5000f, 0f, 0f);
        }

        public static void HandleMoreCompany()
        {
            cosmeticButton = GameObject.Find("TestOverlay(Clone)/Canvas/GlobalScale/ActivateButton");
            if (cosmeticButton == null)
            {
                return;
            }

            var buttonOld = cosmeticButton.GetComponent<Button>();
            buttonOld.onClick = new Button.ButtonClickedEvent();
        }

        public static void HandleLethalPhones(MenuManager __instance)
        {
            if (ModStateChecker.MoreCompanyLoaded)
            {
                if (cosmeticButton == null)
                {
                    Log.Debug("HandleLethalPhones skipped (likely loading screen / UI not initialized yet).");
                    return;
                }

                __instance.StartCoroutine(WaitToRemoveMenuButtons(__instance, true));
            }
        }

        public static void HandleLethalConfig(MenuManager __instance)
        {
            if (ModStateChecker.MoreCompanyLoaded)
            {
                if (cosmeticButton == null)
                {
                    Log.Debug("HandleLethalConfig skipped (likely loading screen / UI not initialized yet).");
                    return;
                }

                __instance.StartCoroutine(WaitToRemoveMenuButtons(__instance, true));
            }
        }

        public static void HandleDawnLib(MenuManager __instance)
        {
            if (ModStateChecker.MoreCompanyLoaded)
            {
                if (cosmeticButton == null)
                {
                    Log.Debug("HandleDawnLib skipped (likely loading screen / UI not initialized yet).");
                    return;
                }

                achievementStatus = ModStateChecker.CheckAchievementStatus();

                // attempt to find the Achievement button and only refactor if it exists
                __instance.StartCoroutine(
                    WaitToRemoveMenuButtons(__instance, achievementStatus));
            }
        }

        public static void HandleTwitchChatAPI(MenuManager __instance)
        {
            // Crit doesn't actually modify the vanilla menu, so we just need to slide his old Twitch button stage right, which we'll do in RemoveMenuButtons
            __instance.StartCoroutine(WaitToRemoveMenuButtons(__instance, false));
        }

        public static IEnumerator WaitToRemoveMenuButtons(MenuManager __instance, bool refactor)
        {
            yield return new WaitForSeconds(0);

            RemoveMenuButtons(__instance, refactor);
        }

        public static void RemoveMenuButtons(MenuManager __instance, bool refactor)
        {
            var parent = GameObject.Find("Canvas/MenuContainer/MainButtons");
            if (parent == null)
            {
                Log.Warning("Menu buttons not found.");
                return;
            }

            // we also need to remove the Achievements button if this doesn't show up, as it means no DawnLib achievements exist and DawnLib is removing the button
            if (!achievementStatus)
            {
                NeatMenuLayout.activeButtons.Remove(MenuButtonType.DawnLibAchievements);
            }

            var buttons = parent.GetComponentsInChildren<Button>();
            foreach (var button in buttons)
            {
                var text = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();

                if (text == null) continue;

                if (text.text == "> Personalize Phone" || text.text == "> LethalConfig" || text.text == "> Achievements")
                {
                    Object.Destroy(button.gameObject);
                    Log.Info($"Destroying menu button: {text.text}");
                }
            }

            var twitchChatButton = GameObject.Find("TwitchChatAPI LethalCompany Canvas/MainMenu");
            if (twitchChatButton != null)
            {
                twitchChatButton.transform.localPosition += new Vector3(5000f, 0f, 0f);
            }

            if (!refactor)
                return;
            __instance.StartCoroutine(RefactorButtons(buttons));
        }

        public static IEnumerator RefactorButtons(Button[] buttons)
        {
            // Wait so objects are fully destroyed
            yield return new WaitForSeconds(0);

            var gameObjects = buttons.ToList();
            gameObjects.RemoveAll(x => x == null);
            var positions = gameObjects
                .Select(b => b.transform as RectTransform)
                .Select(t => t!.anchoredPosition.y);
            var enumerable = positions.ToList();
            var offsets = enumerable
                .Zip(enumerable.Skip(1), (y1, y2) => Mathf.Abs(y2 - y1));
            var offset = offsets.Min();

            var quitButton = gameObjects.Find(q => q.gameObject.name == "QuitButton");

            foreach (var button in gameObjects.Where(g => g != quitButton))
                button.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, offset);
        }

        public static IEnumerator GenerateButtonLayout()
        {
            yield return new WaitForSeconds(0);
            yield return new WaitForSeconds(0);

            List<MenuButtonType> activeButtons = NeatMenuLayout.activeButtons;

            Vector3[] positions =
                NeatMenuLayout.GetPositions(activeButtons.Count);

            neatMenuButtonsContainer = Object.Instantiate(new GameObject("NeatMenuButtons"), menuContainer.transform);
            neatMenuButtonsContainer.transform.SetSiblingIndex(3);
            neatMenuButtonsContainer.AddComponent<MenuMimic>();

            for (int i = 0; i < activeButtons.Count; i++)
            {
                MenuButtonType buttonType = activeButtons[i];

                GameObject buttonObj =
                    CreateButton(buttonType);

                if (buttonObj == null)
                {
                    Log.Debug("Failed to generate button layout. This is probably fine (likely loading screen / UI not initialized yet).");
                    break;
                }

                buttonObj.transform.localScale =
                    NeatMenuLayout.AnchorScale;
                buttonObj.transform.localPosition =
                    NeatMenuLayout.AnchorPosition + positions[i];
            }
        }

        public static GameObject CreateButton(MenuButtonType buttonType)
        {
            if (cosmeticButton == null || neatMenuButtonsContainer == null)
            {
                return null;
            }

            GameObject buttonObj =
                Object.Instantiate(cosmeticButton, neatMenuButtonsContainer.transform);

            var button = buttonObj.GetComponent<Button>();
            var image = buttonObj.GetComponent<Image>();

            switch (buttonType)
            {
                case MenuButtonType.MoreCompany:

                    buttonObj.name = "NeatMenuCosmeticButton";

                    button.onClick.AddListener(() =>
                    {
                        Log.Debug("NeatMenuCosmeticButton clicked!");
                        if (cosmeticScreen == null)
                        {
                            cosmeticScreen = GameObject.Find("TestOverlay(Clone)/Canvas/GlobalScale/CosmeticsScreen");
                        }
                        if (cosmeticScreen == null)
                        {
                            Log.Error("CosmeticScreen not found.");
                            return;
                        }
                        cosmeticScreen.SetActive(true);
                    });

                    break;

                case MenuButtonType.LethalPhones:

                    buttonObj.name = "NeatMenuPhoneButton";

                    button.onClick.AddListener(() =>
                    {
                        if (phoneRoot == null)
                        {
                            phoneRoot = GameObject.Find("CustomizationCanvas");
                            if (phoneRoot == null)
                            {
                                Log.Error("CustomizationCanvas not found.");
                                return;
                            }
                            phoneMenu = phoneRoot.transform.Find("CustomizationPanel")?.gameObject;
                        }

                        Log.Debug("NeatMenuPhoneButton clicked!");
                        if (phoneMenu == null)
                        {
                            Log.Error("CustomizationPanel not found.");
                            return;
                        }
                        phoneMenu.SetActive(true);
                    });

                    image.color = new Color(0.67f, 0.67f, 0.01f);
                    image.sprite = Plugin.phoneButtonSprite;

                    break;

                case MenuButtonType.LethalConfig:

                    buttonObj.name = "NeatMenuConfigButton";

                    button.onClick.AddListener(() =>
                    {
                        Log.Debug("NeatMenuConfigButton clicked!");
                        var configMethod = AccessTools.Method("LethalConfig.MonoBehaviours.Managers.ConfigMenuManager:ShowConfigMenu");
                        configMethod?.Invoke(null, null);
                    });

                    image.color = new Color(0.67f, 0.01f, 0.01f);
                    image.sprite = Plugin.configButtonSprite;

                    break;

                case MenuButtonType.CatButton:

                    buttonObj.name = "NeatMenuCatButton";

                    CatButtonController controller = buttonObj.AddComponent<CatButtonController>();

                    GameObject catObj = new GameObject("FullscreenCat");
                    catObj.transform.SetParent(menuContainer.transform, false);
                    controller.catImage = catObj.AddComponent<Image>();
                    controller.catImage.preserveAspect = true;
                    controller.catImage.raycastTarget = false;

                    controller.SetImageAlpha(0f);

                    RectTransform rect = controller.catImage.rectTransform;

                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;

                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.one;

                    button.onClick.AddListener(() =>
                    {
                        Log.Debug("Cat Button clicked! :3");
                        // I should probably add a sound to this but I'm lazy and too tired rn
                        controller.OnCatButtonClicked();
                    });

                    image.color = new Color(0.67f, 0.43f, 0.01f);
                    image.sprite = Plugin.catButtonSprite;

                    break;

                case MenuButtonType.DawnLibAchievements:

                    buttonObj.name = "NeatMenuAchievementButton";

                    button.onClick.AddListener(() =>
                    {
                        Log.Debug("NeatMenuAchievementButton clicked!");

                        var type = AccessTools.TypeByName("Dusk.AchievementUICanvas");

                        if (type == null)
                        {
                            Log.Error("AchievementUICanvas not found.");
                            return;
                        }

                        var openAchievementsMethod = AccessTools.Method(type, "AchievementsButtonOnClick");
                        var instance = AccessTools.Property(type, "Instance")?.GetValue(null);
                        if (instance == null) return;

                        openAchievementsMethod.Invoke(instance, null);
                    });

                    image.color = new Color(0.67f, 0.225f, 0.43f);
                    image.sprite = Plugin.achievementButtonSprite;

                    break;

                case MenuButtonType.TwitchChatAPI:

                    buttonObj.name = "NeatMenuTwitchButton";

                    button.onClick.AddListener(() =>
                    {
                        Log.Debug("NeatMenuTwitchButton clicked!");

                        var pluginCanvasType = AccessTools.TypeByName("TwitchChatAPI.LethalCompany.MonoBehaviours.PluginCanvas");

                        if (pluginCanvasType == null)
                        {
                            Log.Error("PluginCanvas not found.");
                            return;
                        }

                        var openSettingsMethod = AccessTools.Method(pluginCanvasType, "OpenSettingsWindow");
                        var instance = AccessTools.Property(pluginCanvasType, "Instance")?.GetValue(null);
                        if (instance == null) return;

                        openSettingsMethod.Invoke(instance, null);
                    });

                    image.color = new Color(0.39f, 0.25f, 0.64f);
                    image.sprite = Plugin.twitchButtonSprite;

                    break;
            }

            return buttonObj;
        }
    }
}
