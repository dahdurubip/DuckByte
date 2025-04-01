using System;
using System.IO;
using System.Threading.Tasks;
using UnionAvatars.API;
using UnionAvatars.Editor.Utils;
using UnionAvatars.Settings;
using UnityEditor;
using UnityEditor.VSAttribution.UnionAvatars;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnionAvatars.Editor
{
    [InitializeOnLoad]
    public class SetupScreen : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_UXMLRoot;

        [SerializeField]
        private VisualTreeAsset m_UXMLLogin;

        [SerializeField]
        private VisualTreeAsset m_UXMLSettings;

        [SerializeField]
        private VisualTreeAsset m_UXMLSetup;
        private static UnionAvatarsSDK_Settings sdkSettings;
        const string settingsPath = "Assets/UnionAvatars/Resources/UnionAvatars/";

        static SetupScreen()
        {
            EditorApplication.update -= LoadSetupScreen;
            EditorApplication.update += LoadSetupScreen;
        }

        private static void LoadSetupScreen()
        {
            EditorApplication.update -= LoadSetupScreen;

            if (EditorApplication.isPlaying)
                return;

            LoadSettings();

            if (sdkSettings.firstTimeLoading)
            {
                sdkSettings.firstTimeLoading = false;
                EditorUtility.SetDirty(sdkSettings);
                ShowWindow();
            }
        }

        private static void LoadSettings()
        {
            sdkSettings = Resources.Load<UnionAvatarsSDK_Settings>("UnionAvatars/UnionAvatarsSDK_Settings");

            if (sdkSettings == null)
            {
                sdkSettings = ScriptableObject.CreateInstance<UnionAvatarsSDK_Settings>();

                if (!AssetDatabase.IsValidFolder(settingsPath))
                    Directory.CreateDirectory(settingsPath);

                AssetDatabase.CreateAsset(sdkSettings, settingsPath + "UnionAvatarsSDK_Settings.asset");
                AssetDatabase.SaveAssets();
            }

            //Check and update version in settings
            string packageVersion = PackageUtilities.GetPackageVersion("com.unionavatars.sdk");
            if (sdkSettings.version != packageVersion)
            {
                sdkSettings.version = packageVersion;
                EditorUtility.SetDirty(sdkSettings);
            }
        }

        [MenuItem("Tools/Union Avatars/Project Setup")]
        public static void ShowWindow()
        {
            var window = GetWindow<SetupScreen>();
            window.titleContent = new GUIContent("Union Avatars");
            window.minSize = new Vector2(455, 618);
        }

        private void CreateGUI()
        {
            m_UXMLRoot.CloneTree(rootVisualElement);

            if (sdkSettings == null)
                LoadSettings();

            LoadPage(Page.Login);
            rootVisualElement.Q<VisualElement>("navbar").style.display = DisplayStyle.Flex;

            //Version
            rootVisualElement.Q<Label>("Version").text = $"v{sdkSettings.version}";

            // Social Media Buttons
            rootVisualElement
                .Q<Button>("discord")
                .RegisterCallback<ClickEvent>((evt) => Application.OpenURL("https://discord.gg/aREQw36NgX"));
            rootVisualElement
                .Q<Button>("twitter")
                .RegisterCallback<ClickEvent>((evt) => Application.OpenURL("https://twitter.com/UnionAvatars"));
            rootVisualElement
                .Q<Button>("website")
                .RegisterCallback<ClickEvent>((evt) => Application.OpenURL("https://unionavatars.com/"));

            // Privacy & Cookies
            rootVisualElement
                .Q<Button>("privacy-policy")
                .RegisterCallback<ClickEvent>(
                    (evt) => Application.OpenURL("https://app.unionavatars.com/privacy-policy")
                );
            rootVisualElement
                .Q<Button>("cookie-policy")
                .RegisterCallback<ClickEvent>(
                    (evt) => Application.OpenURL("https://app.unionavatars.com/cookies-policy")
                );

            // Navigation
            rootVisualElement.Q<Button>("nav-login").RegisterCallback<ClickEvent>((evt) => LoadPage(Page.Login));
            rootVisualElement.Q<Button>("nav-settings").RegisterCallback<ClickEvent>((evt) => LoadPage(Page.Settings));
            rootVisualElement.Q<Button>("nav-setup").RegisterCallback<ClickEvent>((evt) => LoadPage(Page.Setup));
            rootVisualElement
                .Q<Button>("nav-docs")
                .RegisterCallback<ClickEvent>(
                    (evt) =>
                        Application.OpenURL("https://docs.unionavatars.com/docs/Integration/Unity_SDK/get_started/")
                );
        }

        private void LoadPage(Page page)
        {
            rootVisualElement.Q<VisualElement>("Container").Clear();

            rootVisualElement
                .Query<Button>(className: "nav-button")
                .ForEach((button) => button.RemoveFromClassList("nav-button-selected"));

            switch (page)
            {
                case Page.Login:
                    rootVisualElement.Q<Button>("nav-login").AddToClassList("nav-button-selected");
                    CreateLoginUI();
                    break;
                case Page.Settings:
                    rootVisualElement.Q<Button>("nav-settings").AddToClassList("nav-button-selected");
                    CreateSettingsUI();
                    break;
                case Page.Setup:
                    rootVisualElement.Q<Button>("nav-setup").AddToClassList("nav-button-selected");
                    CreateSetupUI();
                    break;
            }
        }

        private async void CreateLoginUI()
        {
            VisualElement container = rootVisualElement.Query<VisualElement>("Container");
            container.Add(m_UXMLLogin.Instantiate());

            rootVisualElement
                .Q<Button>("login-button")
                .RegisterCallback<ClickEvent>(
                    async (evt) =>
                    {
                        await GenerateToken();
                    }
                );

            rootVisualElement
                .Q<Button>("logout-button")
                .RegisterCallback<ClickEvent>(
                    (evt) =>
                    {
                        EditorPrefs.DeleteKey("uniondev_token");
                        EditorPrefs.DeleteKey("uniondev_organization");
                        container.Q<VisualElement>("LogIn").style.display = DisplayStyle.Flex;
                        container.Q<VisualElement>("LoggedIn").style.display = DisplayStyle.None;
                    }
                );
            rootVisualElement
                .Q<Button>("generate-key-button")
                .RegisterCallback<ClickEvent>(
                    async (evt) =>
                    {
                        container.Q<Button>("generate-key-button").style.display = DisplayStyle.None;
                        container.Q<Label>("api-key").style.display = DisplayStyle.Flex;
                        container.Q<Label>("api-key").text = "Generating API key...";
                        string key = await APIKeyGenerator.GenerateAPIKey();
                        if (key != null)
                        {
                            container.Q<Label>("api-key").text =
                                $"API Key generated:\n<size=12>Copy it from the output log (and save it somewhere!)</size>\n\n<size=10>{key.Split(':')[0]}\nExpires: {key.Split(':')[1]}</size>";
                            Debug.Log(
                                $"Your UnionAvatars API key:<b> {key.Split(':')[0]}</b>\nExpires: {key.Split(':')[1]}"
                            );
                        }
                        else
                        {
                            container.Q<Button>("generate-key-button").style.display = DisplayStyle.Flex;
                            container.Q<Label>("api-key").style.display = DisplayStyle.None;
                        }
                        container.Q<Button>("generate-key-button").style.display = DisplayStyle.Flex;
                    }
                );

            rootVisualElement
                .Q("GetCredentials")
                .RegisterCallback<ClickEvent>(
                    _ => Application.OpenURL("https://unionavatars.com/select-your-subscription/")
                );

            if (
                EditorPrefs.GetString("uniondev_token") != ""
                && EditorPrefs.GetString("uniondev_organization") != ""
                && await ValidateToken()
            )
                container.Q<VisualElement>("LoggedIn").style.display = DisplayStyle.Flex;
            else
                container.Q<VisualElement>("LogIn").style.display = DisplayStyle.Flex;
        }

        private async Task<bool> ValidateToken()
        {
            ServerSession session = new ServerSession(EditorPrefs.GetString("uniondev_organization"));
            AuthToken auth = new AuthToken
            {
                TokenType = "Bearer",
                AccessToken = EditorPrefs.GetString("uniondev_token")
            };
            session.SessionContext.Authenticate(auth);

            User user = await session.GetCurrentUser();

            if (user == null)
            {
                EditorPrefs.DeleteKey("uniondev_token");
                EditorPrefs.DeleteKey("uniondev_organization");
                return false;
            }
            else
            {
                rootVisualElement.Q<Label>("Logged").text = $"Active user:\n {user.Email}";
                return true;
            }
        }

        private async Task GenerateToken()
        {
            string username = rootVisualElement.Q<TextField>("username-field").value;
            string password = rootVisualElement.Q<TextField>("password-field").value;
            string organization = rootVisualElement.Q<TextField>("organization-field").value;

            if (organization == "")
            {
                Debug.LogError("Organization ID cannot be empty");
                return;
            }

            ServerSession session = new ServerSession(organization);

            bool logged = await session.Login(username, password);

            if (!logged)
                return;

            User user = await session.GetCurrentUser();

            if (user == null)
            {
                rootVisualElement.Q<VisualElement>("LogIn").style.display = DisplayStyle.Flex;
                rootVisualElement.Q<VisualElement>("LoggedIn").style.display = DisplayStyle.None;
            }
            else
            {
                rootVisualElement.Q<VisualElement>("LogIn").style.display = DisplayStyle.None;
                rootVisualElement.Q<VisualElement>("LoggedIn").style.display = DisplayStyle.Flex;
                EditorPrefs.SetString("uniondev_token", session.SessionContext.UserToken.AccessToken);
                EditorPrefs.SetString("uniondev_organization", organization);
                rootVisualElement.Q<Label>("Logged").text = $"Active user:\n {user.Email}";
                Debug.Log($"Logged in successfully as {user.Email}");

                // Unity VS Attribution
                VSAttribution.SendAttributionEvent("login", "UnionAvatars", user.Id.ToString());
            }
        }

        private void CreateSettingsUI()
        {
            VisualElement container = rootVisualElement.Query<VisualElement>("Container");
            container.Add(m_UXMLSettings.Instantiate());

            Toggle cacheToggle = container.Q<Toggle>("setting-cache");
            Toggle optToggle = container.Q<Toggle>("setting-optimization");
            Toggle lodToggle = container.Q<Toggle>("setting-lod");
            Toggle analyticsToggle = container.Q<Toggle>("setting-analytics");
            SliderInt maxLodSlider = container.Q<SliderInt>("setting-maxlod");

            // Initial values
            cacheToggle.value = sdkSettings.useCache;
            optToggle.value = sdkSettings.enableAvatarOptimization;
            lodToggle.value = sdkSettings.enableLOD;
            maxLodSlider.value = sdkSettings.maxLOD;

            RegisterToggleCallback(cacheToggle, newValue => sdkSettings.useCache = newValue);
            RegisterToggleCallback(optToggle, newValue => sdkSettings.enableAvatarOptimization = newValue);
            RegisterToggleCallback(lodToggle, newValue => sdkSettings.enableLOD = newValue);

            // Styles
            Toggle styleRealisticToggle = container.Q<Toggle>("style-realistic");
            Toggle styleCartoonToggle = container.Q<Toggle>("style-cartoon");

            // Initial values
            void SetStyleToggleValues()
            {
                styleRealisticToggle.value = (sdkSettings.enabledStyles & Style.phr) == Style.phr;
                styleCartoonToggle.value = (sdkSettings.enabledStyles & Style.crt) == Style.crt;
            }

            SetStyleToggleValues();

            RegisterToggleCallback(
                styleRealisticToggle,
                (newValue) =>
                {
                    if (newValue)
                        sdkSettings.EnableStyle(Style.phr);
                    else
                        sdkSettings.DisableStyle(Style.phr);

                    SetStyleToggleValues();
                }
            );
            RegisterToggleCallback(
                styleCartoonToggle,
                (newValue) =>
                {
                    if (newValue)
                        sdkSettings.EnableStyle(Style.crt);
                    else
                        sdkSettings.DisableStyle(Style.crt);

                    SetStyleToggleValues();
                }
            );

            maxLodSlider.RegisterValueChangedCallback(
                (evt) =>
                {
                    sdkSettings.maxLOD = evt.newValue;
                    EditorUtility.SetDirty(sdkSettings);
                }
            );
        }

        private void CreateSetupUI()
        {
            VisualElement container = rootVisualElement.Query<VisualElement>("Container");
            container.Add(m_UXMLSetup.Instantiate());

            Label layerStatus = container.Q<VisualElement>("Layer").Q<Label>("Done");

            // Check layers
            var avatarLayer = LayerMask.NameToLayer("Avatar");
            if (avatarLayer > -1)
            {
                layerStatus.text = "Done";
                layerStatus.style.color = new Color(0.341f, 0.914f, 0.219f);
            }
        }

        void RegisterToggleCallback(Toggle toggle, Action<bool> setSettingValue)
        {
            toggle.RegisterValueChangedCallback(
                (evt) =>
                {
                    setSettingValue(evt.newValue);
                    EditorUtility.SetDirty(sdkSettings);
                }
            );
        }

        private enum Page
        {
            Login,
            Settings,
            Setup
        };
    }
}
