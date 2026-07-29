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
                onComplete?.Invoke(ThemeStorageResult.Fail("User is not logged in"));
                yield break;
            }

            string url = $"{AuthService.Instance.ApiBaseUrl}/themes/storage?page={page}&pageSize={pageSize}";

            if (logRequests)

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", TokenManager.GetAuthorizationHeader());
                www.timeout = 30;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    onComplete?.Invoke(ThemeStorageResult.Fail("Connection error"));
                    yield break;
                }

                if (www.responseCode == 401)
                {
                    onComplete?.Invoke(ThemeStorageResult.Fail("Session expired"));
                    yield break;
                }

                if (www.responseCode == 200)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<ThemeStorageResponse>(www.downloadHandler.text);
                        onComplete?.Invoke(ThemeStorageResult.Ok(response));
                    }
                    catch (Exception)
                    {
                        onComplete?.Invoke(ThemeStorageResult.Fail("Error processing response"));
                    }
                    yield break;
                }

                onComplete?.Invoke(ThemeStorageResult.Fail($"Error: {www.responseCode}"));
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
                onComplete?.Invoke(ThemeDownloadResult.Fail("User is not logged in"));
                yield break;
            }

            OnDownloadStatus?.Invoke("Downloading theme data...");
            OnDownloadProgress?.Invoke(0f);

            string url = $"{AuthService.Instance.ApiBaseUrl}/themes/{themeId}/download";

            if (logRequests)

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", TokenManager.GetAuthorizationHeader());
                www.timeout = 60;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    onComplete?.Invoke(ThemeDownloadResult.Fail("Connection error"));
                    yield break;
                }

                if (www.responseCode == 401)
                {
                    onComplete?.Invoke(ThemeDownloadResult.Fail("Session expired"));
                    yield break;
                }

                if (www.responseCode == 404)
                {
                    onComplete?.Invoke(ThemeDownloadResult.Fail("Theme not found"));
                    yield break;
                }

                if (www.responseCode != 200)
                {
                    onComplete?.Invoke(ThemeDownloadResult.Fail($"Error: {www.responseCode}"));
                    yield break;
                }

                ThemeDownloadResponse response;
                try
                {
                    response = JsonUtility.FromJson<ThemeDownloadResponse>(www.downloadHandler.text);
                }
                catch (Exception)
                {
                    onComplete?.Invoke(ThemeDownloadResult.Fail("Error processing theme data"));
                    yield break;
                }

                OnDownloadProgress?.Invoke(0.1f);

                var themeData = ConvertToThemeData(response);

                ThemeStorage.EnsureThemeFolderExists(themeId);

                int totalImages = response.cards.Count + 1;
                int downloadedImages = 0;

                // Usar response.image (nova API) ou response.coverImageUrl (compatibilidade)
                string coverUrl = response.image ?? response.coverImageUrl;

                OnDownloadStatus?.Invoke("Downloading cover...");
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
                    OnDownloadStatus?.Invoke($"Downloading card {downloadedImages}/{response.cards.Count}...");

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

                for (int i = 0; i < themeData.cards.Count; i++)
                {
                    OnDownloadProgress?.Invoke(0.6f + (0.35f * (i + 1) / themeData.cards.Count));
                }

                themeData.downloadedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                ThemeStorage.SaveTheme(themeData);

                OnDownloadStatus?.Invoke("Download complete!");
                OnDownloadProgress?.Invoke(1f);

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
                });
            }

            return themeData;
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
                    catch (Exception)
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

        #region Get Theme Info

        /// <summary>
        /// Busca informações de um tema específico da API (sem baixar)
        /// </summary>
        public void GetThemeInfo(string themeId, Action<ThemeListItem> onComplete)
        {
            StartCoroutine(GetThemeInfoCoroutine(themeId, onComplete));
        }

        private IEnumerator GetThemeInfoCoroutine(string themeId, Action<ThemeListItem> onComplete)
        {
            if (!TokenManager.IsLoggedIn)
            {
                onComplete?.Invoke(null);
                yield break;
            }

            // Busca na lista de temas (página 1, muitos itens para aumentar chance de encontrar)
            string url = $"{AuthService.Instance.ApiBaseUrl}/themes/storage?page=1&pageSize=100";

            if (logRequests)

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", TokenManager.GetAuthorizationHeader());
                www.timeout = 15;

                yield return www.SendWebRequest();

                if (www.responseCode == 200)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<ThemeStorageResponse>(www.downloadHandler.text);
                        var theme = response.items?.Find(t => t.id == themeId);
                        onComplete?.Invoke(theme);
                    }
                    catch (Exception)
                    {
                        onComplete?.Invoke(null);
                    }
                }
                else
                {
                    onComplete?.Invoke(null);
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
