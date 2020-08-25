using System;
using System.Collections.Generic;
using System.Linq;
using NSL.Types;

namespace NSL
{
    public partial class FunctionRegistry
    {
        protected Dictionary<string, NSLFunction> functions = new Dictionary<string, NSLFunction>();

        public void Add(NSLFunction function)
        {
            functions.Add(function.GetName(), function);
        }

        public NSLFunction? Find(string name)
        {
            if (functions.TryGetValue(name, out NSLFunction? function))
            {
                return function;
            }
            else return null;
        }

        public static NSLFunction MakeVariableDefinitionFunction(string varName)
        {
            return new NSLFunction(varName, argsEnum =>
            {
                var type = argsEnum.First() ?? PrimitiveTypes.neverType;
                return new NSLFunction.Signature
                {
                    name = varName,
                    arguments = new TypeSymbol[] { type },
                    result = type
                };
            }, argsEnum =>
            {
                return argsEnum.First();
            });
        }
    }
}