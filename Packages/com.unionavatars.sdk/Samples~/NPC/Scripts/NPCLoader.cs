using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnionAvatars.API;
using UnionAvatars.Avatars;
using System.Threading;

namespace UnionAvatars.Samples
{
    public class NPCLoader : MonoBehaviour
    {
        [Header("API")]
        public string Username = "YOUR USERNAME HERE";
        public string Password = "YOUR PASSWORD HERE";
        public string Organization = "YOUR ORGANIZATION ID HERE";

        [Header("NPCs")]
        [Range(1, 30)]
        public int npcCount = 10;

        [Range(5, 100)]
        public float spawnRadius = 10;

        public RuntimeAnimatorController npcAnimator;
        private ServerSession session;
        private CancellationTokenSource ct = new CancellationTokenSource();

        async void Start()
        {
            //Initialize a Union Avatars session
            //We will use this object as our main interface to perform operations
            //Ex: Login, Downloading Avatars,...
            session = new ServerSession(
                logToUnity: true,
                ct: ct.Token,
                organization: Organization
            );

            //First we need to login
            //The ServerSession object we created will take care of keeping a record of our token access for future operations
            var logged = await session.Login(Username, Password);

            //In case the login fails
            if (!logged)
                return;

            //We retrieve the last 4 avatars of the user
            var avatars = await session.GetAvatars(4, 1);

            if(avatars.Total <= 0)
            {
                Debug.LogWarning("You don't have any avatars in this account");
                return;
            }
            
            List<Task> npcSpawnTasks = new List<Task>();
            
            for (int i = 0; i < npcCount; i++)
            {
                int randomAvatarIndex = Random.Range(0, avatars.Items.Length);
                AvatarMetadata avatar = avatars.Items[randomAvatarIndex];
                npcSpawnTasks.Add(SpawnNPC(avatar));
            }

            session.LogHandler.Info("Fetching and spawning avatars...");

            await Task.WhenAll(npcSpawnTasks);

            session.LogHandler.Info("Successfully loaded all the NPCs");
        }

        private async Task SpawnNPC(AvatarMetadata avatar)
        {
            try
            {
                GameObject npcGameObject = await AvatarImporter.ImportAvatarAsHumanoid(
                    avatar,
                    npcAnimator,
                    ct.Token
                );
                npcGameObject.transform.position = GetRandomPosition();
                npcGameObject.transform.rotation = Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
                npcGameObject.AddComponent<NPCAvatar>();
            }
            catch (System.Exception)
            {
                session.LogHandler.AvatarWarning("Avatar import failed");
            }
            
        }

        private Vector3 GetRandomPosition()
        {
            Vector2 randomPosition = Random.insideUnitCircle * spawnRadius;
            return new Vector3(randomPosition.x, 0, randomPosition.y);
        }

        private void OnDestroy()
        {
            ct.Cancel();
        }
    }
}
