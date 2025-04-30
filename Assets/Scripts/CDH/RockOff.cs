using UnityEngine;

public class RockOff : MonoBehaviour
{
    public GameObject dustEffectPrefab; // 먼지 프리팹 연결
    public float destroyDelay = 2f;     // 먼지 이펙트 제거 시간

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

            // 먼지 이펙트 일정 시간 후 제거
            Destroy(dust, destroyDelay);

        }
    }
}
