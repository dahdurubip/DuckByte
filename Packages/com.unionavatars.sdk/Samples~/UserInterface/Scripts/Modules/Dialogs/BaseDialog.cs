using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

namespace UnionAvatars.UI
{
    public class BaseDialog : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI dialogText;

        [SerializeField]
        private Button cancelButton;

        public virtual void SetupDialog(
            string message,
            Action backAction
        )
        {
            dialogText.text = message;
            cancelButton.onClick.AddListener(new UnityAction(backAction));
        }

        public virtual void Close()
        {
            Destroy(gameObject);
        }
    }
}
