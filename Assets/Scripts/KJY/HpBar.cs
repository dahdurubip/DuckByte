using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HpBar : MonoBehaviour
{
    [SerializeField] private RectTransform yellowRectTr = null;
    [SerializeField] private RectTransform redImgTr = null;
    [SerializeField] private RectTransform transparentTr = null;

    private float maxWidth = 100f;
    private float maxHeight = 100f;

    private void Awake()
    {
        maxWidth = yellowRectTr.sizeDelta.x;
        maxHeight = yellowRectTr.sizeDelta.y;

        // 앵커와 피벗을 왼쪽으로 설정
        yellowRectTr.pivot = new Vector2(0f, 0.5f);  // 왼쪽 중앙 기준
        yellowRectTr.anchorMin = new Vector2(0f, 0.5f);  // 왼쪽 기준
        yellowRectTr.anchorMax = new Vector2(0f, 0.5f);  // 왼쪽 기준

        // redImg도 같은 방식으로 설정
        redImgTr.pivot = new Vector2(0f, 0.5f);  // 왼쪽 중앙 기준
        redImgTr.anchorMin = new Vector2(0f, 0.5f);  // 왼쪽 기준
        redImgTr.anchorMax = new Vector2(0f, 0.5f);

        transparentTr.pivot = new Vector2(0f, 0.5f);  // 왼쪽 중앙 기준
        transparentTr.anchorMin = new Vector2(0f, 0.5f);  // 왼쪽 기준
        transparentTr.anchorMax = new Vector2(0f, 0.5f);  // 왼쪽 기준
    }


    public void UpdateHpBar(float _maxHp, float _curHp)
    {
        UpdateHpBar(_curHp / _maxHp);
    }

    public void UpdateHpBar(float _amount)
    {
        float prevWidth = yellowRectTr.sizeDelta.x;
        float newWidth = maxWidth * _amount;

        StopAllCoroutines();
        if (newWidth < prevWidth)
        {
            StartCoroutine(UpdateHpBarCoroutine(prevWidth, newWidth));
        }
        else
        {
            yellowRectTr.sizeDelta = new Vector2(newWidth, maxHeight);
        }

        redImgTr.sizeDelta = new Vector2(newWidth, maxHeight);
    }

    private IEnumerator UpdateHpBarCoroutine(float _prevWidth, float _newWidth)
    {
        Vector2 size = new Vector2(_prevWidth, maxHeight);
        yellowRectTr.sizeDelta = size;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            size.x = Mathf.Lerp(_prevWidth, _newWidth, t);
            yellowRectTr.sizeDelta = size;
            yield return null;
        }
    }

}

