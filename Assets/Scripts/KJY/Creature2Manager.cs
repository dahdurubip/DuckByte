using System.Collections;
using UnityEngine;

public class Creature2Manager : MonoBehaviour
{
    [Header("Wall Settings")]
    [SerializeField] private WallPosController wallpos;

    [Header("Creature2 Settings")]
    [SerializeField] private Creature2 creature2;
    [SerializeField] private Transform player;

    [Header("Clone Settings")]
    [SerializeField] private GameObject creature2Clone;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float activeDuration = 2f;
    [SerializeField] private Creature2Clone creature2Script;

    //마지막 눈의 위치
    private Vector3 lastEyePos;

    //재호출 방지 플래그
    private bool isTeleporting = false;

    //횟수 가져가서 성장판정, 크리처2 본체 성장한다고 하면 public으로 바꿔서 작업을 해야 함
    private int detectionCount = 0;

    //벽 이동여부 체크
    public bool wallmove = false;

    //타이머를 클래스 멤버로 선언, 아직 없음
    private float lightTimer = 0f;
    public bool TheLight = false;


    private void Awake()
    {
        creature2Clone.SetActive(false);
    }

    //Update에서는 wallmove 관련만 처리 (그 외 타이밍은 OnPlayerDetected나 WhenTheLightOn에서 별도로 처리)
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
        //이미 순간이동 중이면 무시
        if (isTeleporting) return;    
        ++detectionCount;
        if(detectionCount >= 9)
        {
            detectionCount = 9;
        }
        lastEyePos = eyePos;
        ActivateClone();
    }

    private void ActivateClone()
    {
        //순간이동 시작
        isTeleporting = true;     
        creature2Clone.SetActive(true);
        //눈알 위치/방향과 플레이어 전달
        creature2Script.Initialize(lastEyePos, playerTransform);
        StartCoroutine(DeactivateAfterDelay());
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(activeDuration);
        creature2Clone.SetActive(false);
        //순간이동 종료 -> 재호출 허용
        isTeleporting = false;        
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

