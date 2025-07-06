using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class ZoneTextTrigger : MonoBehaviour
{
    public GameObject promptCanvas;
    public TMP_Text promptText;
    public string message = "마우스를 사용해 보세요";
    public float displayDuration = 5f;

    [SerializeField]
    private DialogueManager dialogManager;

    private bool hasTriggered;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
        }

        if (promptCanvas != null)
        {
            promptCanvas.SetActive(false);
        }

        if (dialogManager == null)
        {
           // dialogManager = FindObjectOfType<DialogueManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player"))
        {
            return;
        }

        hasTriggered = true;
        StartCoroutine(PlayDialogueThenShowPrompt());
    }

    private IEnumerator PlayDialogueThenShowPrompt()
    {
        dialogManager.PlayDialogue("souththRoom", "boss");

        // 대사가 시작될 때까지 대기
        yield return new WaitUntil(() => dialogManager.isPlayingDialogue);

        // 대사가 끝날 때까지 대기
        yield return new WaitUntil(() => !dialogManager.isPlayingDialogue);

        ShowMessage();
    }

    private void ShowMessage()
    {
        if (promptCanvas == null || promptText == null)
        {
            return;
        }

        promptText.text = message;
        promptCanvas.SetActive(true);

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (promptCanvas != null)
        {
            promptCanvas.SetActive(false);
        }

        hideCoroutine = null;
    }
}
