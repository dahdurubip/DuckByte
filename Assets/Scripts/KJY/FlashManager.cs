using UnityEngine;

public class FlashManager : MonoBehaviour
{
    [Header("Default Settings")]
    public Light flashlightLight;

    [Header("Camera Tracking")]
    //따라갈 카메라 pos
    public Transform cameraTransform; 
    public bool followCameraRotation = true;


    private bool isOn = true;

    private bool isHeld = false;

    private void Awake()
    {
        if (flashlightLight == null) flashlightLight = GetComponentInChildren<Light>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        flashlightLight.enabled = false;
    }

   private void LateUpdate()
    {
        if (followCameraRotation && cameraTransform != null && isHeld)
        {
            transform.rotation = cameraTransform.rotation;
        }
    }

    public void Toggle()
    {
        isOn = !isOn;
        if (flashlightLight != null) flashlightLight.enabled = isOn;
        Debug.Log("isOn" + isOn);
    }

    public bool IsOn()
    {
        return isOn;
    }

    public void TurnOn() => flashlightLight.enabled = true;
    public void TurnOff() => flashlightLight.enabled = false;

    public void SetHeld(bool held)
    {
        isHeld = held;
    }

}
