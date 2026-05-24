using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeatMenu.MonoBehaviours
{
    public class CatButtonController : MonoBehaviour
    {
        public Image catImage;

        public float fadeDuration = 0.5f;

        private Coroutine currentRoutine;

        private void Start()
        {
            SetImageAlpha(0f);
        }

        public void OnCatButtonClicked()
        {
            var loadedCats = Plugin.loadedCats;

            if (loadedCats.Count == 0)
            {
                Log.Error("Cats failed to load.");
                return;
            }

            Sprite randomCat = loadedCats[Random.Range(0, loadedCats.Count)];

            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
                SetImageAlpha(0f);
            }

            currentRoutine = StartCoroutine(ShowCatRoutine(randomCat));
        }

        private IEnumerator ShowCatRoutine(Sprite sprite)
        {
            catImage.sprite = sprite;

            SetImageAlpha(1f);

            yield return new WaitForSeconds(0.5f);

            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;

                float tNorm = timer / fadeDuration;
                tNorm = Mathf.SmoothStep(0f, 1f, tNorm);

                float alpha = Mathf.Lerp(1f, 0f, tNorm);

                SetImageAlpha(alpha);

                yield return null;
            }
        }

        public void SetImageAlpha(float alpha)
        {
            Color color = catImage.color;
            color.a = alpha;
            catImage.color = color;
        }
    }
}
