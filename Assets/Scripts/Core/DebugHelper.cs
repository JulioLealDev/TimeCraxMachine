using UnityEngine;
using System.Diagnostics;

namespace TimeCrax.Core
{
    /// <summary>
    /// Wrapper para Debug.Log que é automaticamente removido em builds de produção.
    /// Use DebugHelper.Log() ao invés de Debug.Log() para melhor performance.
    ///
    /// Para desabilitar logs em builds:
    /// - Build Settings > Player Settings > Other Settings > Scripting Define Symbols
    /// - Adicione: DISABLE_DEBUG_LOGS
    /// </summary>
    public static class DebugHelper
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object message, Object context)
        {
            UnityEngine.Debug.Log(message, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(object message, Object context)
        {
            UnityEngine.Debug.LogWarning(message, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogError(object message, Object context)
        {
            UnityEngine.Debug.LogError(message, context);
        }
    }
}
