using UnityEngine;
using System;

public class Statue : MonoBehaviour
{
    public bool isLit = false;                    // 불이 켜졌는지
    public event Action<Statue> OnFireLit;        // 불 켤 때 호출할 이벤트
    //public GameObject fireEffect;             // 불 켤 때 나타날 파티클
    public ParticleSystem fireEffect;
    [SerializeField] AudioSource fireAudio;

    private void Awake()
    {
        fireEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void LightFire()
    {
        if (isLit) return;
        isLit = true;

        //fireEffect.SetActive(true);
        fireEffect.Play(true); // 파티클 재생


        // 이펙트, 소리 등 추가 가능
        Debug.Log($"{gameObject.name} 불 켜짐");
        m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.fireOn);
        fireAudio.Play();

        OnFireLit?.Invoke(this);                 // 퍼즐 관리자에게 알림
    }

    public void Extinguish()
    {
        isLit = false;
        // 불 끄는 이펙트/로직 넣기
        if (fireEffect != null)
        {
            //fireEffect.SetActive(false);
            fireEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            fireAudio.Stop();
        }
    }
}
