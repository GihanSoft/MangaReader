using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaReader.Exceptions
{
    /// <summary>
    /// Not Initialized Exception
    /// </summary>
    [Serializable]
    public class NotInitializedException : Exception
    {
        public NotInitializedException() { }
        public NotInitializedException(string message) : base(message) { }
        public NotInitializedException(string message, Exception inner) : base(message, inner) { }
        protected NotInitializedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
