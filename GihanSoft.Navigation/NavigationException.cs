using System;
using System.Runtime.Serialization;

namespace GihanSoft.Navigation
{
    [Serializable]
    public class NavigationException : Exception
    {
        public NavigationException()
        {
        }

        public NavigationException(string message) : base(message)
        {
        }

        public NavigationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NavigationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}