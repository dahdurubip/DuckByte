using System.Threading;
using UnionAvatars.API;
using UnionAvatars.Avatars;
using UnionAvatars.UI;
using UnityEngine;

namespace UnionAvatars.Samples
{
    public class AvatarLoaderUI : MonoBehaviour
    {
        private ServerSession session;
        public string Organization = "YOUR ORGANIZATION ID HERE";
        public GameObject uiPrefab;
        public KeyCode OpenInterfaceKey;
        public bool LoadUIOnStart;
        private bool isUILoaded = false;
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

        private void Start()
        {
            //Initialize a Union Avatars session
            //We will use this object as our main interface to perform operations
            //Ex: Login, Downloading Avatars,...
            session = new ServerSession(
                logToUnity: true,
                ct: cancellationToken.Token,
                organization: Organization
            );
            if(LoadUIOnStart) LoadUI();
        }

        private void LoadUI()
        {
            if (isUILoaded)
                return;

            isUILoaded = true;

            //Create an instance of the UI prefab. The instance contains a AvatarUIManager
            //We can use this manager to setup and interact with the interface easily
            AvatarUIManager unionUI = Instantiate(uiPrefab).GetComponent<AvatarUIManager>();

            //First we initialize/setup the UI with our session
            unionUI.SetupUI(session);

            //The onAvatarSelected event will trigger once an avatar is selected or created
            //It will return its Avatar Metadata
            unionUI.onAvatarSelected += BuildAvatar;

            unionUI.onClose += () => isUILoaded = false;
        }

        private async void BuildAvatar(AvatarMetadata avatar)
        {
            if (avatar == null)
                throw new System.ArgumentNullException("avatar");

            session.LogHandler.Info($"Importing avatar: {avatar.Name} ...");
            try
            {
                GameObject newAvatar = await AvatarImporter.ImportAvatarAsHumanoid(avatar, null, cancellationToken.Token);
                session.LogHandler.Info("Avatar Loaded!");
                SetupAvatarObject(newAvatar);
            }
            catch (System.Exception)
            {
                session.LogHandler.AvatarWarning("Avatar import failed");
            }
        }

        private void SetupAvatarObject(GameObject avatarObject)
        {
            if(avatarObject == null)
                throw new System.ArgumentNullException("avatarObject");

            //Your custom code here...
            //For example, you can add some new components to control the avatar using AddComponent
        }

        private void Update()
        {
            if (Input.GetKeyDown(OpenInterfaceKey))
            {
                LoadUI();
            }
        }

        private void OnDestroy()
        {
            cancellationToken.Cancel();
        }
    }
}
