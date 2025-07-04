using UnityEngine;
using UnityEngine.SceneManagement;

public class Creature2SceneManager : MonoBehaviour
{

    [SerializeField] private DialogueManager dialogueManager;


    private void Start()
    {
        //Creature2AudioManager.instance.PlaySfx(Creature2AudioManager.sfx.Phorror);
        dialogueManager.PlayDialogue("inPrison", "boss");

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
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
