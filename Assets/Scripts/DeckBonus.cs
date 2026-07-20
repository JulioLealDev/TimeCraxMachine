using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

public class DeckBonus : MonoBehaviourPunCallbacks
{
    [SerializeField] private DeckEvent deckEvent;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Canvas gameInfo;
    [SerializeField] private SoundEffects soundEffects;

    private MeshCollider cachedMeshCollider;

    private void Awake()
    {
        cachedMeshCollider = GetComponent<MeshCollider>();
    }

    public void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
        if (CameraController.IsAnimating) return;
        if (!GameManager.TryBeginClick(this)) return;

        if (gameObject.CompareTag("Disabled"))
        {
            photonView.RPC("ClickDraw", RpcTarget.All, 1);

            var players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.GetYourTurn())
                {
                    if (player.GetNumberOfBonusCards() == 5)
                    {
                    }
                    else
                    {

                        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
                        gameInfo.gameObject.SetActive(true);

                        foreach (var info in infos)
                        {
                            if (info.gameObject.name == "ActionInfoBackground")
                            {
                                info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                            }
                        }

                        this.DelayedCall(1.5f, HideActionInfo);
                    }
                }

            }


        }
        else
        {
            // Enviar requisição ao MasterClient para processar a compra
            GameStateManager.TransitionTo(GamePhase.IM_DrewBonusCard);
            photonView.RPC("RequestDrawBonusCard", RpcTarget.MasterClient);
        }

    }

    /// <summary>
    /// RPC enviado ao MasterClient para processar a compra de carta de bonus
    /// </summary>
    [PunRPC]
    public void RequestDrawBonusCard()
    {
        // Apenas MasterClient processa e sincroniza para todos
        if (PhotonNetwork.IsMasterClient)
        {

            // Sortear tipo da carta (0-5 para os 6 tipos)
            int randomType = Random.Range(0, 6);

            // Sincronizar para todos os clientes com o tipo sorteado
            photonView.RPC("ExecuteDrawBonusCard", RpcTarget.All, randomType);
        }
    }

    /// <summary>
    /// RPC executado em todos os clientes para comprar a carta
    /// </summary>
    [PunRPC]
    public void ExecuteDrawBonusCard(int cardType)
    {

        // Tocar som
        soundEffects.PlayDrawCardSound();

        // Bloquear ações em todos os clientes
        gameManager.BlockActions();

        // Desabilitar botão FinishTurn temporariamente (será reativado após animação)
        gameManager.ActivateFinishButton(false);

        // Fechar compartimento esquerdo após comprar carta
        gameManager.CloseLeftCompartment();

        // Apenas o jogador da vez instancia a carta
        var players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.GetYourTurn() && player.photonView.IsMine)
            {
                // Instanciar carta com o tipo embutido nos dados — recebido via OnPhotonInstantiate em todos os clientes
                var cardObject = PhotonNetwork.Instantiate("bonusCard", Vector3.zero, Quaternion.identity, 0, new object[] { (int)cardType });

                // Colocar carta dentro de Camera/HUD/
                var hud = GameObject.Find("Camera/HUD");
                if (hud != null)
                {
                    cardObject.transform.SetParent(hud.transform, false);
                    cardObject.transform.localPosition = new Vector3(36.93f, -20.59f, -40.29f);
                    cardObject.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
                }
                break;
            }
        }

        GameManager.ResetClick(this);
    }

    [PunRPC]
    public void ClickDraw(int idSound, PhotonMessageInfo info)
    {
        if (info.Sender != photonView.Owner) return;
        if(idSound == 1)
        {
            soundEffects.TagSound();
        }
        else if (idSound == 2)
        {
            soundEffects.PlayDrawCardSound();
        }
    }

    public void HideActionInfo()
    {
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "ActionInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        this.DelayedCall(0.5f, DisableGameInfo);
    }

    public void DisableGameInfo()
    {
        gameInfo.gameObject.SetActive(false);
        GameManager.ResetClick(this);
    }

    public void ResetClickProtection()
    {
        GameManager.ResetClick(this);
    }

    public void ActivateCollider()
    {
        if (cachedMeshCollider != null)
            cachedMeshCollider.enabled = true;
    }


}
