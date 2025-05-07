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
        particle = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        particle.Stop();
    }

    public void OnInteract(GameObject heldItem)
    {
        // 이미 켜진 촛불은 무시
        if (isLit) return;

        if (heldItem == null) return;

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
        isLit = true;  // 촛불 상태를 켜진 상태로 설정

        // 매니저에게 이 촛불이 켜졌음을 알림
        manager.OnCandleLit(myIndex);
    }

    public void ResetCandle()
    {
        particle.Stop();
        isLit = false;
    }
}
