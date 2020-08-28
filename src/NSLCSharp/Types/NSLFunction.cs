using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSL.Runtime;

namespace NSL.Types
{
    public class NSLFunction
    {
        protected Func<IEnumerable<TypeSymbol?>, Signature> signatureGenerator;
        protected Func<IEnumerable<NSLValue>, Runner.State, NSLValue> impl;
        public string Name { get; protected set; }

        public struct Signature
        {
            public IEnumerable<TypeSymbol> arguments;
            public TypeSymbol result;
            public string name;
            public string? desc;

            public override string ToString() => $"{name}({String.Join(' ', arguments)}) → {result}" + (desc == null ? "" : " :: " + desc);
        }

        public Signature GetSignature(IEnumerable<TypeSymbol?> providedArguments) => signatureGenerator(providedArguments);

        public NSLValue Invoke(IEnumerable<NSLValue> arguments, Runner.State state) => impl(arguments, state);

        public string GetName() => Name;

        public override string ToString() => Name;

        public NSLFunction(string name, Func<IEnumerable<TypeSymbol?>, Signature> signatureGenerator, Func<IEnumerable<NSLValue>, Runner.State, NSLValue> impl)
        {
            this.signatureGenerator = signatureGenerator;
            this.impl = impl;
            this.Name = name;
        }

        private static Dictionary<Type, TypeSymbol> typeSymbolLookup = new Dictionary<Type, TypeSymbol> {
            { typeof(double), PrimitiveTypes.numberType },
            { typeof(string), PrimitiveTypes.stringType },
            { typeof(void), PrimitiveTypes.voidType },
            { typeof(bool), PrimitiveTypes.boolType }
        };

        public static NSLFunction MakeSimple(string name, IEnumerable<TypeSymbol> arguments, TypeSymbol result, Func<IEnumerable<NSLValue>, Runner.State, NSLValue> impl, string? desc = null) => new NSLFunction(
            name,
            _ => new Signature
            {
                arguments = arguments,
                result = result,
                name = name,
                desc = desc
            },
            impl
        );

        public static void SetTypeLookup(Type type, TypeSymbol symbol) => typeSymbolLookup[type] = symbol;

        public static TypeSymbol LookupSymbol(Type type)
        {
            if (typeSymbolLookup.TryGetValue(type, out TypeSymbol? foundSymbol))
            {
                return foundSymbol!;
            }
            else
            {
                throw new AutoFuncNSLException($"Failed to lookup type symbol for type {type}");
            }
        }

        public static NSLFunction MakeAuto<T>(string name, T func, string? desc = null) => MakeAuto<T>(name, func, new Dictionary<int, TypeSymbol>(), desc);
        public static NSLFunction MakeAuto<T>(string name, T func, Dictionary<int, TypeSymbol> replacements, string? desc = null)
        {
            if (func == null) throw new AutoFuncNSLException("Provided function is null");

            var funcType = func.GetType();
            var invokeMethod = funcType.GetMethod("Invoke") ?? throw new AutoFuncNSLException("Provided function is not invokable");
            var arguments = invokeMethod.GetParameters().Select((v, i) => replacements.TryGetValue(i, out TypeSymbol? result) ? result : LookupSymbol(v.ParameterType));

            var returnType = replacements.TryGetValue(-1, out TypeSymbol? ret) ? ret : LookupSymbol(invokeMethod.ReturnType);

            return NSLFunction.MakeSimple(name, arguments, returnType, (argsEnum, runnerState) =>
            {
                var values = argsEnum.Select(v => v.GetValue());
                try
                {
                    return returnType.Instantiate(invokeMethod.Invoke(func, values.ToArray()));
                }
                catch (System.Reflection.TargetInvocationException err)
                {
                    throw err.InnerException ?? throw new InternalNSLExcpetion("Caught a reflection error but it doesn't have an inner exception");
                }
            }, desc);
        }

        public static (NSLFunction function, Signature signature) GetMatchingFunction(IEnumerable<NSLFunction> functions, IEnumerable<TypeSymbol?> providedArgs, Func<int, TypeSymbol, TypeSymbol>? expandAction = null)
        {
            var failed = new List<Signature>();
            var failedAction = false;
            foreach (var function in functions)
            {
                var signature = function.GetSignature(providedArgs);
                var wantedArgs = signature.arguments.ToArray();

                if (providedArgs.Count() != wantedArgs.Length)
                {
                    failed.Add(signature);
                    continue;
                }
                else
                {
                    var success = true;
                    for (int i = 0, len = providedArgs.Count(); i < len; i++)
                    {
                        var provided = providedArgs.ElementAt(i);
                        var wanted = wantedArgs[i];

                        if (provided == wanted && provided != PrimitiveTypes.neverType)
                        {
                            continue;
                        }
                        else if (provided == null && wanted is ActionTypeSymbol action)
                        {
                            if (expandAction == null)
                            {
                                continue;
                            }
                            else
                            {
                                var resultType = expandAction(i, action.Argument);
                                var actionType = new ActionTypeSymbol(action.Argument, resultType);
                                if (actionType == wanted)
                                {
                                    continue;
                                }
                                else
                                {
                                    success = false;
                                    failedAction = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            success = false;
                            break;
                        }
                    }

                    if (failedAction)
                    {
                        failed.Clear();
                        failed.Add(signature);
                        break;
                    }

                    if (success)
                    {
                        return (function, signature);
                    }
                    else
                    {
                        failed.Add(signature);
                        continue;
                    }
                }
            }

            throw new OverloadNotFoundNSLException(
                $"Failed to find matching overload for '{functions.First().Name}({String.Join(' ', providedArgs.Select(v => v?.ToString() ?? "<action>"))})'\n" +
                String.Join('\n', failed.Select(v => $"  {v}")) +
                (failedAction ? "\n  -- No other overloads tried because a parameter is an action --\n" : "") +
                "\n  ",
                returnType: failed[0].result,
                functionName: failed[0].name
            );
        }
    }
}