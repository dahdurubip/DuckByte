using UnityEngine;
using System.Collections;

public class PlayerTriggerMAnager : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager; // 대사 출력 담당
    [SerializeField] private DialogueData dialogueData;       // 대사 데이터 보관소

    private bool isPlayingDialogue = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bossDoor") && !isPlayingDialogue)
        {
            Debug.Log("보스문 충돌");
            StartCoroutine(PlayDialogue("goToBossDoor"));
        }
    }

    public IEnumerator PlayDialogue(string Name)
    {
        isPlayingDialogue = true;

        if (dialogueData.interactables.ContainsKey(Name))
        {
            foreach (string line in dialogueData.interactables[Name])
            {
                yield return StartCoroutine(dialogueManager.ShowDialogue("플레이어", line));
            }
        }

        isPlayingDialogue = false;
    }
}
