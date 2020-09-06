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
        protected Func<IEnumerable<IValue>, Runner.State, IValue> impl;
        public string Name { get; protected set; }

        public struct Signature
        {
            public IEnumerable<TypeSymbol> arguments;
            public TypeSymbol result;
            public string name;
            public string? desc;
            public bool useConstexpr;
            public bool targetMustBeMutable;
            public delegate void PostProcessDelegate(ref Signature signature);
            public PostProcessDelegate? postProcess;

            public override string ToString() => $"{name}({String.Join(' ', arguments)}) → {result}" + (desc == null ? "" : " :: " + desc);
        }

        public Signature GetSignature(IEnumerable<TypeSymbol?> providedArguments) => signatureGenerator(providedArguments);

        public IValue Invoke(IEnumerable<IValue> arguments, Runner.State state) => impl(arguments, state);

        public string GetName() => Name;

        public override string ToString() => Name;

        public NSLFunction(string name, Func<IEnumerable<TypeSymbol?>, Signature> signatureGenerator, Func<IEnumerable<IValue>, Runner.State, IValue> impl)
        {
            this.signatureGenerator = signatureGenerator;
            this.impl = impl;
            this.Name = name;
        }

        private static Dictionary<Type, TypeSymbol> typeSymbolLookup = new Dictionary<Type, TypeSymbol> {
            { typeof(double), PrimitiveTypes.numberType },
            { typeof(string), PrimitiveTypes.stringType },
            { typeof(void), PrimitiveTypes.voidType },
            { typeof(bool), PrimitiveTypes.boolType },
            { typeof(TypeSymbol), TypeSymbol.typeSymbol }
        };

        private static Dictionary<string, TypeSymbol> typeSymbolLookupByName = typeSymbolLookup.Values.ToDictionary(v => v.Name);

        public static NSLFunction MakeSimple(
            string name,
            IEnumerable<TypeSymbol> arguments,
            TypeSymbol result,
            Func<IEnumerable<IValue>, Runner.State, IValue> impl,
            string? desc = null,
            bool targetMustBeMutable = false,
            bool useConstexpr = false
        ) => new NSLFunction(
            name,
            _ => new Signature
            {
                arguments = arguments,
                result = result,
                name = name,
                desc = desc,
                targetMustBeMutable = targetMustBeMutable,
                useConstexpr = useConstexpr
            },
            impl
        );

        public static void SetTypeLookup(Type type, TypeSymbol symbol)
        {
            typeSymbolLookup[type] = symbol;
            typeSymbolLookupByName[symbol.Name] = symbol;
        }

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

        public static TypeSymbol? LookupSymbol(string name)
        {
            if (typeSymbolLookupByName.TryGetValue(name, out TypeSymbol? foundSymbol))
            {
                return foundSymbol!;
            }
            else
            {
                return null;
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
                var values = argsEnum.Select(v => v.Value);
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

        public static (NSLFunction function, Signature signature) GetMatchingFunction(IEnumerable<NSLFunction> functions, IEnumerable<TypeSymbol?> _providedArgs, Func<int, IEnumerable<TypeSymbol>, TypeSymbol>? expandActions = null)
        {
            var failed = new List<Signature>();
            var failedAction = false;
            var providedArgs = _providedArgs.ToList();
            var foundFunctionIndex = -1;
            foreach (var function in functions)
            {
                foundFunctionIndex++;
                var arguments = providedArgs;
                var signature = function.GetSignature(arguments);
                if (!signature.useConstexpr && signature.result is ConstexprTypeSymbol constResult) signature.result = constResult.Base;
                var wantedArgs = signature.arguments.ToArray();

                if (signature.result == PrimitiveTypes.neverType)
                {
                    failed.Add(signature);
                    continue;
                }

                if (arguments.Count() != wantedArgs.Length)
                {
                    failed.Add(signature);
                    continue;
                }
                else
                {
                    var success = true;
                    for (int i = 0, len = arguments.Count(); i < len; i++)
                    {
                        var provided = arguments.ElementAt(i);
                        var wanted = wantedArgs[i];

                        if (!signature.useConstexpr)
                        {
                            if (provided is ConstexprTypeSymbol constProvided) provided = constProvided.Base;
                            if (wanted is ConstexprTypeSymbol constWanted && (i == 0 ? !signature.targetMustBeMutable : true)) wanted = constWanted.Base;
                        }

                        if (provided == wanted && provided != PrimitiveTypes.neverType)
                        {
                            continue;
                        }
                        else if ((provided == null || provided is ActionTypeSymbol) && wanted is ActionTypeSymbol action)
                        {
                            ActionTypeSymbol actionType;

                            if (provided == null)
                            {
                                if (expandActions == null) continue;
                                var resultType = expandActions(i, action.Arguments);
                                if (action.Result == PrimitiveTypes.voidType && resultType != PrimitiveTypes.neverType)
                                {
                                    actionType = new ActionTypeSymbol(action.Arguments, action.Result == PrimitiveTypes.voidType && resultType != PrimitiveTypes.neverType ? PrimitiveTypes.voidType : resultType);
                                    if (actionType == wanted)
                                    {
                                        actionType = new ActionTypeSymbol(action.Arguments, resultType);
                                        wanted = wantedArgs[i] = actionType;
                                    }
                                }
                                else
                                {
                                    actionType = new ActionTypeSymbol(action.Arguments, resultType);
                                }
                            }
                            else if (provided is ActionTypeSymbol providedAction)
                            {
                                if (providedAction.Result != action.Result && action.Result == PrimitiveTypes.voidType)
                                {
                                    actionType = new ActionTypeSymbol(providedAction.Arguments, PrimitiveTypes.voidType);
                                }
                                else
                                {
                                    actionType = providedAction;
                                }
                            }
                            else continue;

                            providedArgs[i] = actionType;
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
                        if (signature.name[0] != '$') signature.name += $"`{foundFunctionIndex}";
                        signature.arguments = wantedArgs;
                        if (signature.postProcess != null) signature.postProcess(ref signature);
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