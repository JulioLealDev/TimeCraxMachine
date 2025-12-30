# Claude Instructions - Timecrax Machine Game

## Project Overview
Timecrax Machine is a multiplayer educational history card game built in Unity. Players place historical event cards on a timeline in chronological order. The game uses Photon PUN for real-time multiplayer networking.

## Tech Stack
- **Engine:** Unity 2022.3.51f1 (LTS)
- **Language:** C#
- **Networking:** Photon PUN 2 (Photon Unity Networking)
- **UI:** Unity UI + TextMesh Pro
- **Animations:** LeanTween
- **Platform:** PC, Mobile (Android/iOS)

## Project Structure
```
Assets/
├── Animations/      # Animation clips and controllers
├── HUD/             # UI elements and canvases
├── LeanTween/       # Tweening library
├── Materials/       # 3D materials
├── Models/          # 3D models (FBX, etc.)
├── Photon/          # Photon PUN configuration
├── Prefabs/         # Reusable game objects
├── QuickOutline/    # Outline shader for selection
├── Resources/       # Runtime-loaded assets
├── Scenes/          # Unity scenes
│   └── TimeCraxMachine.unity  # Main game scene
├── Scripts/         # C# game scripts
├── Sounds/          # Audio files (SFX, music)
├── TextMesh Pro/    # TMP assets
└── Textures/        # 2D textures and sprites
```

## Core Scripts

### Game Flow
- **GameManager.cs** - Main game controller, handles game state, rounds, turns
- **GameConnection.cs** - Photon connection and room management
- **Menu.cs** - Main menu logic
- **LobbyOptions.cs** - Lobby configuration and player management

### Networking (Photon)
- **CreateRoom.cs** - Room creation logic
- **EnterRoom.cs** - Room joining logic
- **RoomList.cs** - Available rooms display
- **Room.cs** - Room data model
- **PlayerScript.cs** - Player controller with network sync

### Gameplay
- **EventCard.cs** - Historical event card behavior
- **EventSlot.cs** - Timeline slot for placing cards
- **EventCardContent.cs** - Card data (year, description, image)
- **DeckEvent.cs** - Event card deck management
- **DeckRepair.cs** - Repair card deck management
- **RepairCard.cs** - Repair card behavior
- **Timeline.cs** - Timeline visualization
- **GiveCards.cs** - Card distribution logic
- **FinishTurn.cs** - End turn button logic

### UI/UX
- **Camera.cs** - Camera controls and zoom
- **Pages.cs** - UI page navigation
- **NameTag.cs** / **NamePlayerTag.cs** - Player name displays
- **SoundEffects.cs** - SFX manager
- **BackgroundMusic.cs** - Background music controller
- **Tutorial.cs** - Tutorial system

### Game States
- **Victory.cs** - Win condition handling
- **GameOver.cs** - Loss/end game handling
- **QuitGame.cs** / **QuitInGaming.cs** - Exit game logic

### Visual
- **OutlineAction.cs** / **OutlineComponent.cs** - Selection outline effects
- **RandomMaterial.cs** - Random material assignment
- **Sticker.cs** - Sticker/badge visuals

## Conventions

### Photon Networking
- Use `MonoBehaviourPunCallbacks` for networked scripts
- Use `[PunRPC]` attribute for remote procedure calls
- Check `PhotonNetwork.IsMasterClient` for host-only logic
- Use `photonView.RPC()` to call methods on all clients
- RPC targets: `RpcTarget.All`, `RpcTarget.Others`, `RpcTarget.MasterClient`

### Script Pattern
```csharp
using UnityEngine;
using Photon.Pun;

public class MyScript : MonoBehaviourPunCallbacks
{
    // Public fields for Inspector
    public GameObject myObject;

    // Private fields
    private int myValue;

    void Start() { }
    void Update() { }

    // Photon RPC
    [PunRPC]
    public void MyNetworkedMethod() { }
}
```

### Finding Objects
- Use `FindObjectOfType<T>()` to find managers
- GameManager is the central controller
- Camera script handles all camera movements

### Animations
- Use Animator components with bool/int parameters
- LeanTween for programmatic animations
- Common pattern: `GetComponent<Animator>().SetBool("paramName", true)`

## Game Mechanics

### Card System
- **Event Cards:** Historical events with year, placed on timeline
- **Repair Cards:** Special cards to fix mistakes
- Cards are drawn from decks and placed in slots

### Turn Flow
1. Player draws event card
2. Player places card on timeline slot
3. System validates chronological order
4. Turn passes to next player

### Multiplayer
- Master client controls game flow
- All game state synced via Photon RPCs
- Players identified by `PhotonNetwork.NickName`

## Common Patterns

### Network Sync
```csharp
// Host-only execution
if (PhotonNetwork.IsMasterClient)
{
    photonView.RPC("DoSomething", RpcTarget.All, parameter);
}

[PunRPC]
public void DoSomething(int param)
{
    // Executed on all clients
}
```

### Camera Control
```csharp
var camera = FindObjectOfType<Camera>();
camera.ZoomTimeline();
camera.DistanceTimeline();
```

### Sound
```csharp
var soundEffects = FindObjectOfType<SoundEffects>();
soundEffects.PlaySound("soundName");
```

## Build Targets
- **PC:** Windows standalone
- **Mobile:** Android APK, iOS (in Builds/ and MobileBuilds/)

## Important Notes
- Main scene: `Assets/Scenes/TimeCraxMachine.unity`
- Photon App ID configured in `Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset`
- Game requires internet connection for multiplayer
- Single scene architecture (all game states in one scene)

## Common Commands (Unity Editor)
- Play: Ctrl+P
- Build: File > Build Settings > Build
- Photon Dashboard: Window > Photon Unity Networking > Highlight Server Settings
