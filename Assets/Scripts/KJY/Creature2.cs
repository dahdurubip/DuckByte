using UnityEngine;

public class Creature2 : MonoBehaviour
{
    private float moveSpeed = 2f;
    private float teleportDistance = 5f;

    public void UpdateStats(float speed, float teleportDist)
    {
        moveSpeed = speed;
        teleportDistance = teleportDist;
    }

    // 예: 플레이어 추적, 순간이동 등에서 moveSpeed, teleportDistance 활용
}
