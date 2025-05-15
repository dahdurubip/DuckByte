using UnityEngine;

public class StartTriggerZone : MonoBehaviour
{
    public EastLampPuzzleManager puzzleManager;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            puzzleManager.StartPuzzle();
        }
    }
}
