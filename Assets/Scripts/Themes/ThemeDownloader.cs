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
        public static ThemeDownloader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ThemeDownloader>();
                    if (_instance == null)
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

                OnDownloadStatus?.Invoke("Baixando capa...");
                yield return StartCoroutine(DownloadImage(
                    response.coverImageUrl,
                    ThemeStorage.GetLocalImagePath(themeId, "cover.webp"),
                    (success, localPath) =>
                    {
                        if (success)
                            themeData.localCoverPath = localPath;
                        downloadedImages++;
                    }
                ));

                OnDownloadProgress?.Invoke(0.1f + (0.9f * downloadedImages / totalImages));

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

                    OnDownloadProgress?.Invoke(0.1f + (0.9f * downloadedImages / totalImages));
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
                coverImageUrl = response.coverImageUrl,
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
                    imageUrl = card.imageUrl
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
