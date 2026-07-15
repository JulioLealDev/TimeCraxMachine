using System.Collections;
using UnityEngine;
using TMPro;

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

    [Header("PersonsCardImage")]
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
    }

    public void OnSlotAssigned(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex > 2) return;
        assigned[slotIndex] = true;

        if (assigned[0] && assigned[1] && assigned[2])
        {
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

        for (int i = 0; i < 3; i++)
        {
            bool isCorrect = shuffled != null
                && i < shuffled.Count
                && names[i] != null
                && names[i].text == shuffled[i].name;

            if (!isCorrect) anyWrong = true;

            correct[i]?.SetActive(isCorrect);
            incorrect[i]?.SetActive(!isCorrect);

            if (i < 2)
                yield return new WaitForSeconds(1f);
        }

        var gameManager = FindFirstObjectByType<GameManager>();

        // Se qualquer resposta errada, dispara o wrongSlot imediatamente
        if (anyWrong)
            gameManager?.HandlePersonsWrong();

        // Fecha a NewTimeline
        gameManager?.CloseNewTimeline();

        // t+2.5s: reset completo do PersonsFrame
        yield return new WaitForSeconds(2.5f);

        ResetState();
        cardImage01?.ResetToDefault();
        cardImage02?.ResetToDefault();
        cardImage03?.ResetToDefault();

        gameManager?.ResetPersonsFrame(); // desativa personsFrame e agenda zoom out internamente
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
