using UnityEngine;

public class Jar : MonoBehaviour
{
    public GameObject intactJar;   // 깨지지 않은 장독대
    public GameObject brokenJar;   // 깨진 장독대 (조각 상태)
    //public ParticleSystem breakEffect; // 파편 효과 (선택)

    private bool isBroken = false;

    // 이벤트 구독때 필요
    public GameManager GM;

    public void BreakJar()
    {
        if (isBroken) return;
        isBroken = true;

        intactJar.SetActive(false);         // 원본 꺼짐
        brokenJar.SetActive(true);          // 조각 켜짐

        //if (breakEffect != null)
        //    breakEffect.Play();             // 파티클 재생

        // 조각에 Rigidbody 붙어 있으면 중력 적용됨
       // Debug.Log("장독대 깨짐!");

        // 깨지는 소리 재생
        m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.brokenJar);

        // 해당 위치로 몬스터 쫓아옴
        //Vector3 soundPosition = transform.position + transform.forward * 2f;
        //GM.EmitSound(soundPosition);  // GameManager에 이벤트 전달
        GM.EmitSound(transform.position);  // GameManager에 이벤트 전달
    }
}
