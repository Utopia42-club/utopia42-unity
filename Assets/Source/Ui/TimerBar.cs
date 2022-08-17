using System;
using Source.UtopiaException;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Snack
{
    public class TimerBar : VisualElement
    {
        private bool attached = false;
        private bool detached = false;
        private readonly long updateDeltaTime = 50;
        private long remainingTime;
        private readonly Action onFinish;
        private readonly long totalDuration;
        private readonly IVisualElementScheduledItem scheduledItem;

        public TimerBar(long duration, Action onFinish)
        {
            remainingTime = duration;
            this.onFinish = onFinish;
            totalDuration = duration;
            styleSheets.Add(UxmlElement.GlobalStyleSheet);
            AddToClassList("utopia-accent-background");
            style.position = Position.Absolute;
            style.bottom = 0;
            style.left = 0;
            style.width = new Length(100, LengthUnit.Percent);
            style.height = 2;
            scheduledItem = schedule.Execute(UpdateWidth)
                .Every(updateDeltaTime);
            scheduledItem.Pause();

            RegisterCallback<AttachToPanelEvent>(e =>
            {
                if (attached || detached)
                    throw new IllegalStateException("Timer is already attached");
                attached = true;
                scheduledItem.ExecuteLater(updateDeltaTime);
            });
            RegisterCallback<DetachFromPanelEvent>(e =>
            {
                if (detached)
                    throw new IllegalStateException("Timer is already detached");
                detached = true;
                scheduledItem.ExecuteLater(updateDeltaTime);
            });
        }

        public void Pause()
        {
            if (scheduledItem.isActive)
                scheduledItem.Pause();
        }

        public void ResumeIfNotDetached()
        {
            if (!scheduledItem.isActive)
                scheduledItem.ExecuteLater(updateDeltaTime);
        }

        public void Resume()
        {
            if (detached)
                throw new IllegalStateException("Timer is already detached");
            ResumeIfNotDetached();
        }

        public void Stop()
        {
            if (!detached)
                parent.Remove(this);
        }

        private void UpdateWidth(TimerState timerState)
        {
            remainingTime -= updateDeltaTime;
            if (remainingTime < 0)
            {
                Stop();
                onFinish();
                return;
            }

            style.width = new Length(Mathf.FloorToInt((remainingTime * 100f) / totalDuration), LengthUnit.Percent);
        }
    }
}