using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [Header("Hand & Pickup")]
    public Transform handTransform;      // 손 위치 포인트
    public LayerMask itemLayer;         // Item 레이어만 검출
    public float detectRange = 2f;      // 집을 수 있는 최대 거리

    [Header("UI")]
    public GameObject fKeyPromptUI;     // "Press F" UI

    private Camera mainCamera;
    private GameObject currentItem;       // 손에 든 아이템
    private GameObject nearbyItem;        // 근처에 있는 가장 가까운 아이템

    void Start()
    {
        mainCamera = Camera.main;
        if (fKeyPromptUI != null)
            fKeyPromptUI.SetActive(false);
    }

    void Update()
    {
        DetectNearbyItem();

        // F 키 입력 시, 근처 아이템이 있으면 픽업 또는 교체
        if (Input.GetKeyDown(KeyCode.F) && nearbyItem != null)
        {
            PickupItem(nearbyItem);
            // 픽업 후엔 다시 근처 아이템을 검사
            DetectNearbyItem();
        }
    }

    // 반경 내 아이템 검출
    void DetectNearbyItem()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRange, itemLayer);
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var col in hits)
        {
            GameObject go = col.gameObject;

            // ↓ 손에 든 아이템 또는 손 위치의 자식이면 검사 제외
            if (currentItem != null && go == currentItem)
                continue;
            if (handTransform != null && go.transform.IsChildOf(handTransform))
                continue;

            float dist = Vector3.Distance(transform.position, go.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = go;
            }
        }

        nearbyItem = nearest;
        UpdatePromptUI();
    }


    // UI 아이콘 활성/비활성 및 위치 갱신
    void UpdatePromptUI()
    {
        if (fKeyPromptUI == null) return;

        if (nearbyItem != null)
        {
            if (!fKeyPromptUI.activeSelf)
                fKeyPromptUI.SetActive(true);

            // 아이템 머리 위에 프롬프트 위치
            Vector3 worldPos = nearbyItem.transform.position + Vector3.up * 1.5f;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            fKeyPromptUI.transform.position = screenPos;
        }
        else
        {
            if (fKeyPromptUI.activeSelf)
                fKeyPromptUI.SetActive(false);
        }
    }

    // 실제 집기/교체 로직
    void PickupItem(GameObject item)
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

    // 손에 든 아이템 내려놓기
    void DropCurrentItem()
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

    // 에디터에서 거리 확인용
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}

