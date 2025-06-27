using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Default Settings")]
    [SerializeField] private Animator animator;
    //[SerializeField] private GameObject icon;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private AudioSource audioSource;
    //private bool isPlayerNearby = false;
    private bool toggleState = false;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        //if (icon != null)
        //    icon.SetActive(false);
    }

    //private void Update()
    //{
    //    //플레이어가 가까이 있을 때만 작동
    //    if (!isPlayerNearby) return;

    //    if (Input.GetKeyDown(KeyCode.E))
    //    {
    //        if (animator != null)
    //        {
    //            if (toggleState)
    //            {
    //                animator.Play("DoorClose");
    //                PlaySound(closeSound);
    //                toggleState = false;
    //            }
    //            else
    //            {
    //                animator.Play("DoorOpen");
    //                PlaySound(openSound);
    //                toggleState = true;
    //            }
    //        }
    //    }
    //}

    public void TheDorrControl()
    {
        if (animator != null)
        {
            if (toggleState)
            {
                animator.Play("DoorClose");
                PlaySound(closeSound);
                toggleState = false;
            }
            else
            {
                animator.Play("DoorOpen");
                PlaySound(openSound);
                toggleState = true;
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        isPlayerNearby = true;
    //        if (icon != null)
    //            icon.SetActive(true);
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        isPlayerNearby = false;
    //        if (icon != null)
    //            icon.SetActive(false);
    //    }
    //}
}
