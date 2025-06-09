using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    [SerializeField] private ItemManager itemmanager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerPrefs.SetInt("MainItemValue", itemmanager.MainItem);
            PlayerPrefs.Save();
            //SceneManager.LoadScene("Creature1Map");
            SceneLoad.LoadSceneWithLoading("Creature1Map");
        }
    }

    public void RestartScene()
    {
        SceneManager.LoadScene("Creature2Map");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}