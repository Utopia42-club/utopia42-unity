using System;
using System.Collections.Generic;
using Source.Canvas;
using Source.Model;
using UnityEngine;
using LightType = UnityEngine.LightType;

namespace Source.MetaBlocks.LightBlock
{
    public class LightBlockObject : MetaBlockObject
    {
        private const float LightDistance = 0.2f;

        private static readonly Vector3[] LightLocalPositions =
        {
            LightDistance * Vector3.back + 0.5f * (Vector3.right + Vector3.up),
            (LightDistance + 1) * Vector3.forward + 0.5f * (Vector3.right + Vector3.up),

            LightDistance * Vector3.left + 0.5f * (Vector3.forward + Vector3.up),
            (LightDistance + 1) * Vector3.right + 0.5f * (Vector3.forward + Vector3.up),

            LightDistance * Vector3.down + 0.5f * (Vector3.right + Vector3.forward),
            (LightDistance + 1) * Vector3.up + 0.5f * (Vector3.right + Vector3.forward)
        };

        private List<Light> sideLights = new List<Light>();

        public override void OnDataUpdate()
        {
            ResetLights();
        }

        protected override void DoInitialize()
        {
            ResetLights();
        }

        private void ResetLights()
        {
            return;
            if (sideLights.Count == 0)
                foreach (var position in LightLocalPositions)
                    sideLights.Add(CreateSideLight(position));

            var props = GetProps();
            if (props == null) return;

            ModifySideLights(
                ColorUtility.TryParseHtmlString(props.hexColor, out var col) ? col : LightBlockEditor.DefaultColor,
                props.intensity, props.range, true); // TODO: add active
        }

        private Light CreateSideLight(Vector3 localPosition)
        {
            var go = new GameObject();
            go.transform.SetParent(transform);
            go.transform.localPosition = localPosition;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point; // TODO ?
            go.SetActive(false);
            return l;
        }

        private void ModifySideLights(Color color, float intensity, float range, bool active)
        {
            foreach (var l in sideLights)
            {
                l.intensity = intensity;
                l.range = range;
                l.color = color;
                l.gameObject.SetActive(active);
            }
        }

        private void DestroyLights()
        {
            if (sideLights.Count == 0) return;
            foreach (var side in sideLights)
            {
                DestroyImmediate(side.gameObject);
            }

            sideLights.Clear();
        }

        private LightBlockProperties GetProps()
        {
            return (LightBlockProperties) Block.GetProps();
        }

        protected override void OnStateChanged(State state)
        {
        }

        protected virtual List<string> GetSnackLines()
        {
            return new List<string>
            {
                "Press Z for details",
                "Press DEL to delete object" // TODO
            };
        }

        public override void ShowFocusHighlight()
        {
        }

        public override void RemoveFocusHighlight()
        {
        }

        public override GameObject CreateSelectHighlight(Transform parent, bool show = true)
        {
            return null;
        }

        protected override void SetupDefaultSnack()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            if (!CanEdit) return;
            snackItem = Snack.INSTANCE.ShowLines(GetSnackLines(), () =>
            {
                if (Input.GetKeyDown(KeyCode.Z))
                    EditProps();
            });
        }

        private void EditProps()
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("Light Block Properties")
                .WithContent(LightBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<LightBlockEditor>();

            var props = Block.GetProps();
            editor.SetValue(props == null ? null : props as LightBlockProperties);
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                var props = new LightBlockProperties(Block.GetProps() as LightBlockProperties);
                if (value != null)
                {
                    props.intensity = value.intensity;
                    props.range = value.range;
                    props.hexColor = value.hexColor;
                }

                Block.SetProps(props, land);
                manager.CloseDialog(dialog);
            });
        }
    }
}