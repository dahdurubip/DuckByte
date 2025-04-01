using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CastInfo
{
    public bool Hit;
    public Vector3 Point;
    public float Distance;
    public float Angle;
}

public class Creature2Eyes : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private Creature2Manager creature2Manager;

    [Header("타겟 레이어 설정")]
    [SerializeField] private LayerMask playerLayer;

    [Header("시야 설정")]
    [SerializeField, Range(0f, 30f)] private float viewRange = 3f;
    [SerializeField, Range(0f, 360f)] private float viewAngle = 80f;

    [Header("디버그용")]
    [SerializeField] private List<CastInfo> lineList;

    private WaitForSeconds checkDelay = new WaitForSeconds(0.1f);
    private Coroutine checkTargetCoroutine;
    private Coroutine drawRayLineCoroutine;

    void Start()
    {
        Debug.Log("현재 설정된 playerLayer 값: " + playerLayer.value.ToString());
        lineList = new List<CastInfo>();
        StartCheckingTarget();
        StartDrawingRayLines();
    }

    public void StartCheckingTarget()
    {
        if (checkTargetCoroutine == null)
        {
            checkTargetCoroutine = StartCoroutine(CheckTargetRoutine());
        }
    }

    public void StopCheckingTarget()
    {
        if (checkTargetCoroutine != null)
        {
            StopCoroutine(checkTargetCoroutine);
            checkTargetCoroutine = null;
        }
    }

    private IEnumerator CheckTargetRoutine()
    {
        while (true)
        {
            CheckTarget();
            yield return checkDelay;
        }
    }

    private void CheckTarget()
    {
        Vector3 baseDir = transform.forward;
        int rayCount = Mathf.RoundToInt(viewAngle);
        float halfAngle = viewAngle * 0.5f;
        float distance = viewRange;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = -halfAngle + (viewAngle * i / rayCount);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * baseDir;

            if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, distance, playerLayer))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("플레이어 감지됨!");
                    creature2Manager.OnPlayerDetected();
                    break; // 1회 감지만 하고 빠짐
                }
            }
        }
    }

    public void StartDrawingRayLines()
    {
        if (drawRayLineCoroutine == null)
        {
            drawRayLineCoroutine = StartCoroutine(DrawRayLineRoutine());
        }
    }

    public void StopDrawingRayLines()
    {
        if (drawRayLineCoroutine != null)
        {
            StopCoroutine(drawRayLineCoroutine);
            drawRayLineCoroutine = null;
        }
    }

    private IEnumerator DrawRayLineRoutine()
    {
        while (true)
        {
            DrawRayLine();
            yield return null;
        }
    }

    private void DrawRayLine()
    {
        lineList.Clear();

        Vector3 baseDir = transform.forward;
        int rayCount = Mathf.RoundToInt(viewAngle);
        float halfAngle = viewAngle * 0.5f;
        float distance = viewRange;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = -halfAngle + (viewAngle * i / rayCount);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * baseDir;
            Vector3 origin = transform.position + Vector3.up;

            Ray ray = new Ray(origin, dir);
            CastInfo info = new CastInfo { Angle = angle };

            if (Physics.Raycast(ray, out RaycastHit hit, distance))
            {
                info.Hit = true;
                info.Point = hit.point;
                info.Distance = hit.distance;
                Debug.DrawLine(origin, hit.point, Color.red); // 감지 시 빨간선
            }
            else
            {
                info.Hit = false;
                info.Point = origin + dir * distance;
                info.Distance = distance;
                Debug.DrawLine(origin, origin + dir * distance, Color.green); // 미감지시 초록선
            }

            lineList.Add(info);
        }
    }
}
