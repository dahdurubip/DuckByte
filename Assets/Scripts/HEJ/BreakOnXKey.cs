using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BreakOnXKey : MonoBehaviour
{
    [Header("플레이어")]
    [SerializeField] private Transform player;

    [Header("부술 수 있는 최대 거리")]
    [SerializeField] private float breakDistance = 2f;

    [Header("통짜 메시")]
    [SerializeField] private GameObject wholeEggs;

    [Header("파편 그룹")]
    [SerializeField] private GameObject fragmentsParent;

    [Header("폭발력 / 반경")]
    [SerializeField] private float explosionForce = 200f;
    [SerializeField] private float explosionRadius = 1.5f;

    [Header("5초 내 실패 시 활성화할 거미 오브젝트")]
    [SerializeField] private GameObject spiderObject;

    [Header("깨기 제한 시간")]
    [SerializeField] private float breakTimeout = 5f;

    private const int totalStages = 3;
    private int pressCount = 0;

    private List<Transform> fragments = new List<Transform>();
    private int piecesPerStage;

    // 타이머 한 번만 시작하도록
    private bool timerStarted = false;

    void Start()
    {
        fragmentsParent.SetActive(false);
        if (spiderObject != null) spiderObject.SetActive(false);

        foreach (Transform frag in fragmentsParent.transform)
        {
            fragments.Add(frag);
            var rb = frag.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }

        piecesPerStage = Mathf.CeilToInt(fragments.Count / (float)totalStages);
    }

    void Update()
    {
        // 플레이어가 가까이 오면
        if (Vector3.Distance(player.position, transform.position) <= breakDistance)
        {
            // 타이머가 아직 안 돌고, 알이 완전히 깨지기 전이면 타이머 시작
            if (!timerStarted && pressCount < totalStages)
            {
                timerStarted = true;
                StartCoroutine(BreakTimeoutCoroutine());
            }

            // X 누르면 분해 단계 진행
            if (Input.GetKeyDown(KeyCode.X) && pressCount < totalStages)
            {
                pressCount++;
                ApplyBreakStage(pressCount);
            }
        }
    }

    private IEnumerator BreakTimeoutCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < breakTimeout)
        {
            // 완전히 깨졌으면 타이머 중지
            if (pressCount >= totalStages)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 타임아웃: 5초 내에 완전 분해 실패 ⇒ 거미 활성화
        if (spiderObject != null)
            spiderObject.SetActive(true);
    }

    private void ApplyBreakStage(int stage)
    {
        if (stage == 1)
        {
            wholeEggs.SetActive(false);
            fragmentsParent.SetActive(true);
        }

        int start = (stage - 1) * piecesPerStage;
        int end = Mathf.Min(stage * piecesPerStage, fragments.Count);

        for (int i = start; i < end; i++)
        {
            var frag = fragments[i];
            var rb = frag.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddExplosionForce(
                    explosionForce,
                    fragmentsParent.transform.position,
                    explosionRadius
                );
            }
        }
    }
}
