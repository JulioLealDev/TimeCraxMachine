using System;
using System.Collections.Generic;
using UnityEngine;
using TimeCrax.Core;

namespace TimeCrax.Themes
{
    public class ThemeManager : MonoBehaviour
    {
        private static ThemeManager _instance;
        private static bool _isQuitting = false;

        public static ThemeManager Instance
        {
            get
            {
                if (_isQuitting)
                    return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ThemeManager>();
                    if (_instance == null && !_isQuitting)
                    {
                        GameObject go = new GameObject("ThemeManager");
                        _instance = go.AddComponent<ThemeManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private ThemeData _selectedTheme;
        private List<ThemeData> _downloadedThemes;

        public ThemeData SelectedTheme => _selectedTheme;
        public List<ThemeData> DownloadedThemes => _downloadedThemes ??= ThemeStorage.GetDownloadedThemes();
        public bool HasSelectedTheme => _selectedTheme != null;

        public event Action<ThemeData> OnThemeSelected;
        public event Action<ThemeData> OnThemeDownloaded;
        public event Action<string> OnThemeDeleted;

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
            RefreshDownloadedThemes();
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

        public void RefreshDownloadedThemes()
        {
            _downloadedThemes = ThemeStorage.GetDownloadedThemes();
            DebugHelper.Log($"[ThemeManager] Loaded {_downloadedThemes.Count} downloaded themes");
        }

        public void SelectTheme(string themeId)
        {
            var theme = ThemeStorage.GetTheme(themeId);
            if (theme != null)
            {
                _selectedTheme = theme;
                DebugHelper.Log($"[ThemeManager] Selected theme: {theme.name}");
                OnThemeSelected?.Invoke(theme);
            }
            else
            {
                DebugHelper.Log($"[ThemeManager] Theme not found: {themeId}");
            }
        }

        public void SelectTheme(ThemeData theme)
        {
            _selectedTheme = theme;
            DebugHelper.Log($"[ThemeManager] Selected theme: {theme?.name ?? "null"}");
            OnThemeSelected?.Invoke(theme);
        }

        public void ClearSelection()
        {
            _selectedTheme = null;
            OnThemeSelected?.Invoke(null);
        }

        public void DownloadTheme(string themeId, Action<bool, string> onComplete = null)
        {
            ThemeDownloader.Instance.DownloadTheme(themeId, result =>
            {
                if (result.Success)
                {
                    RefreshDownloadedThemes();
                    OnThemeDownloaded?.Invoke(result.Data);
                    onComplete?.Invoke(true, null);
                }
                else
                {
                    onComplete?.Invoke(false, result.ErrorMessage);
                }
            });
        }

        public void DeleteTheme(string themeId)
        {
            if (_selectedTheme != null && _selectedTheme.id == themeId)
            {
                ClearSelection();
            }

            ThemeStorage.DeleteTheme(themeId);
            RefreshDownloadedThemes();
            OnThemeDeleted?.Invoke(themeId);
            DebugHelper.Log($"[ThemeManager] Deleted theme: {themeId}");
        }

        public bool IsThemeDownloaded(string themeId)
        {
            return ThemeStorage.IsThemeDownloaded(themeId);
        }

        public ThemeData GetDownloadedTheme(string themeId)
        {
            return ThemeStorage.GetTheme(themeId);
        }

        public void GetAvailableThemes(int page, int pageSize, Action<ThemeStorageResult> onComplete)
        {
            ThemeDownloader.Instance.GetThemeStorage(page, pageSize, onComplete);
        }

        public Texture2D LoadThemeCover(ThemeData theme)
        {
            if (theme == null) return null;

            if (!string.IsNullOrEmpty(theme.localCoverPath))
            {
                return ThemeStorage.LoadLocalImage(theme.localCoverPath);
            }

            return null;
        }

        public Texture2D LoadCardImage(ThemeCard card)
        {
            if (card == null) return null;

            if (!string.IsNullOrEmpty(card.localImagePath))
            {
                return ThemeStorage.LoadLocalImage(card.localImagePath);
            }

            return null;
        }

        public List<ThemeCard> GetSelectedThemeCards()
        {
            if (_selectedTheme == null) return new List<ThemeCard>();
            return _selectedTheme.cards;
        }

        public string GetStorageInfo()
        {
            long size = ThemeStorage.GetTotalStorageSize();
            int count = DownloadedThemes.Count;
            return $"{count} tema(s) - {ThemeStorage.FormatStorageSize(size)}";
        }

        #region Photon Integration

        public string GetSelectedThemeId()
        {
            return _selectedTheme?.id;
        }

        public bool LoadThemeFromId(string themeId)
        {
            if (string.IsNullOrEmpty(themeId)) return false;

            var theme = ThemeStorage.GetTheme(themeId);
            if (theme != null)
            {
                _selectedTheme = theme;
                return true;
            }

            return false;
        }

        #endregion
    }
}
