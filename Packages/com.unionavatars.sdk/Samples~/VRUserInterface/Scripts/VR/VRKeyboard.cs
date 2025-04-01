using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
using UnityEngine;

namespace UnionAvatars.Samples.LipSync
{
    public class VRKeyboard : MonoBehaviour
    {
        private TMP_InputField inputField;

        [SerializeField]
        private float distance = 0.5f;

        [SerializeField]
        private float verticalOffset = -0.5f;

        void Start()
        { 
            Debug.Log("Init");
            inputField = GetComponent<TMP_InputField>();
            inputField.onSelect.AddListener(_ => OpenKeyboard());
            NonNativeKeyboard.Instance.OnClosed += UnlinkInputField;
        }

        private void OpenKeyboard()
        {
            Debug.Log("Open");
            NonNativeKeyboard.Instance.InputField = inputField;
            NonNativeKeyboard.Instance.PresentKeyboard(inputField.text);

            Transform sourceTransform = Camera.main.transform;
            Vector3 direction = sourceTransform.forward;
            direction.y = 0;
            direction.Normalize();

            Vector3 targetPosition = sourceTransform.position + direction * distance + Vector3.up * verticalOffset;
            NonNativeKeyboard.Instance.RepositionKeyboard(targetPosition);

            SetCaretColorAlpha(1);
        }

        private void UnlinkInputField(object sender, EventArgs e)
        {
            NonNativeKeyboard.Instance.InputField = null;
            SetCaretColorAlpha(0);
        }

        private void SetCaretColorAlpha(float value)
        {
            inputField.customCaretColor = true;
            Color caretColor = inputField.caretColor;
            caretColor.a = value;
            inputField.caretColor = caretColor;
        }

        private void OnDestroy()
        {
            if(NonNativeKeyboard.Instance != null)
                NonNativeKeyboard.Instance.OnClosed -= UnlinkInputField;
        }
    }
}
