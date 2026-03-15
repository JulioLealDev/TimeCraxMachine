# Relatório de Tags - TimeCrax Machine Game

Este documento descreve todas as tags utilizadas no projeto, onde são usadas, para que servem e análise de relevância.

---

## Tags Identificadas

### 1. `Selectable`
**Propósito:** Indica que um objeto pode ser clicado/interagido pelo jogador.

**Arquivos onde é usada:**
| Arquivo | Linha | Uso |
|---------|-------|-----|
| `CameraController.cs` | 49 | Verifica se objeto filho é selecionável para ativar MeshCollider |
| `DeckEvent.cs` | 33 | Verifica se o deck pode ser clicado |
| `MachineComponent.cs` | 271 | Verifica se o componente pode ser reparado |
| `Menu.cs` | 19, 56, 125 | Gerencia estado de objetos no menu |
| `OutlineAction.cs` | 49, 83-85 | Aplica outline em objetos selecionáveis |
| `Timeline.cs` | 29 | Verifica se timeline pode ser clicada |
| `GameManager.cs` | 902, 909, 920-921, 930 | Define objetos como selecionáveis durante o turno |
| `ComponentManager.cs` | 143 | Marca componentes com malfunction=1 como selecionáveis |

**Análise:** Tag essencial para o sistema de interação. Faz sentido e é bem utilizada.

---

### 2. `Disabled`
**Propósito:** Indica que um objeto está desabilitado e não pode ser interagido.

**Arquivos onde é usada:**
| Arquivo | Linha | Uso |
|---------|-------|-----|
| `DeckRepair.cs` | 24 | Verifica se deck está desabilitado para processar clique |
| `EventSlot.cs` | 241, 254, 388, 402 | Marca slots preenchidos como desabilitados |
| `GameManager.cs` | 898, 1490-1492, 1811-1812, 1820, 1829 | Bloqueia objetos quando ações não são permitidas |
| `GiveCards.cs` | 34 | Verifica estado do objeto |
| `ComponentManager.cs` | 111 | Desabilita componentes durante certas ações |

**Análise:** Tag essencial para controle de estado. Faz sentido e é bem utilizada.

---

### 3. `Component`
**Propósito:** Identifica os componentes da máquina do tempo que podem receber malfunctions.

**Arquivos onde é usada:**
| Arquivo | Linha | Uso |
|---------|-------|-----|
| `GameManager.cs` | 357, 1755 | Filtra componentes e reseta estado |
| `MachineComponent.cs` | 602 | Reseta tag ao reiniciar componente |
| `ComponentManager.cs` | 61, 179 | Ativa animators e reseta componentes |

**Análise:** Tag essencial para identificar componentes da máquina. Faz sentido.

---

### 4. `Drew`
**Propósito:** Indica que uma carta de evento foi comprada e está ativa.

**Arquivos onde é usada:**
| Arquivo | Linha | Uso |
|---------|-------|-----|
| `CameraController.cs` | 179 | Verifica se há carta comprada |
| `EventCard.cs` | 60 | Marca carta como comprada |
| `EventSlot.cs` | 62 | Verifica se há carta comprada para processar posicionamento |
| `GameManager.cs` | 1287 | Encontra carta comprada para devolver ao deck |

**Análise:** Tag essencial para o sistema de cartas de evento. Faz sentido.

---

### 5. `Undestructable`
**Propósito:** Protege objetos de serem destruídos ou terem seu estado alterado.

**Arquivos onde é usada:**
| Arquivo | Linha | Uso |
|---------|-------|-----|
| `EventSlot.cs` | 142, 329 | Marca carta que errou posicionamento |
| `LobbyOptions.cs` | 255 | Evita destruir certos objetos de sala |
| `LobbySearchUI.cs` | 43 | Pula objetos protegidos na busca |
| `Menu.cs` | 51, 105, 123 | Protege timeline de alterações de estado |

**Análise:** Tag com propósito duplo (proteção contra destruição E proteção contra mudança de estado). Poderia ser dividida em duas tags para maior clareza.

---

### 6. `Untagged`
**Propósito:** Tag padrão do Unity. Usada quando componente atinge malfunction crítico (=2).

**Arquivos onde é usada:**
| Arquivo | Linha | Uso |
|---------|-------|-----|
| `MachineComponent.cs` | 409 | Remove interação quando componente quebra totalmente |
| `GameManager.cs` | 1302 | Remove tag da carta devolvida ao deck |

**Análise:** Uso adequado da tag padrão do Unity para remover identificação especial.

---

### 7. `InRoom`
**Propósito:** Indica que o jogador está em uma sala (estado de menu/lobby).

**Arquivos onde é usada:**
| Arquivo | Linha | Uso |
|---------|-------|-----|
| `Menu.cs` | 47, 107 | Gerencia estado de objetos quando jogador entra em sala |

**Análise:** Tag específica para gerenciamento de estado do menu. Faz sentido, mas uso limitado.

---

### 8. `Sparks`
**Propósito:** Identifica os efeitos de partículas de faíscas nos componentes.

**Arquivos onde é usada:**
| Arquivo | Linha | Uso |
|---------|-------|-----|
| `MachineComponent.cs` | 110, 128 | Encontra objeto de sparks nos filhos do componente |

**Análise:** Tag útil para identificar efeitos visuais. Faz sentido.

---

### 9. `Smoke`
**Propósito:** Identifica os efeitos de partículas de fumaça nos componentes.

**Arquivos onde é usada:**
| Arquivo | Linha | Uso |
|---------|-------|-----|
| `MachineComponent.cs` | 114, 132 | Encontra objeto de smoke nos filhos do componente |

**Análise:** Tag útil para identificar efeitos visuais. Faz sentido.

---

## Resumo

### Tags em uso ativo:

| Tag | Quantidade de Usos | Status |
|-----|-------------------|--------|
| `Selectable` | 15+ | Essencial |
| `Disabled` | 12+ | Essencial |
| `Component` | 5 | Essencial |
| `Drew` | 4 | Essencial |
| `Undestructable` | 6 | Funcional, mas ambígua |
| `Untagged` | 2 | Adequada |
| `InRoom` | 2 | Funcional |
| `Sparks` | 2 | Funcional |
| `Smoke` | 2 | Funcional |

### Tags obsoletas (podem ser removidas):

| Tag | Status |
|-----|--------|
| `WorldHistoryMaterials` | Não usada |
| `NamePlayerTag` | Não usada |
| `GameInfo` | Não usada |
| `PlateName` | Não usada |
| `Victory` | Não usada |
| `GameOver` | Não usada |
| `InputName` | Não usada |
| `PlayerName` | Não usada |
| `Lobby` | Não usada |
| `Roulette` | Não usada |
| `GameController` | Não usada |
| `Player` | Não usada |

---

## Recomendações

### Tags que fazem sentido:
- `Selectable`, `Disabled`, `Component`, `Drew`, `Sparks`, `Smoke` - Todas têm propósitos claros e bem definidos.

### Tags com potencial de melhoria:

1. **`Undestructable`**
   - Problema: Usada para dois propósitos diferentes (evitar destruição E evitar mudança de estado)
   - Sugestão: Considerar dividir em `Protected` (não destruir) e `Locked` (não mudar estado)

2. **`InRoom`**
   - Problema: Uso muito limitado (apenas em Menu.cs)
   - Sugestão: Poderia ser substituída por uma variável de estado no próprio script

### Tags obsoletas (definidas no Unity mas NÃO usadas no código):

| Tag | Status |
|-----|--------|
| `WorldHistoryMaterials` | **OBSOLETA** - Não usada em nenhum script |
| `NamePlayerTag` | **OBSOLETA** - Não usada em nenhum script |
| `GameInfo` | **OBSOLETA** - Não usada em nenhum script |
| `PlateName` | **OBSOLETA** - Não usada em nenhum script |
| `Victory` | **OBSOLETA** - Existe classe Victory.cs, mas não é tag |
| `GameOver` | **OBSOLETA** - Existe classe GameOver.cs, mas não é tag |
| `InputName` | **OBSOLETA** - Não usada em nenhum script |
| `PlayerName` | **OBSOLETA** - Não usada em nenhum script |
| `Lobby` | **OBSOLETA** - Existe LobbyOptions.cs, mas não é tag |
| `Roulette` | **OBSOLETA** - Não usada em nenhum script |
| `GameController` | **OBSOLETA** - Não usada em nenhum script |
| `Player` | **OBSOLETA** - Existe prefab "Player", mas tag não é verificada |

**Recomendação:** Remover essas tags do Unity Tag Manager para evitar confusão.

### Observações adicionais:

1. O sistema de tags funciona como uma máquina de estados para interação:
   - `Component` → estado inicial dos componentes
   - `Selectable` → pode ser clicado
   - `Disabled` → não pode ser clicado
   - `Untagged` → componente destruído (malfunction=2)

2. Para cartas de evento:
   - `Untagged` → no deck
   - `Drew` → carta comprada/ativa
   - `Disabled` → carta posicionada em slot
   - `Undestructable` → carta que errou posicionamento

3. O código usa `.tag` e `.CompareTag()` de forma inconsistente. Recomenda-se usar sempre `.CompareTag()` por ser mais performático e seguro (não gera erro se a tag não existir).
