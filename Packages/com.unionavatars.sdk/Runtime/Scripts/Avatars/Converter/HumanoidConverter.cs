using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnionAvatars.API;
using UnionAvatars.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnionAvatars.Avatars
{
    [Preserve]
    public static class HumanoidConverter
    {
        // Conversion: Union Avatars Mapping -> Unity Humanid Mapping
        static Dictionary<string, string> boneMapping = new Dictionary<string, string>
        {
            //Union Avatars Bone / Unity Humanoid Bone
            { "LeftEye", "LeftEye" },
            { "RightEye", "RightEye" },
            { "Hips", "Hips" },
            { "LeftUpLeg", "LeftUpperLeg" },
            { "RightUpLeg", "RightUpperLeg" },
            { "LeftLeg", "LeftLowerLeg" },
            { "RightLeg", "RightLowerLeg" },
            { "LeftFoot", "LeftFoot" },
            { "RightFoot", "RightFoot" },
            { "Spine", "Spine" },
            { "Spine1", "Chest" },
            { "Neck", "Neck" },
            { "Head", "Head" },
            { "LeftShoulder", "LeftShoulder" },
            { "RightShoulder", "RightShoulder" },
            { "LeftArm", "LeftUpperArm" },
            { "RightArm", "RightUpperArm" },
            { "LeftForeArm", "LeftLowerArm" },
            { "RightForeArm", "RightLowerArm" },
            { "LeftHand", "LeftHand" },
            { "RightHand", "RightHand" },
            { "LeftToeBase", "LeftToes" },
            { "RightToeBase", "RightToes" },
            { "Spine2", "UpperChest" },
            { "LeftHandThumb1", "Left Thumb Proximal" },
            { "LeftHandThumb2", "Left Thumb Intermediate" },
            { "LeftHandThumb3", "Left Thumb Distal" },
            { "LeftHandIndex1", "Left Index Proximal" },
            { "LeftHandIndex2", "Left Index Intermediate" },
            { "LeftHandIndex3", "Left Index Distal" },
            { "LeftHandMiddle1", "Left Middle Proximal" },
            { "LeftHandMiddle2", "Left Middle Intermediate" },
            { "LeftHandMiddle3", "Left Middle Distal" },
            { "LeftHandRing1", "Left Ring Proximal" },
            { "LeftHandRing2", "Left Ring Intermediate" },
            { "LeftHandRing3", "Left Ring Distal" },
            { "LeftHandPinky1", "Left Little Proximal" },
            { "LeftHandPinky2", "Left Little Intermediate" },
            { "LeftHandPinky3", "Left Little Distal" },
            { "RightHandThumb1", "Right Thumb Proximal" },
            { "RightHandThumb2", "Right Thumb Intermediate" },
            { "RightHandThumb3", "Right Thumb Distal" },
            { "RightHandIndex1", "Right Index Proximal" },
            { "RightHandIndex2", "Right Index Intermediate" },
            { "RightHandIndex3", "Right Index Distal" },
            { "RightHandMiddle1", "Right Middle Proximal" },
            { "RightHandMiddle2", "Right Middle Intermediate" },
            { "RightHandMiddle3", "Right Middle Distal" },
            { "RightHandRing1", "Right Ring Proximal" },
            { "RightHandRing2", "Right Ring Intermediate" },
            { "RightHandRing3", "Right Ring Distal" },
            { "RightHandPinky1", "Right Little Proximal" },
            { "RightHandPinky2", "Right Little Intermediate" },
            { "RightHandPinky3", "Right Little Distal" }
        };

        /// <summary>
        /// Converts an armature GameObject into a unity's humanoid compatible armature
        /// </summary>
        /// <param name="avatar">
        /// The avatar's armature GameObject
        /// </param>
        /// <param name="controller">
        /// The animator controller to assign
        /// </param>
        public static bool ConvertAvatarToHumanoid(
            this GameObject avatar,
            RuntimeAnimatorController controller,
            int version,
            Style style
        )
        {
            if (avatar == null)
                throw new ArgumentNullException("avatar");

            // Workaround for multiple armatur bug
            Transform armature = avatar.transform.FindBFS("Armature");
            int childCount = armature.parent.childCount;
            if (childCount > 2)
            {
                List<Transform> childrenToDelete = new List<Transform>();
                for (int i = 1; i < childCount; i++)
                {
                    Transform armatureSibling = armature.parent.GetChild(i);
                    if (armatureSibling != armature && armatureSibling.name.Contains("Armature"))
                        childrenToDelete.Add(armatureSibling);
                }
                foreach (Transform child in childrenToDelete)
                {
                    if (child.name.Contains("Armature"))
                        GameObject.DestroyImmediate(child.gameObject);
                }
            }

            AvatarPoseUtils.EnforceTPose(avatar, version, style);

            var humanDescription = new HumanDescription
            {
                skeleton = CreateSkeleton(avatar),
                human = boneMapping
                    .Select(mapping =>
                    {
                        // Skip eye bones if not found ( < v4 support )
                        if (mapping.Key.Contains("Eye"))
                        {
                            if (!avatar.transform.TryFindBFS(mapping.Value, out _))
                                return default;
                        }
                        var bone = new HumanBone { humanName = mapping.Value, boneName = mapping.Key };
                        bone.limit.useDefaultValues = true;
                        return bone;
                    })
                    .ToArray(),
            };

            //Build unity's avatar
            var humanoidAvatar = AvatarBuilder.BuildHumanAvatar(avatar, humanDescription);
            humanoidAvatar.name = avatar.name;

            if (!humanoidAvatar.isValid)
            {
                GameObject.Destroy(avatar);
                throw new ArgumentException("Couldn't create a humanoid avatar from the selected object", "avatar");
            }

            //Add the animator component and assing the created avatar
            var animator = avatar.AddComponent<Animator>();
            animator.applyRootMotion = true;
            animator.avatar = humanoidAvatar;
            animator.runtimeAnimatorController = controller;

            return true;
        }

        private static SkeletonBone[] CreateSkeleton(GameObject avatarRoot)
        {
            List<SkeletonBone> skeleton = new List<SkeletonBone>();

            Transform[] avatarTransforms = avatarRoot.GetComponentsInChildren<Transform>();
            foreach (Transform avatarTransform in avatarTransforms)
            {
                SkeletonBone bone = new SkeletonBone()
                {
                    name = avatarTransform.name,
                    position = avatarTransform.localPosition,
                    rotation = avatarTransform.localRotation,
                    scale = avatarTransform.localScale
                };

                skeleton.Add(bone);
            }
            return skeleton.ToArray();
        }
    }
}
