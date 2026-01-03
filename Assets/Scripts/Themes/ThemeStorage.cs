using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TimeCrax.Core;

namespace TimeCrax.Themes
{
    public static class ThemeStorage
    {
        private const string ThemesFolderName = "Themes";
        private const string ManifestFileName = "manifest.json";

        public static string ThemesRootPath => Path.Combine(Application.persistentDataPath, ThemesFolderName);

        public static void Initialize()
        {
            if (!Directory.Exists(ThemesRootPath))
            {
                Directory.CreateDirectory(ThemesRootPath);
                DebugHelper.Log($"[ThemeStorage] Created themes directory: {ThemesRootPath}");
            }
        }

        public static string GetThemeFolderPath(string themeId)
        {
            return Path.Combine(ThemesRootPath, themeId);
        }

        public static string GetManifestPath()
        {
            return Path.Combine(ThemesRootPath, ManifestFileName);
        }

        public static LocalThemeManifest LoadManifest()
        {
            var path = GetManifestPath();
            if (!File.Exists(path))
            {
                return new LocalThemeManifest();
            }

            try
            {
                var json = File.ReadAllText(path);
                return JsonUtility.FromJson<LocalThemeManifest>(json) ?? new LocalThemeManifest();
            }
            catch (Exception ex)
            {
                DebugHelper.Log($"[ThemeStorage] Error loading manifest: {ex.Message}");
                return new LocalThemeManifest();
            }
        }

        public static void SaveManifest(LocalThemeManifest manifest)
        {
            try
            {
                Initialize();
                var json = JsonUtility.ToJson(manifest, true);
                File.WriteAllText(GetManifestPath(), json);
                DebugHelper.Log("[ThemeStorage] Manifest saved");
            }
            catch (Exception ex)
            {
                DebugHelper.Log($"[ThemeStorage] Error saving manifest: {ex.Message}");
            }
        }

        public static List<ThemeData> GetDownloadedThemes()
        {
            var manifest = LoadManifest();
            return manifest.themes;
        }

        public static ThemeData GetTheme(string themeId)
        {
            var manifest = LoadManifest();
            return manifest.themes.FirstOrDefault(t => t.id == themeId);
        }

        public static bool IsThemeDownloaded(string themeId)
        {
            var manifest = LoadManifest();
            return manifest.themes.Any(t => t.id == themeId);
        }

        public static bool IsThemeUpToDate(string themeId, string serverVersion)
        {
            var theme = GetTheme(themeId);
            if (theme == null) return false;
            return theme.version == serverVersion;
        }

        public static void SaveTheme(ThemeData theme)
        {
            var manifest = LoadManifest();

            var existingIndex = manifest.themes.FindIndex(t => t.id == theme.id);
            if (existingIndex >= 0)
            {
                manifest.themes[existingIndex] = theme;
            }
            else
            {
                manifest.themes.Add(theme);
            }

            SaveManifest(manifest);
            DebugHelper.Log($"[ThemeStorage] Theme saved: {theme.name}");
        }

        public static void DeleteTheme(string themeId)
        {
            var manifest = LoadManifest();
            manifest.themes.RemoveAll(t => t.id == themeId);
            SaveManifest(manifest);

            var themePath = GetThemeFolderPath(themeId);
            if (Directory.Exists(themePath))
            {
                try
                {
                    Directory.Delete(themePath, true);
                    DebugHelper.Log($"[ThemeStorage] Theme deleted: {themeId}");
                }
                catch (Exception ex)
                {
                    DebugHelper.Log($"[ThemeStorage] Error deleting theme folder: {ex.Message}");
                }
            }
        }

        public static string GetLocalImagePath(string themeId, string imageName)
        {
            return Path.Combine(GetThemeFolderPath(themeId), imageName);
        }

        public static void EnsureThemeFolderExists(string themeId)
        {
            var path = GetThemeFolderPath(themeId);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static Texture2D LoadLocalImage(string localPath)
        {
            if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
            {
                return null;
            }

            try
            {
                var bytes = File.ReadAllBytes(localPath);
                var texture = new Texture2D(2, 2);
                if (texture.LoadImage(bytes))
                {
                    return texture;
                }
            }
            catch (Exception ex)
            {
                DebugHelper.Log($"[ThemeStorage] Error loading local image: {ex.Message}");
            }

            return null;
        }

        public static void SaveImageBytes(string localPath, byte[] bytes)
        {
            try
            {
                var directory = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllBytes(localPath, bytes);
            }
            catch (Exception ex)
            {
                DebugHelper.Log($"[ThemeStorage] Error saving image: {ex.Message}");
            }
        }

        public static long GetTotalStorageSize()
        {
            if (!Directory.Exists(ThemesRootPath))
                return 0;

            try
            {
                var dirInfo = new DirectoryInfo(ThemesRootPath);
                return dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
            }
            catch
            {
                return 0;
            }
        }

        public static string FormatStorageSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }
    }
}
