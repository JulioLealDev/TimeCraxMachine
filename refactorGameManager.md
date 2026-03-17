# Plano de Refatoração do GameManager

## Visão Geral

O `GameManager.cs` possui **~1955 linhas** e concentra múltiplas responsabilidades, violando o princípio de responsabilidade única (SRP). Este documento detalha um plano de refatoração incremental para distribuir responsabilidades entre managers especializados.

---

## Análise do Estado Atual

### Responsabilidades Identificadas no GameManager

| # | Responsabilidade | Linhas Aprox. | Prioridade |
|---|------------------|---------------|------------|
| 1 | Ciclo de Vida do Jogo | ~150 | Alta |
| 2 | Gerenciamento de Turnos | ~350 | Alta |
| 3 | Gerenciamento de Timer | ~80 | Média |
| 4 | Gerenciamento de Jogadores/Platenames | ~200 | Alta |
| 5 | Gerenciamento de Componentes/Malfunction | ~250 | Alta |
| 6 | Gerenciamento de Cartas de Reparo | ~150 | Média |
| 7 | Gerenciamento de UI (Info/Round) | ~100 | Baixa |
| 8 | Gerenciamento de Estado (Block/Deactivate) | ~80 | Baixa |
| 9 | Timeout Handling | ~120 | Média |
| 10 | Cache de Referências | ~100 | Baixa |

### Managers Existentes

| Manager | Responsabilidade Atual | Estado |
|---------|------------------------|--------|
| `TurnManager` | Sincronização de turnos, materiais de platename | Parcial |
| `PlayerManager` | Cache de players, notificação de saída | Parcial |
| `ComponentManager` | Inicialização de componentes | Parcial |
| `ThermometerManager` | Temperatura e malfunction | Completo |

---

## Plano de Refatoração por Fases

### Fase 1: Consolidar TurnManager (Prioridade: ALTA)

**Objetivo:** Mover toda a lógica de turnos para `TurnManager`

#### Métodos a Mover do GameManager → TurnManager

```
FirstTurn()
Turn()
CheckTimeAndIndex()
SyncTurn() [RPC]
SyncTurnWithRound() [RPC]
SyncTurnInternal()
StartTurn()
EndTurn()
WaitForFinishTurn()
FinishTurn() [RPC]
ShowRoundInfo()
HideRoundInfo()
DisableGameInfo()
```

#### Campos a Mover

```csharp
private int round;
private int roundCompare;
private int time;
private PlayerScript[] orderedPlayers;
```

#### Novo TurnManager (Estrutura)

```csharp
public class TurnManager : MonoBehaviourPunCallbacks
{
    // Singleton já existe

    // === Campos de Estado ===
    private int round;
    private int roundCompare;
    private int time;
    private PlayerScript[] orderedPlayers;

    // === Referências ===
    [SerializeField] private GameObject gameInfo;
    [SerializeField] private Material plateNameMaterial;
    [SerializeField] private Material plateNameMaterial2;

    // === Propriedades Públicas ===
    public int CurrentRound => round;
    public int CurrentTime => time;
    public PlayerScript[] OrderedPlayers => orderedPlayers;
    public PlayerScript CurrentPlayer => orderedPlayers?[time];

    // === Métodos Públicos ===
    public void Initialize() { }
    public void StartFirstTurn() { }
    public void NextTurn() { }
    public void EndCurrentTurn() { }

    // === RPCs ===
    [PunRPC] public void SyncTurn(int syncedTime) { }
    [PunRPC] public void SyncTurnWithRound(int syncedTime, int syncedRound) { }
    [PunRPC] public void FinishTurn() { }

    // === UI de Turno ===
    private void ShowRoundInfo() { }
    private void HideRoundInfo() { }

    // === Callbacks ===
    public event Action OnTurnStarted;
    public event Action OnTurnEnded;
    public event Action<int> OnRoundChanged;
}
```

#### Passos de Implementação

1. Adicionar campos faltantes ao TurnManager
2. Mover métodos um por um, testando após cada mudança
3. Criar eventos/callbacks para notificar GameManager
4. Atualizar referências no GameManager para usar TurnManager.Instance

---

### Fase 2: Consolidar PlayerManager (Prioridade: ALTA)

**Objetivo:** Mover toda a lógica de jogadores e platenames

#### Métodos a Mover do GameManager → PlayerManager

```
UpdatePlayersIndex() [RPC]
RemovePlayersPlatenames() [RPC]
ResetAllPlatenames()
ShowLeftPlayerInfo() [RPC]
HideLeftPlayerInfo()
CheckQuitGamePlayer()
ChangePlateNameMaterial() [RPC]
ChangeRepairCardsView()
GiveCard()
GiveRepairCard() [RPC]
```

#### Campos a Mover

```csharp
private PlayerScript[] players;
private int[] playersList;
private int initialPlayersNumber;
private PlayerScript[] orderedPlayers; // compartilhado com TurnManager
```

#### Novo PlayerManager (Estrutura)

```csharp
public class PlayerManager : MonoBehaviourPunCallbacks
{
    // Singleton já existe

    // === Campos de Estado ===
    private PlayerScript[] players;
    private int[] playersList;
    private int initialPlayersNumber;

    // === Referências ===
    [SerializeField] private GameObject gameInfo;
    [SerializeField] private GameObject playerLeftBackground;
    [SerializeField] private Material plateNameMaterial;
    [SerializeField] private Material plateNameMaterial2;

    // === Métodos Públicos ===
    public void Initialize(int playerCount) { }
    public PlayerScript GetLocalPlayer() { }
    public PlayerScript GetPlayerByIndex(int index) { }
    public PlayerScript GetCurrentTurnPlayer() { }
    public void CheckForDisconnectedPlayers() { } // substituir Update() do GameManager

    // === Platenames ===
    [PunRPC] public void RPC_RemovePlayerPlatename(int index) { }
    public void ResetAllPlatenames() { }
    [PunRPC] public void RPC_ChangePlateNameMaterial(int plateNameIndex) { }

    // === Repair Cards ===
    public void ChangeRepairCardsView(PlayerScript player) { }
    public void GiveCard(int numberPlayer) { }
    [PunRPC] public void RPC_GiveRepairCard(int numberPlayer) { }

    // === Player Left ===
    public void CheckQuitGamePlayer() { }
    [PunRPC] public void RPC_ShowLeftPlayerInfo(string nickname) { }

    // === Callbacks ===
    public event Action<PlayerScript> OnPlayerLeft;
    public event Action<PlayerScript> OnPlayerJoined;
}
```

---

### Fase 3: Consolidar ComponentManager (Prioridade: ALTA)

**Objetivo:** Mover toda a lógica de componentes e malfunction

#### Métodos a Mover do GameManager → ComponentManager

```
RandomComponentNumber()
ComponentRandom() [RPC]
Roulettecomponent() [Coroutine]
AddMalfunctionInComponent()
SetUpComponents()
ResetAllComponents()
CheckGameOverCondition()
```

#### Campos a Mover

```csharp
private MachineComponent[] timeCraxComponents;
private List<int> componentList;
private List<Transform> componentsWithAnimator;
public int randomId;
```

#### Novo ComponentManager (Estrutura)

```csharp
public class ComponentManager : MonoBehaviourPunCallbacks
{
    // Singleton já existe

    // === Campos de Estado ===
    private MachineComponent[] components;
    private List<Transform> componentsWithAnimator;
    private int currentRandomId;

    // === Referências ===
    [SerializeField] private GameObject environment;
    [SerializeField] private SoundEffects soundEffects;

    // === Métodos Públicos ===
    public void Initialize() { }
    public void SelectRandomComponent() { }
    public void AddMalfunctionToComponent(int componentId) { }
    public void ResetAllComponents() { }
    public void SetupComponentsForTurn(PlayerScript currentPlayer) { }
    public bool CheckGameOverCondition() { } // retorna true se game over

    // === RPCs ===
    [PunRPC] public void RPC_ComponentRandom(int id) { }

    // === Propriedades ===
    public MachineComponent[] Components => components;
    public int CriticalComponentCount { get; }

    // === Callbacks ===
    public event Action OnGameOverCondition;
    public event Action<MachineComponent> OnMalfunctionAdded;
}
```

---

### Fase 4: Criar TimerManager (Prioridade: MÉDIA)

**Objetivo:** Centralizar lógica de timer de turno

#### Métodos a Mover do GameManager → TimerManager

```
StartTurnTimerRPC()
StopTurnTimerRPC()
SyncTurnTimer()
RPC_SyncTurnTimer() [RPC]
RPC_StopTurnTimer() [RPC]
RPC_StartTurnTimer() [RPC]
AutoEndTurn()
RPC_HandleTimeoutCleanup() [RPC]
TimeoutMalfunction()
FinishTurnAfterTimeout()
```

#### Nova Classe TimerManager

```csharp
namespace TimeCrax.Managers
{
    public class TimerManager : MonoBehaviourPunCallbacks
    {
        private static TimerManager _instance;
        public static TimerManager Instance => _instance;

        [SerializeField] private TurnTimer turnTimer;

        // === Métodos Públicos ===
        public void StartTimer(float duration) { }
        public void StopTimer() { }
        public void SyncTime(float time) { }

        // === RPCs ===
        [PunRPC] public void RPC_StartTimer(float time) { }
        [PunRPC] public void RPC_StopTimer() { }
        [PunRPC] public void RPC_SyncTime(float time) { }

        // === Timeout ===
        public void HandleTimeout() { }
        [PunRPC] public void RPC_HandleTimeoutCleanup() { }

        // === Callbacks ===
        public event Action OnTimerExpired;
        public event Action<float> OnTimerTick;
    }
}
```

---

### Fase 5: Criar UIManager (Prioridade: BAIXA)

**Objetivo:** Centralizar lógica de UI do jogo

#### Métodos a Mover

```
ShowRoundInfo() → já vai para TurnManager
HideRoundInfo() → já vai para TurnManager
DisableGameInfo()
DisableOnlyGameInfo()
```

#### Campos de UI a Centralizar

```csharp
[SerializeField] private GameObject gameInfo;
[SerializeField] private GameObject hud;
[SerializeField] private FinishTurn endButton;
[SerializeField] private GameObject quitButton;
[SerializeField] private GameOver gameOver;
[SerializeField] private Victory victory;
```

---

### Fase 6: Simplificar GameManager (Prioridade: FINAL)

**Objetivo:** GameManager como coordenador central mínimo

#### GameManager Final (~300 linhas estimadas)

```csharp
public class GameManager : MonoBehaviourPunCallbacks
{
    // === Referências aos Managers ===
    // (acessados via Singleton)

    // === Referências de Cena (que não pertencem a nenhum manager) ===
    [SerializeField] private CameraController gameCamera;
    [SerializeField] private DeckEvent deckEvent;
    [SerializeField] private GameObject deckRepair;
    [SerializeField] private GameObject timeline;
    [SerializeField] private BackgroundMusic backgroundMusic;
    [SerializeField] private RandomMaterial randomMaterial;

    // === Estado do Jogo ===
    private bool gameIsOn;
    public static bool IsInTurnTransition { get; set; }

    // === Ciclo de Vida ===
    public void StartNewGame() { }
    public void StartGame() { }
    [PunRPC] public void ShowHUD() { }
    public void BackToMenu() { }

    // === Coordenação ===
    private void SubscribeToManagerEvents() { }
    private void UnsubscribeFromManagerEvents() { }

    // === Handlers de Eventos ===
    private void OnTurnStarted() { }
    private void OnTurnEnded() { }
    private void OnGameOver() { }
    private void OnPlayerLeft(PlayerScript player) { }
}
```

---

## Ordem de Execução Recomendada

```
Semana 1: Fase 1 (TurnManager)
    ├── Dia 1-2: Mover campos e métodos básicos de turno
    ├── Dia 3-4: Mover RPCs e testar sincronização
    └── Dia 5: Testar multiplayer completo

Semana 2: Fase 2 (PlayerManager)
    ├── Dia 1-2: Mover platenames e repair cards
    ├── Dia 3-4: Mover player left notification
    └── Dia 5: Testar multiplayer

Semana 3: Fase 3 (ComponentManager)
    ├── Dia 1-2: Mover malfunction logic
    ├── Dia 3-4: Mover roulette e game over
    └── Dia 5: Testar completo

Semana 4: Fases 4-6 (Timer, UI, Cleanup)
    ├── Dia 1-2: Criar TimerManager
    ├── Dia 3: Limpar GameManager
    └── Dia 4-5: Testes finais
```

---

## Checklist de Migração por Método

### TurnManager

| Método | Status | Testado |
|--------|--------|---------|
| `FirstTurn()` | [ ] Pendente | [ ] |
| `Turn()` | [ ] Pendente | [ ] |
| `CheckTimeAndIndex()` | [ ] Pendente | [ ] |
| `SyncTurn()` | [ ] Pendente | [ ] |
| `SyncTurnWithRound()` | [ ] Pendente | [ ] |
| `SyncTurnInternal()` | [ ] Pendente | [ ] |
| `StartTurn()` | [ ] Pendente | [ ] |
| `EndTurn()` | [ ] Pendente | [ ] |
| `WaitForFinishTurn()` | [ ] Pendente | [ ] |
| `FinishTurn()` | [ ] Pendente | [ ] |
| `ShowRoundInfo()` | [ ] Pendente | [ ] |
| `HideRoundInfo()` | [ ] Pendente | [ ] |

### PlayerManager

| Método | Status | Testado |
|--------|--------|---------|
| `UpdatePlayersIndex()` | [ ] Pendente | [ ] |
| `RemovePlayersPlatenames()` | [ ] Pendente | [ ] |
| `ResetAllPlatenames()` | [ ] Já existe | [ ] |
| `ShowLeftPlayerInfo()` | [ ] Já existe | [ ] |
| `HideLeftPlayerInfo()` | [ ] Já existe | [ ] |
| `CheckQuitGamePlayer()` | [ ] Pendente | [ ] |
| `ChangePlateNameMaterial()` | [ ] Pendente | [ ] |
| `ChangeRepairCardsView()` | [ ] Já existe | [ ] |
| `GiveCard()` | [ ] Pendente | [ ] |
| `GiveRepairCard()` | [ ] Já existe | [ ] |

### ComponentManager

| Método | Status | Testado |
|--------|--------|---------|
| `RandomComponentNumber()` | [ ] Pendente | [ ] |
| `ComponentRandom()` | [ ] Pendente | [ ] |
| `Roulettecomponent()` | [ ] Pendente | [ ] |
| `AddMalfunctionInComponent()` | [ ] Pendente | [ ] |
| `SetUpComponents()` | [ ] Pendente | [ ] |
| `ResetAllComponents()` | [ ] Pendente | [ ] |
| `CheckGameOverCondition()` | [ ] Pendente | [ ] |

---

## Dependências Entre Managers

```
                    ┌─────────────────┐
                    │   GameManager   │
                    │  (Coordenador)  │
                    └────────┬────────┘
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│  TurnManager    │ │ PlayerManager   │ │ComponentManager │
│                 │ │                 │ │                 │
│ - round/time    │ │ - players[]     │ │ - components[]  │
│ - orderedPlayers│◄┼─ GetCurrent()   │ │ - malfunction   │
│ - turn RPCs     │ │ - repair cards  │ │ - game over     │
└────────┬────────┘ └─────────────────┘ └────────┬────────┘
         │                                       │
         │          ┌─────────────────┐          │
         └─────────►│  TimerManager   │◄─────────┘
                    │                 │
                    │ - turn timer    │
                    │ - timeout       │
                    └─────────────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │ThermometerManager│
                    │   (Existente)   │
                    └─────────────────┘
```

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Quebrar sincronização multiplayer | Alta | Crítico | Testar cada RPC após mover |
| Introduzir null references | Média | Alto | Adicionar null checks em todos os acessos |
| Ordem de inicialização incorreta | Média | Alto | Usar eventos/callbacks ao invés de referências diretas |
| Perder funcionalidade existente | Baixa | Crítico | Manter GameManager funcional durante todo o processo |

---

## Testes Necessários

### Testes Unitários
- [ ] TurnManager.NextTurn() incrementa corretamente
- [ ] PlayerManager.GetCurrentTurnPlayer() retorna jogador correto
- [ ] ComponentManager.CheckGameOverCondition() detecta 2 componentes críticos

### Testes de Integração
- [ ] Turno passa corretamente entre jogadores
- [ ] Malfunction é aplicado ao componente sorteado
- [ ] Game over é acionado com 2 componentes críticos

### Testes Multiplayer
- [ ] Sincronização de turno entre 2 clientes
- [ ] Sincronização de turno entre 4 clientes
- [ ] Jogador sair durante seu turno
- [ ] Jogador sair durante turno de outro
- [ ] Timeout de turno sincronizado

---

## Métricas de Sucesso

| Métrica | Antes | Depois (Meta) |
|---------|-------|---------------|
| Linhas no GameManager | ~1955 | ~300 |
| Responsabilidades no GameManager | 10 | 2-3 |
| Managers especializados | 4 | 6 |
| Cobertura de testes | 0% | 50%+ |

---

## Conclusão

Este plano divide a refatoração em **6 fases incrementais**, permitindo que o jogo continue funcional durante todo o processo. Cada fase pode ser implementada, testada e commitada independentemente.

**Tempo estimado total:** 4 semanas (trabalho parcial)

**Recomendação:** Começar pela **Fase 1 (TurnManager)** pois é a mais crítica e estabelece o padrão para as demais fases.
