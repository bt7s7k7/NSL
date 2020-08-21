using System;
using System.Collections.Generic;
using System.Linq;
using NSL.Types;

namespace NSL
{
    public class FunctionRegistry
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

        public static FunctionRegistry GetStandardFunctionRegistry()
        {
            var registry = new FunctionRegistry();

            registry.Add(new NSLFunction("echo", argsEnum =>
            {
                var args = argsEnum.ToArray();
                var arg = (args.Length < 1 ? PrimitiveTypes.neverType : args[0]) ?? PrimitiveTypes.neverType;
                return new NSLFunction.Signature { name = "echo", arguments = new TypeSymbol[] { arg }, result = arg };
            }, argsEnum => argsEnum.First()));

            registry.Add(new NSLFunction("toString", argsEnum =>
            {
                var args = argsEnum.ToArray();
                var arg = (args.Length < 1 ? PrimitiveTypes.neverType : args[0]) ?? PrimitiveTypes.neverType;
                return new NSLFunction.Signature { name = "toString", arguments = new TypeSymbol[] { arg }, result = PrimitiveTypes.stringType };
            }, argsEnum => PrimitiveTypes.stringType.Instantiate(argsEnum.First()?.GetValue()?.ToString() ?? "null")));

            registry.Add(new NSLFunction("concat", argsEnum =>
            {
                return new NSLFunction.Signature { name = "concat", arguments = Enumerable.Repeat(PrimitiveTypes.stringType, argsEnum.Count()), result = PrimitiveTypes.stringType };
            }, argsEnum => PrimitiveTypes.stringType.Instantiate(String.Join("", argsEnum.Select(v => v?.GetValue())))));

            return registry;
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