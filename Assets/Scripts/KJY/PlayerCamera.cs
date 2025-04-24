using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
//{
//    // 따라갈 대상 (플레이어)
//    public Transform target;

//    // 카메라가 타겟에 대해 수평으로 떨어질 거리 (후방 거리)
//    public float distance = 2f;
//    // 카메라의 Y 좌표를 고정할 값 (원하는 경우 Y=7)
//    public float fixedHeight = 7f;

//    // 카메라 이동 보간 속도
//    public float smoothSpeed = 0.125f;

//    // 마우스 회전 감도
//    public float mouseSensitivity = 2.0f;

//    // yaw 오프셋 복귀 속도
//    public float returnSpeed = 2.0f;

//    // 현재 카메라의 yaw 오프셋 (좌우 회전 각도)
//    private float currentYawOffset = 0f;

//    void LateUpdate()
//    {
//        // 우클릭(마우스 오른쪽 버튼)이 눌린 상태에서만 마우스 좌우 이동으로 회전 조정
//        if (Input.GetMouseButton(1))
//        {
//            float mouseX = Input.GetAxis("Mouse X");
//            currentYawOffset += mouseX * mouseSensitivity;
//        }
//        else
//        {
//            // 우클릭이 풀리고, 전진 입력이 있을 경우(currently forwardInput > 0.1f) 오프셋을 0 방향으로 자연 복귀
//            float forwardInput = Input.GetAxis("Vertical");
//            if (forwardInput > 0.1f)
//            {
//                currentYawOffset = Mathf.Lerp(currentYawOffset, 0f, returnSpeed * Time.deltaTime);
//            }
//        }

//        // 원하는 카메라 회전: 피치 25도, yaw = currentYawOffset, roll 0
//        Quaternion desiredRotation = Quaternion.Euler(25f, currentYawOffset, 0f);

//        // 원하는 카메라 위치:
//        // 1. 타겟의 수평 기준 위치: (target.position.x, fixedHeight, target.position.z)
//        // 2. 여기서 마우스 yaw 오프셋(수평 회전만 적용한 회전)을 사용하여, 뒷방향(distance)을 계산하고 더합니다.
//        Vector3 basePos = new Vector3(target.position.x, fixedHeight, target.position.z - 3f);
//        // yaw만 적용한 회전: x축 피치는 0 (수평 평면에서의 회전)
//        Quaternion yawRotation = Quaternion.Euler(0f, currentYawOffset, 0f);
//        Vector3 offset = yawRotation * new Vector3(0f, 0f, -distance);
//        Vector3 desiredPosition = basePos + offset;

//        // 부드럽게 이동
//        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
//        // 회전은 고정 값으로 적용 (즉, 무조건 피치 25, yaw = currentYawOffset, roll 0)
//        transform.rotation = desiredRotation;
//    }
//}
//{
//    // 따라갈 대상 (플레이어)
//    public Transform target;
//    // 플레이어의 로컬 기준 offset: 예를 들어 (0, 7, -4)는 플레이어 위치에서 7 단위 위, 4 단위 뒤에 위치
//    public Vector3 offset = new Vector3(0, 7, -4);
//    // 카메라 이동 보간 속도
//    public float smoothSpeed = 0.125f;
//    // 고정 피치 각도 (X축) – 25도로 설정하여 플레이어 뒤를 바라봄
//    public float fixedPitch = 25f;

//    void LateUpdate()
//    {
//        // 플레이어의 로컬 좌표계를 기준으로 offset 계산:
//        // target.TransformDirection(offset)는 플레이어의 현재 회전에 따라 offset을 회전시켜 줍니다.
//        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
//        // 부드럽게 위치 이동
//        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

//        // 카메라 회전 설정: 플레이어의 yaw (Y축 회전)은 그대로, 피치는 fixedPitch로 고정, roll은 0
//        // 이렇게 하면 카메라의 forward 벡터는 플레이어의 forward와 같게 되어,
//        // 카메라가 플레이어의 앞면이 아니라 뒤통수를 찍게 됩니다.
//        Quaternion desiredRotation = Quaternion.Euler(fixedPitch, target.eulerAngles.y, 0);
//        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, smoothSpeed);
//    }
//}
{
    // 따라갈 대상 (플레이어)
    public Transform target;
    // 기본 오프셋: 플레이어의 머리 뒤에 위치 (예: (0, 7, -2) -> Y=7, 뒤쪽 2)
    public Vector3 defaultOffset = new Vector3(0, 7, -2);

    // 고정 피치 각도 (X축 회전 값, 25도로 고정)
    public float fixedPitch = 25f;

    // 카메라 이동 보간 속도
    public float smoothSpeed = 0.125f;
    // 마우스 회전 감도
    public float mouseSensitivity = 2.0f;
    // 자동 복귀 속도 (전진 입력 시 yaw 오프셋 복귀)
    public float returnSpeed = 2.0f;

    // 현재 마우스에 의한 yaw 오프셋 (플레이어 기본 방향에 더해짐)
    private float currentYawOffset = 0f;

    void LateUpdate()
    {
        // 오른쪽 마우스 버튼이 눌렸을 때 마우스 좌우 입력으로 yaw 오프셋 조정
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            currentYawOffset += mouseX * mouseSensitivity;
        }
        else
        {
            // 오른쪽 마우스 버튼이 풀린 상태에서, 플레이어가 전진 입력을 줄 경우
            // 카메라의 yaw 오프셋을 기본 상태(0)로 자연스럽게 복귀시킵니다.
            float forwardInput = Input.GetAxis("Vertical");  // 전진 입력 (예: W 키)
            if (forwardInput > 0.1f)
            {
                currentYawOffset = Mathf.Lerp(currentYawOffset, 0f, returnSpeed * Time.deltaTime);
            }
        }

        // 플레이어의 기본 회전(요)는 target.eulerAngles.y입니다.
        // 여기에 currentYawOffset을 더해 최종 yaw를 계산합니다.
        float finalYaw = target.eulerAngles.y + currentYawOffset;
        // 고정 피치와 계산된 yaw, 그리고 roll 0으로 desiredRotation 생성
        Quaternion desiredRotation = Quaternion.Euler(fixedPitch, finalYaw, 0f);

        // 플레이어의 위치를 기준으로 desiredOffset 계산:
        // 기본 오프셋을 desiredRotation으로 회전시키면, 카메라는 플레이어의 현재 방향(마우스 입력에 따라 변하는 yaw 포함)에 대해 뒤쪽으로 배치됩니다.
        Vector3 desiredOffset = desiredRotation * defaultOffset;

        // 목표 카메라 위치 = 플레이어 위치 + 계산된 오프셋
        Vector3 desiredPosition = target.position + desiredOffset;
        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        // 카메라 회전은 원하는 고정 값(desiredRotation)으로 적용합니다.
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, smoothSpeed);
    }
}