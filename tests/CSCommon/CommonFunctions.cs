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
            registry.Add(new NSLFunction("print", (argsEnum) =>
            {
                return new NSLFunction.Signature
                {
                    name = "print",
                    arguments = argsEnum.Select(v => v ?? PrimitiveTypes.voidType),
                    result = PrimitiveTypes.voidType
                };
            }, argsEnum =>
            {
                Console.WriteLine(String.Join(' ', argsEnum.Select(v => ToStringUtil.ToString(v.GetValue()))));
                return PrimitiveTypes.voidType.Instantiate(null);
            }));

            registry.Add(NSLFunction.MakeSimple(
                "getNumbers",
                new List<TypeSymbol> { },
                PrimitiveTypes.numberType.ToArray(),
                argsEnum => PrimitiveTypes.numberType.ToArray().Instantiate(new double[] { 5, 20, 8, 14, 5 })
            ));

            registry.Add(NSLFunction.MakeSimple(
                "exit",
                new List<TypeSymbol> { },
                PrimitiveTypes.voidType,
                _ => { Environment.Exit(0); return PrimitiveTypes.voidType.Instantiate(null); }
            ));
        }
    }
}