using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        SceneManager.LoadScene("Creature1Map");
    }

    public void RestartScene()
    {
        SceneManager.LoadScene("Creature2Map");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
