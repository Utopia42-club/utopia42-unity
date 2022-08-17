using System;
using System.Collections.Generic;
using UnityEngine;

namespace Source.Canvas
{
    public class Snack : MonoBehaviour
    {
        [SerializeField] private GameObject textPanel;

        [SerializeField] private GameObject textObject;

        private readonly List<SnackItem> items = new();

        public static Snack INSTANCE => GameObject.Find("Snack").GetComponent<Snack>();

        private void Update()
        {
            if (items.Count > 0)
                items[items.Count - 1].Update();
        }


        public SnackItem ShowObject(string prefab, Action onUpdate)
        {
            return Add(new SnackItem.Graphic(this, onUpdate, prefab));
        }

        public SnackItem ShowLines(List<string> lines, Action onUpdate)
        {
            var text = string.Join("\n", lines);
            return Add(new SnackItem.Text(this, onUpdate, text, textPanel, textObject));
        }

        private SnackItem Add(SnackItem item)
        {
            if (items.Count > 0) items[items.Count - 1].Hide();
            items.Add(item);
            item.Show();
            return item;
        }

        internal void Remove(SnackItem item)
        {
            var index = items.IndexOf(item);
            if (index > 0)
            {
                item.Hide();
                items.RemoveAt(index);
                if (items.Count > 0) items[items.Count - 1].Show();
            }
        }
    }
}