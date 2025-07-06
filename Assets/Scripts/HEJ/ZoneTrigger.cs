using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ZoneTrigger : MonoBehaviour
{
    [Tooltip("0부터 시작하는 이 존의 인덱스")]
    public int zoneIndex;

    [Tooltip("씬에 배치된 WhiteRoomCommandPuzzle 오브젝트 참조")]
    public WhiteRoomCommandPuzzle puzzleManager;

    [Tooltip("씬에 배치된 DialogueManager 오브젝트 참조")]
    public DialogueManager dialogManager;

    // 인스턴스별 중복 실행 방지
    private bool hasTriggered;
    // 전체 게임 세션에서 인트로 대사 재생 여부
    private static bool introPlayed;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
            col.isTrigger = true;

        if (dialogManager == null)
            dialogManager = FindObjectOfType<DialogueManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player"))
            return;

        hasTriggered = true;

        // 첫 구역(zoneIndex == 0)이고 아직 인트로 대사를 안 재생했으면
        if (zoneIndex == 0 && !introPlayed)
            StartCoroutine(PlayDialogueThenNotify());
        else
            puzzleManager.NotifyZoneReached(zoneIndex);
    }

    private IEnumerator PlayDialogueThenNotify()
    {
        // 대사 재생
        dialogManager.PlayDialogue("westRoom", "boss");

        // 재생이 실제로 시작될 때까지 대기
        yield return new WaitUntil(() => dialogManager.isPlayingDialogue);
        // 대사가 끝날 때까지 대기
        yield return new WaitUntil(() => !dialogManager.isPlayingDialogue);

        // 한 번만 재생되도록 플래그 설정
        introPlayed = true;

        // 퍼즐 매니저에 알림
        puzzleManager.NotifyZoneReached(zoneIndex);
    }

    // 퍼즐 실패 후 트리거 재활성화를 위해
    public void ResetTrigger()
    {
        hasTriggered = false;
        var col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;
    }
}
