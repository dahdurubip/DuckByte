using UnityEngine;
using UnityEngine.UIElements;

public class RockOff : MonoBehaviour
{
    public GameObject dustEffectPrefab; // 먼지 프리팹
    public GameObject burningGroundPrefab; // 불바닥 프리팹
    //public GameObject BurnParticlePrefab; // 불바닥 연기 파티클
    public float destroyDelay = 2f;     // 먼지 이펙트 제거 시간
    public float burnDuration = 3f;        // 불바닥 제거 시간

    public BossPhaseManager bossPhaseManager;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // 충돌 지점
            Vector3 contactPoint = collision.contacts[0].point;

            // 돌 제거
            Destroy(gameObject);

            Quaternion rotation = Quaternion.LookRotation(Vector3.up);

            // 먼지 이펙트 생성
            //GameObject dust = Instantiate(dustEffectPrefab, contactPoint, Quaternion.identity);
            GameObject dust = Instantiate(dustEffectPrefab, contactPoint, Quaternion.LookRotation(Vector3.up));

            if(bossPhaseManager.currentPhase == 2)
            {
            BurningGround(contactPoint);
            }
            // 먼지 이펙트 일정 시간 후 제거
            Destroy(dust, destroyDelay);



        }
    }

    void BurningGround(Vector3 position)
    {
        GameObject fire = Instantiate(burningGroundPrefab, position, Quaternion.identity);
        Destroy(fire, burnDuration);

        //GameObject BurnParticle = Instantiate(BurnParticlePrefab, position, Quaternion.identity);
        //Destroy(BurnParticle, burnDuration);
    }
}
