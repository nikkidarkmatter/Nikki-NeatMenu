using System.Collections.Generic;
using UnityEngine;

namespace NeatMenu
{
    public enum MenuButtonType
    {
        MoreCompany,
        LethalPhones,
        LethalConfig,
        CatButton,
        DawnLibAchievements,
        TwitchChatAPI
    }

    class NeatMenuLayout
    {
        public static bool catButtonEnabled;

        public static List<MenuButtonType> activeButtons = [];

        public static readonly Vector3 AnchorPosition =
            new(386.7359f, -205.2192f, 0f);

        public static readonly Vector3 AnchorScale =
            new(0.485f, 0.485f, 0.485f);

        public static Vector3[] GetPositions(int buttonCount)
        {
            switch (buttonCount)
            {
                case 1:
                    return
                    [
                        new Vector3(0, 0, 0),
                    ];
                case 2:
                    return
                    [
                        new Vector3(0, 0, 0),
                        new Vector3(-54, 0, 0),
                    ];
                case 3:
                    return
                    [
                        new Vector3(0, 0, 0),
                        new Vector3(-54, 0, 0),
                        new Vector3(-108, 0, 0),
                    ];
                case 4:
                    return
                    [
                        new Vector3(0, 0, 0),
                        new Vector3(-54, 0, 0),
                        new Vector3(0, 50, 0),
                        new Vector3(-54, 50, 0),
                    ];
                case 5:
                    return
                    [
                        new Vector3(0, 0, 0),
                        new Vector3(-54, 0, 0),
                        new Vector3(-108, 0, 0),
                        new Vector3(0, 50, 0),
                        new Vector3(-54, 50, 0)
                    ];
                case 6:
                    return
                    [
                        new Vector3(0, 0, 0),
                        new Vector3(-54, 0, 0),
                        new Vector3(-108, 0, 0),
                        new Vector3(0, 50, 0),
                        new Vector3(-54, 50, 0),
                        new Vector3(-108, 50, 0)
                    ];

                default:
                    return [];
            }
        }

        public static void GenerateButtonList()
        {
            // remember to decouple this later if MoreCompany dependency is removed
            activeButtons.Add(MenuButtonType.MoreCompany);

            if (ModStateChecker.LethalPhonesLoaded)
            {
                activeButtons.Add(MenuButtonType.LethalPhones);
            }

            if (ModStateChecker.LethalConfigLoaded)
            {
                activeButtons.Add(MenuButtonType.LethalConfig);
            }

            if (catButtonEnabled)
            {
                activeButtons.Add(MenuButtonType.CatButton);
            }

            if (ModStateChecker.DawnLibLoaded)
            {
                activeButtons.Add(MenuButtonType.DawnLibAchievements);
            }

            if (ModStateChecker.TwitchChatAPILoaded)
            {
                activeButtons.Add(MenuButtonType.TwitchChatAPI);
            }
        }
    }
}
