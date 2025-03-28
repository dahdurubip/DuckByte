using UnityEngine;
using UnityEngine.EventSystems;

namespace UnionAvatars.UI
{
    public class AvatarViewUIRotationZoom : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        [SerializeField] private Camera AvatarCamera;
        [SerializeField] private Transform AvatarParent;
        [SerializeField] private float RotationSpeed = 1;
        [SerializeField] private float MaxVelocity = 2;
        [SerializeField] private float SlowDownSpeed = 1;
        [SerializeField] private float ZoomSpeed = 200;
        private bool isDragging;
        private float velocity;
        const float minFov = 18;
        const float maxFov = 62;
        const float avatarParentLowPosition = -1.54f;
        const float avatarParentHighosition = -0.93f;
        private float mouseAxisX;
        private float mouseAxisY;

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            //Rotation
            mouseAxisX = Input.GetAxis("Mouse X");
            velocity = mouseAxisX;
            AvatarParent.Rotate(Vector3.down, velocity * RotationSpeed, Space.Self);

            //Camera Fov
            mouseAxisY = Input.GetAxis("Mouse Y");
            AvatarCamera.fieldOfView += mouseAxisY * ZoomSpeed;
            AvatarCamera.fieldOfView = Mathf.Clamp(AvatarCamera.fieldOfView, minFov, maxFov);

            //Avatar reposition
            float newParentYPosition = AvatarParent.localPosition.y;
            newParentYPosition += mouseAxisY * (ZoomSpeed / 72f); //Fov/Position rate
            newParentYPosition = Mathf.Clamp(newParentYPosition, avatarParentLowPosition, avatarParentHighosition);
            AvatarParent.localPosition = new Vector3(
                AvatarParent.localPosition.x,
                newParentYPosition,
                AvatarParent.localPosition.z
            );
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;

            //Inertia
            if (Mathf.Abs(mouseAxisX) <= 0.2f)
            {
                velocity = 0;
            }
            else
            {
                velocity = Mathf.Clamp(velocity, -MaxVelocity, MaxVelocity);
            }
        }

        private void Update()
        {
            if (velocity == 0)
                return;

            //Inertia
            if (!isDragging)
            {
                velocity = Mathf.Lerp(velocity, 0, Time.deltaTime * SlowDownSpeed);
                AvatarParent.Rotate(Vector3.down, velocity * RotationSpeed, Space.Self);
            }
        }
    }
}
