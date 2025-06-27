using System.Collections.Generic;
using UnityEngine;

public class Creature2AudioManager : MonoBehaviour
{
    public static Creature2AudioManager instance;

    [Header("BGM")]
    public AudioClip bgmClip;
    public float bgmVolume;
    AudioSource bgmPlayer;

    [Header("SFX")]
    public AudioClip[] sfxClip;
    public float sfxVolume;
    public int channels;
    AudioSource[] sfxPlayers;
    int channelIndex;

    //들어갈 효과음들 인스펙터 순서대로 이름정리
    public enum sfx { PbabyCry, Pwall, PwaterDrop, PhorrorEffect }

    // SFX 중복 재생 방지
    private Dictionary<sfx, float> sfxLastPlayTime = new Dictionary<sfx, float>();
    public float sfxCooldown = 0.1f;

    void Awake()
    {
        instance = this;
        Init();
    }

    void Init()
    {
        //
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = true;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;

        if (bgmClip != null)
        {
            bgmPlayer.Play();
        }


        // SFX 채널 초기화
        GameObject sfxObject = new GameObject("sfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];
        for (int index = 0; index < sfxPlayers.Length; ++index)
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].volume = sfxVolume;
            sfxPlayers[index].loop = false;
        }

        // SFX 쿨타임 초기화
        foreach (sfx s in System.Enum.GetValues(typeof(sfx)))
        {
            sfxLastPlayTime[s] = -999f;
        }
    }


    //사용하는 방법
    //오디오 나와야하는 곳에가서
    //AudioManager.instance.PlaySfx(AudioManager.Sfx.이때 미리 지정해 둔 enum 효과음 이름을 입력);
    public void PlaySfx(sfx sfx)
    {
        // 쿨타임 검사
        if (Time.time - sfxLastPlayTime[sfx] < sfxCooldown)
            return;

        bool played = false;

        for (int index = 0; index < sfxPlayers.Length; ++index)
        {
            int loopIndex = (index + channelIndex) % sfxPlayers.Length;/*맨 마지막에 실행을했던 플레이어의 인덱스*/
            if (sfxPlayers[loopIndex].isPlaying)
            {
                channelIndex = loopIndex;
                sfxPlayers[loopIndex].clip = sfxClip[(int)sfx];
                sfxPlayers[loopIndex].Play();
                played = true;
                break;
                //같은 이름의 효과음이 여러개 존재 할 시 랜덤으로 소리를 재생  시키고 싶을때
                //만약 여러개 있는게 여러개 있을때 스위치 문으로 나눠주면 된다.
                //int ranIndex = 0;
                //if(sfx == sfx.Hit || sfx == sfx.Melee)
                //{
                //    ranIndex = Random.Range(0, 2);
                //}
            }
        }

        // 모든 채널이 사용 중일 경우, 가장 오래된 채널 강제 재사용 (효과음 누락 방지)
        if (!played)
        {
            channelIndex = (channelIndex + 1) % sfxPlayers.Length;
            sfxPlayers[channelIndex].clip = sfxClip[(int)sfx];
            sfxPlayers[channelIndex].Play();
        }

        sfxLastPlayTime[sfx] = Time.time;

        // channel 하나만 쓸때
        //sfxPlayers[0].clip = sfxClip[(int)sfx];
        //sfxPlayers[0].Play();
    }

    public void Play3DSfx(sfx sfx, Vector3 position, float volume, float minDistance, float maxDistance)
    {
        // 임시 오디오 소스 오브젝트 생성
        GameObject tempSfxObject = new GameObject("Temp3DSfx");
        tempSfxObject.transform.position = position;
        AudioSource tempAudio = tempSfxObject.AddComponent<AudioSource>();

        tempAudio.clip = sfxClip[(int)sfx];
        tempAudio.volume = volume;
        tempAudio.spatialBlend = 1f;             // 3D 효과음으로 설정 (0:2D, 1:3D)
        tempAudio.minDistance = minDistance;      // 최소 거리
        tempAudio.maxDistance = maxDistance;      // 최대 거리
        tempAudio.rolloffMode = AudioRolloffMode.Logarithmic; // 감쇠 모드

        tempAudio.Play();

        // AudioClip 길이 + 약간의 버퍼 후에 임시 오브젝트 제거
        Destroy(tempSfxObject, tempAudio.clip.length + 0.5f);
    }
}
