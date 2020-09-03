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
            }, (argsEnum, state) =>
            {
                Console.WriteLine(String.Join(' ', argsEnum.Select(v => ToStringUtil.ToString(v.Value))));
                return PrimitiveTypes.voidType.Instantiate(null);
            }));

            registry.Add(NSLFunction.MakeSimple(
                "exit",
                new List<TypeSymbol> { },
                PrimitiveTypes.voidType,
                (argsEnum, state) => { Environment.Exit(0); return PrimitiveTypes.voidType.Instantiate(null); }
            ));

            registry.Add(NSLFunction.MakeAuto<Func<string>>("readline", () => ReadLine.Read(), "Reads a line from user input"));
            registry.Add(NSLFunction.MakeAuto<Func<string, string>>("readline", (prompt) => ReadLine.Read(prompt), "Reads a line from user input, with the provided prompt"));
        }
    }
}