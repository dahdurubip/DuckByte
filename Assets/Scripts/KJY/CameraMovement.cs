using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CameraMovement : MonoBehaviour
{
    [Header("Default Settings")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Follow Settings")]
    [SerializeField] private Transform objectTofollow;
    [SerializeField] private float originalCamLocalY;

    [Header("Mouse Look Settings")]
    [SerializeField] private float sensitivity = 100f;
    [SerializeField] private float clampAngle = 70f;

    [Header("Camera Collision & Zoom")]
    //실제 카메라 GameObject
    [SerializeField] private GameObject realCam;
    //카메라 Transform
    [SerializeField] private Transform realCamera;
    //카메라 로컬 방향
    [SerializeField] private Vector3 dirNormalized;
    //월드 공간 목표지점 (사용 안 함)
    //[SerializeField] private Vector3 finalDir;                
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    //충돌 보정 거리
    [SerializeField] private float currentCollisionDistance; // 충돌 시 계산된 최종 카메라 거리
    //보간 속도
    [SerializeField] private float smoothness = 10f;
    [SerializeField] private float collisionOffset = 0.2f; // 충돌 시 카메라가 벽에서 살짝 떨어지게 하는 값
    [SerializeField] private LayerMask collisionLayer; // 카메라 충돌을 감지할 레이어 마스크

    // SphereCast에 사용할 카메라 구체 반경
    [SerializeField] private float cameraRadius = 0.2f;

    [Header("Auto-Alignment Settings")]
    [SerializeField] private float noMouseInputThreshold = 0.01f;
    //마우스 입력 없을 시 자동 정렬 시작 시간
    [SerializeField] private float timeBeforeAutoAlign = 1.0f;
    [SerializeField] private float autoAlignSpeed = 3f;
    [SerializeField] private float defaultAutoAlignRotX = 10f;
    [SerializeField] private string autoAlignSceneName = "Creature2Map";

    private float timeSinceLastMouseInput = 0f;
    private bool isAutoAligning = false;
    //시야 고정 토글
    private bool isViewLocked = false;
    private float rotX;
    private float rotY;


    private void Start()
    {
        if (objectTofollow != null)
        {
            transform.localPosition = objectTofollow.transform.position;
            rotY = objectTofollow.transform.eulerAngles.y;
            rotX = defaultAutoAlignRotX;
        }
        else
        {
            rotX = transform.localRotation.eulerAngles.x;
            rotY = transform.localRotation.eulerAngles.y;
        }

        if (realCamera != null)
        {
            // realCamera의 초기 로컬 위치를 기준으로 dirNormalized 설정
            dirNormalized = realCamera.localPosition.normalized;
            // 초기 카메라 거리를 currentCollisionDistance에 저장
            currentCollisionDistance = Vector3.Distance(transform.position, realCamera.position);
            // originalCamLocalY 설정 (realCamera의 초기 로컬 y값을 사용)
            //originalCamLocalY = realCamera.localPosition.y;
        }
        else
        {
            Debug.LogError("Camera Wrong");
        }

        // 초기 currentCollisionDistance를 maxDistance로 설정
        currentCollisionDistance = maxDistance;
    }

    private void Update()
    {
        //Q키로 시야 고정/해제 토글
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isViewLocked = !isViewLocked;
            if (isViewLocked)
            {
                isAutoAligning = false;
                timeSinceLastMouseInput = 0f;
            }
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        //시야가 잠기지 않았을 때만 마우스로 회전
        if (!isViewLocked)
        {
            float mouseXInput = Input.GetAxis("Mouse X");
            float mouseYInput = Input.GetAxis("Mouse Y");

            //마우스 입력 감지
            if (Mathf.Abs(mouseXInput) > noMouseInputThreshold || Mathf.Abs(mouseYInput) > noMouseInputThreshold)
            {
                //마우스로 회전
                rotX += -mouseYInput * sensitivity;// * Time.deltaTime;
                rotY += mouseXInput * sensitivity;// * Time.deltaTime;
                rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

                //마우스 회전중 리셋 & 중지
                timeSinceLastMouseInput = 0f;
                isAutoAligning = false;
            }
            else
            {
                //마우스 비활성시 타이머
                timeSinceLastMouseInput += Time.deltaTime;
            }

            if (currentSceneName == autoAlignSceneName) // <--- 추가된 조건
            {
                if (playerMovement != null && !playerMovement.IsMoving)
                {
                    if (timeSinceLastMouseInput >= timeBeforeAutoAlign && !isAutoAligning)
                    {
                        isAutoAligning = true;
                    }
                }
                else
                {
                    isAutoAligning = false;
                    timeSinceLastMouseInput = 0f;
                }
            }
            else // 자동 정렬을 사용하지 않는 씬일 경우
            {
                isAutoAligning = false; // 자동 정렬 강제 비활성화
                // timeSinceLastMouseInput = 0f; // 이 씬에서는 타이머도 리셋할 수 있음 (선택 사항)
            }

            if (isAutoAligning)
            {
                float targetRotY = objectTofollow.transform.eulerAngles.y;
                float targetRotX = defaultAutoAlignRotX;

                rotY = Mathf.LerpAngle(rotY, targetRotY, autoAlignSpeed * Time.deltaTime);
                rotX = Mathf.Lerp(rotX, targetRotX, autoAlignSpeed * Time.deltaTime);

                if (Mathf.DeltaAngle(rotY, targetRotY) < 0.5f && Mathf.Abs(rotX - targetRotX) < 0.5f)
                {
                    rotY = targetRotY;
                    rotX = targetRotX;
                    isAutoAligning = false;
                    //정렬 후 다시 리셋
                    timeSinceLastMouseInput = 0f;
                }
            }
        }

        Quaternion camPivotRotation = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = camPivotRotation;
    }

    private void LateUpdate()
    {
        if (objectTofollow == null || realCamera == null || playerMovement == null) return;

        // 카메라 피봇을 플레이어 위치에 고정
        transform.position = objectTofollow.position;

        // 카메라의 목표 월드 위치 계산 (최대 거리 기준)
        Vector3 desiredCameraWorldPos = transform.position + transform.rotation * (dirNormalized * maxDistance);

        RaycastHit hit;
        // SphereCast를 사용하여 충돌 감지
        // 플레이어의 위치에서 목표 카메라 위치까지 cameraRadius 크기의 구체를 발사
        if (Physics.SphereCast(transform.position, cameraRadius, (desiredCameraWorldPos - transform.position).normalized, out hit, maxDistance, collisionLayer))
        {
            // 충돌이 감지되면 충돌 지점에서 살짝 뒤로 물러난 거리를 목표 거리로 설정
            currentCollisionDistance = Mathf.Clamp(hit.distance - collisionOffset, minDistance, maxDistance);
        }
        else
        {
            // 충돌이 없으면 최대 거리로 설정
            currentCollisionDistance = maxDistance;
        }

        // 최종 카메라의 로컬 위치 계산 (y축은 따로 처리)
        Vector3 targetLocalPos = dirNormalized * currentCollisionDistance;

        // 플레이어 상태에 따른 카메라 Y 위치 조정
        if (playerMovement != null && playerMovement.playerCrouch)
        {
            targetLocalPos.y = originalCamLocalY * 0.3f; // 웅크릴 때 Y 위치 조정
        }
        else
        {
            targetLocalPos.y = originalCamLocalY; // 평상시 Y 위치 유지
        }

        // realCamera의 로컬 위치를 부드럽게 목표 위치로 이동
        realCamera.localPosition = Vector3.Lerp(
            realCamera.localPosition,
            targetLocalPos,
            Time.deltaTime * smoothness
        );
    }

    //카메라 흔들기 코루틴
    public IEnumerator Shake(float duration, float magnitude)
    {
        if (realCam == null) yield break;

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

        //원래 위치 복구
        realCam.transform.localPosition = originalPos;
    }
}