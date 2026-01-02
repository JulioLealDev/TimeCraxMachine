# CLAUDE.md - Instruções para o Claude

Este arquivo contém instruções para o Claude ao trabalhar neste projeto.

## Sobre o Projeto

**TimeCrax Machine** é um jogo 3D multiplayer cooperativo online desenvolvido em Unity. Os jogadores devem sequenciar eventos na linha do tempo corretamente antes que a máquina do tempo pare de funcionar.

## Stack Tecnológica

- **Engine:** Unity 6000.3.2f1 (C#)
- **Networking:** Photon Unity Networking (PUN 2.52) para multiplayer
- **Animações:** LeanTween
- **UI:** TextMesh Pro
- **Outros:** QuickOutline, AI Navigation

## Migração para Unity 6 (Concluída)

### Status
- Migração de Unity 2022.3.51f1 para Unity 6000.3.2f1 concluída em 01/01/2025

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
│   ├── Auth/      # Sistema de autenticação
│   └── Core/      # Utilitários (DebugHelper, SessionData, etc.)
├── Editor/        # Scripts de Editor (ferramentas)
├── Prefabs/       # Prefabs do Unity
├── Scenes/        # Cenas do jogo
│   ├── Intro.unity        # Cena de intro (fade in/out)
│   ├── LoginScreen.unity  # Tela de login
│   └── TimeCraxMachine.unity  # Cena principal do jogo
├── Materials/     # Materiais e shaders
├── Models/        # Modelos 3D
├── Textures/      # Texturas
├── Fonts/         # Fontes (Cinzel, Marcellus)
├── Animations/    # Animações
├── HUD/           # Elementos de interface
├── Sounds/        # Arquivos de áudio
├── Photon/        # Configurações do Photon
└── Resources/     # Assets carregados em runtime
```

## Fluxo de Cenas

```
Intro → LoginScreen → TimeCraxMachine
```

1. **Intro**: Exibe logo/imagem por 5s com fade in/out (2s cada)
2. **LoginScreen**: Login com email/senha, botão de registro redireciona para website
3. **TimeCraxMachine**: Menu principal e jogo

## Scripts Principais

- `GameManager.cs` - Gerenciador principal do jogo
- `GameConnection.cs` - Gerenciamento de conexão multiplayer
- `CameraController.cs` - Controle de câmera (antigo Camera.cs)
- `MachineComponent.cs` - Componentes da máquina do tempo (antigo Component.cs)
- `PlayerScript.cs` - Lógica do jogador
- `EventCard.cs` / `EventSlot.cs` - Sistema de cartas de eventos
- `DeckEvent.cs` / `DeckRepair.cs` - Sistema de baralhos
- `CreateRoom.cs` / `EnterRoom.cs` - Sistema de salas multiplayer
- `UserNameDisplay.cs` - Exibe nome do usuário logado em TextMeshPro 3D

## Sistema de Autenticação (Assets/Scripts/Auth/) - Namespace: TimeCrax.Auth

Integração com o TimeCrax Backend (ASP.NET Core 8.0 + PostgreSQL) para login.

### Scripts de Autenticação

| Script | Descrição |
|--------|-----------|
| `AuthModels.cs` | DTOs para requests/responses da API |
| `AuthService.cs` | Serviço HTTP para comunicação com a API (Singleton) |
| `TokenManager.cs` | Gerenciamento de tokens JWT (armazenamento em PlayerPrefs) |
| `LoginUI.cs` | Controller da UI de login (tempo mínimo 3s, Tab navigation, bloqueio de input) |
| `IntroController.cs` | Controller da cena de intro (fade in/out automático) |

### Configuração

1. Adicionar `AuthService` a um GameObject na cena de login
2. Configurar `apiBaseUrl` no Inspector:
   - Dev: `http://localhost:5139`
   - Prod: `https://api.timecrax.com` (exemplo)

### Endpoints da API

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/auth/login` | Login com email/senha |
| POST | `/auth/register` | Registro de novo usuário |
| GET | `/me` | Dados do usuário logado |

### Como usar AuthService

```csharp
using TimeCrax.Auth;

// Login
AuthService.Instance.Login("email@exemplo.com", "SenhaSegura123", result =>
{
    if (result.Success)
    {
        // Token salvo automaticamente
        // Dados do usuário buscados automaticamente via /me
        Debug.Log("Logado como: " + TokenManager.UserName);
    }
    else
    {
        Debug.Log("Erro: " + result.ErrorMessage);
    }
});

// Verificar login
if (TokenManager.IsLoggedIn)
{
    string userName = TokenManager.UserName;  // Primeiro nome do usuário
    string userId = TokenManager.UserId;
}

// Logout
AuthService.Instance.Logout();
```

### Exibir Nome do Usuário (UserNameDisplay)

```csharp
// Adicionar UserNameDisplay.cs a um objeto com TextMeshPro
// O script busca automaticamente:
// 1. TokenManager.UserName (se logado)
// 2. SessionData.Nickname (fallback)
// 3. "Jogador" (padrão)
```

**Setup no Unity:**
1. Criar TextMeshPro 3D como filho do objeto desejado
2. Adicionar componente `UserNameDisplay`
3. Ajustar Character Spacing (valores negativos para texto menor)

### Requisitos de Senha

- Mínimo 12 caracteres
- 1 letra maiúscula (A-Z)
- 1 letra minúscula (a-z)
- 1 dígito (0-9)

### Rate Limiting

A API tem limites de tentativas:
- Login: 5 tentativas / 15 min (por email)
- Registro: 10 tentativas / 60 min (por IP)

### Estrutura da Cena de Login

```
LoginScreen
├── Canvas
│   ├── LoginPanel
│   │   ├── EmailInput (TMP_InputField)
│   │   ├── PasswordInput (TMP_InputField)
│   │   ├── LoginButton (Button)
│   │   ├── RegisterButton (Button) → Abre URL do website
│   │   └── ErrorText (TextMeshProUGUI)
│   └── LoadingPanel (tempo mínimo 3s)
├── EventSystem
├── Camera
└── LoginUI (GameObject com LoginUI.cs e AuthService.cs)
```

### Estrutura da Cena de Intro

```
Intro
├── Canvas
│   ├── ContentCanvasGroup (imagem/logo)
│   └── FadeImage (preto, criado automaticamente se não existir)
└── IntroController (GameObject com IntroController.cs)
```

## Ferramentas de Editor (Assets/Editor/)

| Script | Menu | Descrição |
|--------|------|-----------|
| `PlayFromLoginScene.cs` | Edit > Play From Login Scene (Ctrl+Shift+P) | Inicia jogo pela LoginScreen |
| `ClearAuthTokens.cs` | Edit > Clear Auth Tokens | Limpa tokens salvos (para testar login) |
| `EnableMeshReadWrite.cs` | Edit > Fix All Mesh Read-Write | Habilita Read/Write em modelos (OutlineComponent) |
| `MigrateToUnity6.cs` | Tools > TimeCrax | Migração de APIs obsoletas |
| `FindMissingScripts.cs` | Tools > TimeCrax | Encontra scripts faltando |
| `FontReplacerTool.cs` | Tools > TimeCrax | Substitui fontes em massa |

## Comandos de Build

O projeto é construído através do Unity Editor. Builds ficam em:
- `Builds/` - Builds para desktop
- `MobileBuilds/` - Builds para mobile

## Diretrizes de Código

1. **Linguagem:** Scripts em C# seguindo convenções Unity
2. **Namespace:** `TimeCrax.Core` para utilitários, `TimeCrax.Auth` para autenticação
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
- **IMPORTANTE:** Sempre salvar cenas (Ctrl+S) após alterações no Unity Editor

## Scripts Utilitários (Assets/Scripts/Core/) - Namespace: TimeCrax.Core

- `DebugHelper.cs` - Wrapper para Debug.Log que é removido em builds de produção
- `CoroutineHelper.cs` - Helper para substituir Invoke() por Coroutines
- `SessionData.cs` - Dados de sessão em memória (substitui PlayerPrefs para dados temporários)

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

## Arquivos Importantes

- `ProjectSettings/` - Configurações do Unity (não modificar manualmente)
- `Packages/` - Pacotes do Unity Package Manager
- `.gitignore` - Arquivos ignorados pelo Git

## Notas

- O projeto usa Visual Studio / VS Code como IDE
- Arquivos `.meta` são gerados automaticamente pelo Unity
- Não modificar arquivos na pasta `Library/` (cache do Unity)
- Após alterações no Unity Editor, sempre salvar a cena antes de commitar

## Referências Úteis

- [Unity 6 LTS Releases](https://unity.com/releases/unity-6/support)
- [PUN 2 Version History](https://doc.photonengine.com/pun/current/reference/version-history)
- [Photon PUN Documentation](https://doc.photonengine.com/pun/current/getting-started/pun-intro)
