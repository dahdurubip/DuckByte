using Unity.VisualScripting;
using UnityEngine;

public class GimmickManager : MonoBehaviour
{

    //만약에 아이템을 클릭할 때 이 아이템의 이름은 이것이면 이 텍스트를 출력한다.

    //text
    [Header("Text Settings")]
    [SerializeField] private GameObject ItemPaper;
    [SerializeField] private GameObject ItemBook;
    [SerializeField] private GameObject ItemSkel;

    //object
    [Header("SKel Settings")]
    [SerializeField] private GameObject ItemSkel1;
    [SerializeField] private GameObject ItemSkel2;

    //particle
    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem particle1;
    [SerializeField] private ParticleSystem particle2;
    [SerializeField] private ParticleSystem particle3;
    

    private void Update()
    {
        ItemOnClick();
    }

    private void ItemOnClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //첫 번째 아이템 (paper)
                if(hit.transform.gameObject.tag == "Paper")
                {
                    particle1.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    //만약에 클릭하면 텍스트창 띄우기
                    ItemPaper.SetActive(true);
                    //5초후에 꺼진다
                    Invoke("ThePaper", 5f);
                }
                //두 번째 아이템 (일기장)
                else if (hit.transform.gameObject.tag == "Book")
                {
                    particle2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    //만약에 클릭하면 텍스트창 띄우기
                    ItemBook.SetActive(true);
                    //5초후에 꺼진다
                    Invoke("TheBook", 5f);
                }
                //세 번째 아이템 (해골)
                else if (hit.transform.gameObject.tag == "Skel")
                {
                    particle3.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ItemSkel1.SetActive(false);
                    ItemSkel2.SetActive(true);
                    //만약에 클릭하면 해골 손이 땅으로 떨어지고
                    //텍스트창 띄우기
                    ItemSkel.SetActive(true);
                    //5초후에 꺼진다
                    Invoke("TheSkel", 5f);
                }
                
            }
        }
    }

    private void ThePaper()
    {
        ItemPaper.SetActive(false);
    }
    
    private void TheBook()
    {
        ItemBook.SetActive(false);
    }

    private void TheSkel()
    {
        ItemSkel.SetActive(false);
    }

}
