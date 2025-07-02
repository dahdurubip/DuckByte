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

    private bool hasTriggered = false;
    private Coroutine hideCoroutine;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger) col.isTrigger = true;

        if (promptCanvas != null)
            promptCanvas.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            ShowMessage();
            hasTriggered = true;
        }
    }

    private void ShowMessage()
    {
        if (promptCanvas == null || promptText == null)
            return;

        promptText.text = message;
        promptCanvas.SetActive(true);

        // HideAfterDelay 코루틴 재시작
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (promptCanvas != null)
            promptCanvas.SetActive(false);

        hideCoroutine = null;
    }
}
