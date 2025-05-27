using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LightProp : MonoBehaviour
{
    [Tooltip("0-based unique lamp ID")]
    public int lampID;

    public GameObject icon;     // E키 UI 아이콘
    public GameObject lightObj; // 실제 조명 오브젝트
    public Renderer lantern;    // Emission 색 변경용
    public int uvIndex = 0;     // Emission 머티리얼 인덱스

    [Header("Sound")]
    public AudioClip lightOnClip;
    [Range(0.0f, 1.0f)]
    public float volume = 1.0f;

    [Header("Detection")]
    public float detectRange = 3.0f;  // 플레이어와의 최대 상호작용 거리

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.0f;  // 2D 사운드
    }

    /// <summary>
    /// E키를 눌렀을 때 이 램프가 반응 범위 안에 있는지
    /// </summary>
    public bool IsInRange(Transform player)
    {
        return Vector3.Distance(transform.position, player.position) <= detectRange;
    }

    /// <summary>
    /// UI 아이콘 on/off
    /// </summary>
    public void ShowIcon(bool show)
    {
        if (icon) icon.SetActive(show);
    }

    /// <summary>
    /// 램프 켜고 끄기 + 소리
    /// </summary>
    public void SetState(bool on)
    {
        if (lightObj) lightObj.SetActive(on);

        if (lantern)
            lantern.materials[uvIndex]
                   .SetColor("_EmissionColor", on ? Color.white : Color.black);

        if (on && lightOnClip)
            audioSource.PlayOneShot(lightOnClip, volume);
    }
}
