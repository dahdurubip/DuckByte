using UnityEngine;

public class Candle : MonoBehaviour, IInteractable
{
    [Header("Key Settings")]
    //태그
    [SerializeField] private string KeyTag = "Fire";   
    [SerializeField] private ParticleSystem particle;
    private Light candlelight;
    //열리는 애니메이터
    //[SerializeField] private Animator animator; 

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
        candlelight = GetComponentInChildren<Light>();
    }

    private void Start()
    {
        particle.Stop();
        candlelight.enabled = false;
    }

    public void OnInteract(GameObject heldItem)
    {
        //이미 켜진 촛불은 무시
        if (isLit) return;

        if (heldItem == null) return;

        if (heldItem.CompareTag(KeyTag))
        {
            m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.northRsmallFire);
            OpenCandle();
        }
        else
        {
            //Debug.Log("아닙니다.");
        }
    }

    private void OpenCandle()
    {
        //Debug.Log("켜졌습니다!");
        particle.Play();
        candlelight.enabled = true;
        //촛불 상태를 켜진 상태로 설정
        isLit = true;  

        //매니저에게 이 촛불이 켜졌음을 알림
        manager.OnCandleLit(myIndex);
    }

    public void ResetCandle()
    {
        particle.Stop();
        candlelight.enabled = false;
        isLit = false;
    }
}
