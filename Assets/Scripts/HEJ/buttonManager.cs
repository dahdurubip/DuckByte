using GLTFast.Schema;
using UnityEngine;
using UnityEngine.SceneManagement;



public class buttonManager : MonoBehaviour
{

    [SerializeField] private EndingDataSO endingDataSO;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void firstButton()
    {
        //Debug.Log("1");
        endingDataSO.currentEnding = EndingType.Bad;
        SceneManager.LoadScene("Ending");

    }
    public void secondButton()
    {
        //Debug.Log("2");
        endingDataSO.currentEnding = EndingType.True;
        SceneManager.LoadScene("Ending");

    }
    public void thirdButton()
    {
        //Debug.Log("3");
        endingDataSO.currentEnding = EndingType.Good;
        SceneManager.LoadScene("Ending");

    }
}
