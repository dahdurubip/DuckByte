using Unity.Mathematics;
using UnityEngine;

public class Jiockdo : MonoBehaviour, IInteractable
{
    [Header("requiredKeyTag Settings")]
    //필요 열쇠 태그
    public string requiredKeyTag = "Bulsang";
    [SerializeField] private ItemManager itemmanager;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private GameObject JiockdoArt;
    [SerializeField] private GameObject circle;
    [SerializeField] private GameObject clearVolume;

    [Header("first door")]
    [SerializeField] private Collider D1;

    [Header("second door")]
    [SerializeField] private Collider D2;

    [Header("third door")]
    [SerializeField] private Collider D3;

    [Header("fourth door")]
    [SerializeField] private Collider D4;



    //[SerializeField] private GameObject SCDoorLock;

    //IInteractable 구현
    public void OnInteract(GameObject heldItem)
    {
        //1) 아이템이 널(null)이면 무시
        if (heldItem == null) return;

        //2) 태그가 맞는 열쇠인지 확인
        if (heldItem.CompareTag(requiredKeyTag))
        {
            ClearJiockdo();
        }
        else
        {
           // Debug.Log("이건 불상이 아닙니다.");
        }
    }

    private void ClearJiockdo()
    {
        //Debug.Log("불상을 바쳤습니다");
        //Destroy(itemmanager.currentItem);
        itemmanager.DestroyCurrentItem(); //오브젝트 파괴하는 코드
        // 지옥도 그림 나타남
        JiockdoArt.SetActive(true);
        circle.SetActive(false);
        clearVolume.SetActive(true);
        // 풀린 자물쇠가 사라짐
        //SCDoorLock.SetActive(false);
        // 문들 나타나는 대사
        dialogueManager.PlayDialogue("clearToJiockdo", "player");
        // 숨겨진 문과 상호작용 할 수 있게됨
        D1.enabled = true;
        D2.enabled = true;
        D3.enabled = true;
        D4.enabled = true;

    }
}
