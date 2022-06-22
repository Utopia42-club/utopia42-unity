using System;
using System.Collections.Generic;
using Source.Utils;
using UnityEngine;

namespace Source.MetaBlocks
{
    public class ObjectScaleRotationController : MonoBehaviour
    {
        private static readonly Vector3 ScaleDelta = 0.005f * Vector3.one;
        private const float KeyboardRotationMultiplier = 3f;
        private readonly Dictionary<Transform, Action> scaleTargets = new();
        private readonly Dictionary<Transform, Action> rotateTargets = new();

        private bool scaleUp = false;
        private bool scaleDown = false;
        private bool rotationMode = false;
        private float leftRight;
        private float forwardBackward;

        public bool Active => rotationMode || scaleUp || scaleDown;

        private void Update()
        {
            if (!IsAttached()) return;

            leftRight = Input.GetAxis("Horizontal") * KeyboardRotationMultiplier;
            forwardBackward = Input.GetAxis("Vertical") * KeyboardRotationMultiplier;

            scaleUp = Input.GetKey(KeyCode.RightBracket);
            scaleDown = Input.GetKey(KeyCode.LeftBracket);

            if (rotationMode == Input.GetKey(KeyCode.R)) return;
            rotationMode = !rotationMode;
            if (!rotationMode)
            {
                MouseLook.INSTANCE.RemoveRotationTarget();
                return;
            }

            MouseLook.INSTANCE.SetRotationTarget(HandleRotation);
        }

        private void HandleRotation(Vector3 rotation)
        {
            var afterRotated = new List<Action>();
            foreach (var (target, action) in rotateTargets)
            {
                if (target == null) continue;
                
                // target.Rotate(target.InverseTransformVector(
                //     rotation.y * Vector3.up +
                //     rotation.x * Player.INSTANCE.avatar.transform.right));
                
                // target.Rotate(rotation.y * Vector3.up + rotation.x * Player.INSTANCE.avatar.transform.right);

                var up = target.InverseTransformDirection(Vector3.up);
                var right = target.InverseTransformDirection(Player.INSTANCE.Right);
                target.Rotate(rotation.y * up + rotation.x * right);
                
                if (action != null)
                    afterRotated.Add(action);
            }

            foreach (var action in afterRotated)
                action.Invoke();
        }

        private void FixedUpdate()
        {
            if (!IsAttached()) return;
            if (scaleUp) ScaleUp();
            if (scaleDown) ScaleDown();
            if (rotationMode)
                HandleRotation(new Vector3(forwardBackward, leftRight, 0));
        }

        public void AttachScaleTarget(Transform scaleTarget, Action afterScaled)
        {
            if (scaleTarget != null)
                scaleTargets[scaleTarget] = afterScaled;
        }

        public void AttachRotationTarget(Transform rotationTarget, Action afterRotated)
        {
            if (rotationTarget != null)
                rotateTargets[rotationTarget] = afterRotated;
        }

        private void ScaleUp()
        {
            var afterScaled = new List<Action>();
            foreach (var (target, action) in scaleTargets)
            {
                if (target == null) continue;
                target.localScale += ScaleDelta;
                if (action != null)
                    afterScaled.Add(action);
            }

            foreach (var action in afterScaled)
                action.Invoke();
        }

        private void ScaleDown()
        {
            var afterScaled = new List<Action>();
            foreach (var (target, action) in scaleTargets)
            {
                if (target == null) continue;
                var scale = target.localScale - ScaleDelta;
                target.localScale = new Vector3(Mathf.Max(scale.x, 0.1f), Mathf.Max(scale.y, 0.1f),
                    Mathf.Max(scale.z, 0.1f));
                if (action != null)
                    afterScaled.Add(action);
            }

            foreach (var action in afterScaled)
                action.Invoke();
        }

        private bool IsAttached()
        {
            return scaleTargets.Count != 0 || rotateTargets.Count != 0;
        }

        public void Detach(Transform scaleTarget, Transform rotationTarget)
        {
            if (scaleTarget != null)
                scaleTargets.Remove(scaleTarget);
            if (rotationTarget != null)
                rotateTargets.Remove(rotationTarget);
        }

        public void DetachAll()
        {
            scaleTargets.Clear();
            rotateTargets.Clear();
        }
    }
}