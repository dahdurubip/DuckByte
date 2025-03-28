using UnityEngine;

public class Creature2Eyes : MonoBehaviour

{
    [SerializeField] private Creature2Manager creature2Manager;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float detectCooldown = 2f; // 감지 쿨타임

    private bool canDetect = true;

    private void Update()
    {
        if (canDetect)
        {
            DetectPlayer();
        }
    }

    private void DetectPlayer()
    {
        if (creature2Manager != null)
        {
            creature2Manager.IncreaseDetectionCount();
        }
    }

    private void ResetDetection()
    {
        canDetect = true;
    }
}