using System;

namespace NSL
{
    public class NSLException : Exception
    {
        public NSLException(string message) : base(message) { }
    }
    public class InternalNSLExcpetion : Exception
    {
        public InternalNSLExcpetion(string message) : base(message) { }
    }
}