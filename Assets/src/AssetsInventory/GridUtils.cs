using System;
using UnityEngine.UIElements;

namespace src.AssetsInventory
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
            container.style.height = 90 * DivideRoundingUp(size, 3);
        }
        
        private static int DivideRoundingUp(int x, int y)
        {
            var quotient = Math.DivRem(x, y, out var remainder);
            return remainder == 0 ? quotient : quotient + 1;
        }
    }
}