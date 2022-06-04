using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace src.AssetsInventory
{
    public class Utils
    {
        public static Action<T> Debounce<T>(Action<T> func, int milliseconds = 500)
        {
            CancellationTokenSource? cancelTokenSource = null;
            return arg =>
            {
                cancelTokenSource?.Cancel();
                cancelTokenSource = new CancellationTokenSource();
                Task.Delay(milliseconds, cancelTokenSource.Token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            func(arg);
                        }
                    }, TaskScheduler.Default);
            };
        }

        public static void IncreaseScrollSpeed(ScrollView scrollView, float factor)
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