using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZoneTrigger : MonoBehaviour
{
    [Tooltip("0부터 시작하는 이 존의 인덱스")]
    public int zoneIndex;

    [Tooltip("씬에 배치된 WhiteRoomCommandPuzzle 오브젝트 참조")]
    public WhiteRoomCommandPuzzle puzzleManager;

    void Awake()
    {
        var col = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {

        if (!other.CompareTag("Player"))
            return;

        puzzleManager?.NotifyZoneReached(zoneIndex);
    }
}
