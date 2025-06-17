using UnityEngine;

public class bossTriggerCheck : MonoBehaviour
{
    public GameObject bossTM;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bossTM.SetActive(true);
        }
    }
}
