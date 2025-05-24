using UnityEngine;

public class FlashManager : MonoBehaviour
{
    [Header("Default Settings")]
    [SerializeField] private Light flashlightLight;
    [SerializeField] private Transform flashTr;

    [Header("Camera Tracking")]
    [SerializeField] private Transform cameraTransform;


    [Header("Battery Settings")]
    [SerializeField] private float maxBattery = 1000f;
    [SerializeField] private float currentBattery = 1000f;
    [SerializeField] private float batteryDrainRate = 5f; // 초당 배터리 감소량

    private bool isOn = false;
    private bool isHeld = false;


    private void Awake()
    {
        if (flashlightLight == null) flashlightLight = GetComponentInChildren<Light>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        flashlightLight.enabled = false;
    }

    private void Update()
    {
        if (isHeld && isOn)
        {
            DrainBattery();
        }

    }

    private void LateUpdate()
    {
        flashlightLight.transform.position = flashTr.position;
        if (isHeld && cameraTransform != null)
        {
            transform.rotation = cameraTransform.rotation;
        }
    }

    public void Toggle()
    {
        if (currentBattery <= 0f)
        {
            Debug.Log("배터리 없음");
            return;
        }

        isOn = !isOn;
        if (flashlightLight != null) flashlightLight.enabled = isOn;
        Debug.Log("Flashlight isOn: " + isOn);
    }

    public bool IsOn() => isOn;

    public void TurnOn()
    {
        if (currentBattery > 0f)
        {
            isOn = true;
            flashlightLight.enabled = true;
        }
    }

    public void TurnOff()
    {
        isOn = false;
        flashlightLight.enabled = false;
    }

    public void SetHeld(bool held)
    {
        isHeld = held;
    }

    private void DrainBattery()
    {
        currentBattery -= batteryDrainRate * Time.deltaTime;

        if (currentBattery <= 0f)
        {
            currentBattery = 0f;
            TurnOff();
            Debug.Log("배터리 소진");
        }
    }

    public void RefillBattery(float amount)
    {
        currentBattery = Mathf.Clamp(currentBattery + amount, 0, maxBattery);
        Debug.Log("배터리 충전됨: " + currentBattery);
    }
}
