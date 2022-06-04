using System;
using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    internal class TdObjectMoveController : MonoBehaviour
    {
        private const float MoveSpeed = 1f;

        private static readonly Vector3 ScaleDelta = 0.005f * Vector3.one;
        // private static readonly Vector3 RotationDeltaY = 1f * Vector3.up;
        // private static readonly Vector3 RotationDeltaZ = 1f * Vector3.forward;

        private MouseLook mouseLook;
        private Player player;

        private Transform scaleTarget;
        private Transform rotateTarget;
        private Transform moveTarget;

        private float leftRight;
        private float forwardBackward;
        private float upwardDownward;

        private bool rotateY = false;
        private bool rotateZ = false;
        private bool scaleUp = false;
        private bool scaleDown = false;
        private bool rotationMode = false;

        private void Update()
        {
            if (!IsAttached()) return;

            leftRight = Input.GetAxis("Horizontal");
            forwardBackward = Input.GetAxis("Vertical");
            upwardDownward = Input.GetButton("Jump")
                ? (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift) ? -1 : +1)
                : 0;

            scaleUp = Input.GetKey(KeyCode.RightBracket);
            scaleDown = Input.GetKey(KeyCode.LeftBracket);

            if (rotationMode == Input.GetKey(KeyCode.R)) return;
            rotationMode = !rotationMode;
            if (!rotationMode)
                mouseLook.RemoveRotationTarget();
            else
                mouseLook.SetRotationTarget(rotation =>
                {
                    var rotateTargetTransform = rotateTarget.transform;
                    rotateTargetTransform.Rotate(rotateTargetTransform.InverseTransformVector(rotation.y * Vector3.up +
                        rotation.x * player.transform.right)); // TODO [detach metablock] ?
                });
        }

        private void FixedUpdate()
        {
            if (!IsAttached()) return;

            var pivot = Player.INSTANCE.transform;
            var velocity = (pivot.forward * forwardBackward + pivot.right * leftRight + pivot.up * upwardDownward) *
                           Time.fixedDeltaTime *
                           MoveSpeed;
            // moveTarget.position += velocity; // TODO [detach metablock] ?

            if (scaleUp) ScaleUp();
            if (scaleDown) ScaleDown();
        }

        internal void Attach(Transform moveTarget, Transform scaleTarget, Transform rotateTarget)
        {
            this.moveTarget = moveTarget;
            this.scaleTarget = scaleTarget;
            this.rotateTarget = rotateTarget;
            mouseLook = MouseLook.INSTANCE;
            player = Player.INSTANCE;
        }

        private void ScaleUp()
        {
            if (scaleTarget == null) return;
            scaleTarget.transform.localScale += ScaleDelta;
        }

        private void ScaleDown()
        {
            if (scaleTarget == null) return;
            var scale = scaleTarget.transform.localScale - ScaleDelta;
            scaleTarget.transform.localScale = new Vector3(Mathf.Max(scale.x, 0.1f), Mathf.Max(scale.y, 0.1f),
                Mathf.Max(scale.z, 0.1f));
        }

        internal bool IsAttached()
        {
            return moveTarget != null && scaleTarget != null && rotateTarget != null;
        }

        internal void Detach()
        {
            moveTarget = null;
            scaleTarget = null;
            rotateTarget = null;
        }
    }
}