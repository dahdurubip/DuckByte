using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnionAvatars.API;
using UnionAvatars.Settings;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnionAvatars.Avatars
{
    [Preserve]
    public class AvatarImporter
    {
        private const float CULL_LOD_TRANSITION = 0.1f;

        //Disable async not awaiting warning
#pragma warning disable 1998

        /// <summary>
        /// Imports an avatar file and creates a new GameObject
        /// </summary>
        /// <param name="bytes">
        /// The absolute path of the avatar .glb
        /// </param>
        /// <param name="onFinished">
        /// Callback once the import process is finished. It returns the created GameObject
        /// </param>
        public static async Task<GameObject> ImportResource(
            byte[] bytes,
            CancellationToken ct = default,
            string name = "UnionAvatars_Imported"
        )
        {
            if (bytes == null || bytes.Length == 0)
                throw new ArgumentException("Resource data is null or empty, not valid for importing", "bytes");

#if !USING_GLTFAST
            throw new InvalidOperationException(
                "Missing dependency: GLTFast. Import the required packages in 'Union Avatars/Install Requirements'"
            );
#else
            var gltf = new GLTFast.GltfImport();
            bool success = await gltf.LoadGltfBinary(bytes);
            if (ct.IsCancellationRequested)
                return null;
            if (success)
            {
                GameObject newObject = new GameObject(name, typeof(GLTFast.GltfAsset));
                await gltf.InstantiateMainSceneAsync(newObject.transform);
                ct.ThrowIfCancellationRequested();
                return newObject;
            }
            else
                throw new InvalidOperationException("GLTF Import operation failed");
#endif
        }

        public static async Task<GameObject> ImportAvatarAsHumanoid(
            AvatarMetadata avatar,
            RuntimeAnimatorController animController,
            CancellationToken ct = default
        )
        {
            byte[] downloadedAvatar = await ResourceDownloader.Download(
                avatar.AvatarLink,
                ResourceType.Avatar,
                ct,
                updateDate: avatar.UpdatedAt,
                fileId: avatar.Id.ToString()
            );
            if (ct.IsCancellationRequested)
                return null;
            var avatarObject = await ImportResource(downloadedAvatar, ct, "UnionAvatars_" + avatar.Name);
            if (ct.IsCancellationRequested)
                return null;
            avatarObject.ConvertAvatarToHumanoid(animController, avatar.Version, avatar.Style);
            return avatarObject;
        }

        public static async Task<GameObject> ImportHalfBodyAvatarAsHumanoid(
            AvatarMetadata avatar,
            RuntimeAnimatorController animController,
            CancellationToken ct = default
        )
        {
            var downloadedAvatar = await ResourceDownloader.Download(
                avatar.AvatarLink,
                ResourceType.Avatar,
                ct,
                fileId: avatar.Id.ToString()
            );
            if (ct.IsCancellationRequested)
                return null;
            var avatarObject = await ImportResource(downloadedAvatar, ct);
            if (ct.IsCancellationRequested)
                return null;
            avatarObject.ConvertAvatarToHumanoid(animController, 2, Style.phr_vr);
            return avatarObject;
        }

        public static async Task<GameObject> ImportAvatarAsHumanoidLOD(
            AvatarMetadata avatar,
            RuntimeAnimatorController animController,
            int lodLevel = 2,
            CancellationToken ct = default
        )
        {
            Uri avatarLink;

            if (avatar.Lod == null)
            {
                Debug.LogWarning("Avatar doesn't have any valid LOD");
                avatarLink = avatar.AvatarLink;
            }
            else
            {
                avatarLink = avatar.Lod[lodLevel.ToString()];
            }

            byte[] downloadedAvatar = await ResourceDownloader.Download(
                avatarLink,
                ResourceType.Avatar,
                ct,
                updateDate: avatar.UpdatedAt,
                fileId: avatar.Id.ToString()
            );

            var avatarObject = await ImportResource(downloadedAvatar, ct, "UnionAvatars_" + avatar.Name);
            avatarObject.ConvertAvatarToHumanoid(animController, avatar.Version, avatar.Style);
            return avatarObject;
        }

        public static async Task<GameObject> ImportAvatarAsHumanoidLOD(
            AvatarMetadata avatar,
            RuntimeAnimatorController animController,
            CancellationToken ct = default
        )
        {
            if (avatar.Lod == null)
            {
                Debug.LogWarning("Avatar doesn't have any valid LOD, falling back to regular import");
                return await ImportAvatarAsHumanoid(avatar, animController, ct);
            }

            int maxLOD = SettingsManager.Settings.maxLOD;

            byte[] mainAvatarBytes = await ResourceDownloader.Download(
                avatar.Lod[maxLOD.ToString()],
                ResourceType.Avatar,
                ct,
                updateDate: avatar.UpdatedAt,
                fileId: avatar.Id.ToString()
            );
            var mainAvatar = await ImportResource(mainAvatarBytes, ct, "UnionAvatars_" + avatar.Name);
            mainAvatar.ConvertAvatarToHumanoid(animController, avatar.Version, avatar.Style);

            int lodsLength = 4 - maxLOD;
            LODGroup lodGroup = mainAvatar.AddComponent<LODGroup>();
            LOD[] lods = new LOD[lodsLength];

            // Setup the Max LOD level
            lods[0] = new LOD(
                CalculateLODTransition(0, lodsLength),
                mainAvatar.GetComponentsInChildren<SkinnedMeshRenderer>()
            );
            lods[0].renderers.ToList().ForEach(r => r.gameObject.name += "_LOD_" + maxLOD); // Rename objects

            var mainMeshRenderer = mainAvatar.GetComponentInChildren<SkinnedMeshRenderer>();

            // Setup the rest of the LODs
            for (int i = maxLOD + 1; i < 4; i++)
            {
                byte[] lodAvatarBytes = await ResourceDownloader.Download(
                    avatar.Lod[i.ToString()],
                    ResourceType.Avatar,
                    ct,
                    updateDate: avatar.UpdatedAt,
                    fileId: avatar.Id.ToString()
                );
                var lodAvatar = await ImportResource(lodAvatarBytes, ct, "UnionAvatars_" + avatar.Name + "_LOD_" + i);
                List<Renderer> lodRenderers = new List<Renderer>();
                foreach (var renderer in lodAvatar.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    lodRenderers.Add(renderer);
                    renderer.rootBone = mainMeshRenderer.rootBone;
                    renderer.bones = mainMeshRenderer.bones;
                    renderer.transform.parent = mainAvatar.transform;
                    renderer.transform.name += "_LOD_" + i;
                    renderer.transform.SetSiblingIndex(i);
                }
                lods[i - maxLOD] = new LOD(CalculateLODTransition(i - maxLOD, lodsLength), lodRenderers.ToArray());
                GameObject.Destroy(lodAvatar);
            }

            lodGroup.SetLODs(lods);

            return mainAvatar;
        }

        private static float CalculateLODTransition(int level, int lodsLength)
        {
            var lodTransition = (level + 1f) / lodsLength;
            return 1 - lodTransition + CULL_LOD_TRANSITION;
        }
    }
}
