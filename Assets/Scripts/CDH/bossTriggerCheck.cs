using UnityEngine;

public class bossTriggerCheck : MonoBehaviour
{
    public GameObject bossTM;
    //public GameObject camera3;


    //private void Start()
    //{
    //    int savedItemCount = PlayerPrefs.GetInt("MainItemCount", -1);
    //    string savedScene = PlayerPrefs.GetString("SavedScene", "없음");

    //    Debug.Log("puzzle 저장된 아이템 수: " + savedItemCount);
    //    Debug.Log("puzzle 저장된 씬 이름: " + savedScene);
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bossTM.SetActive(true);
            //camera3.SetActive(true);
        }
    }
}
