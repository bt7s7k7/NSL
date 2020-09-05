using System.Collections.Generic;
using System.Linq;
using NSL.Types;

namespace NSL
{
    public partial class FunctionRegistry
    {
        private static void RegisterType(FunctionRegistry registry)
        {
            registry.Add(new NSLFunction("default", argsEnum =>
            {
                var desc = "Instantiates the default value for the provided type";
                if (
                    argsEnum.Count() == 1 &&
                    argsEnum.ElementAt(0) is ConstexprTypeSymbol typeType &&
                    typeType.Value.Value is TypeSymbol type
                )
                {
                    return new NSLFunction.Signature
                    {
                        name = "default",
                        desc = desc,
                        result = type,
                        arguments = new[] { typeType },
                        useConstexpr = true
                    };
                }
                else
                {
                    return new NSLFunction.Signature
                    {
                        name = "default",
                        desc = desc,
                        result = PrimitiveTypes.neverType,
                        arguments = new[] { TypeSymbol.typeSymbol },
                        useConstexpr = true
                    };
                }
            }, (argsEnum, state) =>
            {
                if (argsEnum.ElementAt(0).Value is TypeSymbol typeSymbol)
                {
                    if (typeSymbol is ArrayTypeSymbol arrayTypeSymbol)
                    {
                        return typeSymbol.Instantiate(new List<object>());
                    }
                    else
                    {
                        return typeSymbol.Instantiate(null);
                    }
                }
                else throw new ImplWrongValueNSLException();
            }));

            registry.Add(new NSLFunction("array", argsEnum =>
            {
                var desc = "Makes array type of the specified type";
                if (
                    argsEnum.Count() == 1 &&
                    argsEnum.ElementAt(0) is ConstexprTypeSymbol typeType &&
                    typeType.Value.Value is TypeSymbol type
                )
                {
                    return new NSLFunction.Signature
                    {
                        name = "array",
                        desc = desc,
                        result = TypeSymbol.typeSymbol.Instantiate(type.ToArray()).MakeConstexpr().TypeSymbol,
                        arguments = new[] { typeType },
                        useConstexpr = true
                    };
                }
                else
                {
                    return new NSLFunction.Signature
                    {
                        name = "array",
                        desc = desc,
                        result = PrimitiveTypes.neverType,
                        arguments = new[] { TypeSymbol.typeSymbol },
                        useConstexpr = true
                    };
                }
            }, (argsEnum, state) =>
            {
                if (argsEnum.ElementAt(0).Value is TypeSymbol typeSymbol)
                {
                    return TypeSymbol.typeSymbol.Instantiate(typeSymbol.ToArray()).MakeConstexpr();
                }
                else throw new ImplWrongValueNSLException();
            }));
        }
    }
}