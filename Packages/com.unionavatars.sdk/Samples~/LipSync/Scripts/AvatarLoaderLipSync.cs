using System.Threading;
using UnionAvatars.API;
using UnionAvatars.Avatars;
using UnityEngine;

namespace UnionAvatars.Samples.LipSync
{
    public class AvatarLoaderLipSync : MonoBehaviour
    {
        public string Username = "YOUR USERNAME HERE";
        public string Password = "YOUR PASSWORD HERE";
        public string Organization = "YOUR ORGANIZATION ID HERE";
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private void Start()
        {
            BuildAvatarFromAPI();
        }

        public async void BuildAvatarFromAPI()
        {
            //Initialize a Union Avatars session
            //We will use this object as our main interface to perform operations
            //Ex: Login, Downloading Avatars,...
            ServerSession session = new ServerSession(
                logToUnity: true,
                ct: cancellationTokenSource.Token,
                organization: Organization
            );

            //First we need to login
            //The ServerSession object we created will take care of keeping a record of our token access for future operations
            var logged = await session.Login(Username, Password);

            //In case the login fails
            if (!logged)
                return;

            //We retrieve the last 10 avatars of the user
            var avatars = await session.GetAvatars(10, 1);

            if(avatars.Total <= 0)
            {
                Debug.LogWarning("You don't have any avatars in this account");
                return;
            }

            //Now we import the first avatar of the list
            try
            {
                session.LogHandler.Info("Importing avatar...");
                GameObject newAvatar = await AvatarImporter.ImportAvatarAsHumanoid(
                    avatars.Items[0],
                    null,
                    cancellationTokenSource.Token
                );

                session.LogHandler.Info("Avatar Loaded!");

                SetupLipSyncComponents(newAvatar);
            }
            catch (System.Exception)
            {
                session.LogHandler.AvatarWarning("Avatar import failed");
            }
        }

        private void SetupLipSyncComponents(GameObject avatarObject)
        {
            if (avatarObject == null)
                throw new System.ArgumentNullException("avatarObject");

#if !UNITY_WEBGL || UNITY_EDITOR
            AvatarLipSync.AddMicLipSync(avatarObject);
#else
            session.LogHandler.InfoWarning("WebGL build doesn't have support for Microphone Lip Sync", avatarObject);
#endif
        }

        private void OnDestroy()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}
