using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

namespace UnionAvatars.UI
{
    public class TwoOptionDialog : BaseDialog
    {
        [SerializeField]
        private Button yesButton;

        public void SetupDialog(
            string message,
            Action backAction,
            Action submitAction
        )
        {
            SetupDialog(message, backAction);
            yesButton.onClick.AddListener(new UnityAction(submitAction));
        }
    }
}
