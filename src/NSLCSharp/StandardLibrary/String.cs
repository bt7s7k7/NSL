using System;
using System.Collections.Generic;
using System.Linq;
using NSL.Types;

namespace NSL
{
    public partial class FunctionRegistry
    {
        private static void RegisterString(FunctionRegistry registry)
        {
            registry.Add(new NSLFunction("toString", argsEnum =>
            {
                var args = argsEnum.ToArray();
                var arg = (args.Length < 1 ? PrimitiveTypes.neverType : args[0]) ?? PrimitiveTypes.neverType;
                return new NSLFunction.Signature { name = "toString", arguments = new TypeSymbol[] { arg }, result = PrimitiveTypes.stringType };
            }, (argsEnum, state) => PrimitiveTypes.stringType.Instantiate(ToStringUtil.ToString(argsEnum.First()?.Value))));

            registry.Add(new NSLFunction("concat", (argsEnum) =>
            {
                return new NSLFunction.Signature
                {
                    name = "concat",
                    arguments = argsEnum.Select(v => v ?? PrimitiveTypes.voidType),
                    result = PrimitiveTypes.stringType,
                    desc = "Converts all arguments to string and concatenates them"
                };
            }, (argsEnum, state) =>
            {
                return PrimitiveTypes.stringType.Instantiate(String.Join("", argsEnum.Select(v => ToStringUtil.ToString(v.Value))));
            }));

            registry.Add(NSLFunction.MakeAuto<Func<string, string, bool>>("contains", (text, substr) => text.Contains(substr)));
            registry.Add(NSLFunction.MakeAuto<Func<string, string, double>>("indexOf", (text, substr) => text.IndexOf(substr)));
            registry.Add(NSLFunction.MakeAuto<Func<string, double, string>>("substr", (text, index) => text.Substring((int)index)));
            registry.Add(NSLFunction.MakeAuto<Func<string, double, double, string>>("substr", (text, index, length) => text.Substring((int)index, (int)length)));

            registry.Add(NSLFunction.MakeAuto<Func<string, string, bool>>("eq", (a, b) => a == b));
            registry.Add(NSLFunction.MakeAuto<Func<string, string, bool>>("neq", (a, b) => a != b));

            registry.Add(NSLFunction.MakeAuto<Func<string, string, IEnumerable<object>>>(
                "split",
                (text, delim) => text.Split(delim),
                new Dictionary<int, TypeSymbol> { { -1, PrimitiveTypes.stringType.ToArray() } }
            ));
        }
    }
}