# CLAUDE.md - Instruções para o Claude

Este arquivo contém instruções para o Claude ao trabalhar neste projeto.

## Sobre o Projeto

**TimeCrax Machine** é um jogo 3D multiplayer cooperativo online desenvolvido em Unity. Os jogadores devem sequenciar eventos na linha do tempo corretamente antes que a máquina do tempo pare de funcionar.

## Stack Tecnológica

- **Engine:** Unity 6000.0.3 (C#)
- **Networking:** Photon Unity Networking (PUN 2.52) para multiplayer
- **Animações:** LeanTween
- **UI:** TextMesh Pro
- **Outros:** QuickOutline, AI Navigation

## Migração para Unity 6 (Concluída)

### Status
- Migração de Unity 2022.3.51f1 para Unity 6000.0.3 concluída em 31/12/2024

### Alterações Realizadas
1. **APIs obsoletas migradas (70 ocorrências):**
   - `FindObjectOfType<T>()` → `FindFirstObjectByType<T>()`
   - `FindObjectsOfType<T>()` → `FindObjectsByType<T>(FindObjectsSortMode.None)`
   - `FindObjectOfType<T>(true)` → `FindFirstObjectByType<T>(FindObjectsInactive.Include)`

2. **Classes renomeadas para evitar conflitos:**
   - `Camera.cs` → `CameraController.cs`
   - `Component.cs` → `MachineComponent.cs`

3. **Photon PUN atualizado:** v2.41 → v2.52

## Estrutura do Projeto

```
Assets/
├── Scripts/       # Scripts C# do jogo
├── Prefabs/       # Prefabs do Unity
├── Scenes/        # Cenas do jogo
├── Materials/     # Materiais e shaders
├── Models/        # Modelos 3D
├── Textures/      # Texturas
├── Animations/    # Animações
├── HUD/           # Elementos de interface
├── Sounds/        # Arquivos de áudio
├── Photon/        # Configurações do Photon
├── Resources/     # Assets carregados em runtime
└── Editor/        # Scripts de Editor (ferramentas)
```

## Scripts Principais

- `GameManager.cs` - Gerenciador principal do jogo
- `GameConnection.cs` - Gerenciamento de conexão multiplayer
- `CameraController.cs` - Controle de câmera (antigo Camera.cs)
- `MachineComponent.cs` - Componentes da máquina do tempo (antigo Component.cs)
- `PlayerScript.cs` - Lógica do jogador
- `EventCard.cs` / `EventSlot.cs` - Sistema de cartas de eventos
- `DeckEvent.cs` / `DeckRepair.cs` - Sistema de baralhos
- `CreateRoom.cs` / `EnterRoom.cs` - Sistema de salas multiplayer

## Comandos de Build

O projeto é construído através do Unity Editor. Builds ficam em:
- `Builds/` - Builds para desktop
- `MobileBuilds/` - Builds para mobile

## Diretrizes de Código

1. **Linguagem:** Scripts em C# seguindo convenções Unity
2. **Namespace:** `TimeCrax.Core` para utilitários. Scripts de gameplay usam `using TimeCrax.Core;`
3. **MonoBehaviour:** A maioria dos scripts herda de MonoBehaviour
4. **Photon:** Scripts de rede usam MonoBehaviourPunCallbacks
5. **Comentários:** Preferencialmente em português
6. **APIs Unity 6:** Usar `FindFirstObjectByType` e `FindObjectsByType` ao invés de versões obsoletas
7. **Novos scripts:** Sempre incluir `using TimeCrax.Core;` para acesso a DebugHelper, SessionData e DelayedCall

## Ao Modificar Código

- Manter compatibilidade com Photon PUN 2
- Testar sincronização multiplayer ao modificar lógica de jogo
- Verificar referências de prefabs ao adicionar novos componentes
- Respeitar o padrão de nomenclatura existente (PascalCase para classes e métodos públicos)
- Usar APIs compatíveis com Unity 6
- Não usar nomes de classes que conflitam com UnityEngine (Camera, Component, etc.)

## Melhorias Implementadas (31/12/2024)

### Alto - Concluídas
- [x] **Debug.Log wrapper** - Criado `DebugHelper.cs` que remove logs em builds de produção (269 ocorrências migradas)
- [x] **GetComponent caching** - `CameraController.cs` otimizado como exemplo. Padrão a seguir nos demais scripts.
- [x] **Substituir Invoke() por Coroutines** - 46 ocorrências migradas em 19 arquivos usando `this.DelayedCall()`

### Médio - Concluídas
- [x] **Revisar Update() desnecessários** - 5 métodos vazios removidos
- [x] **Substituir PlayerPrefs por SessionData** - Criado `SessionData.cs` para dados de sessão (nickname, gameStarted, numberOfPlayers)

### Médio - Em Progresso
- [~] **Usar [SerializeField]** - 14 scripts migrados (~55 campos). Padrão documentado abaixo para continuar.

### Baixo - Concluídas
- [x] **Adicionar namespaces** - `TimeCrax.Core` adicionado aos utilitários. 27 scripts atualizados com `using TimeCrax.Core;`

### Baixo - Pendente (Manual)
- [ ] **Reorganizar scripts em subpastas** - Estrutura proposta abaixo. Deve ser feito pelo Unity Editor para preservar .meta files

## Scripts Utilitários (Assets/Scripts/Core/) - Namespace: TimeCrax.Core

- `DebugHelper.cs` - Wrapper para Debug.Log que é removido em builds de produção
- `CoroutineHelper.cs` - Helper para substituir Invoke() por Coroutines
- `SessionData.cs` - Dados de sessão em memória (substitui PlayerPrefs para dados temporários)

### Estrutura de Pastas Proposta (Reorganização Manual)
```
Assets/Scripts/
├── Core/       # Utilitários (já existe) - TimeCrax.Core
├── Gameplay/   # GameManager, EventCard, EventSlot, DeckEvent, etc.
├── Network/    # GameConnection, Room, RoomList
├── UI/         # Menu, CreateRoom, EnterRoom, LobbyOptions, Pages
├── Audio/      # BackgroundMusic, SoundEffects
└── Visual/     # OutlineAction, OutlineComponent, RandomMaterial
```
**Nota:** Mover scripts pelo Unity Editor (arrastar no Project) para preservar arquivos .meta e referências.

### Como usar DebugHelper
```csharp
// Ao invés de:
Debug.Log("mensagem");

// Use:
DebugHelper.Log("mensagem");
```
Os logs são automaticamente removidos em builds de produção.

### Como usar CoroutineHelper
```csharp
// Ao invés de:
Invoke("MetodoX", 1.5f);

// Use:
StartCoroutine(CoroutineHelper.DelayedAction(1.5f, MetodoX));
// Ou:
this.DelayedCall(1.5f, MetodoX);
```

### Como usar SessionData
```csharp
// Ao invés de:
PlayerPrefs.SetString("nickname", value);
var name = PlayerPrefs.GetString("nickname");

// Use:
SessionData.Nickname = value;
var name = SessionData.Nickname;
```

### Padrão [SerializeField]
```csharp
// Ao invés de:
public Animator animator;
public GameObject target;

// Use:
[SerializeField] private Animator animator;
[SerializeField] private GameObject target;
```
Campos que precisam ser acessados externamente devem ter property pública:
```csharp
[SerializeField] private int value;
public int Value => value;
```

## Ferramentas de Editor

Scripts utilitários em `Assets/Editor/`:
- `MigrateToUnity6.cs` - Ferramenta de migração de APIs (já executada)
- `FindMissingScripts.cs` - Encontra e remove scripts faltando

## Arquivos Importantes

- `ProjectSettings/` - Configurações do Unity (não modificar manualmente)
- `Packages/` - Pacotes do Unity Package Manager
- `.gitignore` - Arquivos ignorados pelo Git

## Notas

- O projeto usa Visual Studio / VS Code como IDE
- Arquivos `.meta` são gerados automaticamente pelo Unity
- Não modificar arquivos na pasta `Library/` (cache do Unity)

## Referências Úteis

- [Unity 6 LTS Releases](https://unity.com/releases/unity-6/support)
- [PUN 2 Version History](https://doc.photonengine.com/pun/current/reference/version-history)
- [Photon PUN Documentation](https://doc.photonengine.com/pun/current/getting-started/pun-intro)
