using UnityEngine;

public class bossTriggerCheck : MonoBehaviour
{
    [SerializeField] private ItemManager itemmanager;
    public GameObject bossTM;
    public GameObject camera3;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerPrefs.SetInt("MainItemValue3", itemmanager.MainItem);
            PlayerPrefs.Save();
            bossTM.SetActive(true);
            camera3.SetActive(true);
        }
    }
}
