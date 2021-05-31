using System;

namespace MangaReader.Exceptions
{
    public class DuplicateMangaException : Exception
    {
        public DuplicateMangaException() : base()
        {
        }

        public DuplicateMangaException(string? message) : base(message)
        {
        }

        public DuplicateMangaException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
