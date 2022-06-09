using System;

namespace src.Service.Migration.Models
{
    [Serializable]
    public class BaseImageBlockProperties<T> where T : MetaBlockFaceProperties
    {
        public T front;
        public T back;
        public T right;
        public T left;
        public T top;
        public T bottom;
    }
}