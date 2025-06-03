using System.Collections;
using UnityEngine;
using TMPro;

public class EndingCredit : MonoBehaviour
{
    // public enum EndingType { Good, Bad, True }

    // 엔딩분기 데이터SO
    // 엔딩 분기시작하는 곳에서 데이터 설정해주기
    // 설정방법 : EndingDataSO.currentEnding = EndingType.Bad;
    public EndingDataSO dataSO;

    [Header("UI 프리팹과 부모")]
    public GameObject textPrefab; // TextMeshProUGUI 프리팹
    public Transform textParent; // Text들이 들어갈 부모 (Vertical Layout Group 포함)

    [Header("타이핑 설정")]
    public float typingSpeed = 0.05f; // 글자당 딜레이

    [Header("현재 엔딩 타입")]
    //public EndingType currentEnding;
    //private EndingType currentEnding => dataSO.currentEnding;


    [Header("엔딩별 대사들")]
    [TextArea(2, 4)] public string[] goodEndingLines;
    [TextArea(2, 4)] public string[] badEndingLines;
    [TextArea(2, 4)] public string[] trueEndingLines;

    void Start()
    {
        string[] lines = GetLinesForEnding();
        StartCoroutine(ShowCredits(lines));
    }

    string[] GetLinesForEnding()
    {
        switch (dataSO.currentEnding)
        {
            case EndingType.Good: return goodEndingLines;
            case EndingType.Bad: return badEndingLines;
            case EndingType.True: return trueEndingLines;
            default: return new string[] { "엔딩 타입 오류" };
        }
    }

    IEnumerator ShowCredits(string[] lines)
    {
        foreach (string line in lines)
        {
            GameObject lineObj = Instantiate(textPrefab, textParent);
            TMP_Text tmpText = lineObj.GetComponent<TMP_Text>();
            tmpText.text = "";

            foreach (char c in line)
            {
                tmpText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(1f); // 다음 줄로 넘어가기 전 대기
        }
    }
}
