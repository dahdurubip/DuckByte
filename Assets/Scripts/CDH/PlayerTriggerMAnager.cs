using UnityEngine;
using System.Collections;

public class PlayerTriggerMAnager : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager; // 대사 출력 담당
                                                              //[SerializeField] private DialogueData dialogueData;       // 대사 데이터 보관소
    public bool isInside = false;

   

    // private bool isPlayingDialogue = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bossDoor"))
        {
            Debug.Log("보스문 충돌");
            //m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.);

            //StartCoroutine(PlayDialogue("goToBossDoor"));
            dialogueManager.PlayDialogue("goToBossDoor", "player");
        }

        if (other.CompareTag("DontMonster"))
            isInside = true;

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DontMonster"))
            isInside = false;
    }

    //public void unBrokenJar()
    //{

    //        Debug.Log("안깨지는 장독 충돌");
    //        //StartCoroutine(PlayDialogue("interactUnBrokenJar"));
    //        dialogueManager.PlayDialogue("interactUnBrokenJar");

    //}

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
