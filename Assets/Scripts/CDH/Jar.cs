using UnityEngine;

public class Jar : MonoBehaviour
{
    public GameObject intactJar;   // 깨지지 않은 장독대
    public GameObject brokenJar;   // 깨진 장독대 (조각 상태)
    //public ParticleSystem breakEffect; // 파편 효과 (선택)

    private bool isBroken = false;

    public void BreakJar()
    {
        if (isBroken) return;
        isBroken = true;

        intactJar.SetActive(false);         // 원본 꺼짐
        brokenJar.SetActive(true);          // 조각 켜짐

        //if (breakEffect != null)
        //    breakEffect.Play();             // 파티클 재생

        // 조각에 Rigidbody 붙어 있으면 중력 적용됨
        Debug.Log("장독대 깨짐!");
    }
}
