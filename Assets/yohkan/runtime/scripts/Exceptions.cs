using System;

namespace yohkan.runtime.scripts
{
    [Serializable]
    public class YohkanUserDownloadCancelledException : Exception
    {
        public YohkanUserDownloadCancelledException(string message) : base(message)
        {
        }
        
    }
    
    [Serializable]
    public class YohkanException : Exception
    {
        public Exception AddressableException { get; }
        
        public YohkanException(string message) : base(message)
        {
        }
        
        public YohkanException(string message, Exception exception) : base(message)
        {
            AddressableException = exception;
        }
    }
    
    
}
