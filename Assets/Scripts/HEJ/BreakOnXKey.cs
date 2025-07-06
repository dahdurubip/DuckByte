using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

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

    //[Header("5초 내 실패 시 활성화할 거미 오브젝트")]
    //[SerializeField] private GameObject spiderObject;

    [Header("깨기 제한 시간")]
    [SerializeField] private float breakTimeout = 5f;

    [SerializeField] private AudioSource brokeEgg;

    //**
    private const int totalStages = 10;
    private int pressCount = 0;

    [Header("드랍 아이템 설정")]
    [SerializeField] private GameObject dropItemPrefab;
    [SerializeField] private Slider breakProgressSlider;    // 알 깨는 수치 슬라이더
    [Header("슬라이더 컨트롤")]
    [SerializeField] private GameObject breakSliderObject;  // 슬라이더 껐다 켰다

    [Header("알 페이즈 정보")]
    [SerializeField] private int phase = 1; // 이 알이 몇 페이즈에 속하는지 (인스펙터에서 설정)

    [Header("알 그룹 매니저")]
    [SerializeField] private EggGroupManager eggGroupManager; // 매니저 직접 연결
    [SerializeField] private GameObject brokenEgg;
    [Header("보스 페이즈 매니저")]
    [SerializeField] private BossPhaseManager bossPhaseManager;

    //**/

    private List<Transform> fragments = new List<Transform>();
    private int piecesPerStage;

    // 타이머 한 번만 시작하도록
    private bool timerStarted = false;

    // 레이어 바꿀 오브젝트

    void Start()
    {
        fragmentsParent.SetActive(false);
        //if (spiderObject != null) spiderObject.SetActive(false);

        foreach (Transform frag in fragmentsParent.transform)
        {
            fragments.Add(frag);
            var rb = frag.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }

        piecesPerStage = Mathf.CeilToInt(fragments.Count / (float)totalStages);

        // 알 깨는 슬라이더 초기화
        if (breakProgressSlider != null)
        {
            breakProgressSlider.minValue = 0;
            breakProgressSlider.maxValue = totalStages;
            breakProgressSlider.value = 0;
        }

        //if (eggGroupManager != null)
        //    eggGroupManager.RegisterEgg(this, phase);

        if (eggGroupManager != null)
        {
            eggGroupManager.RegisterEgg(this, phase);

            if (phase == 1 && bossPhaseManager != null)
            {
                bossPhaseManager.OnEggRegistered();
            }
        }

        if (breakSliderObject != null)
        {
            breakSliderObject.SetActive(false); // 시작 시 숨김
        }
    }

    //private void Update()
    //{
    //    // 플레이어가 가까이 오면
    //    if (Vector3.Distance(player.position, transform.position) <= breakDistance)
    //    {
    //        // 타이머가 아직 안 돌고, 알이 완전히 깨지기 전이면 타이머 시작
    //        if (!timerStarted && pressCount < totalStages)
    //        {
    //            timerStarted = true;
    //            StartCoroutine(BreakTimeoutCoroutine());
    //        }

    //        // X 누르면 분해 단계 진행
    //        if (Input.GetKeyDown(KeyCode.X) && pressCount < totalStages)
    //        {
    //            pressCount++;
    //            ApplyBreakStage(pressCount);
    //        }
    //    }
    //}

    public void ProcessEggBreak()
    {
        // 플레이어가 부술 수 있는 거리 내에 있는지 확인
        if (Vector3.Distance(player.position, transform.position) <= breakDistance)
        {
            // 타이머가 아직 시작되지 않았고, 알이 완전히 깨지지 않았다면 타이머 시작
            if (!timerStarted && pressCount < totalStages)
            {
                timerStarted = true;
                StartCoroutine(BreakTimeoutCoroutine());
            }

            // 알이 아직 완전히 깨지지 않았다면 분해 단계 진행
            if (pressCount < totalStages)
            {
                pressCount++;
                ApplyBreakStage(pressCount);
            }
        }
    }

    //private void OnDrawGizmos()
    //{
    //    if (player == null)
    //        return;

    //    // 구의 색상 설정
    //    Gizmos.color = Color.red;

    //    // 부술 수 있는 최대 거리만큼 구를 그림 (알 중심 기준)
    //    Gizmos.DrawWireSphere(transform.position, breakDistance);

    //    // 플레이어와 알을 잇는 선도 함께 그림
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawLine(transform.position, player.position);
    //}

    // 만약 몇초안에 알깨는걸 실패했을 경우 패널티를 주고 싶다면 거미 나타나는 부분에 대신 다른거 넣기 (예를들어 슬로우모션 혹은 피감소 혹은 실패이펙트?)
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
        //if (spiderObject != null)
        //    spiderObject.SetActive(true);
    }

    private void ApplyBreakStage(int stage)
    {
        brokeEgg.Play();
        if (stage == 1)
        {
            wholeEggs.SetActive(false);
            fragmentsParent.SetActive(true);

            if (breakSliderObject != null)
                breakSliderObject.SetActive(true); // 처음 깨면 보이게
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

        if (breakProgressSlider != null)
            breakProgressSlider.value = pressCount;

        if (pressCount >= totalStages)
        {
            //if (dropItemPrefab != null)
            //{
            //    Instantiate(dropItemPrefab, transform.position + Vector3.up, Quaternion.identity);
            //}

            if (eggGroupManager != null && eggGroupManager.IsDropEgg(this, phase))
            {
                if (dropItemPrefab != null)
                    Instantiate(dropItemPrefab, transform.position + Vector3.up, Quaternion.identity);
            }

            if (breakSliderObject != null)
                breakSliderObject.SetActive(false); // 다 깨면 숨김

            // 레이어 바꿈
            brokenEgg.layer = LayerMask.NameToLayer("Default"); // 원하는 레이어 이름으로 변경
        }
    }
}
