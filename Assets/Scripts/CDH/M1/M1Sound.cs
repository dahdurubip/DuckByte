using UnityEngine;

public class M1Sound : MonoBehaviour
{
    [SerializeField] AudioSource lF;
    [SerializeField] AudioSource rF;
    void lFoot()
    {
        lF.Play();
    }

    void rFoot()
    {
        rF.Play();
    }
}
