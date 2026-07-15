using UnityEngine;
using System.Diagnostics;

namespace TimeCrax.Core
{
    /// <summary>
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
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object message, Object context)
        {
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(object message)
        {
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(object message, Object context)
        {
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogError(object message)
        {
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogError(object message, Object context)
        {
        }
    }
}
