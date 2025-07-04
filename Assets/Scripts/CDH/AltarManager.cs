using UnityEngine;

public class AltarManager : MonoBehaviour, IInteractable
{
    [Header("제단 상호작용 아이템")]
    public string requiredItemTag = "Piece";
    public BossPhaseManager bossPhaseManager;
    public ParticleSystem altarEffect;
    [SerializeField] AudioSource itemAudio;
    [SerializeField] private ItemManager itemmanager;




    private void Awake()
    {
        altarEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void OnInteract(GameObject heldItem)
    {
        // 1) 아이템이 널(null)이면 무시
        if (heldItem == null) return;

        // 2) 태그가 맞는 열쇠인지 확인
        if (heldItem.CompareTag(requiredItemTag))
        {
            PhaseClear();
            altarEffect.Play(true); // 파티클 재생
            itemAudio.Play();
        }
        else
        {
            //Debug.Log("이건 열쇠가 아닙니다.");
        }
    }

    void PhaseClear()
    {
        //Debug.Log("상자가 열립니다!");
        //if (animator != null)
        //animator.SetTrigger("Open");
        // TODO: 아이템 스폰, 소리 재생 등 추가

        // 조각 아이템 없애기
        itemmanager.DestroyCurrentItem(); //오브젝트 파괴하는 코드


        int nextPhase = bossPhaseManager.currentPhase + 1;
        //bossPhaseManager.SetPhase(nextPhase);
        if (nextPhase == 4)
        {
            // 마지막 페이즈 도달 시 따로 처리
            bossPhaseManager.SetPhase(nextPhase);
        }
        else
        {
            bossPhaseManager.triggerPhaseTransition(nextPhase);
        }

    }

}
