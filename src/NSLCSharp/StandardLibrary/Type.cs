using System.Collections.Generic;
using System.Linq;
using NSL.Types;

namespace NSL
{
    public partial class FunctionRegistry
    {
        private static void RegisterType(FunctionRegistry registry)
        {
            var stringConstexpr = TypeSymbol.typeSymbol.Instantiate(PrimitiveTypes.stringType).MakeConstexpr();
            registry.Add(NSLFunction.MakeSimple("String", new TypeSymbol[0], stringConstexpr.TypeSymbol, (argsEnum, state) => stringConstexpr));

            registry.Add(new NSLFunction("default", argsEnum =>
            {
                var desc = "Instantiates the default value for the provided type";
                if (
                    argsEnum.Count() == 0 &&
                    argsEnum.ElementAt(0) is ConstexprTypeSymbol typeType
                )
                {
                    return new NSLFunction.Signature
                    {
                        name = "default",
                        desc = desc,
                        result = typeType.Base,
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
        }
    }
}