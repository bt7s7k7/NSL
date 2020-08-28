using System;
using System.Collections.Generic;
using System.Linq;
using NSL.Types;

namespace NSL
{
    public partial class FunctionRegistry
    {
        protected Dictionary<string, List<NSLFunction>> functions = new Dictionary<string, List<NSLFunction>>();

        public void Add(NSLFunction function)
        {
            if (!functions.ContainsKey(function.Name))
            {
                functions[function.Name] = new List<NSLFunction> { function };
            }
            else
            {
                functions[function.Name].Add(function);
            }
        }

        public IEnumerable<NSLFunction> Find(string name)
        {
            if (functions.TryGetValue(name, out List<NSLFunction>? functionsList))
            {
                return functionsList;
            }
            else return new List<NSLFunction>();
        }

        public static NSLFunction MakeVariableDefinitionFunction(string varName)
        {
            return new NSLFunction(varName, argsEnum =>
            {
                var type = (argsEnum.Count() == 0 ? null : argsEnum.First()) ?? PrimitiveTypes.neverType;
                return new NSLFunction.Signature
                {
                    name = varName,
                    arguments = new TypeSymbol[] { type },
                    result = type
                };
            }, (argsEnum, state) =>
            {
                return argsEnum.First();
            });
        }
    }
}