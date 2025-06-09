using UnityEngine;

public class Creature1SceneManager : MonoBehaviour
{

    [SerializeField] private ItemManager itemmanager;

    private void Awake()
    {
        itemmanager.MainItem = PlayerPrefs.GetInt("MainItemValue", 0);
    }

}
