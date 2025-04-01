using UnionAvatars.Utils;
using UnityEngine;

namespace UnionAvatars.Samples.LipSync
{
//Module not supported in WebGL
#if !UNITY_WEBGL || UNITY_EDITOR

    public class AvatarLipSync
    {
        private static int[] shapeKeyArray =
        {
            -1, 56, 54, 59, 52, 62, 51, 58, 63, 57, 60, 53, 61, 64, 65
        };

        /// <summary>
        /// Adds all the required components to setup the lip sync on an avatar with the input from a mic
        /// </summary>
        /// <param name="avatar">
        /// The avatar root GameObject
        /// </param>
        public static void AddMicLipSync(GameObject avatar)
        {
            //Try get head mesh
            if(!avatar.transform.TryFindBFS("UnionAvatars_Head", out Transform headTransform))
                return;

            if(headTransform == null)
            {
                Debug.LogError("Couldn't find the avatar's head model");
                return;
            }

            if(headTransform.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer headMeshRenderer))
            {
                var audioSource = headMeshRenderer.gameObject.AddComponent<AudioSource>();

                headMeshRenderer.gameObject.AddComponent<OVRLipSyncContext>().audioSource = audioSource;
                
                var morphTargets = headMeshRenderer.gameObject.AddComponent<OVRLipSyncContextMorphTarget>();
                morphTargets.visemeToBlendTargets = shapeKeyArray;
                morphTargets.skinnedMeshRenderer = headMeshRenderer;

                headMeshRenderer.gameObject.AddComponent<OVRLipSyncMicInput>().audioSource = audioSource;
            }
            else
            {
                Debug.LogError("The head model doesn't have a Mesh Renderer");
                return;
            }
        }
    }
#endif
}
