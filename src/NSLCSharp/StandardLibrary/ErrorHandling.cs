using System;
using System.Linq;
using NSL.Types;

namespace NSL
{
    public partial class FunctionRegistry
    {
        private static void RegisterErrorHandling(FunctionRegistry registry)
        {
            registry.Add(NSLFunction.MakeAuto<Action<bool>>("assert", (value) =>
            {
                if (!value)
                {
                    throw new UserNSLException("Assert failed!");
                }
            }, "Assert that the a value is true else throw an error"));

            registry.Add(NSLFunction.MakeAuto<Action<string>>("throw", (value) =>
            {
                throw new UserNSLException(value);
            }, "Throw an error with the specified messsage"));

            registry.Add(NSLFunction.MakeAuto<Action<UserNSLException>>("throw", (value) =>
            {
                throw value;
            }, "Throw the provided error"));

            registry.Add(NSLFunction.MakeSimple(
                name: "try",
                arguments: new TypeSymbol[] {
                    new ActionTypeSymbol(PrimitiveTypes.voidType, PrimitiveTypes.voidType),
                    new ActionTypeSymbol(UserNSLException.typeSymbol, PrimitiveTypes.voidType)
                },
                result: PrimitiveTypes.voidType,
                impl: (argsEnum, state) =>
                {
                    if (
                        argsEnum.ElementAt(0).Value is NSLAction tryAction &&
                        argsEnum.ElementAt(1).Value is NSLAction catchAction
                    )
                    {
                        try
                        {
                            tryAction.Invoke(state.Runner, PrimitiveTypes.voidType.Instantiate(null));
                        }
                        catch (UserNSLException err)
                        {
                            catchAction.Invoke(state.Runner, UserNSLException.typeSymbol.Instantiate(err));
                        }
                        return PrimitiveTypes.voidType.Instantiate(null);
                    }
                    else throw new ImplWrongValueNSLException();
                },
                desc: "Catches errors in the first action, invokes the second action with the caught error"
            ));
        }
    }
}