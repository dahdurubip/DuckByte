using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{

    public Transform objectTofollow;
    public float followSpeed = 10f;
    public float sensitivity = 100f;
    public float clampAngle = 70f;

    private float rotX;
    private float rotY;

    [SerializeField] private GameObject realCam;
    public Transform realCamera;
    public Vector3 dirNormalized;
    public Vector3 finalDir;
    public float minDistance;
    public float maxDistance;
    public float finalDistance;
    public float smoothness = 10f;

    //시야 고정 토글
    private bool isViewLocked = false;


    private void Start()
    {
        rotX = transform.localRotation.eulerAngles.x;
        rotY = transform.localRotation.eulerAngles.y;

        dirNormalized = realCamera.localPosition.normalized;
        finalDistance = realCamera.localPosition.magnitude;

       
    }

    private void Update()
    {

        //Q키를 눌렀을 때 토글: 한 번 누르면 잠금, 다시 누르면 해제
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isViewLocked = !isViewLocked;
        }

        //시야가 잠겨 있지 않을 때만 마우스 입력으로 회전값 업데이트
        if (!isViewLocked)
        {
            rotX += -Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
            rotY += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);
        }

        //회전 적용 (잠긴 상태여도 마지막 rotX/rotY가 유지됨)
        Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = rot;

    }

    private void LateUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, objectTofollow.position, followSpeed * Time.deltaTime);
        
        finalDir = transform.TransformPoint(dirNormalized * maxDistance);

        RaycastHit hit;
        if(Physics.Linecast(transform.position, finalDir, out hit))
        {
            finalDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }
        else
        {
            finalDistance = maxDistance;
        }
        realCamera.localPosition = Vector3.Lerp(realCamera.localPosition, dirNormalized * finalDistance, Time.deltaTime * smoothness);
    }

    //흔들릴 시간, 세기
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = realCam.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            //랜덤 오프셋 계산
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            //카메라 위치 갱신
            realCam.transform.localPosition = new Vector3(originalPos.x + offsetX,
                                                  originalPos.y + offsetY,
                                                  originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 원래 위치로 복구
        realCam.transform.localPosition = originalPos;
    }

}
