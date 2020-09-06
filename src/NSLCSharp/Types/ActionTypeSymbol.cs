using System;
using System.Collections.Generic;

namespace NSL.Types
{
    public class ActionTypeSymbol : TypeSymbol
    {
        public IEnumerable<TypeSymbol> Arguments { get; protected set; }
        public TypeSymbol Result { get; protected set; }

        public ActionTypeSymbol(IEnumerable<TypeSymbol> arguments, TypeSymbol result) : base("(" + String.Join(", ", arguments) + ")" + " => " + result)
        {
            this.Arguments = arguments;
            this.Result = result;
        }

        public IEnumerable<TypeSymbol> GetArguments() => Arguments;
        public TypeSymbol GetResult() => Result;
    }
}