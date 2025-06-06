using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CameraMovement : MonoBehaviour
{
    [Header("Default Settings")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Follow Settings")]
    [SerializeField] private Transform objectTofollow;
    [SerializeField] private float followSpeed = 10f;
    //원래 카메라 로컬 Y 위치 저장용
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
    //월드 공간 목표지점
    [SerializeField] private Vector3 finalDir;                        
    [SerializeField] private float minDistance;                        
    [SerializeField] private float maxDistance;                       
    //충돌 보정 거리
    [SerializeField] private float finalDistance;                     
    //보간 속도
    [SerializeField] private float smoothness = 10f;                  

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
            dirNormalized = realCamera.localPosition.normalized;
            finalDistance = realCamera.localPosition.magnitude;
            //originalCamLocalY = realCamera.localPosition.y; 
        }
        else
        {
            Debug.LogError("Camera Wrong");
        }
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
                rotX += -mouseYInput * sensitivity * Time.deltaTime;
                rotY += mouseXInput * sensitivity * Time.deltaTime;
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

        //transform.position = Vector3.MoveTowards(
        //    transform.position,
        //    objectTofollow.position,
        //    followSpeed * Time.deltaTime
        //);
        transform.position = objectTofollow.position;


        finalDir = transform.TransformPoint(dirNormalized * maxDistance);
        RaycastHit hit;
        if (Physics.Linecast(transform.position, finalDir, out hit))
        {
            finalDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }
        else
        {
            finalDistance = maxDistance;
        }

        Vector3 targetLocalPos = dirNormalized * finalDistance;

        if (playerMovement != null && playerMovement.playerCrouch)
        {
            targetLocalPos.y = originalCamLocalY * 0.3f;
        }
        else
        {
            targetLocalPos.y = originalCamLocalY;
        }

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
