using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    public int zoneIndex;
    public WhiteRoomCommandPuzzle puzzleManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var state = other.GetComponent<PlayerZoneState>();
            if (state != null)
                state.SetZone(zoneIndex);

            if (!puzzleManager.PuzzleStarted)
                puzzleManager.BeginPuzzle();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var state = other.GetComponent<PlayerZoneState>();
            if (state != null)
                state.ExitZone();
        }
    }
}
