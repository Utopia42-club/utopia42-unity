using UnityEngine;

namespace src
{
    public class AvatarController
    {
        private Vector3 lastPosition;
        private float lastTime;

        private readonly Animator animator;
        private readonly CharacterController controller;
        private readonly Transform transform;

        public AvatarController(Animator animator, CharacterController controller, Transform transform)
        {
            this.animator = animator;
            this.controller = controller;
            this.transform = transform;
        }

        public void Move(Vector3 motion)
        {
            controller.Move(motion);
        }

        public void LookAt(Vector3 cameraForward)
        {
            cameraForward.y = 0;
            transform.rotation = Quaternion.LookRotation(cameraForward);
        }

        public void RotateTo(Vector3 rotation)
        {
            rotation.x = 0;
            transform.Rotate(rotation);
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public void UpdateAnimation(Vector3 position, Vector3 cameraForward, bool floating, bool sprint)
        {
            var velocity = (position - lastPosition) / (Time.time - lastTime);
            var movement = position - lastPosition;

            lastPosition = position;
            lastTime = Time.time;
            if (floating || movement.y != 0 && movement.x == 0 && movement.z == 0)
            {
                animator.SetFloat("X", 0);
                animator.SetFloat("Z", 0);
                animator.SetFloat("Floating", floating ? 1 : 0);
                // if (movement.y < -0.5f && !floating)
                //     animator.CrossFade("Falling", 0.05f);
                return;
            }

            var newX = Vector3.Dot(velocity, Quaternion.Euler(0, 90, 0) * cameraForward);
            var newY = Vector3.Dot(velocity, cameraForward);

            animator.SetFloat("X", newX);
            animator.SetFloat("Z", newY);

            animator.SetFloat("Speed", sprint ? 0.9f : 0.1f);
        }

        public void JumpAnimation()
        {
            animator.CrossFade("Jump", 0.01f);
        }
    }
}