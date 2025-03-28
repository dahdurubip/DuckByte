using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UnionAvatars.UI
{ 
    public class BaseModule : UIModule
    {
        [SerializeField] private TwoOptionDialog closeDialog;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI stateText;
        public ClosingMode closingMode = ClosingMode.Immediate;
        public Action OnBack;

        public void Close()
        {
            switch(closingMode)
            {
                case ClosingMode.Immediate:
                    CloseRecursive(this);
                    uiManager.CloseUI();
                    break;
                case ClosingMode.Warn:
                    closeButton.interactable = false;
                    bool backButtonState = backButton.interactable;
                    backButton.interactable = false;
                    TwoOptionDialog newDialog = Instantiate(closeDialog, transform);
                    newDialog.SetupDialog("Are you sure you want to close?",
                                          () => {newDialog.Close(); closeButton.interactable = true; backButton.interactable = backButtonState;},
                                          () => { CloseRecursive(this); uiManager.CloseUI(); });
                    break;
            }
        }

        public void ToggleBackButton(bool enable)
        {
            backButton.interactable = enable;
        }

        /// <summary>
        /// Makes the current sub-module to swap with it's previous module
        /// </summary>
        /// <param name="showDialog">
        /// If true, a prompt will appear asking user for confirmation
        /// </param>
        public void GoBack(bool showDialog = true)
        {
            if(showDialog)
            {
                closeButton.interactable = false;
                bool backButtonState = backButton.interactable;
                backButton.interactable = false;
                TwoOptionDialog newDialog = Instantiate(closeDialog, transform);
                newDialog.SetupDialog("Are you sure you want to go back?",
                                      () => {newDialog.Close(); closeButton.interactable = true; backButton.interactable = backButtonState;},
                                      () => { OnBack.Invoke(); newDialog.Close(); closeButton.interactable = true;});
            }
            else
                OnBack.Invoke();
        }

        public void SetStateText(string state)
        {
            stateText.text = state;
        }
    }

    public enum ClosingMode
    {
        Immediate,
        Warn
    }
}