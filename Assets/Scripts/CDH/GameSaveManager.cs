using UnityEngine;

public static class GameSaveManager
{
    public static void SaveProgress(string nextSceneName)
    {
        PlayerPrefs.SetString("SavedScene", nextSceneName);
        PlayerPrefs.SetInt("MainItemCount", MainItemManager.Instance.mainItem);
        PlayerPrefs.Save();
    }
}
