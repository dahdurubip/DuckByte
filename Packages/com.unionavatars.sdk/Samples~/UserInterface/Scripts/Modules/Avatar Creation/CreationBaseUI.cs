using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnionAvatars.API;
using UnityEngine;

namespace UnionAvatars.UI
{
    public class CreationBaseUI : UIModule
    {
        [Header("Modules")]
        [SerializeField]
        private UIModule firstStepModule; // Gender/Style selection

        [SerializeField]
        private UIModule customizationModule; // Avatar customization

        [Header("Loading UI")]
        [SerializeField]
        private GameObject loadingUI;

        [SerializeField]
        private TextMeshProUGUI loadingText;

        [Header("Prefabs")]
        [SerializeField]
        private BaseDialog paymentDialog;

        // Private variables
        private AvatarMetadata avatarData;
        private bool isUpdate = false;
        private bool isComplete = false;

        public Gender Gender
        {
            get { return avatarData.Gender; }
            set { avatarData.Gender = value; }
        }

        public Style Style
        {
            get { return avatarData.Style; }
            set { avatarData.Style = value; }
        }

        private void Start()
        {
            (root as BaseModule).closingMode = ClosingMode.Warn;
        }

        public async void Initialize(AvatarMetadata avatar = null)
        {
            if (avatar != null)
            {
                isUpdate = true;
                avatarData = avatar;

                // Since we don't know the gender, we have to obtain it from the avatar's head
                Head head = await uiManager.session.GetHead(avatar.HeadId);
                avatarData.Gender = head.Metadata == null ? Gender.all : head.Metadata.Gender;

                SwapChild(customizationModule);
                InitializeAvatarCustomization();
            }
            else
            {
                isUpdate = false;
                avatarData = new AvatarMetadata(Guid.Empty, Guid.Empty, Gender.all, Style.phr);
                SwapChild(firstStepModule);
            }
        }

        public async void CreateAndSendHeadRequest(string base64Photo)
        {
            CloseRecursive(child);

            loadingText.text = "Processing your photo...";
            loadingUI.SetActive(true);

            HeadRequest headRequest = new HeadRequest()
            {
                Name = Guid.NewGuid().ToString(),
                SelfieImg = base64Photo,
                Style = avatarData.Style,
                Version = avatarData.Style == Style.crt ? 1 : 3 // TODO: Add to constants class
            };

            (root as BaseModule).ToggleBackButton(false); // Disable going back while creating the avatar

            Head newHead = await uiManager.session.CreateHead(headRequest); // Create and store the head in a variable

            if (cancellationToken.Token.IsCancellationRequested)
            {
                _ = uiManager.session.DeleteHead(newHead.Id);
                return;
            }

            loadingUI.SetActive(true);

            if (newHead != null) // If the head was created correctly, load customization UI
            {
                avatarData.HeadId = newHead.Id;
                SwapChild(customizationModule);
                InitializeAvatarCustomization();
            }
            else
            {
                SwapChild(firstStepModule); // If the head creation failed, go back
            }

            loadingUI.SetActive(false);
        }

        private void InitializeAvatarCustomization()
        {
            (child as AvatarCustomizationUI).InitializeCreationModule(avatarData);
            (child as AvatarCustomizationUI).OnAvatarFinished += OnAvatarCreated;
        }

        public void OnAvatarCreated(string name, Outfit outfit, Hair hair, Garment[] garments, Color hairColor)
        {
            // If the avatar doesn't have an outfit already and there are no selected assets, return
            if (outfit == null && garments == null && avatarData.OutfitId == Guid.Empty)
            {
                uiManager.session.LogHandler.UIError("Invalid outfit and garments");
                (root as BaseModule).GoBack(false);
                return;
            }

            (root as BaseModule).ToggleBackButton(false); // Disable going back while building the avatar

            // Payable assets
            List<UnionAsset> assetsToPay = new List<UnionAsset>();

            // Check if there are any payable assets selected
            if (garments != null)
                assetsToPay.AddRange(garments.Where(asset => asset?.SourceType == SourceType.payable));

            if (outfit != null && outfit.SourceType == SourceType.payable)
                assetsToPay.Add(outfit);

            if (hair != null && hair.SourceType == SourceType.payable)
                assetsToPay.Add(hair);

            if (assetsToPay.Count > 0)
            {
                // If some assets need to be payed, wait until they are paid, then create/update the avatar
                StartCoroutine(
                    PaymentCoroutine(assetsToPay, () => CreateAndUpdateAvatar(name, outfit, hair, garments, hairColor))
                );
            }
            else
            {
                CreateAndUpdateAvatar(name, outfit, hair, garments, hairColor);
            }
        }

        private async void CreateAndUpdateAvatar(
            string name,
            Outfit outfit,
            Hair hair,
            Garment[] garments,
            Color hairColor
        )
        {
            CloseRecursive(child);

            loadingText.text = "Loading your avatar...";
            loadingUI.SetActive(true);

            // Patch HEAD to update the hair
            if (hair != null)
            {
                await uiManager.session.UpdateHead(avatarData.HeadId, new HeadUpdateRequest(hair.Id, hairColor));
                if (cancellationToken.IsCancellationRequested)
                    return;
            }

            // Assemble body if garments have been selected
            if (garments != null)
            {
                // If theres no outfit, or it is not custom, create a new one
                // Otherwise, update it
                if (outfit == null || outfit.SourceType != SourceType.custom)
                {
                    outfit = await uiManager.session.AssembleOutfit(name, garments);
                }
                else
                    await uiManager.session.UpdateOutfit(outfit.Id, garments);

                if (cancellationToken.IsCancellationRequested)
                    return;

                if (outfit == null)
                {
                    (root as BaseModule).GoBack(false);
                    return;
                }
            }

            // Assign values to the avatar
            avatarData.OutfitId = outfit.Id;
            avatarData.Name = name;

            // Create or update the avatar
            AvatarMetadata newAvatar;

            if (isUpdate)
            {
                newAvatar = await uiManager.session.UpdateAvatar(avatarData);
            }
            else
            {
                AvatarRequest avatarRequest = new AvatarRequest()
                {
                    Name = (name == "") ? Guid.NewGuid().ToString() : avatarData.Name,
                    OutfitId = avatarData.OutfitId,
                    HeadId = avatarData.HeadId,
                    Style = avatarData.Style,
                    CreateThumbnail = true
                };

                newAvatar = await uiManager.session.CreateAvatar(avatarRequest);
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            if (newAvatar == null)
            {
                (root as BaseModule).GoBack(false);
            }
            else
            {
                //Once avatar is created, close UI and invoke event
                isComplete = true;
                uiManager
                    .session
                    .LogHandler
                    .CustomLog(
                        $"Avatar {(isUpdate ? "Updated" : "Created")}",
                        $"Avatar {(isUpdate ? "Updated" : "Created")} Successfully",
                        Log.AvatarSDKLogType.Info
                    );
                CloseRecursive(root);
                uiManager.ReturnAvatar(newAvatar);
            }
        }

        private IEnumerator PaymentCoroutine(List<UnionAsset> assetsToPay, Action successCallback)
        {
            // Check if user already has any of the assets purchased
            Task<PaidAssets> paidAssetsTask = uiManager.session.GetPaidAssets();

            yield return new WaitUntil(() => paidAssetsTask.IsCompleted);

            if (cancellationToken.IsCancellationRequested)
                yield break;

            PaidAssets paidAssets = paidAssetsTask.Result;

            // Remove already paid assets from the list
            assetsToPay.RemoveAll(
                assetToPay => paidAssets.Assets.Any(paidAssetId => paidAssetId == assetToPay.ContainerId)
            );

            if (assetsToPay.Count > 0)
            {
                Task<CheckoutCreate> checkoutTask = uiManager.session.CreateCheckout(assetsToPay.ToArray());

                yield return new WaitUntil(() => checkoutTask.IsCompleted);

                if (cancellationToken.IsCancellationRequested)
                    yield break;


                BaseDialog newDialog = Instantiate(paymentDialog, transform);

                bool waitingForPayment = true;

                newDialog.SetupDialog(
                    "Please complete the checkout, then return to this window",
                    () =>
                    {
                        waitingForPayment = false;
                        newDialog.Close();
                    }
                );

                CheckoutCreate checkout = checkoutTask.Result;
                
                Application.OpenURL(checkout.SessionUrl.ToString());

                while (waitingForPayment)
                {
                    yield return new WaitForSeconds(5); // Check status every 5 seconds

                    if (cancellationToken.IsCancellationRequested || !waitingForPayment)
                        yield break;

                    Task<CheckoutStatus> statusTask = uiManager.session.CheckoutStatus(checkout.CartId);

                    yield return new WaitUntil(() => statusTask.IsCompleted);

                    if (statusTask.Result.completed)
                    {
                        newDialog.Close();
                        successCallback?.Invoke();
                        break;
                    }
                }
            }

            // The payable assets are already owned, so no need to buy them
            successCallback?.Invoke();
        }

        protected override void OnExitModule()
        {
            base.OnExitModule();

            if (!isUpdate && avatarData.HeadId != Guid.Empty && !isComplete)
                _ = uiManager.session.DeleteHead(avatarData.HeadId);

            (root as BaseModule).closingMode = ClosingMode.Immediate;
        }
    }
}
