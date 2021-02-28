
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MangaReader.PagesViewer
{
    public class PagesViewerException : Exception
    {
        public PagesViewerException() : base()
        {
        }

        public PagesViewerException(string? message) : base(message)
        {
        }

        public PagesViewerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected PagesViewerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
