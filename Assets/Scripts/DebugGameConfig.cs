using UnityEngine;
using TimeCrax.Core;

/// <summary>
/// Configuração de debug para testes rápidos no Inspector.
/// Adicione este componente a qualquer GameObject na cena para ativar os overrides.
/// Remova o componente (ou delete este arquivo) para voltar ao comportamento normal.
/// Não interfere em nada quando ausente da cena.
/// </summary>
public class DebugGameConfig : MonoBehaviour
{
    public static DebugGameConfig Instance { get; private set; }

    [Header("Pesos das Cartas Bonus")]
    [Tooltip("Valores relativos — serão normalizados automaticamente. Ex: 50/0/0/0/0/0 força sempre RepairComponent.")]
    [Range(0, 100)] public float RepairComponent     = 16;
    [Range(0, 100)] public float BonusTime           = 16;
    [Range(0, 100)] public float SecondChanceSlot    = 16;
    [Range(0, 100)] public float CoolThermometer     = 16;
    [Range(0, 100)] public float KillChallengeOption = 16;
    [Range(0, 100)] public float SkipChallenge       = 20;

    [Header("Challenge — Map vs Persons")]
    [Tooltip("Chance (%) de sair Map quando ambos estão disponíveis. 0 = sempre Persons, 100 = sempre Map.")]
    [Range(0, 100)] public float MapChancePct = 50;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ─────────────────────────────────────────────────────────────
    // API estática — usada por DeckBonus e GameManager.
    // Quando Instance == null o comportamento é idêntico ao original.
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Retorna o índice do tipo de carta bonus (0-5, mapeado para BonusCardType).
    /// Usa pesos configurados no Inspector ou Random.Range(0,6) se o script não estiver na cena.
    /// </summary>
    public static int PickBonusCardType()
    {
        if (Instance == null) return Random.Range(0, 6);

        float[] weights =
        {
            Instance.RepairComponent,
            Instance.BonusTime,
            Instance.SecondChanceSlot,
            Instance.CoolThermometer,
            Instance.KillChallengeOption,
            Instance.SkipChallenge,
        };

        float total = 0f;
        foreach (var w in weights) total += w;
        if (total <= 0f) return Random.Range(0, 6);

        float roll = Random.Range(0f, total);
        float accum = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            accum += weights[i];
            if (roll < accum) return i;
        }
        return weights.Length - 1;
    }

    /// <summary>
    /// Retorna o tipo de challenge: 1 = Map, 0 = Persons.
    /// Respeita hasMap / hasPersons da carta — a % de Map só se aplica quando ambos estão disponíveis.
    /// </summary>
    public static int PickChallengeType(bool hasMap, bool hasPersons)
    {
        if (!hasMap) return 0;
        if (!hasPersons) return 1;

        if (Instance == null) return Random.Range(0, 2);

        return Random.value * 100f < Instance.MapChancePct ? 1 : 0;
    }
}
