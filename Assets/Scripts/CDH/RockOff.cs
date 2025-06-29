using UnityEngine;
using UnityEngine.UIElements;

public class RockOff : MonoBehaviour
{
    public GameObject dustEffectPrefab; // 먼지 프리팹
    //public GameObject BurnParticlePrefab; // 불바닥 연기 파티클
    public float destroyDelay = 2f;     // 먼지 이펙트 제거 시간
    public GameObject burningGroundPrefab; // 불바닥 프리팹
    public float burnDuration = 3f;        // 불바닥 제거 시간
    public Vector3 contactPoint;

    public BossPhaseManager bossPhaseManager;
    //[SerializeField] AudioSource rockAudio;
    public AudioClip breakingClip;               // 돌 깨지는 소리
    public GameObject soundPlayerPrefab;         // 사운드 재생용 프리팹


    //public bool IsRockOff { get; private set; }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Player"))
        {
            // 충돌 지점
            contactPoint = collision.contacts[0].point;

            // 돌 제거
            Destroy(gameObject);

            // 돌 깨지는 소리
            //rockAudio.Play();


            GameObject soundObj = Instantiate(soundPlayerPrefab, contactPoint, Quaternion.identity);
            AudioSource audio = soundObj.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.clip = breakingClip;
                audio.spatialBlend = 1f;
                audio.minDistance = 1f;
                audio.maxDistance = 15f;
                audio.Play();
                Destroy(soundObj, breakingClip.length);
            }


            Quaternion rotation = Quaternion.LookRotation(Vector3.up);

            // 먼지 이펙트 생성
            //GameObject dust = Instantiate(dustEffectPrefab, contactPoint, Quaternion.identity);
            GameObject dust = Instantiate(dustEffectPrefab, contactPoint, Quaternion.LookRotation(Vector3.up));

            //IsRockOff = true;
            //Debug.Log("락오프 스크립트" + IsRockOff);

            //Debug.Log("락오프 스크립트" + bossPhaseManager.currentPhase);

            //if(bossPhaseManager.currentPhase == 2)
            //{
            //BurningGround(contactPoint);
            //}

            if (collision.gameObject.CompareTag("Ground") && bossPhaseManager != null && bossPhaseManager.currentPhase == 2)
            {
                BurningGround(contactPoint);
            }

            // 먼지 이펙트 일정 시간 후 제거
            Destroy(dust, destroyDelay);



        }
    }


    public void BurningGround(Vector3 position)
    {
        //Debug.Log("땅 불탐");
        GameObject fire = Instantiate(burningGroundPrefab, position, Quaternion.identity);
        Destroy(fire, burnDuration);

        //GameObject BurnParticle = Instantiate(BurnParticlePrefab, position, Quaternion.identity);
        //Destroy(BurnParticle, burnDuration);
    }
}
