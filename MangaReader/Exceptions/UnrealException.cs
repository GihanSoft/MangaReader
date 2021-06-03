namespace MangaReader.Exceptions
{
    [System.Serializable]
    public class UnrealException : System.Exception
    {
        public UnrealException() { }
        public UnrealException(string message) : base(message) { }
        public UnrealException(string message, System.Exception inner) : base(message, inner) { }
        protected UnrealException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
