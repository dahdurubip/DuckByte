using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoad : MonoBehaviour
{
    public static void LoadSceneWithLoading(string targetSceneName)
    {
        PlayerPrefs.SetString("NextScene", targetSceneName);
        SceneManager.LoadScene("Loading");
    }
}
