using System.Collections;
using UnityEngine;

public class Creature2Manager : MonoBehaviour
{
    [SerializeField] private WallPosController wallpos;
    public bool wallmove = false;

    [Header("Creature2 Settings")]
    [SerializeField] private Creature2 creature2;
    [SerializeField] private int growThreshold = 3;
    [SerializeField] private Transform player;

    [Header("Clone Settings")]
    [SerializeField] private GameObject creature2Clone;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float activeDuration = 3f;
    [SerializeField] private Creature2Clone creature2Script;
    private Vector3 lastEyePos;
    private Vector3 lastEyeForward;

    // 타이머를 클래스 멤버로 선언
    private float lightTimer = 0f;

    // 재호출 방지 플래그
    private bool isTeleporting = false;

    public int detectionCount = 0;
    public bool CanGrow = false;
    public bool TheLight = false;

    //순간이동할 때 다른 크리처를 활성화해서 순간이동을 시킨다.


    private void Awake()
    {
        creature2Clone.SetActive(false);
    }



    // Update에서는 wallmove 관련만 처리 (그 외 타이밍은 OnPlayerDetected나 WhenTheLightOn에서 별도로 처리)
    private void Update()
    {
        if (wallmove)
        {
            wallpos.MoveThewall(detectionCount);
            wallmove = false;
        }

    }

    public void OnEyeDetected(Vector3 eyePos)
    {
        if (isTeleporting) return;    // 이미 순간이동 중이면 무시

        lastEyePos = eyePos;
        ActivateClone();
    }

    private void ActivateClone()
    {
        isTeleporting = true;         // 순간이동 시작
        creature2Clone.SetActive(true);
        // 눈알 위치/방향과 플레이어 전달
        creature2Script.Initialize(lastEyePos, playerTransform);
        StartCoroutine(DeactivateAfterDelay());
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(activeDuration);
        creature2Clone.SetActive(false);
        isTeleporting = false;        // 순간이동 종료 → 재호출 허용
    }

    private void GrowWithCreature2()
    {
        // growThreshold 이상일 때마다 성장
        if (detectionCount > 0 && detectionCount % growThreshold == 0)
        {
            CanGrow = true;
        }
    }

    //지금은 없음
    //외부 이벤트에서 호출: 손전등이 켜진 상태를 지속할 때
    public void WhenTheLightOn()
    {
        if (TheLight)
        {
            lightTimer += Time.deltaTime;
            if (lightTimer >= 3f)
            {
                //CommandTeleport();
            }
        }
        else
        {
            lightTimer = 0f;
        }
    }


}

