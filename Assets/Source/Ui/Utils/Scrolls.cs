using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Utils
{
    public static class Scrolls
    {
        public static void IncreaseScrollSpeed(ScrollView scrollView, float factor = 600)
        {
            //Workaround to increase scroll speed...
            //There is this issue that verticalPageSize has no effect on speed
            scrollView.RegisterCallback<WheelEvent>((evt) =>
            {
                scrollView.scrollOffset = new Vector2(0, scrollView.scrollOffset.y + factor * evt.delta.y);
                evt.StopPropagation();
            });
        }
    }
}