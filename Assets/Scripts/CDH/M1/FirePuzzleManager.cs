using System.Collections;
using UnityEngine;
using TMPro; // 타이머 UI에 필요

public class FirePuzzleManager : MonoBehaviour
{
    public Statue[] statues;                  // 퍼즐에 포함된 석상들
    // public Door doorToOpen;                   // 퍼즐 클리어 시 열릴 문
    public TextMeshProUGUI timerText;         // 남은 시간 UI 표시용

    private int litCount = 0;                 // 현재 불 붙은 석상 수
    private Coroutine timerCoroutine;         // 타이머 코루틴 추적 변수
    public float timeLimit = 30f;            // 제한 시간

    public GameObject bonginEffect;           // 보스 문 봉인 이펙트
    [SerializeField] private Collider bossDoor; 
    //public GameObject bossDoor1;
    //public GameObject bossDoor2;

    [SerializeField] private DialogueManager dialogueManager;


    private void Start()
    {
        foreach (var statue in statues)       // 각 석상에 이벤트 등록
        {
            statue.OnFireLit += OnStatueLit;
        }
        timerText.gameObject.SetActive(false); // UI 초기에는 꺼놓기
    }

    private void OnStatueLit(Statue statue)
    {
        litCount++;                            // 불 붙은 석상 수 증가

        if (litCount == 1 && timerCoroutine == null)
        {
            timerCoroutine = StartCoroutine(FireTimer()); // 첫 불이면 타이머 시작
        }

        if (litCount >= statues.Length)
        {
            PuzzleComplete();                  // 전부 붙으면 퍼즐 클리어
        }
    }

    private IEnumerator FireTimer()
    {
        float timer = timeLimit;
        timerText.gameObject.SetActive(true); // 타이머 UI 표시

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            timerText.text = $"남은 시간: {timer:F1}초"; // UI 업데이트
            yield return null;
        }

        //Debug.Log("시간 초과!");
        ResetPuzzle();                        // 제한 시간 내 실패 → 초기화
    }

    private void PuzzleComplete()
    {
        StopCoroutine(timerCoroutine);         // 타이머 중지
        timerCoroutine = null;
        timerText.gameObject.SetActive(false); // UI 숨기기

       // Debug.Log("퍼즐 완료!");
        // 문이 열리는 소리가 들린다 텍스트 띄우기
        dialogueManager.PlayDialogue("clearToBossDoor", "player");
        // 보스문 봉인 되어 있던 이펙트 없애기
        bonginEffect.SetActive(false);
        // 문이랑 상호작용 가능해지는 기능 추가
        bossDoor.enabled = true;
        //bossDoor1.layer = LayerMask.NameToLayer("Interactable");
        //bossDoor2.layer = LayerMask.NameToLayer("Interactable");
    }

    private void ResetPuzzle()
    {
        foreach (var statue in statues)
        {
            statue.Extinguish();              // 모든 석상 불 끄기
        }

        litCount = 0;
        timerCoroutine = null;
        timerText.gameObject.SetActive(false); // UI 숨기기
    }
}
