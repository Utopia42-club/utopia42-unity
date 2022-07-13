using System;
using UnityEngine.UIElements;

namespace Source.Ui
{
    public class CountDownTimer : VisualElement
    {
        private readonly int startValue;
        private Label label;

        public CountDownTimer(int startValueInSeconds)
        {
            startValue = startValueInSeconds;
            styleSheets.Add(UxmlElement.LoadStyleSheet(typeof(CountDownTimer)));
            AddToClassList("root");
        }


        public void ShowMessage(string msg)
        {
            Clear();
            label = new Label();
            label.AddToClassList("message");
            label.text = msg;
            Add(label);
        }

        public void Start(Action onTimeout)
        {
            Clear();
            label = new Label();
            label.AddToClassList("label");
            int current = startValue + 1;
            label.schedule.Execute(state =>
                {
                    if (!Contains(label))
                    {
                        current = 0;
                        return;
                    }

                    label.text = (--current).ToString();
                    if (current == 0)
                    {
                        Clear();
                        onTimeout();
                    }
                }).Every(1000)
                .Until(() => current <= 0);
            Add(label);
        }

        public void Stop()
        {
            Clear();
        }
    }
}