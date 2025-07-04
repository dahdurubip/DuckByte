using UnityEngine;
using TMPro;
using System.Collections;


public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text bossDialogueText;
    [SerializeField] private GameObject bossDialoguePanel;

    [Header("Data")]
    [SerializeField] private DialogueData dialogueData;       // 대사 데이터 보관소


    public bool isTyping = false;
    public bool isPlayingDialogue = false;
    private bool skipRequested = false;
    //private bool allSkip = false;

    void Update()
    {

        // 문장이 완성되기전 스페이스바를 누르는걸 알게 할 변수
        if (Input.GetKeyDown(KeyCode.Space))
        {
            skipRequested = true;
        }

        //if (Input.GetKeyDown(KeyCode.Tab))
        //{
        //    allSkip = true;
        //}
    }

    //public void PlayDialogue(string Name)
    //{
    //    if (!isPlayingDialogue)
    //        StartCoroutine(PlayDialogueCoroutine(Name));
    //}


    //public IEnumerator PlayDialogueCoroutine(string Name)
    //{
    //    isPlayingDialogue = true;

    //    //Debug.Log("코루틴 들어옴");

    //    if (dialogueData.interactables.ContainsKey(Name))
    //    {
    //        foreach (string line in dialogueData.interactables[Name])
    //        {

    //            yield return StartCoroutine(ShowDialogue("플레이어", line));
    //        }
    //    }

    //    isPlayingDialogue = false;
    //}

    public void PlayDialogue(string key, string speaker)
    {
        if (!isPlayingDialogue)
            StartCoroutine(PlayDialogueCoroutine(key, speaker));
    }

    private IEnumerator PlayDialogueCoroutine(string key, string speaker)
    {
        isPlayingDialogue = true;

        if (dialogueData.interactables.ContainsKey(key))
        {
            foreach (string line in dialogueData.interactables[key])
            {
                yield return StartCoroutine(ShowDialogue(speaker, line));
            }
        }

        isPlayingDialogue = false;
    }


    //public IEnumerator ShowDialogue(string speaker, string line)
    //{
    //    if (isTyping) yield break;

    //    isTyping = true;
    //    dialoguePanel.SetActive(true);
    //    dialogueText.text = "";

    //    //bool skip = false;
    //    //float typingSpeed = 0.05f; // 글자당 출력 시간
    //    //float extraWait = 0.7f;    // 대사 출력 후 추가 대기 시간
    //    skipRequested = false;


    //    // 타이핑 효과
    //    foreach (char c in line)
    //    {

    //        if (skipRequested)
    //        {
    //            // 문장 완성전 스페이스 바 눌렀을 시 전체문장 띄우기
    //            // 같은 나레이션을 볼 시 빠르게 넘어가기 위한 기능
    //            dialogueText.text = line;
    //            break;
    //        }

    //        dialogueText.text += c;

    //        yield return new WaitForSeconds(0.05f);
    //    }


    //    // 대사가 다 타이핑되면 사용자 입력 대기 (스페이스바 등)
    //    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
    //    // 자동 진행
    //    // yield return new WaitForSeconds(1.5f);
    //    // 전체 대사 길이에 따라 대기 시간 계산
    //    //float totalWait = (line.Length * typingSpeed) + extraWait;
    //    //yield return new WaitForSeconds(totalWait);



    //    // 다음 줄 준비
    //    dialogueText.text = "";
    //    yield return new WaitForSeconds(0.2f); // 줄 간 잠깐 텀

    //    dialoguePanel.SetActive(false); // 사라지는 효과 주고 싶으면 여기에 이펙트 추가
    //    isTyping = false;
    //}


    public IEnumerator ShowDialogue(string speaker, string line)
    {
        if (isTyping) yield break;

        isTyping = true;
        skipRequested = false;

        GameObject activePanel = null;
        TMP_Text activeText = null;

        // 어떤 UI를 띄울지 분기
        if (speaker == "boss")
        {
            activePanel = bossDialoguePanel;
            activeText = bossDialogueText;
        }
        else
        {
            activePanel = dialoguePanel;
            activeText = dialogueText;
        }

        // 기존 UI 꺼두고, 새 UI 켜기
        dialoguePanel.SetActive(false);
        bossDialoguePanel.SetActive(false);
        activePanel.SetActive(true);
        activeText.text = "";

        foreach (char c in line)
        {
            if (skipRequested)
            {
                activeText.text = line;
                break;
            }

            activeText.text += c;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        activeText.text = "";
        yield return new WaitForSeconds(0.2f);
        activePanel.SetActive(false);

        isTyping = false;
    }


}
