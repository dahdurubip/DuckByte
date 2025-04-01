using System.Threading;
using UnionAvatars.API;
using UnionAvatars.Avatars;
using UnityEngine;

namespace UnionAvatars.Samples
{
    public class AvatarLoaderAPI : MonoBehaviour
    {
        public string Username = "YOUR USERNAME HERE";
        public string Password = "YOUR PASSWORD HERE";
        public string Organization = "YOUR ORGANIZATION ID HERE";
        public RuntimeAnimatorController PlayerAnimator;
        public bool AttachCamera;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private CameraFollow cameraFollowComponent;

        private void Start()
        {
            if (AttachCamera)
            {
                if (!Camera.main.gameObject.TryGetComponent<CameraFollow>(out cameraFollowComponent))
                {
                    cameraFollowComponent = Camera.main.gameObject.AddComponent<CameraFollow>();
                }
            }

            BuildAvatarFromAPI();
        }

        private async void BuildAvatarFromAPI()
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
            session.LogHandler.Info("Importing avatar...");
            try
            {
                GameObject newAvatar = await AvatarImporter.ImportAvatarAsHumanoid(
                    avatars.Items[0],
                    PlayerAnimator,
                    cancellationTokenSource.Token
                );
                session.LogHandler.Info("Avatar Loaded!");
                newAvatar.AddComponent<PlayerMovement>();

                if (AttachCamera)
                    cameraFollowComponent.SetupTarget(newAvatar.transform);
            }
            catch (System.Exception)
            {
                session.LogHandler.AvatarWarning("Avatar import failed");
            }
        }

        private void OnDestroy()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}
