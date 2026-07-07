using System;
using System.Collections;
using UnityEngine;

namespace TimeCrax.Core
{
    /// <summary>
    /// Helper para substituir Invoke() por Coroutines.
    /// Invoke() usa strings que não são verificadas em tempo de compilação.
    ///
    /// Uso:
    /// Ao invés de: Invoke("MetodoX", 1.5f);
    /// Use: StartCoroutine(CoroutineHelper.DelayedAction(1.5f, MetodoX));
    ///
    /// Ou use: this.DelayedCall(1.5f, MetodoX);
    /// </summary>
    public static class CoroutineHelper
    {
        /// <summary>
        /// Executa uma ação após um delay.
        /// </summary>
        public static IEnumerator DelayedAction(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        /// <summary>
        /// Executa uma ação após um delay (versão com parâmetro).
        /// </summary>
        public static IEnumerator DelayedAction<T>(float delay, Action<T> action, T parameter)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke(parameter);
        }
    }

    /// <summary>
    /// Extension methods para MonoBehaviour.
    /// </summary>
    public static class MonoBehaviourExtensions
    {
        /// <summary>
        /// Executa uma ação após um delay usando Coroutine.
        /// Mais seguro que Invoke() pois é verificado em tempo de compilação.
        /// </summary>
        public static Coroutine DelayedCall(this MonoBehaviour mono, float delay, Action action)
        {
            return mono.StartCoroutine(CoroutineHelper.DelayedAction(delay, action));
        }
    }
}
