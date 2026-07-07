using System.Collections;
using UnityEngine;

public class DelayedInteraction : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        StartCoroutine(EnableInteractionNextFrame());
    }

    IEnumerator EnableInteractionNextFrame()
    {
        yield return null;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
}
