using UnityEngine;

public class bossTriggerCheck : MonoBehaviour
{
    [SerializeField] private ItemManager itemmanager;
    public GameObject bossTM;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerPrefs.SetInt("MainItemValue3", itemmanager.MainItem);
            PlayerPrefs.Save();
            bossTM.SetActive(true);
        }
    }
}
