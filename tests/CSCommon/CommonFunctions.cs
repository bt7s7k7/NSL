using System.Linq;
using System;
using System.Collections.Generic;
using NSL;
using NSL.Types;

namespace CSCommon
{
    public static class CommonFunctions
    {
        public static void RegisterCommonFunctions(FunctionRegistry registry)
        {
            registry.Add(NSLFunction.MakeSimple("print", new List<TypeSymbol> { PrimitiveTypes.stringType }, PrimitiveTypes.voidType, argsEnum =>
            {
                var args = argsEnum.ToArray();
                if (args[0].GetValue() is string message)
                {
                    Console.WriteLine(message);
                }
                return PrimitiveTypes.voidType.Instantiate(null);
            }));
        }
    }
}