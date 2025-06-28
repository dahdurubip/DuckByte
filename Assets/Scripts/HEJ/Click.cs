using UnityEngine;

public class Click : MonoBehaviour
{
    public bool activate = true;
    public GameObject icon;

    // Start is called before the first frame update
    void Start()
    {
        activate = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (activate)
        {
            icon.SetActive(true);
            
        }
        else
        {
            icon.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activate = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activate = false;
        }
    }
}