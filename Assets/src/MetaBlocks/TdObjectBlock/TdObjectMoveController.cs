using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    internal class TdObjectMoveController : MonoBehaviour
    {
        public float moveSpeed = 4f;

        private Transform scaleTarget;
        private Transform rotateTarget;
        private Transform moveTarget;

        private float leftRight;
        private float forwardBackward;
        private float upwardDownward;

        private bool rotate = false;
        private bool scaleUp = false;
        private bool scaleDown = false;

        private void Update()
        {
            if (!IsAttached()) return;

            leftRight = Input.GetAxis("Horizontal");
            forwardBackward = Input.GetAxis("Vertical");
            upwardDownward = Input.GetButton("Jump")
                ? (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift) ? -1 : +1)
                : 0;

            rotate = Input.GetKey(KeyCode.R);
            scaleUp = Input.GetKey(KeyCode.RightBracket);
            scaleDown = Input.GetKey(KeyCode.LeftBracket);
        }

        private void FixedUpdate()
        {
            if (!IsAttached()) return;

            var pivot = Player.INSTANCE.transform;
            var velocity = (pivot.forward * forwardBackward + pivot.right * leftRight + pivot.up * upwardDownward) *
                           Time.fixedDeltaTime *
                           moveSpeed;
            moveTarget.position += velocity;

            if (rotate) RotateAroundY();
            if (scaleUp) ScaleUp();
            if (scaleDown) ScaleDown();
        }

        internal void Attach(Transform moveTarget, Transform scaleTarget, Transform rotateTarget)
        {
            this.moveTarget = moveTarget;
            this.scaleTarget = scaleTarget;
            this.rotateTarget = rotateTarget;
        }

        private void RotateAroundY()
        {
            if (rotateTarget == null) return;
            rotateTarget.transform.Rotate(3 * Vector3.up);
        }

        private void ScaleUp()
        {
            if (scaleTarget == null) return;
            scaleTarget.transform.localScale += Vector3.one * 0.1f;
        }

        private void ScaleDown()
        {
            if (scaleTarget == null) return;
            scaleTarget.transform.localScale -= Vector3.one * 0.1f;
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