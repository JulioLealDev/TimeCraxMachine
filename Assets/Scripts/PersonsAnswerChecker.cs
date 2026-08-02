using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using TimeCrax.Core;
using TimeCrax.Managers;
using TimeCrax.Themes;

public class PersonsAnswerChecker : MonoBehaviour
{
    public static PersonsAnswerChecker Instance { get; private set; }

    [Header("Nome por slot")]
    [SerializeField] private TMP_Text personName01;
    [SerializeField] private TMP_Text personName02;
    [SerializeField] private TMP_Text personName03;

    [Header("Outline - PersonsDescription")]
    [SerializeField] private OutlineComponent descriptionOutline01;
    [SerializeField] private OutlineComponent descriptionOutline02;
    [SerializeField] private OutlineComponent descriptionOutline03;

    [Header("Outline - PersonsCardImage")]
    [SerializeField] private OutlineComponent cardImageOutline01;
    [SerializeField] private OutlineComponent cardImageOutline02;
    [SerializeField] private OutlineComponent cardImageOutline03;

    [Header("Ícones de resultado")]
    [SerializeField] private GameObject correctIcon01;
    [SerializeField] private GameObject correctIcon02;
    [SerializeField] private GameObject correctIcon03;
    [SerializeField] private GameObject incorrectIcon01;
    [SerializeField] private GameObject incorrectIcon02;
    [SerializeField] private GameObject incorrectIcon03;
    [SerializeField] private SoundEffects soundEffects;

    [Header("ResetPersonsFrame()")]
    [SerializeField] private TMP_Text personText01;
    [SerializeField] private TMP_Text personText02;
    [SerializeField] private TMP_Text personText03;
    [SerializeField] private GameObject personsFrame;
    [SerializeField] private PersonCardImage cardImage01;
    [SerializeField] private PersonCardImage cardImage02;
    [SerializeField] private PersonCardImage cardImage03;

    private bool[] assigned = new bool[3];

    void Awake()
    {
        Instance = this;
        HideAllIcons();
        DisableAllOutlines();
    }

    public void ResetState()
    {
        assigned = new bool[3];
        HideAllIcons();
        DisableAllOutlines();

        if (personName01 != null) personName01.text = string.Empty;
        if (personName02 != null) personName02.text = string.Empty;
        if (personName03 != null) personName03.text = string.Empty;

        cardImage01?.ResetToDefault();
        cardImage02?.ResetToDefault();
        cardImage03?.ResetToDefault();
    }

    public void OnSlotAssigned(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex > 2) return;
        assigned[slotIndex] = true;

        if (assigned[0] && assigned[1] && assigned[2])
        {
            GameStateManager.TransitionTo(GamePhase.IM_ChallengeFeedback);
            InputBlocker.Block();
            Cursor.visible = true;
            StartCoroutine(RevealFeedbackSequence());
        }
    }

    private IEnumerator RevealFeedbackSequence()
    {
        // Aguarda um frame para OutlineAction terminar qualquer limpeza de hover pendente
        yield return null;
        yield return new WaitForSeconds(1f);

        var shuffled = GameManager.ShuffledPersonEntries;
        TMP_Text[]   names     = { personName01,   personName02,   personName03   };
        GameObject[] correct   = { correctIcon01,  correctIcon02,  correctIcon03  };
        GameObject[] incorrect = { incorrectIcon01, incorrectIcon02, incorrectIcon03 };

        bool anyWrong = false;
        bool[] results = new bool[3];

        for (int i = 0; i < 3; i++)
        {
            bool isCorrect = shuffled != null
                && i < shuffled.Count
                && names[i] != null
                && names[i].text == shuffled[i].name;
            results[i] = isCorrect;
            if (!isCorrect) anyWrong = true;
        }

        var gameManager = FindFirstObjectByType<GameManager>();

        // Broadcast result to observers before showing locally so timing aligns
        if (gameManager != null && PhotonNetwork.InRoom)
            gameManager.photonView.RPC("RPC_ShowPersonsFeedback", RpcTarget.Others, results[0], results[1], results[2]);

        for (int i = 0; i < 3; i++)
        {
            correct[i]?.SetActive(results[i]);
            incorrect[i]?.SetActive(!results[i]);

            if (i < 2)
                yield return new WaitForSeconds(1f);
        }

        bool allCorrect = !anyWrong;
        if (allCorrect)
        {
            soundEffects.PlayRightSlotSound();
            SlotLinkManager.Instance?.CheckAndActivateLinks(GameManager.CurrentPersonsSlotCount);
        }

        var local = PlayerManager.Instance?.GetLocalPlayer();
        if (local != null)
        {
            if (gameManager != null && PhotonNetwork.InRoom)
                gameManager.photonView.RPC("RPC_TrackPersonsChallenge", RpcTarget.All, allCorrect, local.actorNumber, local.nickname);
            else if (allCorrect) MatchStats.AddPersonsChallengeCorrect(local.actorNumber, local.nickname);
            else MatchStats.AddPersonsError(local.actorNumber, local.nickname);
        }

        if (anyWrong)
        {
            soundEffects.PlayWrongSlotSound();
            if (gameManager != null && PhotonNetwork.InRoom)
                gameManager.photonView.RPC("RPC_HandlePersonsWrong", RpcTarget.All);
            else
                gameManager?.HandlePersonsWrong();
        }

        // Fecha a NewTimeline
        gameManager?.CloseNewTimeline();

        // t+2.5s: reset completo do PersonsFrame
        yield return new WaitForSeconds(2.5f);

        ResetState();
        cardImage01?.ResetToDefault();
        cardImage02?.ResetToDefault();
        cardImage03?.ResetToDefault();

        ResetPersonsFrame(gameManager);
    }

    public void OnSlotAssignedFromRPC(int slotIndex, string personName)
    {
        if (slotIndex < 0 || slotIndex > 2) return;

        PersonsCarousel.Instance?.CloseForObserver();

        TMP_Text[]       names = { personName01, personName02, personName03 };
        PersonCardImage[] cards = { cardImage01,  cardImage02,  cardImage03  };

        if (names[slotIndex] != null)
            names[slotIndex].text = personName;

        var entry = GameManager.ShuffledPersonEntries?.Find(e => e.name == personName);
        if (entry != null && !string.IsNullOrEmpty(entry.localImagePath))
        {
            var texture = ThemeStorage.LoadLocalImage(entry.localImagePath);
            var renderer = cards[slotIndex]?.GetComponent<Renderer>();
            if (renderer != null && texture != null)
                renderer.material.mainTexture = texture;
        }

        assigned[slotIndex] = true;

        if (assigned[0] && assigned[1] && assigned[2])
        {
            GameStateManager.TransitionTo(GamePhase.IM_ChallengeFeedback);
            InputBlocker.Block();
            Cursor.visible = true;
        }
    }

    public void ShowPersonsFeedbackForObserver(bool slot0, bool slot1, bool slot2)
    {
        StartCoroutine(ObserverPersonsFeedbackSequence(slot0, slot1, slot2));
    }

    private IEnumerator ObserverPersonsFeedbackSequence(bool s0, bool s1, bool s2)
    {
        bool[]       results  = { s0, s1, s2 };
        GameObject[] correct  = { correctIcon01,  correctIcon02,  correctIcon03  };
        GameObject[] incorrect = { incorrectIcon01, incorrectIcon02, incorrectIcon03 };

        for (int i = 0; i < 3; i++)
        {
            correct[i]?.SetActive(results[i]);
            incorrect[i]?.SetActive(!results[i]);
            if (i < 2) yield return new WaitForSeconds(1f);
        }

        bool allCorrect = s0 && s1 && s2;
        if (allCorrect)
        {
            soundEffects?.PlayRightSlotSound();
            SlotLinkManager.Instance?.CheckAndActivateLinks(GameManager.CurrentPersonsSlotCount);
        }
        else
        {
            soundEffects?.PlayWrongSlotSound();
        }

        var gameManager = FindFirstObjectByType<GameManager>();
        gameManager?.CloseNewTimeline();

        yield return new WaitForSeconds(2.5f);

        ResetState();
        cardImage01?.ResetToDefault();
        cardImage02?.ResetToDefault();
        cardImage03?.ResetToDefault();

        ResetPersonsFrame(gameManager);
    }

    private void ResetPersonsFrame(GameManager gameManager)
    {
        if (personName01 != null) personName01.text = string.Empty;
        if (personName02 != null) personName02.text = string.Empty;
        if (personName03 != null) personName03.text = string.Empty;
        if (personText01 != null) personText01.text = string.Empty;
        if (personText02 != null) personText02.text = string.Empty;
        if (personText03 != null) personText03.text = string.Empty;

        foreach (var img in FindObjectsByType<PersonCardImage>(FindObjectsSortMode.None))
        {
            img.gameObject.tag = "Untagged";
            var col = img.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
        foreach (var desc in FindObjectsByType<PersonDescriptionClick>(FindObjectsSortMode.None))
        {
            desc.gameObject.tag = "Untagged";
            var col = desc.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        if (personsFrame != null) personsFrame.SetActive(false);
        EventSlot.ResetClickProtection();

        gameManager?.DelayedCall(0.5f, gameManager.ZoomOutAfterFeedback);
        gameManager?.CheckWinAfterMiniGame();
    }

    public void DisableInteraction()
    {
        foreach (var img in FindObjectsByType<PersonCardImage>(FindObjectsSortMode.None))
        {
            img.gameObject.tag = "Untagged";
            var col = img.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
        foreach (var desc in FindObjectsByType<PersonDescriptionClick>(FindObjectsSortMode.None))
        {
            desc.gameObject.tag = "Untagged";
            var col = desc.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }

    public void ApplyKillChallengeOption()
    {
        var candidates = new List<int>();
        for (int i = 0; i < 3; i++)
            if (!assigned[i]) candidates.Add(i);
        if (candidates.Count == 0) return;

        int targetSlot = candidates[UnityEngine.Random.Range(0, candidates.Count)];

        OutlineComponent[] outlines = { cardImageOutline01, cardImageOutline02, cardImageOutline03 };
        StartCoroutine(KillPersonSlotRoulette(candidates, outlines, targetSlot));
    }

    private IEnumerator KillPersonSlotRoulette(List<int> candidates, OutlineComponent[] outlines, int targetSlot)
    {
        int lastIdx = -1;
        float interval = 0.3f;

        // Desativar colliders durante a roulette
        foreach (var img in FindObjectsByType<PersonCardImage>(FindObjectsSortMode.None))
        {
            var col = img.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
        foreach (var desc in FindObjectsByType<PersonDescriptionClick>(FindObjectsSortMode.None))
        {
            var col = desc.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        for (int cond = 0; cond < 15; cond++)
        {
            int idx = UnityEngine.Random.Range(0, candidates.Count);
            while (idx == lastIdx && candidates.Count > 1)
                idx = UnityEngine.Random.Range(0, candidates.Count);
            lastIdx = idx;

            var outline = outlines[candidates[idx]];
            if (outline != null) outline.enabled = true;
            soundEffects?.PlayRouletteSound();
            yield return new WaitForSeconds(interval);
            if (outline != null) outline.enabled = false;

            interval -= 0.015f;
        }

        // Destaque final no slot alvo
        var finalOutline = outlines[targetSlot];
        if (finalOutline != null)
        {
            finalOutline.enabled = true;
            soundEffects?.PlayRouletteSound();
            yield return new WaitForSeconds(interval);
            finalOutline.enabled = false;
        }

        // Aplicar resposta correta
        var shuffled = GameManager.ShuffledPersonEntries;
        if (shuffled == null || targetSlot >= shuffled.Count) yield break;
        PersonEntry entry = shuffled[targetSlot];

        TMP_Text[]       names  = { personName01,  personName02,  personName03  };
        PersonCardImage[] cards = { cardImage01,   cardImage02,   cardImage03   };
        OutlineComponent[] descOutlines = { descriptionOutline01, descriptionOutline02, descriptionOutline03 };

        // Imagem e nome corretos
        var texture = ThemeStorage.LoadLocalImage(entry.localImagePath);
        var renderer = cards[targetSlot]?.GetComponent<Renderer>();
        if (renderer != null && texture != null)
            renderer.material.mainTexture = texture;
        if (names[targetSlot] != null)
            names[targetSlot].text = entry.name;

        // Bloquear apenas PersonCardImage do slot sorteado
        if (cards[targetSlot] != null)
        {
            cards[targetSlot].gameObject.tag = "Untagged";
            var col = cards[targetSlot].GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        // PersonDescriptionClick do slot sorteado permanece interativo
        if (descOutlines[targetSlot] != null)
        {
            var col = descOutlines[targetSlot].GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }

        // Reativar colliders dos slots restantes (não sorteados)
        for (int i = 0; i < 3; i++)
        {
            if (i == targetSlot || assigned[i]) continue;

            if (cards[i] != null)
            {
                var col = cards[i].GetComponent<Collider>();
                if (col != null) col.enabled = true;
            }
            if (descOutlines[i] != null)
            {
                var col = descOutlines[i].GetComponent<Collider>();
                if (col != null) col.enabled = true;
            }
        }

        OnSlotAssigned(targetSlot);
    }

    private void HideAllIcons()
    {
        GameObject[] all = { correctIcon01, correctIcon02, correctIcon03,
                             incorrectIcon01, incorrectIcon02, incorrectIcon03 };
        foreach (var icon in all)
            icon?.SetActive(false);
    }

    private void DisableAllOutlines()
    {
        OutlineComponent[] all = { descriptionOutline01, descriptionOutline02, descriptionOutline03,
                                   cardImageOutline01,   cardImageOutline02,   cardImageOutline03   };
        foreach (var outline in all)
        {
            if (outline == null) continue;
            outline.SetColor(Color.white);
            outline.enabled = false;
        }
    }
}
