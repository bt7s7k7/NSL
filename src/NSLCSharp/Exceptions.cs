using System;
using NSL.Types;

namespace NSL
{
    public class NSLException : Exception
    {
        public NSLException(string message) : base(message) { }
    }

    public class InternalNSLExcpetion : NSLException
    {
        public InternalNSLExcpetion(string message) : base(message) { }
    }

    public class ImplWrongValueNSLException : NSLException
    {
        public ImplWrongValueNSLException() : base("Wrong variable values for function impl") { }
    }

    public class AutoFuncNSLException : NSLException
    {
        public AutoFuncNSLException(string message) : base(message) { }
    }

    public class OverloadNotFoundNSLException : InternalNSLExcpetion
    {
        public TypeSymbol ReturnType { get; protected set; }
        public string FunctionName { get; protected set; }
        public OverloadNotFoundNSLException(string message, TypeSymbol returnType, string functionName) : base(message)
        {
            ReturnType = returnType;
            FunctionName = functionName;
        }
    }
}