using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UnionAvatars.UI
{
    public class LoginUI : UIModule
    {
        [SerializeField] private GameObject userInput;
        [SerializeField] private GameObject loadingUI;
        [SerializeField] private TMP_InputField usernameField;
        [SerializeField] private TMP_InputField passwordField;
        [SerializeField] private Button submitButton;
        [SerializeField] private UIModule nextModule;

        private void Start()
        {  
            if(uiManager.session.SessionContext.UserToken != null)
            {
                SwapModule(nextModule);
                return;
            }

            InputNavigator inputNavigator = GetComponent<InputNavigator>();
            usernameField.onSubmit.AddListener(inputNavigator.SelectNextElement);
            passwordField.onSubmit.AddListener((s)=>Login());
        }

        public async void Login()
        {
            userInput.SetActive(false);
            loadingUI.SetActive(true);
            submitButton.interactable = false;

            bool logged = await uiManager.session.Login(usernameField.text, passwordField.text);

            if (cancellationToken.IsCancellationRequested)
                return;

            if(logged)
            {
                SwapModule(nextModule);
            }
            else
            {
                userInput.SetActive(true);
                loadingUI.SetActive(false);
                submitButton.interactable = true;
                return;
            }
        }
    }
}