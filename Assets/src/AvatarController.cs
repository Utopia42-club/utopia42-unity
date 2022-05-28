using UnityEngine;

namespace src
{
    public class AvatarController
    {
        private float updatedTime;

        private readonly Animator animator;
        private readonly CharacterController controller;
        private readonly Transform transform;
        private PlayerState lastState;

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

        private void LookAt(Vector3 cameraForward)
        {
            cameraForward.y = 0;
            transform.rotation = Quaternion.LookRotation(cameraForward);
        }

        private void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public void UpdatePlayerState(PlayerState playerState, bool updateAnimation)
        {
            SetPosition(playerState.position);
            LookAt(playerState.cameraForward);
            if (updateAnimation)
                UpdateAnimation(playerState);
        }

        private void UpdateAnimation(PlayerState state)
        {
            var movement = state.position - (lastState?.position ?? state.position);
            var velocity = movement / (Time.time - updatedTime);

            updatedTime = Time.time;
            lastState = state;

            if (state.floating || movement.y != 0 && movement.x == 0 && movement.z == 0)
            {
                animator.SetFloat("X", 0);
                animator.SetFloat("Z", 0);
                animator.SetFloat("Floating", state.floating ? 1 : 0);
                // if (movement.y < -0.5f && !floating)
                //     animator.CrossFade("Falling", 0.05f);
                return;
            }

            var newX = Vector3.Dot(velocity, Quaternion.Euler(0, 90, 0) * state.cameraForward);
            var newY = Vector3.Dot(velocity, state.cameraForward);

            animator.SetFloat("X", newX);
            animator.SetFloat("Z", newY);

            animator.SetFloat("Speed", state.sprint ? 0.9f : 0.1f);
        }

        public void JumpAnimation()
        {
            animator.CrossFade("Jump", 0.01f);
        }

        public void ReportToServer()
        {
        }

        public class PlayerState
        {
            public Vector3 position;

            public Vector3 cameraForward;

            public bool floating;

            public bool sprint;

            public PlayerState(Vector3 position, Vector3 cameraForward, bool floating, bool sprint)
            {
                this.position = position;
                this.cameraForward = cameraForward;
                this.floating = floating;
                this.sprint = sprint;
            }
        }
    }
}