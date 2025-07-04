using UnityEngine;

public class bossTriggerCheck : MonoBehaviour
{
    public GameObject bossTM;
    //public GameObject camera3;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bossTM.SetActive(true);
            //camera3.SetActive(true);
        }
    }
}
