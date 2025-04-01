using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

namespace UnionAvatars.VRUI
{
    public class InputDialog : BaseDialog
    {
        [SerializeField]
        private TMP_InputField textField;

        [SerializeField]
        private Button submitButton;

        public void SetupDialog(
            string message,
            Action backAction,
            Action<string> submitAction,
            string defaultText = ""
        )
        {
            SetupDialog(message, backAction);
            textField.text = defaultText;
            submitButton.onClick.AddListener(new UnityAction(() => submitAction(textField.text)));
        }
    }
}
