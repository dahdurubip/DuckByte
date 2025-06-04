using UnityEngine;

public class Jiockdo : MonoBehaviour, IInteractable
{
    [Header("requiredKeyTag Settings")]
    //필요 열쇠 태그
    public string requiredKeyTag = "Bulsang";
    [SerializeField] private ItemManager itemmanager;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private GameObject JiockdoArt;

    [Header("first door")]
    [SerializeField] private GameObject fDoor1;
    [SerializeField] private GameObject fDoor2;
    [Header("second door")]
    [SerializeField] private GameObject sDoor1;
    [SerializeField] private GameObject sDoor2;
    [Header("third door")]
    [SerializeField] private GameObject tDoor1;
    [SerializeField] private GameObject tDoor2;
    [Header("fourth door")]
    [SerializeField] private GameObject FDoor1;
    
    
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
            Debug.Log("이건 불상이 아닙니다.");
        }
    }

    private void ClearJiockdo()
    {
        Debug.Log("불상을 바쳤습니다");
        Destroy(itemmanager.currentItem);
        // 지옥도 그림 나타남
        JiockdoArt.SetActive(true);
        // 풀린 자물쇠가 사라짐
        //SCDoorLock.SetActive(false);
        // 문들 나타나는 대사
        dialogueManager.PlayDialogue("clearToJiockdo");
        // 숨겨진 문과 상호작용 할 수 있게됨
        fDoor1.layer = LayerMask.NameToLayer("Interactable");
        fDoor2.layer = LayerMask.NameToLayer("Interactable");
        sDoor1.layer = LayerMask.NameToLayer("Interactable");
        sDoor2.layer = LayerMask.NameToLayer("Interactable");
        tDoor1.layer = LayerMask.NameToLayer("Interactable");
        tDoor2.layer = LayerMask.NameToLayer("Interactable");
        FDoor1.layer = LayerMask.NameToLayer("Interactable");

    }
}
