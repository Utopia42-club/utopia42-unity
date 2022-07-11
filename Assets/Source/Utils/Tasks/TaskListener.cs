using System;

namespace Source.Utils.Tasks
{
    public class TaskListener<T>
    {
        public readonly Action onFailure;
        public readonly Action<T> onSuccess;

        public TaskListener(Action onFailure, Action<T> onSuccess)
        {
            this.onFailure = onFailure;
            this.onSuccess = onSuccess;
        }
    }
}