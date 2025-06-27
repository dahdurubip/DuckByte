using System.Collections;
using UnityEngine;

public class Creature2Manager : MonoBehaviour
{
    [Header("Wall Settings")]
    [SerializeField] private WallPosController wallpos;

    [Header("Creature2 Settings")]
    [SerializeField] private Creature2 creature2;

    [Header("Clone Settings")]
    [SerializeField] private GameObject eyeDetectionCloneObject;
    [SerializeField] private GameObject flashlightCloneObject;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float activeDuration = 2f;

    private Creature2Clone eyeDetectionClone;
    private Creature2Clone flashlightClone;

    //마지막 눈의 위치
    private Vector3 lastEyePos;

    //재호출 방지 플래그
    private bool isTeleporting = false;

    //횟수 가져가서 성장판정, 크리처2 본체 성장한다고 하면 public으로 바꿔서 작업을 해야 함
    private int detectionCount = 0;

    //벽 이동여부 체크
    public bool wallmove = false;

    //크론2 활성화 여부
    private bool isLightCloneActive = false;


    private void Awake()
    {
        eyeDetectionClone = eyeDetectionCloneObject.GetComponent<Creature2Clone>();
        flashlightClone = flashlightCloneObject.GetComponent<Creature2Clone>();

        eyeDetectionCloneObject.SetActive(false);
        flashlightCloneObject.SetActive(false);
    }

    //Update에서는 wallmove 관련만 처리 (그 외 타이밍은 OnPlayerDetected나 WhenTheLightOn에서 별도로 처리)
    private void Update()
    {
        if (wallmove)
        {
            wallpos.MoveThewall(detectionCount);
            wallmove = false;
        }

        if(creature2.flashOn)
        {
            if (!isLightCloneActive)
            {
                WhenTheLightOn();
            }

        }
        else
        {
            if (isLightCloneActive) 
            {
                flashlightCloneObject.SetActive(false);
                isLightCloneActive = false;
            }

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
        ActivateEyeClone();
    }

    private void ActivateEyeClone()
    {
        //순간이동 시작
        isTeleporting = true;
        eyeDetectionCloneObject.SetActive(true);
        //눈 발견 클론 초기화
        eyeDetectionClone.InitializeAtPosition(lastEyePos, playerTransform);
        StartCoroutine(DeactivateAfterDelay());
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(activeDuration);
        eyeDetectionCloneObject.SetActive(false);
        //순간이동 종료 -> 재호출 허용
        isTeleporting = false;
    }

    //손전등이 켜진 상태를 지속할 때
    private  void WhenTheLightOn()
    {

        flashlightCloneObject.SetActive(true);
        isLightCloneActive = true;
        //손전등 클론 초기화
        flashlightClone.InitializeInFrontOfPlayer(playerTransform);
    }


}

