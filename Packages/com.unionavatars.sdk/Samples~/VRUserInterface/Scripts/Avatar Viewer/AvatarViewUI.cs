using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;
using UnionAvatars.API;
using UnionAvatars.Avatars;
using UnionAvatars.Utils;
using UnityEngine;

namespace UnionAvatars.VRUI
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
                    case (AvatarViewType.Outfit):
                        garmentCache = new KeyValuePair<Garment, GameObject>[4];
                        break;
                    case (AvatarViewType.Garments):
                        break;
                }
            }
        }
        private Head headCache;
        private byte[] headModelCache;
        private readonly Dictionary<string, string> MaleBody = new Dictionary<string, string>()
        {
            {
                "UnionAvatars_Arms_top",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/male/v4/UnionAvatars_male_Arms_top.glb"
            },
            {
                "UnionAvatars_Arms_bottom",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/male/v4/UnionAvatars_male_Arms_bottom.glb"
            },
            {
                "UnionAvatars_Feet",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/male/v4/UnionAvatars_male_Feet.glb"
            },
            {
                "UnionAvatars_Hands",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/male/v4/UnionAvatars_male_Hands.glb"
            },
            {
                "UnionAvatars_Legs_bottom",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/male/v4/UnionAvatars_male_Legs_bottom.glb"
            },
            {
                "UnionAvatars_Legs_top",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/male/v4/UnionAvatars_male_Legs_top.glb"
            },
            {
                "UnionAvatars_Neck",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/male/v4/UnionAvatars_male_Neck.glb"
            },
            {
                "UnionAvatars_Hips",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/male/v4/UnionAvatars_male_Hips.glb"
            },
            {
                "UnionAvatars_Chest",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/male/v4/UnionAvatars_male_Chest.glb"
            },
            {
                "UnionAvatars_Belly",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/male/v4/UnionAvatars_male_Belly.glb"
            }
        };
        private readonly Dictionary<string, string> FemaleBody = new Dictionary<string, string>()
        {
            {
                "UnionAvatars_Arms_top",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/female/v4/UnionAvatars_female_Arms_top.glb"
            },
            {
                "UnionAvatars_Arms_bottom",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/female/v4/UnionAvatars_female_Arms_bottom.glb"
            },
            {
                "UnionAvatars_Feet",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/female/v4/UnionAvatars_female_Feet.glb"
            },
            {
                "UnionAvatars_Hands",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/female/v4/UnionAvatars_female_Hands.glb"
            },
            {
                "UnionAvatars_Legs_bottom",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/female/v4/UnionAvatars_female_Legs_bottom.glb"
            },
            {
                "UnionAvatars_Legs_top",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/female/v4/UnionAvatars_female_Legs_top.glb"
            },
            {
                "UnionAvatars_Neck",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/female/v4/UnionAvatars_female_Neck.glb"
            },
            {
                "UnionAvatars_Hips",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/female/v4/UnionAvatars_female_Hips.glb"
            },
            {
                "UnionAvatars_Chest",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/female/v4/UnionAvatars_female_Chest.glb"
            },
            {
                "UnionAvatars_Belly",
                "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/female/v4/UnionAvatars_female_Belly.glb"
            }
        };
        private Dictionary<string, GameObject> bodyCache = new Dictionary<string, GameObject>();
        private KeyValuePair<Garment, GameObject>[] garmentCache = new KeyValuePair<Garment, GameObject>[4];
        private GameObject outfitCache = null;
        private GameObject hairCache = null;
        public string selectedGender = "male";

        private void Awake()
        {
            AotHelper.EnsureList<BoneWeight1>();
        }

        public async Task SetHeadCache(Head head)
        {
            headCache = head;
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
                var downloadedOutfit = await ResourceDownloader.Download(outfit.Url);

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
                    if (bodyDisplay.Value == false)
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
                    var downloadedGarment = await ResourceDownloader.Download(garments[i].Url);

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
                        if (bodyDisplay.Value == false)
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

            foreach (var item in selectedGender == "female" ? FemaleBody : MaleBody)
            {
                tasks.Add(BuildBodyPart(item.Key, item.Value));
            }

            await Task.WhenAll(tasks);

            await BuildBodyPart("head", headModelCache);
        }

        private async Task BuildBodyPart(string partName, string url)
        {
            byte[] modelBytes = await ResourceDownloader.Download(
                new Uri(url),
                ResourceType.Garment,
                cancellationToken.Token
            );
            if (cancellationToken.IsCancellationRequested)
                return;
            await BuildBodyPart(partName, modelBytes);
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
            newBodyPart.transform.localPosition =
                partName == "head" ? new Vector3(0, 1.70537f, -0.039149f) : Vector3.zero;
            newBodyPart.transform.localRotation = Quaternion.identity;
            newBodyPart.transform.localScale =
                partName == "head" ? new Vector3(1.02266f, 1.02266f, 1.02266f) : Vector3.one;

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
                newHair.transform.localPosition = new Vector3(0, 1.70537f, -0.039149f);
                newHair.transform.localScale = new Vector3(1.06f, 1.06f, 1.06f);
                newHair.transform.localRotation = Quaternion.identity;

                // Setup layer
                newHair.SetLayer<Renderer>(LayerMask.NameToLayer("Avatar"), true);

                hairCache = newHair;
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

        private void OnDestroy()
        {
            cancellationToken.Cancel();
            Resources.UnloadUnusedAssets();
        }
    }
}
