using System.Linq;
using System.Text;
using NSL.Types;

namespace NSL
{
    public partial class FunctionRegistry
    {
        private static void RegisterBasic(FunctionRegistry registry)
        {
            registry.Add(new NSLFunction("echo", argsEnum =>
            {
                var args = argsEnum.ToArray();
                var arg = (args.Length < 1 ? PrimitiveTypes.neverType : args[0]) ?? PrimitiveTypes.neverType;
                return new NSLFunction.Signature { name = "echo", arguments = new TypeSymbol[] { arg }, result = arg, desc = "Echoes the argument" };
            }, (argsEnum, state) => argsEnum.First()));

            registry.Add(new NSLFunction(
                name: "void",
                signatureGenerator: argsEnum => new NSLFunction.Signature { name = "void", arguments = argsEnum.Select(v => v ?? PrimitiveTypes.neverType), result = PrimitiveTypes.voidType, desc = "Returns void" },
                impl: (argsEnum, state) => PrimitiveTypes.voidType.Instantiate(null)
            ));

            registry.Add(NSLFunction.MakeSimple("help", new TypeSymbol[] { }, PrimitiveTypes.stringType, (argsEnum, state) =>
            {
                var builder = new StringBuilder();

                foreach (var keyValue in state.FunctionRegistry.functions)
                {
                    foreach (var function in keyValue.Value)
                    {
                        builder.AppendLine(function.GetSignature(new TypeSymbol[] { }).ToString());
                    }
                }

                return PrimitiveTypes.stringType.Instantiate(builder.ToString());
            }, "Prints all functions"));

            registry.Add(NSLFunction.MakeSimple("help", new[] { PrimitiveTypes.stringType }, PrimitiveTypes.stringType, (argsEnum, state) =>
            {
                if (argsEnum.ElementAt(0).Value is string name)
                {
                    var builder = new StringBuilder();

                    foreach (var function in state.FunctionRegistry.Find(name))
                    {
                        builder.AppendLine(function.GetSignature(new TypeSymbol[] { }).ToString());
                    }

                    return PrimitiveTypes.stringType.Instantiate(builder.ToString());
                }
                else throw new ImplWrongValueNSLException();
            }, "Prints all overloads for the function"));

            registry.Add(NSLFunction.MakeSimple(
                name: "if",
                arguments: new TypeSymbol[] {
                   PrimitiveTypes.boolType,
                   new ActionTypeSymbol(new TypeSymbol[0], PrimitiveTypes.voidType),
                },
                result: PrimitiveTypes.boolType,
                impl: (argsEnum, state) =>
                {
                    if (
                        argsEnum.ElementAt(0).Value is bool value &&
                        argsEnum.ElementAt(1).Value is NSLAction thenAction
                    )
                    {
                        if (value)
                        {
                            thenAction.Invoke(state.Runner, new IValue[0]);
                        }
                        return PrimitiveTypes.boolType.Instantiate(value);
                    }
                    else throw new ImplWrongValueNSLException();
                },
                desc: "Runs the first action if the predicate is true, else runs second action"
            ));

            registry.Add(NSLFunction.MakeSimple(
                name: "else",
                arguments: new TypeSymbol[] {
                   PrimitiveTypes.boolType,
                   new ActionTypeSymbol(new TypeSymbol[0], PrimitiveTypes.voidType),
                },
                result: PrimitiveTypes.boolType,
                impl: (argsEnum, state) =>
                {
                    if (
                        argsEnum.ElementAt(0).Value is bool value &&
                        argsEnum.ElementAt(1).Value is NSLAction thenAction
                    )
                    {
                        if (!value)
                        {
                            thenAction.Invoke(state.Runner, new IValue[0]);
                        }
                        return PrimitiveTypes.boolType.Instantiate(value);
                    }
                    else throw new ImplWrongValueNSLException();
                },
                desc: "Runs the first action if the predicate is true, else runs second action"
            ));

            registry.Add(NSLFunction.MakeSimple(
                name: "elseif",
                arguments: new TypeSymbol[] {
                   PrimitiveTypes.boolType,
                   new ActionTypeSymbol(new TypeSymbol[0], PrimitiveTypes.boolType),
                   new ActionTypeSymbol(new TypeSymbol[0], PrimitiveTypes.voidType),
                },
                result: PrimitiveTypes.boolType,
                impl: (argsEnum, state) =>
                {
                    if (
                        argsEnum.ElementAt(0).Value is bool value &&
                        argsEnum.ElementAt(1).Value is NSLAction predicate &&
                        argsEnum.ElementAt(2).Value is NSLAction thenAction
                    )
                    {
                        if (!value)
                        {
                            value = predicate.Invoke(state.Runner, new IValue[0]).GetValue<bool>();
                            if (value)
                            {
                                thenAction.Invoke(state.Runner, new IValue[0]);
                            }
                        }
                        return PrimitiveTypes.boolType.Instantiate(value);
                    }
                    else throw new ImplWrongValueNSLException();
                },
                desc: "Runs the first action if the predicate is true, else runs second action"
            ));

            registry.Add(NSLFunction.MakeSimple(
                name: "while",
                arguments: new TypeSymbol[] {
                   new ActionTypeSymbol(new TypeSymbol[0], PrimitiveTypes.boolType),
                   new ActionTypeSymbol(new TypeSymbol[0], PrimitiveTypes.voidType)
                },
                result: PrimitiveTypes.voidType,
                impl: (argsEnum, state) =>
                {
                    if (
                        argsEnum.ElementAt(0).Value is NSLAction predicate &&
                        argsEnum.ElementAt(1).Value is NSLAction action
                    )
                    {
                        while (predicate.Invoke(state.Runner, new IValue[0]).GetValue<bool>())
                        {
                            action.Invoke(state.Runner, new IValue[0]);
                        }
                        return PrimitiveTypes.voidType.Instantiate(null);
                    }
                    else throw new ImplWrongValueNSLException();
                },
                desc: "Executes the second action while the predicate is true"
            ));

            registry.Add(new NSLFunction("set", argsEnum =>
            {
                var desc = "Sets the reference to a different value";
                if
                (
                    argsEnum.Count() == 2 &&
                    argsEnum.ElementAt(0) is TypeSymbol valueType
                )
                {
                    return new NSLFunction.Signature
                    {
                        name = "set",
                        desc = desc,
                        arguments = new TypeSymbol[] { valueType, valueType },
                        result = valueType,
                        targetMustBeMutable = true
                    };
                }
                else
                {
                    return new NSLFunction.Signature
                    {
                        name = "set",
                        desc = desc,
                        arguments = new TypeSymbol[] { PrimitiveTypes.neverType, PrimitiveTypes.neverType },
                        result = PrimitiveTypes.neverType,
                        targetMustBeMutable = true
                    };
                }
            }, (argsEnum, state) =>
            {
                argsEnum.ElementAt(0).Value = argsEnum.ElementAt(1).Value;
                return argsEnum.ElementAt(0);
            }));
        }
    }
}