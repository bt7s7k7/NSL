using System;
using System.Linq;
using NSL.Types;

namespace NSL
{
    public partial class FunctionRegistry
    {
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
            }, argsEnum => PrimitiveTypes.stringType.Instantiate(ToStringUtil.ToString(argsEnum.First()?.GetValue()))));

            registry.Add(new NSLFunction("concat", (argsEnum) =>
            {
                return new NSLFunction.Signature
                {
                    name = "concat",
                    arguments = argsEnum.Select(v => v ?? PrimitiveTypes.voidType),
                    result = PrimitiveTypes.stringType
                };
            }, argsEnum =>
            {
                return PrimitiveTypes.stringType.Instantiate(String.Join("", argsEnum.Select(v => ToStringUtil.ToString(v.GetValue()))));
            }));

            return registry;
        }
    }
}