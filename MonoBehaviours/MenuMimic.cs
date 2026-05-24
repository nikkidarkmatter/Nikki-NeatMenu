using System.Collections.Generic;
using UnityEngine;

namespace NeatMenu.MonoBehaviours
{
    public class MenuMimic : MonoBehaviour
    {
        public GameObject mainButtons;
        public List<GameObject> neatMenuButtons = [];

        public void Start()
        {
            mainButtons = GameObject.Find("Canvas/MenuContainer/MainButtons");
            foreach (Transform child in transform)
            {
                neatMenuButtons.Add(child.gameObject);
            }
        }

        public void Update()
        {
            if (mainButtons == null || neatMenuButtons.Count == 0) return;

            foreach (GameObject buttonObj in neatMenuButtons)
            {
                buttonObj.SetActive(mainButtons.activeSelf);
            }
        }
    }
}
