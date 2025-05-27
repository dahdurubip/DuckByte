// ZoneTrigger.cs
using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    public int zoneIndex;
    public WhiteRoomCommandPuzzle puzzleManager;
    private Collider myCol;

    void Awake()
    {
        myCol = GetComponent<Collider>();
        if (myCol == null)
            Debug.LogError("ZoneTrigger needs a Collider.");
        if (puzzleManager == null)
            Debug.LogError("ZoneTrigger: assign puzzleManager in Inspector.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!puzzleManager.PuzzleStarted)
            puzzleManager.BeginPuzzle(zoneIndex);
    }
}
