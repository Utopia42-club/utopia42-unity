using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas.Map
{
    internal class GridLine : MonoBehaviour
    {
        public static int SPACE = 50;
        private static int THICKNESS = 1;
        private RectTransform rectTransform;
        private bool vertical;
        private int index;

        private void Init(Transform parent, bool vertical, int index)
        {
            this.vertical = vertical;
            rectTransform = gameObject.AddComponent<RectTransform>();

            rectTransform.SetParent(parent);
            rectTransform.pivot = new Vector2(0, 0);

            rectTransform.SetSizeWithCurrentAnchors(vertical ? RectTransform.Axis.Horizontal
                : RectTransform.Axis.Vertical, THICKNESS);

            gameObject.AddComponent<CanvasRenderer>();
            gameObject.AddComponent<Image>().color = new Color(39, 174, 96);

            this.index = index + 1;
            SetIndex(index);
        }

        void Update()
        {
            UpdateLength();
        }

        private void UpdateLength()
        {
            if (vertical)
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);
            else
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
        }

        internal void SetIndex(int index)
        {
            if (this.index == index) return;
            this.index = index;
            gameObject.GetComponent<Image>().color = index == 0 ? Color.red : Color.green;
            gameObject.name = index + " " + vertical;
            rectTransform.localPosition = (vertical ? Vector3.right : Vector3.up) * SPACE * index;
        }

        internal int GetIndex()
        {
            return index;
        }

        internal void SetPos(float center)
        {
            Vector3 pos = rectTransform.localPosition;
            if (vertical)
                pos.y = center;
            else pos.x = center;
            rectTransform.localPosition = pos;
        }

        static internal GridLine create(Transform parent, bool vertical, int index)
        {
            var gameObject = new GameObject();
            var res = gameObject.AddComponent<GridLine>();
            res.Init(parent, vertical, index);
            return res;
        }
    }
}
