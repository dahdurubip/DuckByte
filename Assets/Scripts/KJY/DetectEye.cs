using UnityEngine;

public class DetectEye : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Collider eyeDetect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            dialogueManager.PlayDialogue("inPrison1");
            eyeDetect.enabled = false; 
        }
    }

}
