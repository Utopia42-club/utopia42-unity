using System.Collections.Generic;
using UnityEngine;

namespace src.Canvas
{
    public class Snack : MonoBehaviour
    {
        [SerializeField]
        private GameObject textPanel;
        [SerializeField]
        private GameObject textObject;
        private List<SnackItem> items = new List<SnackItem>();


        public SnackItem ShowObject(string prefab, System.Action onUpdate)
        {
            return Add(new SnackItem.Graphic(this, onUpdate, prefab));
        }

        public SnackItem ShowLines(List<string> lines, System.Action onUpdate)
        {
            string text = string.Join("\n", lines);
            int maxChars = 0;
            foreach (var l in lines)
                if (maxChars < l.Length) maxChars = l.Length;


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
            int index = items.IndexOf(item);
            if (index > 0)
            {
                item.Hide();
                items.RemoveAt(index);
                if (items.Count > 0) items[items.Count - 1].Show();
            }
        }

        private void Update()
        {
            if (items.Count > 0)
                items[items.Count - 1].Update();
        }

        public static Snack INSTANCE
        {
            get
            {
                return GameObject.Find("Snack").GetComponent<Snack>();
            }
        }
    }
}