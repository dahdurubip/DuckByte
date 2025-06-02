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
            //StartCoroutine(PlayDialogue("goToBossDoor"));
            dialogueManager.PlayDialogue("goToBossDoor");
        }
    }

    public void unBrokenJar()
    {
        if (!isPlayingDialogue)
        {
            Debug.Log("안깨지는 장독 충돌");
            //StartCoroutine(PlayDialogue("interactUnBrokenJar"));
            dialogueManager.PlayDialogue("interactUnBrokenJar");
        }
    }

    //public IEnumerator PlayDialogue(string Name)
    //{
    //    isPlayingDialogue = true;

    //    Debug.Log("코루틴 들어옴");

    //    if (dialogueData.interactables.ContainsKey(Name))
    //    {
    //        foreach (string line in dialogueData.interactables[Name])
    //        {
    //            yield return StartCoroutine(dialogueManager.ShowDialogue("플레이어", line));
    //        }
    //    }

    //    isPlayingDialogue = false;
    //}
}
