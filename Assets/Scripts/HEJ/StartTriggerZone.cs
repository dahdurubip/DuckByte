using UnityEngine;

public class StartTriggerZone : MonoBehaviour
{
    private bool started = false;

    void OnTriggerEnter(Collider other)
    {
        if (started) return;
        if (other.CompareTag("Player"))
        {
            started = true;
            EastRoomPuzzleManager.Instance.StartPuzzle();

            var col = GetComponent<Collider>();
            if (col) col.enabled = false;
        }
    }
}
