using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemManager : MonoBehaviour
{
    [Header("Hand & Pickup Settings")]
    //손 위치
    [SerializeField] private Transform handTransform;
    //레이어설정
    [SerializeField] private LayerMask interacterLayer;
    [SerializeField] private LayerMask pickableLayer;
    //전방 원뿔 반경
    [SerializeField] private float detectRange = 5f;
    //전방 원뿔 절반 각도
    [SerializeField] private float panAngle = 30f;
    //flash
    [SerializeField] private FlashManager flashManager;
    [SerializeField] private ObanggiUIManager obanggiUIManager;


    [Header("Key UI Settings")]
    [SerializeField] private GameObject EKeyUI;

    [Header("Gimmick UI Settings")]
    [SerializeField] private GameObject paperUI;
    [SerializeField] private GameObject bookUI;
    [SerializeField] private GameObject skelUI;
    [SerializeField] private GameObject noteUI;

    [Header("Note UI Settings")]
    public TextMeshProUGUI noteUIText;

    //object
    [Header("SKel Settings")]
    [SerializeField] private GameObject ItemSkel1;
    [SerializeField] private GameObject ItemSkel2;

    //particle
    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem paperParticle;
    [SerializeField] private ParticleSystem bookParticle;
    [SerializeField] private ParticleSystem skelParticle;

    //메인 아이템
    public int MainItem = 0;

    //상호작용 키
    private KeyCode interactKey = KeyCode.E;
    private Camera mainCamera;
    //손에 든 아이템
    public GameObject currentItem;
    //현재 근처 대상
    private GameObject nearbyInteractable;
    //픽업 대상
    private GameObject pickableTarget;
    //애니메이션
    private Animator animator;


   // public PlayerTriggerMAnager PTM;

    private void Start()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        if (EKeyUI) EKeyUI.SetActive(false);
        if (paperUI) paperUI.SetActive(false);
        if (bookUI) bookUI.SetActive(false);
        if (skelUI) skelUI.SetActive(false);
    }

    private void Update()
    {

        //1) 카메라 전방 원뿔 검사
        DetectNearbyInteractable();
        DetectNearbyPickable();

        //2) UI 토글 & 위치 업데이트
        bool showUI = (nearbyInteractable != null) || (pickableTarget != null);
        if (EKeyUI.activeSelf != showUI)
            EKeyUI.SetActive(showUI);

        if (showUI)
        {
            var target = nearbyInteractable != null ? nearbyInteractable : pickableTarget;
            Vector3 worldPos = target.transform.position + Vector3.up * 0.3f;
            EKeyUI.transform.position = mainCamera.WorldToScreenPoint(worldPos);
        }

        if (Input.GetKeyDown(interactKey))
        {
            //E키를 누르면 무조건 끄는 것
            if (noteUI.activeSelf)
            {
                noteUI.SetActive(false);
                return;
            }
            if (paperUI.activeSelf)
            {
                paperUI.SetActive(false);
                return;
            }
            if (bookUI.activeSelf)
            {
                bookUI.SetActive(false);
                return;
            }
            if (skelUI.activeSelf)
            {
                skelUI.SetActive(false);
                return;
            }

            //읽기 기믹 (Paper/Book/Skel)
            if (nearbyInteractable != null)
            {
                //Debug.Log("Pick");
                animator.SetTrigger("pickStand");

                HandleCreature2MapInteractions();
                //if (nearbyInteractable.CompareTag("Paper"))
                //{
                //    paperParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                //    paperUI.SetActive(!paperUI.activeSelf);
                //    return;
                //}
                //if (nearbyInteractable.CompareTag("Book"))
                //{
                //    bookParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                //    bookUI.SetActive(!bookUI.activeSelf);
                //    return;
                //}
                //if (nearbyInteractable.CompareTag("Skel"))
                //{
                //    skelParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                //    skelUI.SetActive(!skelUI.activeSelf);
                //    ItemSkel1.SetActive(false);
                //    ItemSkel2.SetActive(true);
                //    return;
                //}

                //쪽지들은 전부 note태그
                //각 쪽지마다 noteText스크립트 달고 해당하는 내용은 각각 수정
                //상호작용 된 쪽지의 컴포넌트 속 Text를 가져와서 글자를 띄움
                if (nearbyInteractable.CompareTag("Note"))
                {
                    NoteText content = nearbyInteractable.GetComponent<NoteText>();
                    if (content != null)
                    {
                        noteUIText.text = content.noteText;
                    }

                    noteUI.SetActive(!noteUI.activeSelf);
                    return;
                }
                // 크리처2맵에서 석상 불켜기 할 때 필요한 태그
                if (nearbyInteractable.CompareTag("Statue"))
                {
                    Statue statue = nearbyInteractable.GetComponent<Statue>();
                    if (statue != null)
                    {
                        statue.LightFire();
                    }
                    return;
                }
                ////Door 열기/닫기
                //if (nearbyInteractable.TryGetComponent<Door>(out var door))
                //{
                //    //Debug.Log("Door");
                //    door.Toggle();
                //    return;
                //}

                //IInteractable
                if (nearbyInteractable.TryGetComponent<IInteractable>(out var inter))
                {
                    inter.OnInteract(currentItem);
                    return;
                }

                //장독대
                if (nearbyInteractable.CompareTag("Jar"))
                {
                    Jar breaker = nearbyInteractable.GetComponent<Jar>();
                    if (breaker != null)
                    {
                        breaker.BreakJar();
                    }
                    return;


                }
                if (nearbyInteractable.CompareTag("UnBrokenJar"))
                {
                    PlayerTriggerMAnager PTM = GetComponent<PlayerTriggerMAnager>();
                    Debug.Log("흠");
                    if (PTM != null)
                    {
                    Debug.Log("흠2");
                        PTM.unBrokenJar();
                    }
                    return;
                }
                if (nearbyInteractable.CompareTag("BossDoor"))
                {
                    SceneManager.LoadScene("Boss");
                    return;
                }


                //메인 아이템이면
                if (nearbyInteractable.CompareTag("MainItem"))
                {
                    MainItem += 1;
                    return;
                }

                if (nearbyInteractable.CompareTag("Obanggi"))
                {
                    obanggiUIManager.mainUIPanel.SetActive(true);
                    return;
                }


            }

            //픽업
            if (pickableTarget != null)
            {
                //animator.SetTrigger("pickSit");
                //PickupItem(pickableTarget);
                //return;


                Vector3 directionToTarget = pickableTarget.transform.position - transform.position;
                float verticalOffset = directionToTarget.y;

                if (verticalOffset < 0.5f) //바닥에 있을 경우
                {
                    animator.SetTrigger("pickSit");
                }
                else //앞에 있을 경우
                {
                    animator.SetTrigger("pickStand");
                }

                return;

            }
            //후레쉬 On/Off 전용 처리
            if (currentItem != null && currentItem.CompareTag("Flashlight"))
            {
                flashManager?.Toggle();
            }

        }

    }

    private bool HandleCreature2MapInteractions()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Creature2Map")
            return false;

        if (nearbyInteractable.CompareTag("Paper"))
        {
            paperParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            paperUI.SetActive(!paperUI.activeSelf);
            return true;
        }
        if (nearbyInteractable.CompareTag("Book"))
        {
            bookParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            bookUI.SetActive(!bookUI.activeSelf);
            return true;
        }
        if (nearbyInteractable.CompareTag("Skel"))
        {
            skelParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            skelUI.SetActive(!skelUI.activeSelf);
            ItemSkel1.SetActive(false);
            ItemSkel2.SetActive(true);
            return true;
        }
        if (nearbyInteractable.CompareTag("Battery"))
        {
            if (flashManager != null)
            {
                flashManager.RefillBattery(30f);
            }
            Destroy(nearbyInteractable);
            return true;
        }

        return false;
    }



    public void OnPickupAnimationEnd()
    {
        if (pickableTarget != null)
        {
            PickupItem(pickableTarget);
        }
    }

    private void DetectNearbyInteractable()
    {
        nearbyInteractable = null;
        Vector3 origin = mainCamera.transform.position;
        Vector3 forward = mainCamera.transform.forward;

        var hits = Physics.OverlapSphere(origin, detectRange, interacterLayer, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) =>
            Vector3.Distance(origin, a.transform.position)
            .CompareTo(Vector3.Distance(origin, b.transform.position))
        );

        foreach (var col in hits)
        {
            Vector3 toTarget = (col.transform.position - origin).normalized;
            if (Vector3.Angle(forward, toTarget) <= panAngle)
            {
                nearbyInteractable = col.gameObject;
                break;
            }
        }
    }

    //private void DetectNearbyInteractable()
    //{
    //    nearbyInteractable = null;

    //    Vector3 forward = transform.forward;
    //    Vector3 basePosition = transform.position;
    //    float minHeight = 0f;     // 발밑
    //    float maxHeight = 6f;     // 머리
    //    int rayCount = 30;           // 총 레이 개수
    //    float detectDistance = detectRange; // 기존 값 사용

    //    for (int i = 0; i < rayCount; i++)
    //    {
    //        float t = i / (float)(rayCount - 1);
    //        float height = Mathf.Lerp(minHeight, maxHeight, t);
    //        Vector3 rayOrigin = basePosition + Vector3.up * height;

    //        if (Physics.Raycast(rayOrigin, forward, out RaycastHit hit, detectDistance, interacterLayer))
    //        {
    //            nearbyInteractable = hit.collider.gameObject;
    //            Debug.DrawRay(rayOrigin, forward * detectDistance, Color.green);
    //            break; // 하나라도 맞으면 종료
    //        }

    //        Debug.DrawRay(rayOrigin, forward * detectDistance, Color.gray); // 디버그용
    //    }
    //}




    private void DetectNearbyPickable()
    {
        pickableTarget = null;
        Vector3 origin = mainCamera.transform.position;
        Vector3 forward = mainCamera.transform.forward;

        var hits = Physics.OverlapSphere(origin, detectRange, pickableLayer, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) =>
            Vector3.Distance(origin, a.transform.position)
            .CompareTo(Vector3.Distance(origin, b.transform.position))
        );

        foreach (var col in hits)
        {
            //손에 든 아이템은 제외
            var obj = col.gameObject;
            if (currentItem != null &&
                (obj == currentItem || obj.transform.IsChildOf(handTransform)))
                continue;

            Vector3 toTarget = (col.transform.position - origin).normalized;
            if (Vector3.Angle(forward, toTarget) <= panAngle)
            {
                pickableTarget = obj;
                break;
            }
        }
    }



    private void PickupItem(GameObject item)
    {
        if (currentItem != null)
            DropCurrentItem();

        currentItem = item;
        if (item.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;

        if (item.CompareTag("Flashlight"))
        {
            flashManager = item.GetComponent<FlashManager>();
            flashManager.TurnOn();
            flashManager.SetHeld(true);
        }

        item.transform.SetParent(handTransform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }

    public void DropCurrentItem()
    {
        if (currentItem == null) return;


        if (currentItem.CompareTag("Flashlight"))
        {
            flashManager?.SetHeld(false); 
        }

        var item = currentItem;
        item.transform.SetParent(null);
        if (item.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.AddForce(transform.forward * 2f, ForceMode.Impulse);
        }
        currentItem = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || mainCamera == null)
            return;

        Vector3 origin = mainCamera.transform.position;
        Vector3 forward = mainCamera.transform.forward;
        float halfAngle = panAngle;

        //팬 가장자리 두 선
        Quaternion leftRot = Quaternion.AngleAxis(-halfAngle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(+halfAngle, Vector3.up);
        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + leftDir * detectRange);
        Gizmos.DrawLine(origin, origin + rightDir * detectRange);

        //원뿔 면(호) 시각화
        int segments = 20;
        Vector3 prevPoint = origin + (Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward) * detectRange;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + (2f * halfAngle) * (i / (float)segments);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;
            Vector3 nextP = origin + dir * detectRange;

            Gizmos.DrawLine(prevPoint, nextP);
            prevPoint = nextP;
        }

    }


}