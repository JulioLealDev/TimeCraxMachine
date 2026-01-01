using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TimeCrax.Core;

namespace TimeCrax.Auth
{
    /// <summary>
    /// Controller da cena de Intro.
    /// Exibe a tela por um tempo com efeitos de fade in/out.
    /// Cria automaticamente o fade se não configurado.
    /// </summary>
    public class IntroController : MonoBehaviour
    {
        [Header("Configurações")]
        [SerializeField] private float displayDuration = 5f;
        [SerializeField] private float fadeInDuration = 2f;
        [SerializeField] private float fadeOutDuration = 2f;
        [SerializeField] private string nextSceneName = "LoginScreen";

        [Header("Fade (opcional - cria automaticamente se vazio)")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private CanvasGroup contentCanvasGroup;

        private void Start()
        {
            DebugHelper.Log("[Intro] Iniciando intro...");

            // Cria fade image automaticamente se não configurado
            if (fadeImage == null)
            {
                CreateFadeImage();
            }

            StartCoroutine(IntroSequence());
        }

        private void CreateFadeImage()
        {
            // Procura ou cria um Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("FadeCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999; // Na frente de tudo
                canvasObj.AddComponent<CanvasScaler>();
            }

            // Cria a imagem de fade
            GameObject fadeObj = new GameObject("FadeImage");
            fadeObj.transform.SetParent(canvas.transform, false);

            fadeImage = fadeObj.AddComponent<Image>();
            fadeImage.color = Color.black;

            // Estica para cobrir toda a tela
            RectTransform rect = fadeObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            DebugHelper.Log("[Intro] FadeImage criado automaticamente");
        }

        private IEnumerator IntroSequence()
        {
            DebugHelper.Log("[Intro] Sequência iniciada");

            // Inicializa: tela preta, conteúdo invisível
            if (fadeImage != null)
            {
                fadeImage.color = Color.black;
                fadeImage.gameObject.SetActive(true);
            }

            if (contentCanvasGroup != null)
            {
                contentCanvasGroup.alpha = 0f;
            }

            // Fade In
            DebugHelper.Log("[Intro] Fade In...");
            yield return StartCoroutine(FadeIn());

            // Aguarda o tempo de exibição
            DebugHelper.Log($"[Intro] Aguardando {displayDuration}s...");
            yield return new WaitForSeconds(displayDuration);

            // Fade Out
            DebugHelper.Log("[Intro] Fade Out...");
            yield return StartCoroutine(FadeOut());

            // Carrega próxima cena
            DebugHelper.Log($"[Intro] Carregando cena: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }

        private IEnumerator FadeIn()
        {
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDuration);

                // Fade da tela preta para transparente
                if (fadeImage != null)
                {
                    fadeImage.color = new Color(0, 0, 0, 1f - t);
                }

                // Fade do conteúdo de invisível para visível
                if (contentCanvasGroup != null)
                {
                    contentCanvasGroup.alpha = t;
                }

                yield return null;
            }

            // Garante valores finais
            if (fadeImage != null)
            {
                fadeImage.color = new Color(0, 0, 0, 0);
            }

            if (contentCanvasGroup != null)
            {
                contentCanvasGroup.alpha = 1f;
            }
        }

        private IEnumerator FadeOut()
        {
            float elapsed = 0f;

            if (fadeImage != null)
            {
                fadeImage.gameObject.SetActive(true);
            }

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeOutDuration);

                // Fade da tela transparente para preta
                if (fadeImage != null)
                {
                    fadeImage.color = new Color(0, 0, 0, t);
                }

                // Fade do conteúdo de visível para invisível
                if (contentCanvasGroup != null)
                {
                    contentCanvasGroup.alpha = 1f - t;
                }

                yield return null;
            }

            // Garante valores finais
            if (fadeImage != null)
            {
                fadeImage.color = Color.black;
            }

            if (contentCanvasGroup != null)
            {
                contentCanvasGroup.alpha = 0f;
            }
        }
    }
}
