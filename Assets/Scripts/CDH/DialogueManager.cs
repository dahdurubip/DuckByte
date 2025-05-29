using UnityEngine;
using TMPro;
using System.Collections;
using UnityEditor.Rendering.PostProcessing;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialoguePanel;

    public bool isTyping = false;

    public IEnumerator ShowDialogue(string speaker, string line)
    {
        if (isTyping) yield break;

        isTyping = true;
        dialoguePanel.SetActive(true);
        dialogueText.text = "";


        //float typingSpeed = 0.05f; // 글자당 출력 시간
        //float extraWait = 0.7f;    // 대사 출력 후 추가 대기 시간

        // 타이핑 효과
        foreach (char c in line)
        {
            dialogueText.text += c;

            

            yield return new WaitForSeconds(0.05f);
        }

        // 대사가 다 타이핑되면 사용자 입력 대기 (스페이스바 등)
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        // 자동 진행
        // yield return new WaitForSeconds(1.5f);
        // 전체 대사 길이에 따라 대기 시간 계산
        //float totalWait = (line.Length * typingSpeed) + extraWait;
        //yield return new WaitForSeconds(totalWait);

        // 다음 줄 준비
        dialogueText.text = "";
        yield return new WaitForSeconds(0.2f); // 줄 간 잠깐 텀

        dialoguePanel.SetActive(false); // 사라지는 효과 주고 싶으면 여기에 이펙트 추가
        isTyping = false;
    }

    

}
