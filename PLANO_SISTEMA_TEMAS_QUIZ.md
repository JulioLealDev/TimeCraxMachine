# Plano de Implementação: Sistema de Temas com Quiz - TimeCrax Machine Game

## Resumo

Migrar o sistema de temas para suportar:
- Número variável de cartas (não mais fixo em 7)
- Sistema de Quiz obrigatório (4 tipos)
- Novo fluxo: acertar slot → quiz → resultado

## Requisitos Confirmados

| Requisito | Decisão |
|-----------|---------|
| Quizzes | Obrigatórios em todas as cartas |
| Momento do Quiz | Após acertar o slot |
| Erro no Quiz | Carta volta ao deck |
| Spawn de Cartas | Slots fixos na cena (ativar/desativar) |

---

## Estrutura da Nova API

```json
{
  "name": "string",
  "resume": "string",
  "recommendation": "string",
  "image": "string",
  "uploadSessionId": "guid",
  "cards": [
    {
      "orderIndex": 0,
      "year": 0,
      "era": "string",
      "caption": "string",
      "imageUrl": "string",
      "imageQuiz": {
        "question": "string",
        "options": [{ "imageUrl": "string" }],
        "correctIndex": 0
      },
      "textQuiz": {
        "question": "string",
        "options": [{ "text": "string" }],
        "correctIndex": 0
      },
      "trueFalseQuiz": {
        "statement": "string",
        "answer": true
      },
      "correlationQuiz": {
        "items": [{ "imageUrl": "string", "text": "string" }]
      }
    }
  ]
}
```

---

## Fase 1: Modelos de Dados

### 1.1 Criar `Assets/Scripts/Themes/QuizModels.cs` (NOVO)

```csharp
namespace TimeCrax.Themes
{
    public enum QuizType { None, ImageQuiz, TextQuiz, TrueFalseQuiz, CorrelationQuiz }

    [Serializable] public class QuizOption { string text; string imageUrl; string localImagePath; }
    [Serializable] public class ImageQuiz { string question; List<QuizOption> options; int correctIndex; }
    [Serializable] public class TextQuiz { string question; List<QuizOption> options; int correctIndex; }
    [Serializable] public class TrueFalseQuiz { string statement; bool answer; }
    [Serializable] public class CorrelationItem { string imageUrl; string localImagePath; string text; }
    [Serializable] public class CorrelationQuiz { List<CorrelationItem> items; }
    [Serializable] public class CardQuizData { ImageQuiz, TextQuiz, TrueFalseQuiz, CorrelationQuiz + HasQuiz() }
}
```

### 1.2 Modificar `Assets/Scripts/Themes/ThemeModels.cs`

Adicionar a `ThemeCard`:
- `string caption` (novo campo da API)
- `CardQuizData quizData` (dados do quiz)

Adicionar classes Response para API:
- `ImageQuizResponse`, `TextQuizResponse`, `TrueFalseQuizResponse`, `CorrelationQuizResponse`

---

## Fase 2: Download de Temas

### 2.1 Modificar `Assets/Scripts/Themes/ThemeDownloader.cs`

1. **Novo método `ConvertQuizData()`** - Converter responses da API para QuizModels
2. **Novo método `DownloadQuizImages()`** - Baixar imagens de ImageQuiz e CorrelationQuiz
3. **Modificar `DownloadThemeCoroutine()`** - Incluir download de imagens de quiz após cartas

Nomenclatura de arquivos:
- `quiz_{cardIndex}_option_{i}.webp` (ImageQuiz)
- `correlation_{cardIndex}_item_{i}.webp` (CorrelationQuiz)

---

## Fase 3: Sistema de Cartas Dinâmico

### 3.1 Modificar `Assets/Scripts/DeckEvent.cs`

- **Remover**: `private int[] numbers = { 1, 2, 3, 4, 5, 6, 7 };`
- **Adicionar**: `private int cardCount;`
- **Novo método**: `InitializeForTheme(int numberOfCards)` - Gera lista dinâmica

### 3.2 Modificar `Assets/Scripts/EventSlot.cs`

- **Modificar `CheckIfWin()`**: `if (slotsFilled == 7)` → `if (slotsFilled == deckEvent.GetCardCount())`
- Slots extras na cena ficam desativados

### 3.3 Modificar `Assets/Scripts/RandomMaterial.cs`

- **Remover**: Arrays fixos de tamanho 7
- **Adicionar**: `InitializeForTheme(ThemeData theme)` - Aloca arrays dinamicamente
- **Modificar**: Carregar texturas do ThemeStorage ao invés de Materials fixos

### 3.4 Modificar `Assets/Scripts/EventCard.cs`

- **Adicionar**: `ThemeCard themeCard` (referência aos dados do tema)
- **Adicionar**: `SetThemeCard()`, `GetThemeCard()`, `HasQuiz()`

---

## Fase 4: Sistema de Quiz

### 4.1 Criar `Assets/Scripts/Quiz/QuizManager.cs` (NOVO)

```csharp
public class QuizManager : MonoBehaviourPunCallbacks
{
    // Singleton
    // Eventos: OnQuizCompleted(bool correct)

    // Métodos principais:
    void StartQuiz(ThemeCard card, int slotCount)
    void SubmitAnswer(int selectedIndex)
    void SubmitTrueFalseAnswer(bool answer)
    void SubmitCorrelationAnswer(List<int> order)

    // RPCs (Multiplayer):
    [PunRPC] void RPC_StartQuiz(int slotCount, int quizType)
    [PunRPC] void RPC_QuizResult(int slotCount, bool correct)
}
```

### 4.2 Criar `Assets/Scripts/Quiz/QuizUI.cs` (NOVO)

Gerencia os 4 painéis de UI:
- ImageQuizPanel (4 botões com imagens)
- TextQuizPanel (4 botões com texto)
- TrueFalsePanel (botões Verdadeiro/Falso)
- CorrelationPanel (drag & drop)

### 4.3 Criar Prefab de UI (Unity Editor)

```
QuizCanvas
├── Background (semi-transparente)
├── TimerBar
├── QuestionText
├── ImageQuizPanel
├── TextQuizPanel
├── TrueFalsePanel
├── CorrelationPanel
└── ResultFeedback
```

---

## Fase 5: Integração do Quiz no Fluxo de Jogo

### 5.1 Modificar `Assets/Scripts/EventSlot.cs`

Novo fluxo em `ClickedRightSlot()`:

```
1. Jogador acerta slot
2. SE carta tem quiz:
   - QuizManager.StartQuiz(card, slotCount)
   - Aguarda OnQuizCompleted
   - SE acertou quiz: FinalizeCorrectSlot()
   - SE errou quiz: QuizFailed() → carta volta ao deck
3. SE carta não tem quiz:
   - FinalizeCorrectSlot() (fluxo atual)
```

### 5.2 Novo RPC `QuizFailed(int slotCount)`

- Carta volta ao deck (`DeckEvent.eventList.Add(slotCount)`)
- Animação de erro
- Slot volta para "Selectable"
- SOM de erro

---

## Fase 6: Integração no GameManager

### 6.1 Modificar `Assets/Scripts/GameManager.cs`

Em `StartNewGame()`:
```csharp
// Carregar tema
currentTheme = ThemeStorage.GetTheme(themeId);

// Inicializar sistemas
deckEvent.InitializeForTheme(currentTheme.cardCount);
randomMaterial.InitializeForTheme(currentTheme);

// Ativar/desativar slots conforme cardCount
ActivateSlotsForCardCount(currentTheme.cardCount);
```

---

## Arquivos Críticos para Modificação

| Arquivo | Tipo | Prioridade |
|---------|------|------------|
| `Assets/Scripts/Themes/ThemeModels.cs` | Modificar | Alta |
| `Assets/Scripts/Themes/QuizModels.cs` | Criar | Alta |
| `Assets/Scripts/Themes/ThemeDownloader.cs` | Modificar | Alta |
| `Assets/Scripts/DeckEvent.cs` | Modificar | Alta |
| `Assets/Scripts/EventSlot.cs` | Modificar | Alta |
| `Assets/Scripts/RandomMaterial.cs` | Modificar | Média |
| `Assets/Scripts/EventCard.cs` | Modificar | Média |
| `Assets/Scripts/Quiz/QuizManager.cs` | Criar | Alta |
| `Assets/Scripts/Quiz/QuizUI.cs` | Criar | Média |
| `Assets/Scripts/GameManager.cs` | Modificar | Média |

---

## Ordem de Implementação

1. **Modelos** - QuizModels.cs + ThemeModels.cs
2. **Download** - ThemeDownloader.cs (adaptar para nova API)
3. **Cartas Dinâmicas** - DeckEvent, RandomMaterial, EventCard
4. **Quiz System** - QuizManager + QuizUI
5. **Integração** - EventSlot + GameManager
6. **UI Unity** - Criar prefabs de quiz
7. **Testes Multiplayer**

---

## Considerações Multiplayer (Photon)

- Somente o jogador do turno responde o quiz
- Todos os jogadores veem o quiz (sincronizado via RPC)
- Timer sincronizado
- Resultado distribuído via RPC para todos

---

## Fluxo do Jogo (Resumo Visual)

```
┌─────────────────┐
│ Comprar Carta   │
└────────┬────────┘
         ▼
┌─────────────────┐
│ Posicionar Slot │
└────────┬────────┘
         ▼
    ┌────────────┐
    │ Slot Certo?│
    └─────┬──────┘
          │
    ┌─────┴─────┐
    │           │
   SIM         NÃO
    │           │
    ▼           ▼
┌────────┐  ┌────────────┐
│  QUIZ  │  │ Malfunction│
└───┬────┘  └────────────┘
    │
┌───┴───┐
│       │
ACERTOU ERROU
│       │
▼       ▼
┌──────────┐  ┌─────────────┐
│ Confirma │  │ Carta volta │
│  Carta   │  │  ao deck    │
└──────────┘  └─────────────┘
```

---

## Data de Criação

08/01/2026
