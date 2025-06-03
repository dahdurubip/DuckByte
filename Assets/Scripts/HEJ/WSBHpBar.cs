using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class WSBHpBar : MonoBehaviour
{
    [SerializeField] private RectTransform redImg = null;
    private float maxWidth = 0f;
    private float maxHeight = 0f;

    private void Awake()
    {
       maxWidth = redImg.sizeDelta.x;
       maxHeight = redImg.sizeDelta.y;

    }

    public void UpdateHpBar(float _maxHp, float _curHp)
    {
        UpdateHpBar(_curHp / _maxHp);
    }

    public void UpdateHpBar(float _amount)
    {
        float prevWidth = redImg.sizeDelta.x;
        //float prevWidth = yellowRectTr.sizeDelta.x;
        float newWidth = maxWidth * _amount;

        StopAllCoroutines();

        if (newWidth < prevWidth)
        {
            StartCoroutine(UpdateHpBarCoroutine(prevWidth, newWidth));

        }
        else
        {
            //yellowRectTr.sizeDelta = new Vector2(newWidth, maxHeight);
        }

        redImg.sizeDelta = new Vector2(newWidth, maxHeight);
    }


    private IEnumerator UpdateHpBarCoroutine(float _prevWidth, float _newWidth)
    {
        Vector3 size = new Vector2(_prevWidth, maxHeight);
        //yellowRectTr.sizeDelta = size;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;

            size.x = Mathf.Lerp(_prevWidth, _newWidth, t);
            //yellowRectTr.sizeDelta = size;
        }
        yield return null;
    }




}

