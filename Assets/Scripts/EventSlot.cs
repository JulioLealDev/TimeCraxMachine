using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;
using TimeCrax.Quiz;

public class EventSlot : MonoBehaviourPunCallbacks
{
    [SerializeField] private int slotNumber;
    [SerializeField] private int randomNumber;
    [SerializeField] private SoundEffects soundEffects;

    private GameManager gameManager;
    private BackgroundMusic backgroundMusic;
    private Victory victory;
    private QuizManager quizManager;

    // Estado do quiz pendente
    private int pendingQuizSlotCount = -1;
    private EventCard pendingQuizCard;

    public int SlotNumber => slotNumber;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        victory = FindFirstObjectByType<Victory>();
        backgroundMusic = FindFirstObjectByType<BackgroundMusic>();
        quizManager = QuizManager.Instance;

        // Inscrever no evento de quiz completado
        if (quizManager != null)
        {
            quizManager.OnQuizCompleted += OnQuizCompleted;
        }
    }

    private void OnDestroy()
    {
        if (quizManager != null)
        {
            quizManager.OnQuizCompleted -= OnQuizCompleted;
        }
    }

    public void OnMouseDown()
    {
        var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);

        photonView.RPC("ClickSlotSound", RpcTarget.All, 1);

        foreach (var card in eventCards)
        {
            if (card.CompareTag("Drew"))
            {
                SetUpSlots(false, "Undestructable");

                if (slotNumber == card.slotCount)
                {
                    //DebugHelper.Log("� igual!");
                    photonView.RPC("ClickSlotSound", RpcTarget.All, 2);

                    photonView.RPC("ClickedRightSlot", RpcTarget.All, card.slotCount);
                }
                else
                {
                    //DebugHelper.Log("No� igual!");
                    photonView.RPC("ClickSlotSound", RpcTarget.All, 3);
                    this.DelayedCall(5f, RandomComponent);

                    photonView.RPC("ClickedWrongSlot", RpcTarget.All, card.slotCount);

                }
            }

            //photonView.RPC("ClickSlot", RpcTarget.All);
        }


    }

    [PunRPC]
    public void ClickSlotSound(int idSound)
    {
        if(idSound == 1)
        {
            soundEffects.PlayClickSlotSound();
        }
        else if(idSound == 2)
        {
            this.DelayedCall(3.3f, PlayRightSound);
        }
        else if(idSound == 3) 
        {
            this.DelayedCall(3.3f, PlayWrongSound);
        }

    }

    [PunRPC]
    public void PlayRightSound()
    {
        soundEffects.PlayRightSlotSound();
    }

    [PunRPC]
    public void PlayWrongSound()
    {
        soundEffects.PlayWrongSlotSound();
    }

    public void RandomComponent()
    {
        gameManager.RandomComponentNumber();
    }

    [PunRPC]
    public void ClickedWrongSlot(int slotCount)
    {
        var cards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        foreach (var card in cards)
        {
            //DebugHelper.Log("cardslotcount: "+ card.slotCount+" -- slotcount:"+slotCount);
            if (card.slotCount == slotCount)
            {
                card.gameObject.GetComponent<Animator>().SetInteger("slotClicked", slotNumber);
                //DebugHelper.Log("cardname: "+card.name);
                card.gameObject.GetComponent<Animator>().SetBool("wrongSlot", true);
                card.tag = "Undestructable";
                card.waitToDistance();
            }
        }
    }

    [PunRPC]
    public void ClickedRightSlot(int slotCount)
    {
        var cards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        EventCard targetCard = null;

        foreach (var card in cards)
        {
            if (card.slotCount == slotCount)
            {
                targetCard = card;
                break;
            }
        }

        if (targetCard == null) return;

        // Verificar se a carta tem quiz
        if (targetCard.HasQuiz() && quizManager != null)
        {
            // Guardar estado pendente para quando o quiz terminar
            pendingQuizSlotCount = slotCount;
            pendingQuizCard = targetCard;

            // Animar a carta para o slot (mas ainda não confirma)
            targetCard.gameObject.GetComponent<Animator>().SetInteger("slotClicked", slotNumber);

            // Iniciar quiz (apenas o Master Client inicia, sincroniza via RPC)
            if (PhotonNetwork.IsMasterClient)
            {
                var themeCard = targetCard.GetThemeCard();
                quizManager.StartQuiz(themeCard, slotCount);
            }
        }
        else
        {
            // Sem quiz - fluxo original
            FinalizeCorrectSlot(slotCount, targetCard);
        }
    }

    /// <summary>
    /// Finaliza a colocação correta da carta no slot (após quiz ou sem quiz)
    /// </summary>
    private void FinalizeCorrectSlot(int slotCount, EventCard card)
    {
        gameObject.tag = "Disabled";

        var deckEvent = FindFirstObjectByType<DeckEvent>();
        deckEvent.RemoveIndex(slotCount);

        card.gameObject.GetComponent<Animator>().SetInteger("slotClicked", slotNumber);
        card.tag = "Disabled";
        card.waitToDistance();

        CheckIfWin();
    }

    /// <summary>
    /// Callback quando o quiz é completado
    /// </summary>
    private void OnQuizCompleted(bool correct)
    {
        if (pendingQuizSlotCount < 0 || pendingQuizCard == null) return;

        if (correct)
        {
            DebugHelper.Log($"[EventSlot] Quiz correto! Finalizando slot {pendingQuizSlotCount}");
            FinalizeCorrectSlot(pendingQuizSlotCount, pendingQuizCard);
        }
        else
        {
            DebugHelper.Log($"[EventSlot] Quiz errado! Carta volta ao deck");
            // Quiz falhou - carta volta ao deck
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("QuizFailed", RpcTarget.All, pendingQuizSlotCount);
            }
        }

        // Limpar estado pendente
        pendingQuizSlotCount = -1;
        pendingQuizCard = null;
    }

    [PunRPC]
    public void QuizFailed(int slotCount)
    {
        DebugHelper.Log($"[EventSlot] RPC QuizFailed - slotCount: {slotCount}");

        // Encontrar a carta
        var cards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        foreach (var card in cards)
        {
            if (card.slotCount == slotCount)
            {
                // Animar erro
                card.gameObject.GetComponent<Animator>().SetBool("wrongSlot", true);
                card.tag = "Undestructable";
                card.waitToDistance();
                break;
            }
        }

        // Reativar slots para nova tentativa
        this.DelayedCall(3.5f, () =>
        {
            SetUpSlots(true, "Selectable");
        });

        // Tocar som de erro
        soundEffects.PlayWrongSlotSound();
    }

    //[PunRPC]
    //public void ClickSlot()
    //{
    //    var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
    //    foreach (var card in eventCards)
    //    {
    //        if (card.CompareTag("Drew"))
    //        {
    //            SetUpSlots(false, "Undestructable");
    //            //DebugHelper.Log("CardName: " + card.slotCount + " -- SlotName: " + slotNumber);
    //            card.gameObject.GetComponent<Animator>().SetInteger("slotClicked", slotNumber);

    //            if (slotNumber == card.slotCount)
    //            {
    //                DebugHelper.Log("� igual!");
    //                card.tag = "Disabled";
    //                gameObject.tag = "Disabled";
    //                var deckEvent = FindFirstObjectByType<DeckEvent>();
    //                deckEvent.RemoveIndex(card.slotCount);
    //                card.waitToDistance();
    //                CheckIfWin();
    //            }
    //            else
    //            {
    //                var gameManager = FindFirstObjectByType<GameManager>();
    //                gameManager.RandomComponentNumber();
    //                card.gameObject.GetComponent<Animator>().SetBool("wrongSlot", true);
    //                card.tag = "Undestructable";
    //                DebugHelper.Log("No� igual!");
    //                card.waitToDistance();
    //            }
    //        }
    //    }
    //}

    public void SetUpSlots(bool activateSlot, string tag)
    {
        //DebugHelper.Log("SetUpSlots");

        var slots = FindObjectsByType<EventSlot>(FindObjectsSortMode.None);
        foreach (var slot in slots)
        {
            //DebugHelper.Log("slot "+slot.slotNumber+" -- tag: "+slot.tag);
            if (!slot.CompareTag("Disabled"))
            {
                slot.tag = tag;
                slot.GetComponentInChildren<MeshCollider>().enabled = activateSlot;
            }

        }

    }

    public void CheckIfWin()
    {
        var slots = FindObjectsByType<EventSlot>(FindObjectsSortMode.None);
        int slotsFilled = 0;
        foreach (var slot in slots)
        {
            //DebugHelper.Log("slot " + slot.slotNumber + " -- tag: " + slot.tag);
            if (slot.CompareTag("Disabled"))
            {
                slotsFilled++;
            }

        }
        if (slotsFilled == 7)
        {
            //GameOver gameOver = FindFirstObjectByType<GameOver>();
            //gameOver.gameIsOver = true;

            gameManager.DeactivateAll();
            gameManager.ResetAllComponents();
            gameManager.ResetAllPlatenames();
            this.DelayedCall(5.5f, Victory);
        }
    }

    public void Victory()
    {
        victory.transform.GetChild(0).gameObject.SetActive(true);
        gameManager.hud.SetActive(false);
        backgroundMusic.PlayVictorySound();
        //Invoke("ReturningToMenu", 2f);
    }

    //public void ReturningToMenu()
    //{
    //    victory.transform.GetChild(0).gameObject.SetActive(false);
    //    var gameManager = FindFirstObjectByType<GameManager>();
    //    gameManager.QuitGame();
    //}
}
