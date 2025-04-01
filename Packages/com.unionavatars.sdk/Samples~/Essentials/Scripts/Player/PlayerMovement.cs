// This is a very simple Monobehaviour to add player movement to an avatar
// It's not intended for actual use other than basic showcase in the examples

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnionAvatars.Samples
{
    public class PlayerMovement : MonoBehaviour
    {
        //Movement Variables
        const float walkSpeed = 1.4f;
        const float runSpeed = 3;
        const float jumpForce = 4;
        const float gravity = -.2f;
        const float rotationSpeed = 20;
        const float animationBlendSpeed = 10;

        //Component References
        private Animator playerAnimator;

        //State
        private float movementBlend = 0;
        private float yDirection = 0;

        //Input Variables
        private float inputHorizontal;
        private float inputVertical;
        private bool isRunning;

        public bool IsGrounded 
        {
            get
            {
                return transform.position.y <= 0;
            }
        }

        private void Start()
        {
            playerAnimator = GetComponent<Animator>();
        }

        private void Update()
        {
            GetInput();
        }

        void FixedUpdate()
        {
            //Gravity
            if(!IsGrounded)
            {
                yDirection += gravity;
            }
            else if(yDirection < 0)
            {
                //Stop the falling
                yDirection = 0;
            }

            var speed = isRunning ? runSpeed : walkSpeed;

            var horizontalMovement = inputHorizontal * speed;
            var verticalMovement = inputVertical * speed;

            Vector3 direction = new Vector3(horizontalMovement, yDirection, verticalMovement);

            transform.Translate(direction * Time.deltaTime, Space.World);
            
            RotateCharacter(direction);

            AnimateCharacter();
        }

        private void GetInput()
        {
            //Keys: A, D
            inputHorizontal = Input.GetAxisRaw("Horizontal");
            //Keys: W, S
            inputVertical = Input.GetAxisRaw("Vertical");

            isRunning = Input.GetKey(KeyCode.LeftShift);

            //Jump
            if(Input.GetKeyDown(KeyCode.Space) && IsGrounded)
            {
                yDirection = jumpForce;
            }
        }

        private void AnimateCharacter()
        {
            playerAnimator.SetBool("isFalling", !IsGrounded);

            if(!IsGrounded) return;

            float speedBlend = 0;

            if(InputIsPressed())
            {
                if(isRunning)
                    speedBlend = 1;
                else
                    speedBlend = 0.5f;
            }

            movementBlend = Mathf.Lerp(movementBlend, speedBlend, Time.deltaTime * animationBlendSpeed);

            playerAnimator.SetFloat("Speed", movementBlend);
        }

        private bool InputIsPressed()
        {
            return Math.Abs(inputHorizontal) > 0 || Math.Abs(inputVertical) > 0;
        }

        private void RotateCharacter(Vector3 direction)
        {
            if(!InputIsPressed()) return;

            //Remove y axis
            direction.y = 0;

            var newRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * rotationSpeed);
        }
    }
}