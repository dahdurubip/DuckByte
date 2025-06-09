using UnityEngine;

public class BossSceneManager : MonoBehaviour
{

    [SerializeField] private ItemManager itemmanager;


    private void Awake()
    {
        itemmanager.MainItem = PlayerPrefs.GetInt("MainItemValue", 0);
    }

}
