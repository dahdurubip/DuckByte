using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BurnAttack : MonoBehaviour
{
    private Coroutine damageCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            damageCoroutine = StartCoroutine(DealDamageOverTime(other));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    IEnumerator DealDamageOverTime(Collider player)
    {
        while (true)
        {
            player.GetComponent<Player>().TakeDamage(5); // 피해 함수 호출
            yield return new WaitForSeconds(1f); // 초당 1회 피해
        }
    }

}
