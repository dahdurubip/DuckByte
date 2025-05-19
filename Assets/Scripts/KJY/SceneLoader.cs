using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    //public void LoadScene(string sceneName)
    //{
    //    SceneManager.LoadScene(sceneName);
    //}


    private void OnTriggerEnter(Collider other)
    {
        SceneManager.LoadScene("Creature1Map");
    }
}
