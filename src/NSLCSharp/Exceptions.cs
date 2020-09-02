using System;
using System.Linq;
using System.Collections.Generic;
using NSL.Tokenization.General;
using NSL.Types;
using System.Text;

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

    public class OperatorNSLException : NSLException
    {
        public OperatorNSLException(string message) : base(message) { }
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

    public class UserNSLException : NSLException
    {
        protected Stack<Position> stack = new Stack<Position>();

        public UserNSLException(string message) : base(message) { }

        public void Add(Position position)
        {
            stack.Push(position);
        }

        public void Log()
        {
            ILogger.instance?.Error().Message(Message).End();
            foreach (var frame in stack.Reverse())
            {
                ILogger.instance?.Message("     ").Pos(frame).End();
            }
        }

        public string GetNSLStacktrace()
        {
            var builder = new StringBuilder();
            builder.AppendLine(Message);

            foreach (var frame in stack.Reverse())
            {
                builder.AppendLine(frame.ToString());
            }

            return builder.ToString();
        }

        override public string ToString() => Message;

        public static readonly TypeSymbol typeSymbol = new TypeSymbol("Error");

        static UserNSLException()
        {
            NSLFunction.SetTypeLookup(typeof(UserNSLException), typeSymbol);
        }
    }
}