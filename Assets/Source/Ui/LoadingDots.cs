using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui
{
    public class LoadingDots : VisualElement
    {
        public LoadingDots()
        {
            styleSheets.Add(UxmlElement.LoadStyleSheet(typeof(LoadingDots)));
            AddToClassList("loading-dots");
            var dots = new List<Dot>();
            dots.Add(new Dot(0));
            Add(dots[0]);
            dots.Add(new Dot(1));
            Add(dots[1]);
            dots.Add(new Dot(2));
            Add(dots[2]);
            schedule
                .Execute(t =>
                {
                    foreach (var dot in dots)
                        dot.Update();
                }).Every(100);
            RegisterCallback<GeometryChangedEvent>(e =>
            {
                var size =
                    Mathf.Min(72, Mathf.Max(e.newRect.width / 2f, 24), e.newRect.width - 4, e.newRect.height - 4) / 3f;
                size = Mathf.Max(size, 0);
                dots.ForEach(dot =>
                {
                    dot.style.width = size;
                    dot.style.height = size;
                });
            });
        }

        private class Dot : VisualElement
        {
            private float scale = 1;
            private int sign = -1;
            private int waits;

            public Dot(int index)
            {
                AddToClassList("dot");
                waits = 2 * index;
            }

            public void Update()
            {
                if (waits > 0)
                {
                    waits--;
                    return;
                }

                scale += sign * 0.2f;
                if (scale <= 0)
                {
                    scale = 0;
                    sign = 1;
                }
                else if (scale >= 1)
                {
                    scale = 1;
                    sign = -1;
                }

                transform.scale = Vector3.one * scale;
            }
        }
    }
}