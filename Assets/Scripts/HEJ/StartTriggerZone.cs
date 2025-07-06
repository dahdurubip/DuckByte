using UnityEngine;
using System.Collections;

public class StartTriggerZone : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private DialogueManager dialogManager;  

    private bool started = false;

    void Awake()
    {
        //if (dialogManager == null)
        //    dialogManager = FindObjectOfType<DialogueManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (started) return;
        if (other.CompareTag("Player"))
        {
            started = true;

            // 콜라이더 비활성화 → 중복 진입 방지
            var col = GetComponent<Collider>();
            if (col) col.enabled = false;

            // 대사 재생 후 퍼즐 시작
            StartCoroutine(PlayDialogueThenStart());
        }
    }

    private IEnumerator PlayDialogueThenStart()
    {
        // 1) 대사 재생
        dialogManager.PlayDialogue("eastRoom", "boss");

        // 2) DialogueManager.isPlayingDialogue가 false가 될 때까지 대기
        yield return new WaitUntil(() => !dialogManager.isPlayingDialogue);

        // 3) 대사 종료 시 퍼즐 시작
        EastRoomPuzzleManager.Instance.StartPuzzle();
    }
}
