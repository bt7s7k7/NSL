using System.Linq;
using NSL.Types;

namespace NSL
{
    public partial class FunctionRegistry
    {
        private static void RegisterBoolean(FunctionRegistry registry)
        {
            foreach (var operation in new (string name, System.Func<bool, bool, bool> callback)[] {
                (name: "and", callback: (a, b) => a && b),
                (name: "or", callback: (a, b) => a || b),
                (name: "eq", callback: (a, b) => a == b),
                (name: "neq", callback: (a, b) => a != b),
            })
            {
                registry.Add(NSLFunction.MakeAuto(operation.name, operation.callback));
            }

            foreach (var operation in new (string name, System.Func<bool, bool> callback)[] {
                (name: "not", callback: (a) => !a)
            })
            {
                registry.Add(NSLFunction.MakeAuto(operation.name, operation.callback));
            }

            registry.Add(new NSLFunction("invPost", argsEnum =>
            {
                var desc = "Inverts the value of the reference, returns the old value";
                return new NSLFunction.Signature
                {
                    name = "invPost",
                    desc = desc,
                    arguments = new TypeSymbol[] { PrimitiveTypes.boolType },
                    result = PrimitiveTypes.boolType
                };
            }, (argsEnum, state) =>
            {
                var value = (bool)argsEnum.ElementAt(0).Value!;
                argsEnum.ElementAt(0).Value = !value;
                return PrimitiveTypes.boolType.Instantiate(value);
            }));

            registry.Add(new NSLFunction("invPrev", argsEnum =>
            {
                var desc = "Inverts the value of the reference";
                return new NSLFunction.Signature
                {
                    name = "invPrev",
                    desc = desc,
                    arguments = new TypeSymbol[] { PrimitiveTypes.boolType },
                    result = PrimitiveTypes.boolType
                };
            }, (argsEnum, state) =>
            {
                var value = (bool)argsEnum.ElementAt(0).Value!;
                argsEnum.ElementAt(0).Value = !value;
                return PrimitiveTypes.boolType.Instantiate(argsEnum.ElementAt(0).Value);
            }));
        }
    }
}