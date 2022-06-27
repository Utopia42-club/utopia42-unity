using System;
using UnityEngine.UIElements;

namespace Source.Ui.AssetInventory
{
    public class GridUtils
    {
        public static void SetChildPosition(VisualElement element, int width, int height, int index, int itemsInARow)
        {
            var s = element.style;
            s.position = new StyleEnum<Position>(Position.Absolute);
            var div = index / itemsInARow;
            var rem = index % itemsInARow;
            s.left = rem * (width + 10);
            s.top = div * (height + 10);
        }

        public static void SetContainerSize(VisualElement container, int count, int height, int itemsInARow)
        {
            container.style.height = height * DivideRoundingUp(count, itemsInARow);
        }

        private static int DivideRoundingUp(int x, int y)
        {
            var quotient = Math.DivRem(x, y, out var remainder);
            return remainder == 0 ? quotient : quotient + 1;
        }
    }
}