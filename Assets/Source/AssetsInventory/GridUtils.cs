using UnityEngine.UIElements;

namespace Source.AssetsInventory
{
    public class GridUtils
    {
        public static void SetChildPosition(VisualElement element, int size, int index, int itemsInARow)
        {
            var s = element.style;
            s.position = new StyleEnum<Position>(Position.Absolute);
            var div = index / itemsInARow;
            var rem = index % itemsInARow;
            s.left = rem * (size + 10);
            s.top = div * (size + 10);
        }

        public static void SetContainerSize(VisualElement container, int size)
        {
            container.style.height = 90 * (size / 3 + 1);
        }
    }
}