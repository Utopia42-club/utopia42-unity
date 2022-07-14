using System.Linq;
using Source.UtopiaException;
using UnityEngine.UIElements;

namespace Source.Ui.AssetInventory
{
    internal class PackFoldout<T> : Foldout where T : VisualElement
    {
        public PackFoldout(string name, bool clearOnClose)
        {
            value = false;
            text = name;
            if (clearOnClose)
            {
                this.RegisterValueChangedCallback(e =>
                {
                    if (!e.newValue)
                        SetContent(null);
                    System.GC.Collect();
                });
            }
            // SetValueWithoutNotify(true);
            style.marginRight = style.marginLeft = style.marginBottom = style.marginTop = 5;
        }

        public void SetContent(T content)
        {
            contentContainer.Clear();
            if (content != null)
                contentContainer.Add(content);
        }

        public T GetContent()
        {
            if (contentContainer.childCount == 0) return null;
            if (contentContainer.childCount > 1)
                throw new IllegalStateException("Foldout children must not be more than one");
            var content = contentContainer.Children().ElementAt(0);
            if (content is not T element)
                throw new IllegalStateException("Foldout children must be of type " + typeof(T).FullName);
            return element;
        }
    }
}