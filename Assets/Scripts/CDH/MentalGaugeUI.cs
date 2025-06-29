using UnityEngine;
using UnityEngine.UI;

public class MentalGaugeUI : MonoBehaviour
{
    public Image mentalImage; // UI 이미지
    public PlayerMental playerMental; // 정신력 관리 클래스 참조

    private float maxMental = 100f;

    void Update()
    {
        float cM = playerMental.currentMental;
        float fillValue = cM / maxMental;
        mentalImage.fillAmount = fillValue;
    }
}
