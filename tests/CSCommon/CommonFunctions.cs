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
            registry.Add(NSLFunction.MakeSimple("echo", new List<TypeSymbol> { PrimitiveTypes.stringType }, PrimitiveTypes.voidType, args =>
            {
                if (args[0].GetValue() is string message)
                {
                    Console.WriteLine(message);
                }
                return PrimitiveTypes.voidType.Instantiate(null);
            }));
        }
    }
}