# Plano de Implementação: Sistema de Temas com Quiz - TimeCrax Machine Game

## Status: Implementação de Código Concluída

**Atualizado em:** 09/01/2026

---

## Resumo

Migrar o sistema de temas para suportar:
- Temas com número variável de cartas (ex: 15, 20, 30...)
- **Seleção aleatória de 7 cartas** do tema ao iniciar partida
- Sistema de Quiz obrigatório (4 tipos)
- Novo fluxo: acertar slot → quiz → resultado

## Requisitos Confirmados

| Requisito | Decisão |
|-----------|---------|
| Cartas por tema | Variável (N cartas) |
| Cartas por partida | **Fixo em 7** (selecionadas aleatoriamente) |
| Slots na timeline | **Fixo em 7** (mantém estrutura atual) |
| Quizzes | Obrigatórios em todas as cartas |
| Momento do Quiz | Após acertar o slot |
| Erro no Quiz | Carta volta ao deck |

---

## Progresso da Implementação

| Fase | Status | Descrição |
|------|--------|-----------|
| Fase 1 | ✅ Concluída | Modelos de Dados |
| Fase 2 | ✅ Concluída | Download de Temas |
| Fase 3 | ✅ Concluída | Seleção Aleatória de Cartas |
| Fase 4 | ✅ Concluída | Sistema de Quiz |
| Fase 5 | ✅ Concluída | Integração no Fluxo de Jogo |
| Fase 6 | ✅ Concluída | Integração no GameManager |
| Fase 7 | ⏳ Pendente | Criar Prefabs de UI (Unity Editor) |
| Fase 8 | ⏳ Pendente | Testes Multiplayer |

---

## Arquivos Modificados/Criados

| Arquivo | Tipo | Status |
|---------|------|--------|
| `Assets/Scripts/Themes/QuizModels.cs` | Criado | ✅ |
| `Assets/Scripts/Themes/ThemeModels.cs` | Modificado | ✅ |
| `Assets/Scripts/Themes/ThemeDownloader.cs` | Modificado | ✅ |
| `Assets/Scripts/RandomMaterial.cs` | Modificado | ✅ |
| `Assets/Scripts/EventCard.cs` | Modificado | ✅ |
| `Assets/Scripts/EventSlot.cs` | Modificado | ✅ |
| `Assets/Scripts/DeckEvent.cs` | Modificado | ✅ |
| `Assets/Scripts/GameManager.cs` | Modificado | ✅ |
| `Assets/Scripts/Quiz/QuizManager.cs` | Criado | ✅ |
| `Assets/Scripts/Quiz/QuizUI.cs` | Criado | ✅ |

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

## Fase 7: Criar Prefab de UI (Unity Editor) - PENDENTE

### Estrutura do QuizCanvas

```
QuizCanvas
├── Background (Image semi-transparente, cor preta alpha 0.7)
├── QuizPanel
│   ├── TimerBar (Image com fillAmount, horizontal)
│   ├── QuestionText (TextMeshProUGUI)
│   ├── ImageQuizPanel
│   │   ├── ImageOptionButton0 (Button + RawImage)
│   │   ├── ImageOptionButton1 (Button + RawImage)
│   │   ├── ImageOptionButton2 (Button + RawImage)
│   │   └── ImageOptionButton3 (Button + RawImage)
│   ├── TextQuizPanel
│   │   ├── TextOptionButton0 (Button + TextMeshProUGUI)
│   │   ├── TextOptionButton1 (Button + TextMeshProUGUI)
│   │   ├── TextOptionButton2 (Button + TextMeshProUGUI)
│   │   └── TextOptionButton3 (Button + TextMeshProUGUI)
│   ├── TrueFalsePanel
│   │   ├── StatementText (TextMeshProUGUI)
│   │   ├── TrueButton (Button)
│   │   └── FalseButton (Button)
│   ├── CorrelationPanel
│   │   ├── CorrelationImages (4 RawImages)
│   │   ├── CorrelationTexts (4 TextMeshProUGUI)
│   │   └── ConfirmButton (Button)
│   └── ResultFeedback
│       ├── ResultText (TextMeshProUGUI)
│       └── ResultIcon (Image)
└── QuizUI (Script Component)
```

### Passos para Criar no Unity Editor

1. **Criar Canvas**
   - GameObject > UI > Canvas
   - Renomear para "QuizCanvas"
   - Adicionar CanvasGroup component

2. **Adicionar QuizManager**
   - Criar GameObject vazio na cena principal
   - Adicionar script `QuizManager.cs`
   - Adicionar PhotonView component

3. **Adicionar QuizUI**
   - Adicionar script `QuizUI.cs` ao QuizCanvas
   - Arrastar referências no Inspector:
     - quizCanvas, canvasGroup
     - questionText, timerBar
     - Painéis e botões de cada tipo de quiz
     - resultFeedback, resultText, resultIcon

4. **Configurar Painéis**
   - Apenas um painel visível por vez
   - Iniciar todos desativados
   - QuizUI ativa o painel correto conforme tipo de quiz

---

## Fase 8: Testes Multiplayer - PENDENTE

### Checklist de Testes

- [ ] Quiz inicia corretamente após acertar slot
- [ ] Todos os jogadores veem o quiz (RPC_StartQuiz)
- [ ] Apenas jogador do turno pode responder
- [ ] Timer sincronizado entre jogadores
- [ ] Resultado correto confirma carta no slot
- [ ] Resultado errado devolve carta ao deck
- [ ] Carta devolvida pode ser comprada novamente
- [ ] Sons tocam corretamente
- [ ] Animações funcionam (wrongSlot, etc.)
- [ ] Próximo turno inicia normalmente após quiz

---

## Fluxo do Jogo com Quiz

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

## Considerações Multiplayer (Photon)

- Somente o jogador do turno responde o quiz
- Todos os jogadores veem o quiz (sincronizado via RPC)
- Timer sincronizado
- Resultado distribuído via RPC para todos

### RPCs Implementados

| RPC | Script | Descrição |
|-----|--------|-----------|
| `RPC_StartQuiz` | QuizManager | Inicia quiz em todos os clientes |
| `RPC_QuizResult` | QuizManager | Envia resultado do quiz |
| `QuizFailed` | EventSlot | Carta volta ao deck |

---

## Data de Criação

08/01/2026

## Última Atualização

09/01/2026 - Implementação de código concluída
