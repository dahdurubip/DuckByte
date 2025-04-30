using UnityEngine;

public class Candle : MonoBehaviour, IInteractable
{
    [Header("Key Settings")]
    public string requiredKeyTag = "Fire";    // 필요 열쇠 태그
    public ParticleSystem particle;
    //public Animator animator;                // 열리는 애니메이터

    private CandleManager manager;
    private int myIndex;
    private bool isLit = false;

    public void Initialize(CandleManager mgr, int index)
    {
        manager = mgr;
        myIndex = index;
    }


    private void Awake()
    {
        //if (animator == null) animator = GetComponent<Animator>();
        particle = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        particle.Stop();
    }
    public void OnInteract(GameObject heldItem)
    {
        // 1) 아이템이 널(null)이면 무시
        if (heldItem == null) return;

        // 2) 태그가 맞는 열쇠인지 확인
        if (heldItem.CompareTag(requiredKeyTag))
        {
            OpenChest();
        }
        else
        {
            Debug.Log("아닙니다.");
        }
    }

    private void OpenChest()
    {
        Debug.Log("켜졌습니다!");
        particle.Play();
        //if (animator != null)
        //animator.SetTrigger("Open");
        // TODO: 아이템 스폰, 소리 재생 등 추가
    }

    /// <summary>
    /// 매니저의 오답 처리 시 호출되어 촛불을 초기 상태로 되돌립니다.
    /// </summary>
    public void ResetCandle()
    {
        particle.Stop();
        isLit = false;
    }

}
