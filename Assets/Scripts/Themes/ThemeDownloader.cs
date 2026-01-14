using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TimeCrax.Core;
using TimeCrax.Auth;

namespace TimeCrax.Themes
{
    public class ThemeDownloader : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool logRequests = true;

        private static ThemeDownloader _instance;
        private static bool _isQuitting = false;

        public static ThemeDownloader Instance
        {
            get
            {
                if (_isQuitting)
                    return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ThemeDownloader>();
                    if (_instance == null && !_isQuitting)
                    {
                        GameObject go = new GameObject("ThemeDownloader");
                        _instance = go.AddComponent<ThemeDownloader>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            ThemeStorage.Initialize();
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public event Action<float> OnDownloadProgress;
        public event Action<string> OnDownloadStatus;

        #region Get Theme List

        public void GetThemeStorage(int page, int pageSize, Action<ThemeStorageResult> onComplete)
        {
            StartCoroutine(GetThemeStorageCoroutine(page, pageSize, onComplete));
        }

        private IEnumerator GetThemeStorageCoroutine(int page, int pageSize, Action<ThemeStorageResult> onComplete)
        {
            if (!TokenManager.IsLoggedIn)
            {
                onComplete?.Invoke(ThemeStorageResult.Fail("Usuário não está logado"));
                yield break;
            }

            string url = $"{AuthService.Instance.ApiBaseUrl}/themes/storage?page={page}&pageSize={pageSize}";

            if (logRequests)
                DebugHelper.Log($"[ThemeDownloader] GET {url}");

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", TokenManager.GetAuthorizationHeader());
                www.timeout = 30;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    onComplete?.Invoke(ThemeStorageResult.Fail("Erro de conexão"));
                    yield break;
                }

                if (www.responseCode == 401)
                {
                    onComplete?.Invoke(ThemeStorageResult.Fail("Sessão expirada"));
                    yield break;
                }

                if (www.responseCode == 200)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<ThemeStorageResponse>(www.downloadHandler.text);
                        onComplete?.Invoke(ThemeStorageResult.Ok(response));
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.Log($"[ThemeDownloader] Parse error: {ex.Message}");
                        onComplete?.Invoke(ThemeStorageResult.Fail("Erro ao processar resposta"));
                    }
                    yield break;
                }

                onComplete?.Invoke(ThemeStorageResult.Fail($"Erro: {www.responseCode}"));
            }
        }

        #endregion

        #region Download Theme

        public void DownloadTheme(string themeId, Action<ThemeDownloadResult> onComplete)
        {
            StartCoroutine(DownloadThemeCoroutine(themeId, onComplete));
        }

        private IEnumerator DownloadThemeCoroutine(string themeId, Action<ThemeDownloadResult> onComplete)
        {
            if (!TokenManager.IsLoggedIn)
            {
                onComplete?.Invoke(ThemeDownloadResult.Fail("Usuário não está logado"));
                yield break;
            }

            OnDownloadStatus?.Invoke("Baixando dados do tema...");
            OnDownloadProgress?.Invoke(0f);

            string url = $"{AuthService.Instance.ApiBaseUrl}/themes/{themeId}/download";

            if (logRequests)
                DebugHelper.Log($"[ThemeDownloader] GET {url}");

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", TokenManager.GetAuthorizationHeader());
                www.timeout = 60;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    onComplete?.Invoke(ThemeDownloadResult.Fail("Erro de conexão"));
                    yield break;
                }

                if (www.responseCode == 401)
                {
                    onComplete?.Invoke(ThemeDownloadResult.Fail("Sessão expirada"));
                    yield break;
                }

                if (www.responseCode == 404)
                {
                    onComplete?.Invoke(ThemeDownloadResult.Fail("Tema não encontrado"));
                    yield break;
                }

                if (www.responseCode != 200)
                {
                    onComplete?.Invoke(ThemeDownloadResult.Fail($"Erro: {www.responseCode}"));
                    yield break;
                }

                ThemeDownloadResponse response;
                try
                {
                    response = JsonUtility.FromJson<ThemeDownloadResponse>(www.downloadHandler.text);
                }
                catch (Exception ex)
                {
                    DebugHelper.Log($"[ThemeDownloader] Parse error: {ex.Message}");
                    onComplete?.Invoke(ThemeDownloadResult.Fail("Erro ao processar dados do tema"));
                    yield break;
                }

                OnDownloadProgress?.Invoke(0.1f);

                var themeData = ConvertToThemeData(response);

                ThemeStorage.EnsureThemeFolderExists(themeId);

                int totalImages = response.cards.Count + 1;
                int downloadedImages = 0;

                // Usar response.image (nova API) ou response.coverImageUrl (compatibilidade)
                string coverUrl = response.image ?? response.coverImageUrl;

                OnDownloadStatus?.Invoke("Baixando capa...");
                yield return StartCoroutine(DownloadImage(
                    coverUrl,
                    ThemeStorage.GetLocalImagePath(themeId, "cover.webp"),
                    (success, localPath) =>
                    {
                        if (success)
                            themeData.localCoverPath = localPath;
                        downloadedImages++;
                    }
                ));

                OnDownloadProgress?.Invoke(0.1f + (0.5f * downloadedImages / totalImages));

                // Download das imagens das cartas
                foreach (var card in response.cards)
                {
                    OnDownloadStatus?.Invoke($"Baixando carta {downloadedImages}/{response.cards.Count}...");

                    string imageName = $"card_{card.orderIndex}.webp";
                    string localPath = ThemeStorage.GetLocalImagePath(themeId, imageName);

                    yield return StartCoroutine(DownloadImage(
                        card.imageUrl,
                        localPath,
                        (success, path) =>
                        {
                            if (success)
                            {
                                var themeCard = themeData.cards.Find(c => c.id == card.id);
                                if (themeCard != null)
                                    themeCard.localImagePath = path;
                            }
                            downloadedImages++;
                        }
                    ));

                    OnDownloadProgress?.Invoke(0.1f + (0.5f * downloadedImages / totalImages));
                }

                // Download das imagens dos quizzes
                OnDownloadStatus?.Invoke("Baixando imagens dos quizzes...");
                for (int i = 0; i < themeData.cards.Count; i++)
                {
                    var themeCard = themeData.cards[i];
                    if (themeCard.quizData != null && themeCard.quizData.HasQuiz)
                    {
                        OnDownloadStatus?.Invoke($"Baixando quiz da carta {i + 1}/{themeData.cards.Count}...");
                        yield return StartCoroutine(DownloadQuizImages(themeId, themeCard, i));
                    }
                    OnDownloadProgress?.Invoke(0.6f + (0.35f * (i + 1) / themeData.cards.Count));
                }

                themeData.downloadedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                ThemeStorage.SaveTheme(themeData);

                OnDownloadStatus?.Invoke("Download concluído!");
                OnDownloadProgress?.Invoke(1f);

                DebugHelper.Log($"[ThemeDownloader] Theme downloaded: {themeData.name}");
                onComplete?.Invoke(ThemeDownloadResult.Ok(themeData));
            }
        }

        private ThemeData ConvertToThemeData(ThemeDownloadResponse response)
        {
            var themeData = new ThemeData
            {
                id = response.id,
                name = response.name,
                version = response.version,
                creatorName = response.creatorName,
                resume = response.resume,
                recommendation = response.recommendation,
                coverImageUrl = response.image ?? response.coverImageUrl, // Nova API usa 'image'
                cardCount = response.cardCount,
                cards = new List<ThemeCard>()
            };

            foreach (var card in response.cards)
            {
                themeData.cards.Add(new ThemeCard
                {
                    id = card.id,
                    orderIndex = card.orderIndex,
                    year = card.year,
                    era = card.era,
                    title = card.title,
                    imageUrl = card.imageUrl,
                    quizData = ConvertQuizData(card)
                });
            }

            return themeData;
        }

        private CardQuizData ConvertQuizData(ThemeCardResponse card)
        {
            var quizData = new CardQuizData();

            // Converter ImageQuiz
            if (card.imageQuiz != null)
            {
                quizData.imageQuiz = new ImageQuiz
                {
                    question = card.imageQuiz.question,
                    correctIndex = card.imageQuiz.correctIndex,
                    options = new List<QuizOption>()
                };

                if (card.imageQuiz.options != null)
                {
                    foreach (var option in card.imageQuiz.options)
                    {
                        quizData.imageQuiz.options.Add(new QuizOption
                        {
                            text = option.text,
                            imageUrl = option.imageUrl
                        });
                    }
                }
            }

            // Converter TextQuiz
            if (card.textQuiz != null)
            {
                quizData.textQuiz = new TextQuiz
                {
                    question = card.textQuiz.question,
                    correctIndex = card.textQuiz.correctIndex,
                    options = new List<QuizOption>()
                };

                if (card.textQuiz.options != null)
                {
                    foreach (var option in card.textQuiz.options)
                    {
                        quizData.textQuiz.options.Add(new QuizOption
                        {
                            text = option.text
                        });
                    }
                }
            }

            // Converter TrueFalseQuiz
            if (card.trueFalseQuiz != null)
            {
                quizData.trueFalseQuiz = new TrueFalseQuiz
                {
                    statement = card.trueFalseQuiz.statement,
                    answer = card.trueFalseQuiz.answer
                };
            }

            // Converter CorrelationQuiz
            if (card.correlationQuiz != null)
            {
                quizData.correlationQuiz = new CorrelationQuiz
                {
                    items = new List<CorrelationItem>()
                };

                if (card.correlationQuiz.items != null)
                {
                    foreach (var item in card.correlationQuiz.items)
                    {
                        quizData.correlationQuiz.items.Add(new CorrelationItem
                        {
                            imageUrl = item.imageUrl,
                            text = item.text
                        });
                    }
                }
            }

            return quizData;
        }

        private IEnumerator DownloadImage(string url, string localPath, Action<bool, string> onComplete)
        {
            if (string.IsNullOrEmpty(url))
            {
                onComplete?.Invoke(false, null);
                yield break;
            }

            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                www.timeout = 30;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        ThemeStorage.SaveImageBytes(localPath, www.downloadHandler.data);
                        onComplete?.Invoke(true, localPath);
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.Log($"[ThemeDownloader] Error saving image: {ex.Message}");
                        onComplete?.Invoke(false, null);
                    }
                }
                else
                {
                    DebugHelper.Log($"[ThemeDownloader] Error downloading image: {www.error}");
                    onComplete?.Invoke(false, null);
                }
            }
        }

        /// <summary>
        /// Baixa imagens de quiz de uma carta (ImageQuiz options e CorrelationQuiz items)
        /// </summary>
        private IEnumerator DownloadQuizImages(string themeId, ThemeCard card, int cardIndex)
        {
            // Download de imagens do ImageQuiz
            if (card.quizData?.imageQuiz?.options != null)
            {
                for (int i = 0; i < card.quizData.imageQuiz.options.Count; i++)
                {
                    var option = card.quizData.imageQuiz.options[i];
                    if (!string.IsNullOrEmpty(option.imageUrl))
                    {
                        string imageName = $"quiz_{cardIndex}_option_{i}.webp";
                        string localPath = ThemeStorage.GetLocalImagePath(themeId, imageName);

                        yield return StartCoroutine(DownloadImage(
                            option.imageUrl,
                            localPath,
                            (success, path) =>
                            {
                                if (success)
                                    option.localImagePath = path;
                            }
                        ));
                    }
                }
            }

            // Download de imagens do CorrelationQuiz
            if (card.quizData?.correlationQuiz?.items != null)
            {
                for (int i = 0; i < card.quizData.correlationQuiz.items.Count; i++)
                {
                    var item = card.quizData.correlationQuiz.items[i];
                    if (!string.IsNullOrEmpty(item.imageUrl))
                    {
                        string imageName = $"correlation_{cardIndex}_item_{i}.webp";
                        string localPath = ThemeStorage.GetLocalImagePath(themeId, imageName);

                        yield return StartCoroutine(DownloadImage(
                            item.imageUrl,
                            localPath,
                            (success, path) =>
                            {
                                if (success)
                                    item.localImagePath = path;
                            }
                        ));
                    }
                }
            }
        }

        #endregion

        #region Check for Updates

        public void CheckThemeUpdate(string themeId, Action<bool, string> onComplete)
        {
            StartCoroutine(CheckThemeUpdateCoroutine(themeId, onComplete));
        }

        private IEnumerator CheckThemeUpdateCoroutine(string themeId, Action<bool, string> onComplete)
        {
            string url = $"{AuthService.Instance.ApiBaseUrl}/themes/{themeId}/download";

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", TokenManager.GetAuthorizationHeader());
                www.timeout = 15;

                yield return www.SendWebRequest();

                if (www.responseCode == 200)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<ThemeDownloadResponse>(www.downloadHandler.text);
                        bool needsUpdate = !ThemeStorage.IsThemeUpToDate(themeId, response.version);
                        onComplete?.Invoke(needsUpdate, response.version);
                    }
                    catch
                    {
                        onComplete?.Invoke(false, null);
                    }
                }
                else
                {
                    onComplete?.Invoke(false, null);
                }
            }
        }

        #endregion
    }

    #region Result Classes

    public class ThemeStorageResult
    {
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }
        public ThemeStorageResponse Data { get; private set; }

        public static ThemeStorageResult Ok(ThemeStorageResponse data)
        {
            return new ThemeStorageResult { Success = true, Data = data };
        }

        public static ThemeStorageResult Fail(string message)
        {
            return new ThemeStorageResult { Success = false, ErrorMessage = message };
        }
    }

    public class ThemeDownloadResult
    {
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }
        public ThemeData Data { get; private set; }

        public static ThemeDownloadResult Ok(ThemeData data)
        {
            return new ThemeDownloadResult { Success = true, Data = data };
        }

        public static ThemeDownloadResult Fail(string message)
        {
            return new ThemeDownloadResult { Success = false, ErrorMessage = message };
        }
    }

    #endregion
}
