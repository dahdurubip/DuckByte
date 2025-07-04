using UnityEngine;
using UnityEngine.SceneManagement;

public class Creature2SceneManager : MonoBehaviour
{

    [SerializeField] private DialogueManager dialogueManager;


    private void Start()
    {
        //Creature2AudioManager.instance.PlaySfx(Creature2AudioManager.sfx.Phorror);
        dialogueManager.PlayDialogue("inPrison", "boss");

        //int savedItemCount = PlayerPrefs.GetInt("MainItemCount", -1);
        //string savedScene = PlayerPrefs.GetString("SavedScene", "없음");

        //Debug.Log("2 저장된 아이템 수: " + savedItemCount);
        //Debug.Log("2 저장된 씬 이름: " + savedScene);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //SceneManager.LoadScene("Creature1Map");
            // 1씬 클리어 후 현재 씬 및 아이템 갯수 저장
            GameSaveManager.SaveProgress("Creature1Map");
            SceneLoad.LoadSceneWithLoading("Creature1Map");
        }
    }

    public void RestartScene()
    {
        SceneManager.LoadScene("Creature2Map");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
