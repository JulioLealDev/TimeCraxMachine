using UnityEngine;
using Photon.Pun;
using TMPro;
using TimeCrax.Core;

/// <summary>
/// Gerencia o cronômetro de turno com sincronização multiplayer.
/// Cada jogador tem um tempo limite para realizar suas ações.
/// Não usa PhotonView próprio - os RPCs são enviados através do GameManager.
/// </summary>
public class TurnTimer : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float timeLimit = 120f;
    [SerializeField] private float syncInterval = 1f; // Intervalo de sincronização

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private float warningThreshold = 30f;
    [SerializeField] private float criticalThreshold = 10f;

    // Estado do timer
    private float remainingTime;
    private bool isRunning;
    private float lastSyncTime;

    // Referência ao GameManager (para enviar RPCs e auto-end)
    private GameManager gameManager;

    // Proteção contra múltiplas chamadas de auto-end
    private bool hasAutoEnded;

    public float RemainingTime => remainingTime;
    public bool IsRunning => isRunning;
    public float TimeLimit => timeLimit;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        // Inicialmente oculto
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isRunning) return;

        // Apenas MasterClient controla a contagem
        if (PhotonNetwork.IsMasterClient)
        {
            remainingTime -= Time.deltaTime;

            // Sincronizar tempo periodicamente através do GameManager
            if (Time.time - lastSyncTime >= syncInterval && gameManager != null)
            {
                lastSyncTime = Time.time;
                gameManager.SyncTurnTimer(remainingTime);
            }

            // Verificar se o tempo acabou
            if (remainingTime <= 0 && !hasAutoEnded)
            {
                remainingTime = 0;
                hasAutoEnded = true;
                OnTimeExpired();
            }
        }

        // Atualizar UI localmente
        UpdateTimerDisplay();
    }

    /// <summary>
    /// Inicia o timer (chamado pelo GameManager.StartTurn)
    /// </summary>
    public void StartTimer()
    {
        // Inicia localmente - GameManager vai sincronizar via RPC
        StartTimerLocal(timeLimit);
    }

    /// <summary>
    /// Inicia o timer localmente (chamado por RPC através do GameManager)
    /// </summary>
    public void StartTimerLocal(float time)
    {
        DebugHelper.Log($"[TurnTimer] StartTimerLocal: {time}s");
        remainingTime = time;
        isRunning = true;
        hasAutoEnded = false;
        lastSyncTime = Time.time;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
        }

        UpdateTimerDisplay();
    }

    /// <summary>
    /// Para o timer (chamado pelo GameManager.EndTurn)
    /// </summary>
    public void StopTimer()
    {
        // Para localmente - GameManager vai sincronizar via RPC
        StopTimerLocal();
    }

    /// <summary>
    /// Para o timer localmente (chamado por RPC através do GameManager)
    /// </summary>
    public void StopTimerLocal()
    {
        DebugHelper.Log("[TurnTimer] StopTimerLocal");
        isRunning = false;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Sincroniza o tempo (chamado por RPC através do GameManager)
    /// </summary>
    public void SyncTime(float time)
    {
        // Apenas não-MasterClients recebem a sincronização
        if (!PhotonNetwork.IsMasterClient)
        {
            remainingTime = time;
        }
    }

    /// <summary>
    /// Atualiza a exibição do timer na UI
    /// </summary>
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        // Exibir segundos restantes (arredondado para cima)
        int seconds = Mathf.CeilToInt(Mathf.Max(0, remainingTime));
        timerText.text = seconds.ToString();

        // Mudar cor baseado no tempo restante
        if (remainingTime <= criticalThreshold)
        {
            timerText.color = criticalColor;
        }
        else if (remainingTime <= warningThreshold)
        {
            timerText.color = warningColor;
        }
        else
        {
            timerText.color = normalColor;
        }
    }

    /// <summary>
    /// Chamado quando o tempo expira
    /// </summary>
    private void OnTimeExpired()
    {
        DebugHelper.Log("[TurnTimer] Tempo expirado! Auto-end do turno.");

        // Parar o timer em todos os clientes
        if (gameManager != null)
        {
            gameManager.StopTurnTimerRPC();
        }

        // Apenas MasterClient chama o auto-end
        if (PhotonNetwork.IsMasterClient && gameManager != null)
        {
            // Usar DelayedCall para dar um pequeno delay antes de passar o turno
            this.DelayedCall(0.5f, AutoEndTurn);
        }
    }

    /// <summary>
    /// Passa o turno automaticamente
    /// </summary>
    private void AutoEndTurn()
    {
        if (gameManager != null)
        {
            DebugHelper.Log("[TurnTimer] Chamando AutoEndTurn no GameManager");
            gameManager.AutoEndTurn();
        }
    }

    /// <summary>
    /// Reseta o timer para o valor padrão
    /// </summary>
    public void ResetTimer()
    {
        remainingTime = timeLimit;
        hasAutoEnded = false;
        UpdateTimerDisplay();
    }
}
