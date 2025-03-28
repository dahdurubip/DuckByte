using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnionAvatars.API;
using UnionAvatars.Utils;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnionAvatars.Avatars
{
    [Preserve]
    public class AvatarPoseUtils
    {
        public static void EnforceTPose(GameObject avatarRoot, int headVersion, Style style)
        {
            // TODO: Refactor into a new class with a custom constructor to allow using different dictionaries
            var jsonDict = Resources.Load<TextAsset>(
                $"UnionAvatars/BoneOffsets/v{headVersion}_{style.ToString().ToLower()}"
            );

            if (jsonDict == null)
                throw new ArgumentException($"Unsupported avatar skeleton version [{headVersion}] and style [{style}]");

            Dictionary<string, Quaternion> offsetDict = JsonConvert.DeserializeObject<Dictionary<string, Quaternion>>(
                jsonDict.text,
                new JsonSerializerSettings() { Converters = new[] { new QuaternionConverter(), } }
            );

            Transform[] boneTransforms = avatarRoot.GetComponentsInChildren<Transform>();
            foreach (Transform boneTransform in boneTransforms)
            {
                Quaternion boneNewRotation;

                if (offsetDict.ContainsKey(boneTransform.name))
                    boneNewRotation = offsetDict[boneTransform.name] * boneTransform.localRotation;
                else
                    boneNewRotation = boneTransform.localRotation;

                boneTransform.localRotation = boneNewRotation;
            }
        }
    }
}
