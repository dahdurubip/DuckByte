using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class OpeningTyper : MonoBehaviour
{

    [SerializeField] private AudioSource Typing;
    [Header("UI 프리팹과 부모")]
    public GameObject textPrefab; // TextMeshProUGUI 프리팹
    public Transform textParent; // Text들이 들어갈 부모 (Vertical Layout Group 포함)
    public GameObject canbas;
    public GameObject TM;

    [Header("타이핑 설정")]
    public float typingSpeed = 0.05f; // 글자당 딜레이


    [Header("엔딩별 대사들")]
    [TextArea(2, 4)] public string[] OPLines;

    [Header("엔딩 페이드 관련")]
    // 검은 화면
    public CanvasGroup fadeGroup;
    public float fadeDuration = 3f;
    // 전환할 씬 이름

    void Start()
    {
        string[] lines = OPLines;
        StartCoroutine(ShowCredits(lines));
    }

    //string[] GetLinesForEnding()
    //{
    //    switch (dataSO.currentEnding)
    //    {
    //        case EndingType.Good: return goodEndingLines;
    //        case EndingType.Bad: return badEndingLines;
    //        case EndingType.True: return trueEndingLines;
    //        default: return new string[] { "엔딩 타입 오류" };
    //    }
    //}

    IEnumerator ShowCredits(string[] lines)
    {
        foreach (string line in lines)
        {
            GameObject lineObj = Instantiate(textPrefab, textParent);
            TMP_Text tmpText = lineObj.GetComponent<TMP_Text>();
            tmpText.text = "";
            //Typing.Play();


            foreach (char c in line)
            {
                tmpText.text += c;
                Typing.Play();
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(1f); // 다음 줄로 넘어가기 전 대기
        }

        yield return StartCoroutine(FadeToBlack());
        yield return new WaitForSeconds(2f);

        canbas.SetActive(false);
        TM.SetActive(true);
    }

    IEnumerator FadeToBlack()
    {
        float alphaValue = 0f;
        while (alphaValue < fadeDuration)
        {
            float t = alphaValue / fadeDuration;
            //fadeGroup.alpha = Mathf.Lerp(0f, 1f, t);
            fadeGroup.alpha = Mathf.SmoothStep(0f, 1f, t);

            alphaValue += Time.deltaTime;
            yield return null;
        }
        fadeGroup.alpha = 1f;
    }

}
