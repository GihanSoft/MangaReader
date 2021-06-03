using System;

namespace MangaReader.Exceptions
{
    [Serializable]
    public class DuplicateMangaException : Exception
    {
        public DuplicateMangaException() { }
        public DuplicateMangaException(string message) : base(message) { }
        public DuplicateMangaException(string message, Exception inner) : base(message, inner) { }
        protected DuplicateMangaException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
