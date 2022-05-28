using System.Collections;
using UnityEngine;

namespace src
{
    public class AvatarController : MonoBehaviour
    {
        public GameObject avatarPrefab;

        private Animator animator;
        private CharacterController controller;
        private GameObject avatar;

        private float updatedTime;
        private PlayerState lastAnimationState;
        private PlayerState state;

        public void Start()
        {
            avatar = Instantiate(avatarPrefab, transform);
            animator = avatar.GetComponent<Animator>();
            controller = GetComponent<CharacterController>();
            StartCoroutine(UpdateAnimationCoroutine());
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

        public void UpdatePlayerState(PlayerState playerState)
        {
            SetPosition(playerState.position);
            LookAt(playerState.forward);
            state = playerState;
        }

        private void UpdateAnimation()
        {
            var movement = state.position - (lastAnimationState?.position ?? state.position);
            var velocity = movement / (Time.time - updatedTime);

            updatedTime = Time.time;
            lastAnimationState = state;

            if (state.floating || movement.y != 0 && movement.x == 0 && movement.z == 0)
            {
                animator.SetFloat("X", 0);
                animator.SetFloat("Z", 0);
                animator.SetFloat("Floating", state.floating ? 1 : 0);
                // if (movement.y < -0.5f && !floating)
                //     animator.CrossFade("Falling", 0.05f);
                return;
            }

            var newX = Vector3.Dot(velocity, Quaternion.Euler(0, 90, 0) * state.forward);
            var newY = Vector3.Dot(velocity, state.forward);

            animator.SetFloat("X", newX);
            animator.SetFloat("Z", newY);

            animator.SetFloat("Speed", state.sprint ? 0.9f : 0.1f);
        }

        public void JumpAnimation()
        {
            animator.CrossFade("Jump", 0.01f);
        }

        IEnumerator UpdateAnimationCoroutine()
        {
            while (true)
            {
                if (GameManager.INSTANCE.GetState() == GameManager.State.PLAYING)
                    UpdateAnimation();
                yield return new WaitForSeconds(0.1f);
            }
        }

        public void ReportToServer()
        {
        }

        public class PlayerState
        {
            public Vector3 position;

            public Vector3 forward;

            public bool floating;

            public bool sprint;

            public PlayerState(Vector3 position, Vector3 forward, bool floating, bool sprint)
            {
                this.position = position;
                this.forward = forward;
                this.floating = floating;
                this.sprint = sprint;
            }
        }
    }
}