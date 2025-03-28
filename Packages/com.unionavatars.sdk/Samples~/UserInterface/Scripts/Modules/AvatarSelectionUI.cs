using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnionAvatars.API;
using UnionAvatars.Log;
using UnityEngine;
using UnityEngine.UI;

namespace UnionAvatars.UI
{
    public class AvatarSelectionUI : UIModule
    {
        [Header("Components")]
        [SerializeField]
        private Transform avatarGrid;

        [SerializeField]
        private UIModule avatarCreationModule;

        [SerializeField]
        private AvatarViewUI avatarView;

        [SerializeField]
        private Button loadButton;

        [Header("Avatar Slot")]
        [SerializeField]
        private AvatarSlotUI avatarSlotPrefab;
        private List<AvatarSlotUI> avatarSlots = new List<AvatarSlotUI>();

        [SerializeField]
        private TwoOptionDialog optionDialog;

        [Header("Load More")]
        [SerializeField]
        private Button loadMoreButton;

        [SerializeField]
        private GameObject loadingSpinner;

        [SerializeField]
        private TextMeshProUGUI loadMoreText;
        private AvatarMetadata selectedAvatar;
        private int avatarCurrentPage = 1;

        const int chunkSize = 8;
        const string deleteDialogText = "Are you sure you want to delete this avatar?<br> ";
        const string editDialogText = "Do you want to edit this avatar?<br> ";

        private async void Start()
        {
            LoadChunk();

            //Automatically load last avatar
            var lastAvatar = await uiManager.session.GetAvatars(1, 1);

            if (cancellationToken.IsCancellationRequested)
                return;

            //If the function above retrieves null, something went wrong reaching the API
            if (lastAvatar == null)
            {
                uiManager.session.LogHandler.APIWarning("Error while reaching API, please try again later");
                (root as BaseModule).GoBack(false);
                return;
            }

            if (lastAvatar.Total > 0)
            {
                selectedAvatar = lastAvatar.Items[0];
                await avatarView.LoadAvatarView(selectedAvatar);
                if (cancellationToken.IsCancellationRequested)
                    return;
                loadButton.interactable = true;
            }
        }

        public async void LoadChunk()
        {
            loadMoreText.text = "Loading Avatars...";
            loadingSpinner.SetActive(true);
            loadMoreButton.interactable = false;

            Paginated<AvatarMetadata> avatars = await uiManager.session.GetAvatars(10, avatarCurrentPage);

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
            if (avatarCurrentPage * 10 >= avatars.Total)
            {
                uiManager
                    .session
                    .LogHandler
                    .CustomLog("No more avatars", "All the avatars have been loaded", AvatarSDKLogType.Info);
                loadMoreText.text = "Load More...";
                loadingSpinner.SetActive(false);
                loadMoreButton.interactable = true;
            }
            if (avatars.Total <= 0)
            {
                uiManager
                    .session
                    .LogHandler
                    .CustomLog("No avatars", "You don't have any avatar yet, create one!", AvatarSDKLogType.Info);
                StartAvatarCreation();
                return;
            }

            avatarCurrentPage++;

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
                avatarSlot.SetupAvatarSlot(avatar);

                if (avatar.ThumbnailUrl != null)
                {
                    avatarImageTasks.Add(DownloadItemThumbnail(avatar, avatarSlot));
                }

                avatarSlot
                    .selectButton
                    .onClick
                    .AddListener(async () =>
                    {
                        loadButton.interactable = false;
                        await avatarView.LoadAvatarView(avatar);
                        if (cancellationToken.IsCancellationRequested)
                            return;
                        loadButton.interactable = true;
                        selectedAvatar = avatar;
                    });

                //Setup an event action when the delete button gets pressed
                avatarSlot
                    .deleteButton
                    .onClick
                    .AddListener(() =>
                    {
                        avatarSlot.deleteButton.interactable = false;
                        var newDialog = Instantiate(optionDialog, transform);
                        newDialog.SetupDialog(
                            deleteDialogText + $"<b>{avatar.Name}</b>",
                            () =>
                            {
                                newDialog.Close();
                                avatarSlot.deleteButton.interactable = true;
                            },
                            () =>
                            {
                                newDialog.Close();
                                avatarSlots.Remove(avatarSlot);
                                DeleteAvatar(avatar, avatarSlot);

                                // If the avatar is selected and deleted
                                if (avatarSlots.Count > 0 && selectedAvatar.Id == avatar.Id) // Select most recent avatar
                                {
                                    avatarSlots[0].selectButton.onClick.Invoke();
                                }
                                else if (avatarSlots.Count <= 0) // If no more avatars, disable load button and clear view
                                {
                                    avatarView.ClearPreviousAvatar();
                                    loadButton.interactable = false;
                                    selectedAvatar = null;
                                }
                            }
                        );
                    });

                avatarSlot
                    .editButton
                    .onClick
                    .AddListener(() =>
                    {
                        avatarSlot.editButton.interactable = false;
                        var newDialog = Instantiate(optionDialog, transform);
                        newDialog.SetupDialog(
                            editDialogText + $"<b>{avatar.Name}</b>",
                            () =>
                            {
                                newDialog.Close();
                                avatarSlot.editButton.interactable = true;
                            },
                            () =>
                            {
                                var updateModule = SwapModule(avatarCreationModule);
                                (updateModule as CreationBaseUI).Initialize(avatar);
                            }
                        );
                    });
            }

            await Task.WhenAll(avatarImageTasks);

            if (cancellationToken.IsCancellationRequested)
                return;

            loadMoreText.text = "Load More...";
            loadingSpinner.SetActive(false);
            loadMoreButton.interactable = true;
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

        private void DeleteAvatar(AvatarMetadata avatar, AvatarSlotUI avatarSlot)
        {
            _ = uiManager.session.DeleteAvatar(avatar.Id);
            uiManager
                .session
                .LogHandler
                .CustomLog("Avatar deleted", $"The avatar {avatar.Name} has been deleted", AvatarSDKLogType.Info);
            Destroy(avatarSlot.gameObject);
        }

        public void SpawnAvatar()
        {
            CloseRecursive(root);
            uiManager.ReturnAvatar(selectedAvatar);
        }

        public void StartAvatarCreation()
        {
            var creationModule = SwapModule(avatarCreationModule);
            (creationModule as CreationBaseUI).Initialize();
        }

        protected override void GoBack()
        {
            base.GoBack();
            //Logout from Union Avatars
            uiManager.session.SessionContext.Unauthenticate();
        }
    }
}
