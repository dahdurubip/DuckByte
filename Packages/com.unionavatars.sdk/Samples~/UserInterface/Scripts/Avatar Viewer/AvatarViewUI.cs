using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;
using UnionAvatars.API;
using UnionAvatars.Avatars;
using UnionAvatars.Utils;
using UnityEngine;

namespace UnionAvatars.UI
{
    public class AvatarViewUI : MonoBehaviour
    {
        [SerializeField]
        private Transform avatarParent;

        [SerializeField]
        private GameObject loadingUI;

        [SerializeField]
        private RuntimeAnimatorController defaultAnimator;
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private bool _loading = false;
        private bool loading
        {
            get => _loading;
            set
            {
                _loading = value;
                if (cancellationToken.IsCancellationRequested)
                    return;
                loadingUI?.SetActive(value);
                avatarParent?.gameObject.SetActive(!value);
            }
        }

        public enum AvatarViewType
        {
            Full,
            Outfit,
            Garments
        }

        private AvatarViewType _currentViewType;
        public AvatarViewType CurrentViewType
        {
            get => _currentViewType;
            set
            {
                _currentViewType = value;
                switch (value)
                {
                    case AvatarViewType.Outfit:
                        garmentCache = new KeyValuePair<Garment, GameObject>[4];
                        break;
                    case AvatarViewType.Garments:
                        break;
                }
            }
        }
        private byte[] headModelCache;
        private Dictionary<string, GameObject> bodyCache = new Dictionary<string, GameObject>();
        private KeyValuePair<Garment, GameObject>[] garmentCache = new KeyValuePair<Garment, GameObject>[4];
        private GameObject outfitCache = null;
        private GameObject hairCache = null;

        public Gender selectedGender = Gender.male;
        public Style selectedStyle = Style.phr;
        public int selectedVersion = 1;
        public Color? selectedHairColor = Color.black;

        private void Awake()
        {
            AotHelper.EnsureList<BoneWeight1>();
        }

        public async Task SetHeadCache(Head head)
        {
            headModelCache = await ResourceDownloader.DownloadToMemory(head.Url, cancellationToken.Token);

            if (headModelCache == null)
                throw new ArgumentException("Invalid head data, couldn't load the avatar viewer");
        }

        /// <summary>
        /// Loads an avatar to be displayed in the UI
        /// </summary>
        /// <param name="avatar">Avatar Metadata to be displayed</param>
        public async Task LoadAvatarView(AvatarMetadata avatar)
        {
            if (loading)
                return;

            if (avatar == null)
            {
                Debug.LogError("Null avatar object");
                return;
            }

            CurrentViewType = AvatarViewType.Full;

            loading = true;

            if (cancellationToken.IsCancellationRequested)
                return;

            ClearPreviousAvatar();

            try
            {
                GameObject avatarGO = await AvatarImporter.ImportAvatarAsHumanoid(
                    avatar,
                    defaultAnimator,
                    cancellationToken.Token
                );
                if (cancellationToken.IsCancellationRequested)
                {
                    Destroy(avatarGO);
                    return;
                }

                SetupAvatarTransformLayer(avatarGO);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return;
            }
            finally
            {
                loading = false;
            }
        }

        /// <summary>
        /// Loads an avatar to be displayed in the UI
        /// </summary>
        /// <param name="outfit">Body data to be displayed</param>
        public async Task LoadAvatarView(Outfit outfit)
        {
            if (loading)
                return;

            if (outfit == null)
            {
                Debug.LogError("Null outfit object");
                return;
            }

            loading = true;

            // If we changed from outfit to garment, clean the scene
            if (CurrentViewType != AvatarViewType.Outfit)
            {
                foreach (var garment in garmentCache)
                {
                    Destroy(garment.Value);
                }
                CurrentViewType = AvatarViewType.Outfit;
            }

            if (outfitCache != null)
                Destroy(outfitCache);

            try
            {
                // Download & Import
                var downloadedOutfit = await ResourceDownloader.Download(outfit.Url, ResourceType.Body);

                if (cancellationToken.IsCancellationRequested)
                    return;

                GameObject newOutfit = await AvatarImporter.ImportResource(
                    downloadedOutfit,
                    cancellationToken.Token,
                    outfit.Name
                );

                if (cancellationToken.IsCancellationRequested)
                {
                    Destroy(newOutfit);
                    return;
                }

                // Reposition
                newOutfit.transform.parent = avatarParent;
                newOutfit.transform.localPosition = Vector3.zero;
                newOutfit.transform.localRotation = Quaternion.identity;
                newOutfit.transform.localScale = Vector3.one;

                // Hide body parts
                foreach (var bodyPart in bodyCache)
                {
                    bodyPart.Value.SetActive(true);
                }
                foreach (var bodyDisplay in outfit.Metadata.Body)
                {
                    if (bodyDisplay.Value == false && bodyCache.ContainsKey(bodyDisplay.Key))
                        bodyCache[bodyDisplay.Key].SetActive(false);
                }

                // Assign layer
                avatarParent.gameObject.SetLayer<Renderer>(LayerMask.NameToLayer("Avatar"), true);

                outfitCache = newOutfit;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return;
            }
            finally
            {
                loading = false;
            }
        }

        /// <summary>
        /// Loads an avatar to be displayed in the UI with garments
        /// </summary>
        /// <param name="avatar">Avatar Metadata to be displayed</param>
        /// <param name="garments">Array of garments in order (head, top, bottom, shoes)</param>
        public async Task LoadAvatarViewWithGarments(Garment[] garments)
        {
            if (loading)
                return;

            if (garments == null)
            {
                Debug.LogError("Null garments object");
                return;
            }

            loading = true;

            // If we changed from outfit to garment, clean the scene
            if (CurrentViewType != AvatarViewType.Garments)
            {
                Destroy(outfitCache);
                CurrentViewType = AvatarViewType.Garments;
            }

            try
            {
                for (int i = 0; i < garmentCache.Length; i++)
                {
                    // If the slot is empty, delete the old garment and skip
                    if (garments[i] == null)
                    {
                        Destroy(garmentCache[i].Value);
                        garmentCache[i] = new KeyValuePair<Garment, GameObject>(null, null); // Update cache
                        continue;
                    }

                    if (garmentCache[i].Key == garments[i]) // If garment is the same as the previous, skip
                        continue;

                    // Download & Import
                    var downloadedGarment = await ResourceDownloader.Download(garments[i].Url, ResourceType.Garment);

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    GameObject newGarment = await AvatarImporter.ImportResource(
                        downloadedGarment,
                        cancellationToken.Token,
                        garments[i].Name
                    );

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Destroy(newGarment);
                        return;
                    }

                    // Reposition
                    newGarment.transform.parent = avatarParent;
                    newGarment.transform.localPosition = Vector3.zero;
                    newGarment.transform.localRotation = Quaternion.identity;
                    newGarment.transform.localScale = Vector3.one;

                    Destroy(garmentCache[i].Value); // Destroy the old garment instance

                    garmentCache[i] = new KeyValuePair<Garment, GameObject>(garments[i], newGarment); // Cache
                }

                // Hide body parts
                foreach (var bodyPart in bodyCache)
                {
                    bodyPart.Value.SetActive(true);
                }
                for (int i = 0; i < garments.Length; i++)
                {
                    if (garments[i] == null)
                        continue;
                    Garment garment = garments[i];
                    foreach (var bodyDisplay in garment.Metadata.Body)
                    {
                        if (bodyDisplay.Value == false && bodyCache.ContainsKey(bodyDisplay.Key))
                            bodyCache[bodyDisplay.Key].SetActive(false);
                    }
                }

                // Assign layer
                avatarParent.gameObject.SetLayer<Renderer>(LayerMask.NameToLayer("Avatar"), true);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return;
            }
            finally
            {
                loading = false;
            }
        }

        // Creates a list of tasks and waits for them
        // Each tasks load a different body part from it's URL
        public async Task BuildBaseBodyForGarments()
        {
            List<Task> tasks = new List<Task>();

            foreach (AvatarBodyPart bodyPart in Enum.GetValues(typeof(AvatarBodyPart)))
            {
                tasks.Add(
                    BuildBodyPart(
                        bodyPart.ToString(),
                        Constants.GetAvatarBodyPartURL(selectedGender, selectedStyle, selectedVersion, bodyPart)
                    )
                );
            }

            await Task.WhenAll(tasks);

            await BuildBodyPart("head", headModelCache);
        }

        private async Task BuildBodyPart(string partName, string url)
        {
            try
            {
                byte[] modelBytes = await ResourceDownloader.Download(
                    new Uri(url),
                    ResourceType.Garment,
                    cancellationToken.Token,
                    // Since file name is the same, create a custom one for correct caching
                    fileId: $"{partName}_{selectedGender}_{selectedStyle}_v{selectedVersion}"
                );

                if (cancellationToken.IsCancellationRequested)
                    return;

                await BuildBodyPart(partName, modelBytes);
            }
            catch (Exception)
            {
                // Fail silently
                // Sometimes this might fail for X reason. For example, certain style doesn't have
                // the body part that's being asked. Since it's not a crucial part of the app, it's fine.
                // Debug.LogWarning($"Couldn't donwload {partName}");
            }
        }

        private async Task BuildBodyPart(string partName, byte[] bytes)
        {
            var gltf = new GLTFast.GltfImport();
            await gltf.Load(bytes);

            if (cancellationToken.IsCancellationRequested)
                return;

            GameObject newBodyPart = new GameObject("temp_body");
            await gltf.InstantiateMainSceneAsync(newBodyPart.transform);

            if (cancellationToken.IsCancellationRequested)
            {
                Destroy(newBodyPart);
                return;
            }

            // Reposition
            newBodyPart.transform.parent = avatarParent;
            newBodyPart.transform.localRotation = Quaternion.identity;
            newBodyPart.transform.localPosition = Vector3.zero;
            newBodyPart.transform.localScale = Vector3.one;

            if (partName == "head")
            {
                newBodyPart.transform.localPosition = Constants.headAssemblyPosition[selectedStyle];
                newBodyPart.transform.localScale = Constants.headAssemblyScale[selectedStyle];
            }

            bodyCache.Add(partName, newBodyPart);
        }

        public async Task LoadHair(Hair hair)
        {
            if (loading)
                return;

            if (hair == null)
            {
                Debug.LogWarning("Null hair object");
                return;
            }

            loading = true;

            if (avatarParent.TryFindBFS("UnionAvatars_Hair", out Transform hairCacheTf))
                hairCache = hairCacheTf.gameObject;

            if (hairCache != null)
                Destroy(hairCache);

            try
            {
                // Download & Import
                var downloadedHair = await ResourceDownloader.Download(
                    hair.Url,
                    ResourceType.Garment,
                    cancellationToken: cancellationToken.Token
                );

                if (cancellationToken.IsCancellationRequested)
                    return;

                GameObject newHair = await AvatarImporter.ImportResource(
                    downloadedHair,
                    cancellationToken.Token,
                    "UnionAvatars_Hair"
                );

                if (cancellationToken.IsCancellationRequested)
                {
                    Destroy(newHair);
                    return;
                }

                // Reposition the new hair
                newHair.transform.parent = avatarParent;
                newHair.transform.localPosition = Constants.headAssemblyPosition[selectedStyle];
                newHair.transform.localRotation = Quaternion.identity;
                newHair.transform.localScale = Constants.headAssemblyScale[selectedStyle];

                // Setup layer
                newHair.SetLayer<Renderer>(LayerMask.NameToLayer("Avatar"), true);

                hairCache = newHair;

                // Handle the edge case of the cap, where we don't want any color applied
                if(!hair.Name.Contains("_cap"))
                    ChangeHairColor();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return;
            }
            finally
            {
                loading = false;
            }

            loading = false;
        }

        // Reposition the avatar and assign correct layer
        private void SetupAvatarTransformLayer(GameObject avatarGO)
        {
            avatarGO.transform.parent = avatarParent;
            avatarGO.transform.localPosition = Vector3.zero;
            avatarGO.transform.localRotation = Quaternion.identity;
            avatarGO.transform.localScale = Vector3.one;

            //Add the layer to the gameobject so the UI camera will render it
            avatarGO.SetLayer<Renderer>(LayerMask.NameToLayer("Avatar"), true);
        }

        public void ClearPreviousAvatar()
        {
            if (avatarParent.childCount > 0)
            {
                foreach (Transform child in avatarParent)
                {
                    Destroy(child.gameObject);
                }
                Resources.UnloadUnusedAssets();
            }
        }

        public void ChangeHairColor(Color? color = null)
        {
            if (color != null)
                selectedHairColor = color;

            if (hairCache == null)
                return;

            var hairRenderers = hairCache.GetComponentsInChildren<Renderer>();
            foreach (var renderer in hairRenderers)
            {
                renderer.sharedMaterial.color = selectedHairColor ?? Color.black;
            }
        }

        private void OnDestroy()
        {
            cancellationToken.Cancel();
            Resources.UnloadUnusedAssets();
        }
    }
}
