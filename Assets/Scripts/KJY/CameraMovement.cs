using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    [Header("Gimmick Manager")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Follow Settings")]
    public Transform objectTofollow;
    public float followSpeed = 10f;

    [Header("Mouse Look Settings")]
    public float sensitivity = 100f;
    public float clampAngle = 70f;

    private float rotX;
    private float rotY;

    [Header("Camera Collision & Zoom")]
    [SerializeField] private GameObject realCam;    // 실제 카메라 GameObject
    public Transform realCamera;                    // 카메라 Transform
    public Vector3 dirNormalized;                   // 카메라 로컬 방향
    public Vector3 finalDir;                        // 월드 공간 목표지점
    public float minDistance;                       // 최소 거리
    public float maxDistance;                       // 최대 거리
    public float finalDistance;                     // 충돌 보정 거리
    public float smoothness = 10f;                  // 보간 속도

    // 시야 고정 토글
    private bool isViewLocked = false;

    // 원래 카메라 로컬 Y 위치 저장용
    public float originalCamLocalY;


    private void Awake()
    {
        transform.localPosition = objectTofollow.transform.position;
    }

    private void Start()
    {
        // 마우스 회전 초기값 세팅
        rotX = transform.localRotation.eulerAngles.x;
        rotY = transform.localRotation.eulerAngles.y;

        // 카메라 로컬 방향 & 거리 초기화
        dirNormalized = realCamera.localPosition.normalized;
        finalDistance = realCamera.localPosition.magnitude;

        // 원래 카메라 Y 위치 저장
        //originalCamLocalY = realCamera.localPosition.y;
    }

    private void Update()
    {
        // Q키로 시야 고정/해제 토글
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isViewLocked = !isViewLocked;
        }

        // 시야가 잠기지 않았을 때만 마우스로 회전
        if (!isViewLocked)
        {
            rotX += -Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
            rotY += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);
        }

        // 회전 적용 (고정 상태여도 마지막 rotX/rotY 유지)
        Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = rot;
    }

    private void LateUpdate()
    {
        // 1) 대상 따라가기
        transform.position = Vector3.MoveTowards(
            transform.position,
            objectTofollow.position,
            followSpeed * Time.deltaTime
        );

        // 2) 카메라 충돌 거리 계산
        finalDir = transform.TransformPoint(dirNormalized * maxDistance);
        if (Physics.Linecast(transform.position, finalDir, out RaycastHit hit))
        {
            finalDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }
        else
        {
            finalDistance = maxDistance;
        }

        // 3) 목표 로컬 포지션 계산
        Vector3 targetLocalPos = dirNormalized * finalDistance;

        // 4) 크라우치 상태에 따른 Y축 보정
        if (playerMovement.playerCrouch)
        {
            targetLocalPos.y = originalCamLocalY * 0.3f;
        }
        else
        {
            targetLocalPos.y = originalCamLocalY;
        }

        // 5) 부드럽게 보간 적용
        realCamera.localPosition = Vector3.Lerp(
            realCamera.localPosition,
            targetLocalPos,
            Time.deltaTime * smoothness
        );
    }

    // 카메라 흔들기 코루틴
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = realCam.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            realCam.transform.localPosition = new Vector3(
                originalPos.x + offsetX,
                originalPos.y + offsetY,
                originalPos.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 원래 위치 복구
        realCam.transform.localPosition = originalPos;
    }
}
