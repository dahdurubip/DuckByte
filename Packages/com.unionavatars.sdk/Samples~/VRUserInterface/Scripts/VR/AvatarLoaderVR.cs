using UnityEngine;
using UnityEngine.Animations;
using UnionAvatars.Utils;
using System.Threading;
using UnionAvatars.Avatars;
using System;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnionAvatars.API;

namespace UnionAvatars.Samples.VR
{
    public class AvatarLoaderVR : MonoBehaviour
    {
        [Header("Union Avatars")]
        public RuntimeAnimatorController PlayerAnimator;

        [Header("VR Hands")]
        [Tooltip("If true, it will use the avatar's hands")]
        public bool UseHands = true;

        [Tooltip("If true, it will match the hands gesture to the controller sensors")]
        public bool UseHandsAnimations = true;
        private VRControllerActions vrActionAsset;

        [Header("VR Tracking")]
        public Transform HeadTransform; // VR Head
        public Transform RighHandTransform; // VR Hand
        public Transform LeftHandTransform; // VR Hand
        public Vector3 HeadPositionOffset;
        public Vector3 HeadRotationOffset;
        public Vector3 HandsPositionOffset;
        public Vector3 HandsRotationOffset;

        private GameObject previousAvatar;
        Transform headTarget;
        Transform leftHandTarget;
        Transform rightHandTarget;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Animator avatarAnimator;

        public async void LoadNew(AvatarMetadata avatar)
        {
            if (previousAvatar != null)
            {
                Destroy(previousAvatar);
                Destroy(headTarget.gameObject);
                Destroy(leftHandTarget.gameObject);
                Destroy(rightHandTarget.gameObject);
            }
            await BuildAvatar(avatar);
            BindVRTargets();
            HideHeadMeshes();

            if (UseHands && UseHandsAnimations)
                SetupHandAnimations();
            else if (!UseHands)
                HideHands();
        }

        public async Task BuildAvatar(AvatarMetadata avatar)
        {
            //We import the avatar file into Unity asynchronously
            //Once it's finished the method "SetupAvatar" will be called
            Debug.Log("Importing avatar...");
            previousAvatar = await AvatarImporter.ImportHalfBodyAvatarAsHumanoid(
                avatar,
                PlayerAnimator,
                cts.Token
            );
            avatarAnimator = previousAvatar.GetComponent<Animator>();
        }

        private void BindVRTargets()
        {
            // Create custom gameObjects and assign their position and rotation to avatar's relevant bones
            headTarget = CreateTargetGameObjectForBone("VR_HeadTarget", HumanBodyBones.Head);
            leftHandTarget = CreateTargetGameObjectForBone("VR_LeftHandTarget", HumanBodyBones.LeftHand);
            rightHandTarget = CreateTargetGameObjectForBone("VR_RightHandTarget", HumanBodyBones.RightHand);

            // Add rotation and position constraints on the avatar's hip
            // Create an offset vector so the body keeps always the same distance with the head bone
            var bodyHeadOffset = avatarAnimator.GetBoneTransform(HumanBodyBones.Hips).position - headTarget.position;
            bodyHeadOffset.x = 0;
            bodyHeadOffset.z = 0;
            AddRotationConstraintToBone(HumanBodyBones.Hips, headTarget);
            AddPositionConstraintToBone(HumanBodyBones.Hips, headTarget, bodyHeadOffset);

            // Create constraints on avatar's bones and link their source to the respective targets
            AddParentConstraintToBone(HumanBodyBones.Head, headTarget);
            AddParentConstraintToBone(HumanBodyBones.LeftLowerArm, leftHandTarget);
            AddParentConstraintToBone(HumanBodyBones.RightLowerArm, rightHandTarget);

            // Reparent targets to VR Rig
            headTarget.parent = HeadTransform;
            headTarget.localPosition = HeadPositionOffset;
            headTarget.localEulerAngles = HeadPositionOffset;

            rightHandTarget.parent = RighHandTransform;
            rightHandTarget.localPosition = HandsPositionOffset;
            rightHandTarget.localEulerAngles = HandsRotationOffset;

            leftHandTarget.parent = LeftHandTransform;
            leftHandTarget.localPosition = new Vector3(
                -HandsPositionOffset.x,
                HandsPositionOffset.y,
                HandsPositionOffset.z
            );
            leftHandTarget.localEulerAngles = new Vector3(
                HandsRotationOffset.x,
                -HandsRotationOffset.y,
                -HandsRotationOffset.z
            );
        }

        private Transform CreateTargetGameObjectForBone(string gameObjectName, HumanBodyBones bone)
        {
            Transform target = new GameObject(gameObjectName).transform;
            target.position = avatarAnimator.GetBoneTransform(bone).position;
            target.rotation = avatarAnimator.GetBoneTransform(bone).rotation;
            return target;
        }

        private void AddParentConstraintToBone(HumanBodyBones bone, Transform target)
        {
            ParentConstraint constraint = avatarAnimator.GetBoneTransform(bone).gameObject.AddComponent<ParentConstraint>();
            ConstraintSource source = new ConstraintSource { sourceTransform = target, weight = 1 };
            constraint.AddSource(source);
            constraint.constraintActive = true;
        }

        private void AddRotationConstraintToBone(HumanBodyBones bone, Transform target)
        {
            RotationConstraint constraint = avatarAnimator.GetBoneTransform(bone).gameObject.AddComponent<RotationConstraint>();
            ConstraintSource source = new ConstraintSource { sourceTransform = target, weight = 1 };
            constraint.AddSource(source);
            constraint.rotationAxis = Axis.Y;
            constraint.constraintActive = true;
        }

        private void AddPositionConstraintToBone(HumanBodyBones bone, Transform target, Vector3 offset)
        {
            PositionConstraint constraint = avatarAnimator.GetBoneTransform(bone).gameObject.AddComponent<PositionConstraint>();
            ConstraintSource source = new ConstraintSource { sourceTransform = target, weight = 1 };
            constraint.AddSource(source);
            constraint.translationOffset = offset;
            constraint.constraintActive = true;
        }

        private void HideHeadMeshes()
        {
            if (avatarAnimator.transform.TryFindBFS("UnionAvatars_Head", out Transform headObj))
                headObj.gameObject.layer = LayerMask.NameToLayer("VRHead");
            if (avatarAnimator.transform.TryFindBFS("UnionAvatars_Hair", out Transform hairObj))
                hairObj.gameObject.layer = LayerMask.NameToLayer("VRHead");
        }

        private void HideHands()
        {
            avatarAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm).localScale = Vector3.zero;
            avatarAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm).localScale = Vector3.zero;
        }

        private void SetupHandAnimations()
        {
            if (vrActionAsset != null)
                return;

            vrActionAsset = new VRControllerActions();
            vrActionAsset.LeftHand.Enable();
            vrActionAsset.RightHand.Enable();
        }

        private void Update()
        {
            if (!UseHands || !UseHandsAnimations || vrActionAsset == null)
                return;

            float thumbLValue =
                1
                - (vrActionAsset.LeftHand.Primary2DAxisTouch.ReadValue<float>() * 0.5f)
                - (
                    vrActionAsset.LeftHand.Grip.ReadValue<float>()
                    * 0.5f
                    * vrActionAsset.LeftHand.Primary2DAxisTouch.ReadValue<float>()
                );

            float thumbRValue =
                1
                - (vrActionAsset.RightHand.Primary2DAxisTouch.ReadValue<float>() * 0.5f)
                - (
                    vrActionAsset.RightHand.Grip.ReadValue<float>()
                    * 0.5f
                    * vrActionAsset.RightHand.Primary2DAxisTouch.ReadValue<float>()
                );

            float indexLValue =
                1
                - (vrActionAsset.LeftHand.TriggerTouch.ReadValue<float>() * 0.5f)
                - (vrActionAsset.LeftHand.Trigger.ReadValue<float>() * 0.5f);

            float indexRValue =
                1
                - (vrActionAsset.RightHand.TriggerTouch.ReadValue<float>() * 0.5f)
                - (vrActionAsset.RightHand.Trigger.ReadValue<float>() * 0.5f);

            float fingersLValue = 0.5f - (vrActionAsset.LeftHand.Grip.ReadValue<float>() * 0.5f);

            float fingersRValue = 0.5f - (vrActionAsset.RightHand.Grip.ReadValue<float>() * 0.5f);

            avatarAnimator.SetFloat("ThumbL", Mathf.Lerp(avatarAnimator.GetFloat("ThumbL"), thumbLValue, Time.deltaTime * 8));
            avatarAnimator.SetFloat("ThumbR", Mathf.Lerp(avatarAnimator.GetFloat("ThumbR"), thumbRValue, Time.deltaTime * 8));
            avatarAnimator.SetFloat("IndexL", Mathf.Lerp(avatarAnimator.GetFloat("IndexL"), indexLValue, Time.deltaTime * 8));
            avatarAnimator.SetFloat("IndexR", Mathf.Lerp(avatarAnimator.GetFloat("IndexR"), indexRValue, Time.deltaTime * 8));
            avatarAnimator.SetFloat("FingersL", Mathf.Lerp(avatarAnimator.GetFloat("FingersL"), fingersLValue, Time.deltaTime * 8));
            avatarAnimator.SetFloat("FingersR", Mathf.Lerp(avatarAnimator.GetFloat("FingersR"), fingersRValue, Time.deltaTime * 8));
        }

        private void OnDestroy()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}
