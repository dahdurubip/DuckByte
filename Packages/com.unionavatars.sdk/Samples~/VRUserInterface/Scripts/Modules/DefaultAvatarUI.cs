using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnionAvatars.API;
using UnionAvatars.Log;
using UnityEngine;

namespace UnionAvatars.VRUI
{
    public class DefaultAvatarUI : UIModule
    {
        [Header("Components")]
        [SerializeField]
        private Transform avatarGrid;

        [Header("Avatar Slot")]
        [SerializeField]
        private AvatarSlotUI avatarSlotPrefab;
        private List<AvatarSlotUI> avatarSlots = new List<AvatarSlotUI>();

        [SerializeField]
        private GameObject loadingSpinner;

        [SerializeField]
        private AvatarMetadata selectedAvatar;

        private void Start()
        {
            LoadChunk();
        }

        public async void LoadChunk()
        {
            loadingSpinner.SetActive(true);

            Paginated<AvatarMetadata> avatars = await uiManager.session.GetDefaultAvatars();

            if (cancellationToken.IsCancellationRequested)
                return;

            if (avatars == null)
            {
                uiManager
                    .session
                    .LogHandler
                    .CustomLog(
                        "No avatars",
                        "Error while getting your avatars from the server",
                        AvatarSDKLogType.Error
                    );
                return;
            }

            List<Task> avatarImageTasks = new List<Task>();

            //Avatar slot instantiation
            for (int i = 0; i < avatars.Items.Length; i++)
            {
                AvatarMetadata avatar = avatars.Items[i];
                //Only display GLB avatars
                if (avatar.Output != OutputFormat.GLB)
                    continue;

                var avatarSlot = Instantiate(avatarSlotPrefab, avatarGrid);
                avatarSlots.Add(avatarSlot); // Save slot into a list
                avatarSlot.transform.SetSiblingIndex(avatarGrid.childCount - 2); // Reposition slot in hierarchy (newer first)

                if (avatar.ThumbnailUrl != null)
                {
                    avatarImageTasks.Add(DownloadItemThumbnail(avatar, avatarSlot));
                }
                avatarSlot
                    .selectButton
                    .onClick
                    .AddListener(() =>
                    {
                        selectedAvatar = avatar;
                        SpawnAvatar();
                    });
            }

            await Task.WhenAll(avatarImageTasks);

            if (cancellationToken.IsCancellationRequested)
                return;

            loadingSpinner.SetActive(false);
        }

        private async Task<bool> DownloadItemThumbnail(AvatarMetadata avatar, AvatarSlotUI avatarSlot)
        {
            try
            {
                byte[] thumbnail = await ResourceDownloader.Download(
                    avatar.ThumbnailUrl,
                    ResourceType.Thumbnail,
                    cancellationToken.Token,
                    updateDate: avatar.UpdatedAt,
                    fileId: avatar.Id.ToString(),
                    timeout: 10
                );

                if (cancellationToken.IsCancellationRequested)
                    return false;

                var avatarTex = new Texture2D(2, 2);
                avatarTex.wrapMode = TextureWrapMode.Clamp;
                avatarTex.LoadImage(thumbnail);
                avatarSlot.SetAvatarImage(avatarTex);

                return true;
            }
            catch
            {
                if (avatarSlot != null)
                    Destroy(avatarSlot.gameObject);

                return false;
            }
        }

        public void SpawnAvatar()
        {
            CloseRecursive(root);
            uiManager.ReturnAvatar(selectedAvatar);
        }

        protected override void GoBack()
        {
            base.GoBack();
        }
    }
}
