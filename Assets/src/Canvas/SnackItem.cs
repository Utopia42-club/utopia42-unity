using System;
using System.Collections.Generic;
using UnityEngine;

namespace src.Canvas
{
    public abstract class SnackItem
    {
        private readonly Snack snack;
        private readonly Action onUpdate;
        private bool expired;

        protected SnackItem(Snack snack, Action onUpdate)
        {
            this.snack = snack;
            this.onUpdate = onUpdate;
        }

        public virtual void Remove()
        {
            snack.Remove(this);
            expired = true;
        }

        public bool IsExpired()
        {
            return expired;
        }

        internal void Update()
        {
            if (onUpdate != null)
                onUpdate();
        }

        internal abstract void Hide();
        internal abstract void Show();

        public class Text : SnackItem
        {
            private string text;
            private readonly GameObject textPanel;
            private readonly GameObject textObject;

            public Text(Snack snack, Action onUpdate, string text, GameObject textPanel, GameObject textObject)
                : base(snack, onUpdate)
            {
                this.text = text;
                this.textPanel = textPanel;
                this.textObject = textObject;
            }

            internal override void Hide()
            {
                if (textPanel != null)
                    textPanel.SetActive(false);
            }

            internal override void Show()
            {
                textPanel.SetActive(true);
                textObject.GetComponent<TMPro.TextMeshProUGUI>().text = text;
            }

            public void UpdateText(string newText)
            {
                text = newText;
                textObject.GetComponent<TMPro.TextMeshProUGUI>().text = text;
            }

            public void UpdateLines(List<string> lines)
            {
                UpdateText(string.Join("\n", lines));
            }
        }

        public class Graphic : SnackItem
        {
            private GameObject gameObject;
            public readonly string prefab;

            public Graphic(Snack snack, Action onUpdate, string prefab) : base(snack, onUpdate)
            {
                this.prefab = prefab;
            }


            public override void Remove()
            {
                base.Remove();
                if (gameObject != null)
                    UnityEngine.Object.DestroyImmediate(gameObject);
            }

            internal override void Hide()
            {
                if (gameObject != null)
                    gameObject.SetActive(false);
            }

            internal override void Show()
            {
                if (gameObject == null)
                    gameObject = UnityEngine.Object.Instantiate(Resources.Load(prefab), snack.transform) as GameObject;
                gameObject.SetActive(true);
            }
        }
    }
}