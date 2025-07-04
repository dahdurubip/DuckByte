using UnityEngine;

public static class GameSaveManager
{
    public static void SaveProgress(string nextSceneName)
    {
        //Debug.Log("SaveProgress 호출됨! 저장될 씬 이름: " + nextSceneName);
        //Debug.Log("StackTrace:\n" + System.Environment.StackTrace);

        PlayerPrefs.SetString("SavedScene", nextSceneName);
        PlayerPrefs.SetInt("MainItemCount", MainItemManager.Instance.mainItem);
        PlayerPrefs.Save();
    }
}
