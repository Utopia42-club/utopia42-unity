using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Source.Canvas
{
    public abstract class SnackItem
    {
        private readonly Action onUpdate;
        private readonly Snack snack;
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
            private readonly GameObject textObject;
            private readonly GameObject textPanel;
            private string text;

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
                textObject.GetComponent<TextMeshProUGUI>().text = text;
            }

            public void UpdateText(string newText)
            {
                text = newText;
                textObject.GetComponent<TextMeshProUGUI>().text = text;
            }

            public void UpdateLines(List<string> lines)
            {
                UpdateText(string.Join("\n", lines));
            }
        }

        public class Graphic : SnackItem
        {
            public readonly string prefab;
            private GameObject gameObject;

            public Graphic(Snack snack, Action onUpdate, string prefab) : base(snack, onUpdate)
            {
                this.prefab = prefab;
            }


            public override void Remove()
            {
                base.Remove();
                if (gameObject != null)
                    Object.DestroyImmediate(gameObject);
            }

            internal override void Hide()
            {
                if (gameObject != null)
                    gameObject.SetActive(false);
            }

            internal override void Show()
            {
                if (gameObject == null)
                    gameObject = Object.Instantiate(Resources.Load(prefab), snack.transform) as GameObject;
                gameObject.SetActive(true);
            }
        }
    }
}