using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [Header("Hand & Pickup")]
    //손 위치 포인트
    public Transform handTransform;
    //필요하는 레이어만 검출
    public LayerMask pickUpLayer;
    //집을 수 있는 최대 거리
    public float detectRange = 2f;      

    [Header("UI")]
    public GameObject FKey;    
    public GameObject EKey;

    private Camera mainCamera;
    //손에 든 아이템
    public GameObject currentItem;
    //근처에 있는 가장 가까운 아이템
    private GameObject nearbyItem;        

    [Header("Interaction Settings")]
    //상호작용 키
    private KeyCode interactKey = KeyCode.E;
    //최대 거리
    public float interactRange = 15f;
    //팬(원뿔) 각도 절반
    public float panAngle = 30f;
    //상호작용 레이어
    public LayerMask interactableLayer;

    [SerializeField] private Door door;

    private void Start()
    {
        mainCamera = Camera.main;
        if (FKey != null) FKey.SetActive(false);
        if (EKey != null) EKey.SetActive(false);
    }

    private void Update()
    {
        DetectNearbyItem();
        UpdateInteractUI();   //인터랙트 가능 여부 체크

        //F 키 입력 시, 근처 아이템이 있으면 픽업 또는 교체
        if (Input.GetKeyDown(KeyCode.F) && nearbyItem != null)
        {
            if (nearbyItem.tag == "LeftDoor")
            {
                //좌측문 열기 함수 추가
                door.OpenTheLeftDoor();
            }
            else
            {
                PickupItem(nearbyItem);

                //픽업 후엔 다시 근처 아이템을 검사
                DetectNearbyItem();
            }
        }

        //손에 든 아이템이 있을 때만 상호작용 시도
        if (currentItem != null && Input.GetKeyDown(interactKey))
        {
            TryInteractWithTarget();
        }
    }

    //반경 내 아이템 검출
    private void DetectNearbyItem()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRange, pickUpLayer);
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var col in hits)
        {
            GameObject item = col.gameObject;

            //손에 든 아이템 또는 손 위치의 자식이면 검사 제외
            if (currentItem != null && item == currentItem)
                continue;
            if (handTransform != null && item.transform.IsChildOf(handTransform))
                continue;

            float dist = Vector3.Distance(transform.position, item.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = item;
            }
        }

        nearbyItem = nearest;
        UpdatePromptUI();
    }


    //UI 아이콘 활성/비활성 및 위치 갱신
    private void UpdatePromptUI()
    {
        if (FKey == null) return;

        if (nearbyItem != null)
        {
            if (!FKey.activeSelf)
                FKey.SetActive(true);

            //아이템 위에 프롬프트 위치
            Vector3 worldPos = nearbyItem.transform.position + Vector3.up * 0.5f;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            FKey.transform.position = screenPos;
        }
        else
        {
            if (FKey.activeSelf)
                FKey.SetActive(false);
        }
    }

    //실제 집기/교체 로직
    private void PickupItem(GameObject item)
    {
        // 손에 든 게 있으면 내려놓기
        if (currentItem != null)
            DropCurrentItem();

        currentItem = item;
        var rb = currentItem.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;    // 물리 비활성

        currentItem.transform.SetParent(handTransform);
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
    }

    //손에 든 아이템 내려놓기
    //충돌시 이 함수를 호출
    public void DropCurrentItem()
    {
        currentItem.transform.SetParent(null);
        var rb = currentItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(transform.forward * 2f, ForceMode.Impulse);
        }
        currentItem = null;
    }

    //// 에디터에서 거리 확인용
    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, detectRange);
    //}

    //private void UpdateInteractUI()
    //{
    //    if (pressEIconUI == null)
    //        return;

    //    bool canInteract = false;

    //    // 손에 든 아이템이 있을 때만 레이캐스트로 상호작용 검사
    //    if (currentItem != null)
    //    {

    //        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward * interactRange);
    //        Debug.Log("없다");
    //        Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * interactRange, Color.red);
    //        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
    //        {
    //            // IInteractable 구현체가 있는지 확인
    //            if (hit.collider.GetComponent<IInteractable>() != null)
    //                Debug.Log("있다");
    //                canInteract = true;
    //        }
    //    }

    //    pressEIconUI.SetActive(canInteract);
    //}

    ///// <summary>
    ///// 카메라 정면으로 레이를 쏴서 IInteractable 대상이 있으면 상호작용 호출
    ///// </summary>
    //private void TryInteractWithTarget()
    //{
    //    //Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
    //    Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

    //    if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
    //    {
    //        // IInteractable 인터페이스 검사
    //        if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
    //        {
    //            interactable.OnInteract(currentItem);
    //        }
    //        else
    //        {
    //            Debug.Log("이 오브젝트는 상호작용 대상이 아닙니다.");
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("범위 내 상호작용 대상이 없습니다.");
    //    }
    //}

    private void UpdateInteractUI()
    {
        if (EKey == null)
            return;

        bool canInteract = false;

        if (currentItem != null)
        {
            Vector3 origin = mainCamera.transform.position;
            Vector3 forward = mainCamera.transform.forward;

            // 1) 반경 내 모든 후보 뽑기
            Collider[] hits = Physics.OverlapSphere(origin, interactRange, interactableLayer, QueryTriggerInteraction.Collide);

            foreach (var col in hits)
            {
                // 2) 카메라-대상 방향 벡터
                Vector3 toTarget = (col.transform.position - origin).normalized;

                // 3) 각도 검사
                if (Vector3.Angle(forward, toTarget) <= panAngle)
                {
                    // 4) IInteractable 구현체인지 확인
                    if (col.GetComponentInParent<IInteractable>() != null)
                    {
                        canInteract = true;
                        break;
                    }
                }
            }
        }

        EKey.SetActive(canInteract);
    }

    private void TryInteractWithTarget()
    {
        Vector3 origin = mainCamera.transform.position;
        Vector3 forward = mainCamera.transform.forward;

        // 반경 내 모든 후보
        Collider[] hits = Physics.OverlapSphere(origin, interactRange, interactableLayer, QueryTriggerInteraction.Collide);

        // 거리 순 정렬
        System.Array.Sort(hits, (a, b) =>
            Vector3.Distance(origin, a.transform.position)
            .CompareTo(Vector3.Distance(origin, b.transform.position))
        );

        foreach (var col in hits)
        {
            Vector3 toTarget = (col.transform.position - origin).normalized;
            if (Vector3.Angle(forward, toTarget) <= panAngle)
            {
                if (col.GetComponentInParent<IInteractable>() is IInteractable interactable)
                {
                    interactable.OnInteract(currentItem);
                    return;
                }
            }
        }

        Debug.Log("범위 내 상호작용 대상이 없습니다.");
    }

    // OnDrawGizmosSelected() 에도 팬 형태 시각화 추가 가능
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || mainCamera == null)
            return;

        Vector3 origin = mainCamera.transform.position;
        Vector3 forward = mainCamera.transform.forward;

        // 팬 폭 절반 각도
        float halfAngle = panAngle;

        // 원뿔을 분할할 세그먼트 수
        int segments = 20;

        // 1) 팬 가장자리 두 선
        Quaternion leftRot = Quaternion.AngleAxis(-halfAngle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(+halfAngle, Vector3.up);
        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + leftDir * interactRange);
        Gizmos.DrawLine(origin, origin + rightDir * interactRange);

        // 2) 원뿔 면(호) 시각화: segments 개수만큼 라인으로 연결
        Vector3 prevPoint = origin + (Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward) * interactRange;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + (2f * halfAngle) * (i / (float)segments);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;
            Vector3 nextPoint = origin + dir * interactRange;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }


}



