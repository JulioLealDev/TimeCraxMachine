using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

/// <summary>
/// Gerencia o cronômetro de turno com sincronização multiplayer.
/// Cada jogador tem um tempo limite para realizar suas ações.
/// Não usa PhotonView próprio - os RPCs são enviados através do GameManager.
/// </summary>
public class TurnTimer : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float timeLimit = 60f; // Timer fixo em 60 segundos
    [SerializeField] private float syncInterval = 1f; // Intervalo de sincronização

    [Header("Animator do Relógio")]
    [SerializeField] private Animator highestPointerAnimator;

    // Estado do timer
    private float remainingTime;
    private bool isRunning;
    private float lastSyncTime;

    // Valor original do timeLimit (para restaurar após reparo da bateria)
    private float originalTimeLimit;
    private bool isBatteryMalfunctioning = false;

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
        originalTimeLimit = timeLimit;
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
        remainingTime = time;
        isRunning = true;
        hasAutoEnded = false;
        lastSyncTime = Time.time;

        // Iniciar animação do relógio
        SetClockAnimatorStartCount(true);
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
        isRunning = false;

        // Parar animação do relógio
        SetClockAnimatorStartCount(false);
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
    /// Define o parâmetro startCount do animator do relógio
    /// </summary>
    private void SetClockAnimatorStartCount(bool value)
    {
        if (highestPointerAnimator != null && highestPointerAnimator.runtimeAnimatorController != null)
        {
            highestPointerAnimator.SetBool("startCount", value);
        }
    }

    /// <summary>
    /// Define o parâmetro halfTime do animator do relógio (efeito da bateria)
    /// </summary>
    private void SetClockAnimatorHalfTime(bool value)
    {
        if (highestPointerAnimator != null && highestPointerAnimator.runtimeAnimatorController != null)
        {
            foreach (var param in highestPointerAnimator.parameters)
            {
                if (param.name == "halfTime" && param.type == AnimatorControllerParameterType.Bool)
                {
                    highestPointerAnimator.SetBool("halfTime", value);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Chamado quando o tempo expira
    /// </summary>
    private void OnTimeExpired()
    {

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
    }

    /// <summary>
    /// Adiciona tempo ao timer (efeito da TimeCard)
    /// </summary>
    public void AddTime(float seconds)
    {
        if (!isRunning) return;

        remainingTime += seconds;

        // Sincronizar com outros clientes se for MasterClient
        if (PhotonNetwork.IsMasterClient && gameManager != null)
        {
            gameManager.SyncTurnTimer(remainingTime);
        }

    }

    /// <summary>
    /// Reduz o timeLimit pela metade (efeito da bateria com malfunction)
    /// </summary>
    public void ApplyBatteryMalfunction()
    {
        if (isBatteryMalfunctioning) return;

        isBatteryMalfunctioning = true;
        timeLimit = originalTimeLimit / 2f;
        SetClockAnimatorHalfTime(true);
    }

    /// <summary>
    /// Restaura o timeLimit original (quando bateria é reparada)
    /// </summary>
    public void RestoreBatteryEffect()
    {
        if (!isBatteryMalfunctioning) return;

        isBatteryMalfunctioning = false;
        timeLimit = originalTimeLimit;
        SetClockAnimatorHalfTime(false);
    }

    /// <summary>
    /// Reseta completamente o timer (usado ao iniciar nova partida)
    /// </summary>
    public void ResetToDefault()
    {
        isBatteryMalfunctioning = false;
        timeLimit = originalTimeLimit;
        remainingTime = timeLimit;
        hasAutoEnded = false;
        SetClockAnimatorHalfTime(false);
    }
}
