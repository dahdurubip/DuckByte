using UnityEngine;
using UnityEngine.UI;

public class FlashManager : MonoBehaviour
{
    [Header("Default Settings")]
    [SerializeField] private Light flashlightLight;

    [Header("Camera Tracking")]
    [SerializeField] private Transform cameraTransform;

    [Header("Battery Settings")]
    //배터리 기본 전량
    [SerializeField] private float maxBattery = 1000f;
    [SerializeField] private float currentBattery = 1000f;
    //초당 배터리 감소량
    [SerializeField] private float batteryDrainRate = 5f; 

    [Header("Battery UI")]
    [SerializeField] private Image batteryUI; 

    [Header("Flash UI Settings")]
    [SerializeField] private GameObject flashUI;

    [Header("Flash Particle")]
    public ParticleSystem flashUIParticleSystem;

    [Header("Creature2 Settings")]
    [SerializeField] private Creature2 creature2;

    private bool isOn = false;
    private bool isHeld = false;
    private float flashTimer = 0f;


    private void Awake()
    {
        if (flashlightLight == null) flashlightLight = GetComponentInChildren<Light>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (flashUI != null)  flashUI.SetActive(false);
        if (flashlightLight != null) flashlightLight.enabled = false;
        UpdateBatteryUI();
    }

    private void Update()
    {
        if (isHeld && isOn)
        {
            flashUI.SetActive(true);
            DrainBattery();

            //timer
            flashTimer += Time.deltaTime;
            if (flashTimer >= 5f)
            {
                //creature2 추적활성화
                if (creature2 != null) creature2.flashOn = true;
            }
        }
        else
        {
            flashUI.SetActive(false);

            //만약 isHeld는 true인데 isOn이 false가 되면 creature2.flashOn도 false로 처리
            if (isHeld && !isOn && creature2 != null && creature2.flashOn)
            {
                flashTimer = 0f;
                creature2.flashOn = false;
            }
        }

    }

    private void LateUpdate()
    {

        transform.rotation = cameraTransform.rotation;
    }

    public void Toggle()
    {
        if (currentBattery <= 0f && !isOn)
        {
            //Debug.Log("배터리 없음");
            return;
        }

        isOn = !isOn;
        if (flashlightLight != null) flashlightLight.enabled = isOn;
        //Debug.Log("Flashlight isOn: " + isOn);

        if (!isOn) 
        {
            flashTimer = 0f;
            if (creature2 != null) creature2.flashOn = false;
        }
    }

    public bool IsOn() => isOn;

    public void TurnOn()
    {
        if (currentBattery > 0f)
        {
            isOn = true;
            if (flashlightLight != null) flashlightLight.enabled = true;
        }
    }

    public void TurnOff()
    {
        isOn = false;
        if (flashlightLight != null) flashlightLight.enabled = false;

        flashTimer = 0f;
        if (creature2 != null) creature2.flashOn = false;
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
            //Debug.Log("배터리 소진");
        }

        UpdateBatteryUI();
    }

    public void RefillBattery(float amount)
    {
        currentBattery = Mathf.Clamp(currentBattery + amount, 0, maxBattery);
        //Debug.Log("배터리 충전됨: " + currentBattery);

        UpdateBatteryUI();
    }

    private void UpdateBatteryUI()
    {
        if (batteryUI != null)
        {
            batteryUI.fillAmount = currentBattery / maxBattery; 
        }
    }

}
