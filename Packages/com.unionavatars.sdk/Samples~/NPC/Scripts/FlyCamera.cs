using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnionAvatars.Samples
{
    public class FlyCamera : MonoBehaviour
    {
        public float flySpeed = 1.4f;
        public float rotSensivity = 1f;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Vector2 cameraRotation = Vector2.zero;

        private void Update()
        {
            // Rotation
            cameraRotation.x += Input.GetAxis("Mouse X") * rotSensivity;
            cameraRotation.y += Input.GetAxis("Mouse Y") * rotSensivity;
            var xQuat = Quaternion.AngleAxis(cameraRotation.x, Vector3.up);
            var yQuat = Quaternion.AngleAxis(cameraRotation.y, Vector3.left);

            transform.localRotation = xQuat * yQuat;

            // Movement
            var inputHorizontal = Input.GetAxisRaw("Horizontal");
            var inputVertical = Input.GetAxisRaw("Vertical");

            inputHorizontal = inputHorizontal * Time.deltaTime * flySpeed;
            inputVertical = inputVertical * Time.deltaTime * flySpeed;

            Vector3 moveVector = new Vector3(inputHorizontal, 0, inputVertical);

            transform.Translate(moveVector, Space.Self);
        }
    }
}
