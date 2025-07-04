using UnityEngine;

public class NorthRoomDR : MonoBehaviour
{

    [SerializeField] private DialogueManager dialogueManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            dialogueManager.PlayDialogue("northRoom", "boss");
        }
    }


}
