using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NSL.Executable;
using System.Text;
using NSL.Types;
using NSL.Types.Values;

namespace NSL
{
    public partial class FunctionRegistry
    {
        public static FunctionRegistry GetStandardFunctionRegistry()
        {
            var registry = new FunctionRegistry();

            // Basic constructs
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

                foreach (var (name, functions) in state.FunctionRegistry.functions)
                {
                    foreach (var function in functions)
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
                   new ActionTypeSymbol(PrimitiveTypes.voidType, PrimitiveTypes.voidType),
                   new ActionTypeSymbol(PrimitiveTypes.voidType, PrimitiveTypes.voidType)
                },
                result: PrimitiveTypes.voidType,
                impl: (argsEnum, state) =>
                {
                    if (
                        argsEnum.ElementAt(0).Value is bool value &&
                        argsEnum.ElementAt(1).Value is NSLAction thenAction &&
                        argsEnum.ElementAt(2).Value is NSLAction elseAction
                    )
                    {
                        if (value)
                        {
                            thenAction.Invoke(state.Runner, PrimitiveTypes.voidType.Instantiate(null));
                        }
                        else
                        {
                            elseAction.Invoke(state.Runner, PrimitiveTypes.voidType.Instantiate(null));
                        }
                        return PrimitiveTypes.voidType.Instantiate(null);
                    }
                    else throw new ImplWrongValueNSLException();
                },
                desc: "Runs the first action if the predicate is true, else runs second action"
            ));

            registry.Add(NSLFunction.MakeSimple(
                name: "while",
                arguments: new TypeSymbol[] {
                   new ActionTypeSymbol(PrimitiveTypes.voidType, PrimitiveTypes.boolType),
                   new ActionTypeSymbol(PrimitiveTypes.voidType, PrimitiveTypes.voidType)
                },
                result: PrimitiveTypes.voidType,
                impl: (argsEnum, state) =>
                {
                    if (
                        argsEnum.ElementAt(0).Value is NSLAction predicate &&
                        argsEnum.ElementAt(1).Value is NSLAction action
                    )
                    {
                        while (predicate.Invoke(state.Runner, PrimitiveTypes.voidType.Instantiate(null)).GetValue<bool>())
                        {
                            action.Invoke(state.Runner, PrimitiveTypes.voidType.Instantiate(null));
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
                        result = valueType
                    };
                }
                else
                {
                    return new NSLFunction.Signature
                    {
                        name = "set",
                        desc = desc,
                        arguments = new TypeSymbol[] { PrimitiveTypes.neverType, PrimitiveTypes.neverType },
                        result = PrimitiveTypes.neverType
                    };
                }
            }, (argsEnum, state) =>
            {
                argsEnum.ElementAt(0).Value = argsEnum.ElementAt(1).Value;
                return argsEnum.ElementAt(0);
            }));

            // Error handling
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

            // String
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

            // Numbers
            foreach (var operation in new (string name, System.Func<double, double, double> callback)[] {
                (name: "add", callback: (a, b) => a + b),
                (name: "sub", callback: (a, b) => a - b),
                (name: "mul", callback: (a, b) => a * b),
                (name: "div", callback: (a, b) => a / b),
                (name: "mod", callback: (a, b) => a % b),
                (name: "xor", callback: (a, b) => (double)((long)a ^ (long)b)),
                (name: "and", callback: (a, b) => (double)((long)a & (long)b)),
                (name: "or", callback: (a, b) => (double)((long)a | (long)b)),
                (name: "shr", callback: (a, b) => (double)((long)a >> (int)b)),
                (name: "shl", callback: (a, b) => (double)((long)a << (int)b)),
                (name: "pow", callback: (a, b) => Math.Pow(a, b)),
            })
            {
                registry.Add(NSLFunction.MakeAuto(operation.name, operation.callback));
            }

            foreach (var operation in new (string name, System.Func<double, double> callback)[] {
                (name: "neg", callback: (a) => -a),
                (name: "not", callback: (a) => ~(int)a),
                (name: "floor", callback: (a) => Math.Floor(a)),
                (name: "ceil", callback: (a) => Math.Ceiling(a)),
                (name: "sqrt", callback: (a) => Math.Sqrt(a)),
                (name: "sin", callback: (a) => Math.Sin(a)),
                (name: "cos", callback: (a) => Math.Cos(a)),
                (name: "tan", callback: (a) => Math.Tan(a)),
                (name: "abs", callback: (a) => Math.Abs(a)),
                (name: "sign", callback: (a) => Math.Sign(a)),
            })
            {
                registry.Add(NSLFunction.MakeAuto(operation.name, operation.callback));
            }

            registry.Add(NSLFunction.MakeAuto<Func<string, double>>("parseInt", text =>
            {
                try
                {
                    return Int32.Parse(text, CultureInfo.InvariantCulture);
                }
                catch (System.FormatException)
                {
                    return Double.NaN;
                }
            }));

            registry.Add(NSLFunction.MakeAuto<Func<string, double>>("parseFloat", text =>
            {
                try
                {
                    return Int32.Parse(text, CultureInfo.InvariantCulture);
                }
                catch (System.FormatException)
                {
                    return Double.NaN;
                }
            }));

            registry.Add(NSLFunction.MakeAuto<Func<double, double, bool>>("lt", (a, b) => a < b));
            registry.Add(NSLFunction.MakeAuto<Func<double, double, bool>>("gt", (a, b) => a > b));
            registry.Add(NSLFunction.MakeAuto<Func<double, double, bool>>("lte", (a, b) => a <= b));
            registry.Add(NSLFunction.MakeAuto<Func<double, double, bool>>("gte", (a, b) => a >= b));
            registry.Add(NSLFunction.MakeAuto<Func<double, double, bool>>("eq", (a, b) => a == b));
            registry.Add(NSLFunction.MakeAuto<Func<double, double, bool>>("neq", (a, b) => a != b));

            foreach (var (name, verb, callback) in new (string name, string verb, Func<double, double> callback)[] {
                (name: "inc", verb: "Increments", callback: v => v + 1),
                (name: "dec", verb: "Decrements", callback: v => v - 1)
            })
            {
                registry.Add(new NSLFunction(name + "Post", argsEnum =>
                {
                    var desc = verb + " the value of the reference, returns the old value";
                    return new NSLFunction.Signature
                    {
                        name = name + "Post",
                        desc = desc,
                        arguments = new TypeSymbol[] { PrimitiveTypes.numberType },
                        result = PrimitiveTypes.numberType
                    };
                }, (argsEnum, state) =>
                {
                    var value = (double)argsEnum.ElementAt(0).Value!;
                    argsEnum.ElementAt(0).Value = callback(value);
                    return PrimitiveTypes.boolType.Instantiate(value);
                }));

                registry.Add(new NSLFunction(name + "Prev", argsEnum =>
                {
                    var desc = verb + " the value of the reference";
                    return new NSLFunction.Signature
                    {
                        name = name + "Prev",
                        desc = desc,
                        arguments = new TypeSymbol[] { PrimitiveTypes.numberType },
                        result = PrimitiveTypes.numberType
                    };
                }, (argsEnum, state) =>
                {
                    var value = (double)argsEnum.ElementAt(0).Value!;
                    argsEnum.ElementAt(0).Value = callback(value);
                    return PrimitiveTypes.boolType.Instantiate(argsEnum.ElementAt(0).Value);
                }));
            }

            // Booleans
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

            // Arrays
            registry.Add(new NSLFunction(
                name: "arr",
                signatureGenerator: argsEnum => new NSLFunction.Signature
                {
                    name = "arr",
                    arguments = argsEnum.Select(v => argsEnum.First() ?? PrimitiveTypes.neverType),
                    result = argsEnum.Count() != 0 ? argsEnum.First()?.ToArray() ?? PrimitiveTypes.neverType : PrimitiveTypes.neverType,
                    desc = "Creates a new array with the arguments as elements"
                },
                impl: (argsEnum, state) => argsEnum.First().TypeSymbol.ToArray().Instantiate(argsEnum.Select(v => v.Value).ToArray())
            ));

            registry.Add(new NSLFunction(
                name: "filter",
                signatureGenerator: argsEnum =>
                {
                    if (
                        argsEnum.Count() == 2 &&
                        argsEnum.ElementAt(0) is ArrayTypeSymbol array &&
                        array.ItemType is TypeSymbol itemType &&
                        (argsEnum.ElementAt(1) == null || argsEnum.ElementAt(1) == new ActionTypeSymbol(itemType, PrimitiveTypes.boolType))
                    )
                    {
                        return new NSLFunction.Signature
                        {
                            name = "filter",
                            arguments = new TypeSymbol[] { array, new ActionTypeSymbol(itemType, PrimitiveTypes.boolType) },
                            result = array
                        };
                    }
                    else
                    {
                        return new NSLFunction.Signature
                        {
                            name = "filter",
                            arguments = new[] { PrimitiveTypes.neverType, new ActionTypeSymbol(PrimitiveTypes.neverType, PrimitiveTypes.neverType) },
                            result = PrimitiveTypes.neverType
                        };
                    }
                },
                impl: (argsEnum, state) =>
                {
                    var arrayValue = argsEnum.ElementAt(0);
                    var actionValue = argsEnum.ElementAt(1);
                    if (
                        arrayValue.Value is IEnumerable<object> array &&
                        actionValue.Value is NSLAction action &&
                        arrayValue.TypeSymbol is ArrayTypeSymbol arrayType
                    )
                    {
                        var itemType = arrayType.ItemType;
                        var resultArray = array.Where(value =>
                        {
                            var result = action.Invoke(state.Runner, itemType.Instantiate(value)).Value;
                            if (result == null) throw new NullReferenceException();
                            return (bool)result;
                        }).ToArray();

                        return arrayType.Instantiate(resultArray);
                    }
                    else throw new ImplWrongValueNSLException();
                }
            ));

            registry.Add(new NSLFunction(
                name: "foreach",
                signatureGenerator: argsEnum =>
                {
                    if (
                        argsEnum.Count() == 2 &&
                        argsEnum.ElementAt(0) is ArrayTypeSymbol array &&
                        array.ItemType is TypeSymbol itemType &&
                        (argsEnum.ElementAt(1) == null || argsEnum.ElementAt(1) == new ActionTypeSymbol(itemType, PrimitiveTypes.voidType))
                    )
                    {
                        return new NSLFunction.Signature
                        {
                            name = "foreach",
                            arguments = new TypeSymbol[] { array, new ActionTypeSymbol(itemType, PrimitiveTypes.voidType) },
                            result = PrimitiveTypes.voidType
                        };
                    }
                    else
                    {
                        return new NSLFunction.Signature
                        {
                            name = "foreach",
                            arguments = new[] { PrimitiveTypes.neverType, new ActionTypeSymbol(PrimitiveTypes.neverType, PrimitiveTypes.neverType) },
                            result = PrimitiveTypes.voidType
                        };
                    }
                },
                impl: (argsEnum, state) =>
                {
                    var arrayValue = argsEnum.ElementAt(0);
                    var actionValue = argsEnum.ElementAt(1);
                    if (
                        arrayValue.Value is IEnumerable<object> array &&
                        actionValue.Value is NSLAction action &&
                        arrayValue.TypeSymbol is ArrayTypeSymbol arrayType
                    )
                    {
                        var itemType = arrayType.ItemType;

                        foreach (var item in array)
                        {
                            action.Invoke(state.Runner, itemType.Instantiate(item));
                        }

                        return PrimitiveTypes.voidType.Instantiate(null);
                    }
                    else throw new ImplWrongValueNSLException();
                }
            ));

            registry.Add(new NSLFunction("push", argsEnum =>
            {
                var desc = "Pushes an element to the end of the array in place";
                if (
                    argsEnum.Count() == 2 &&
                    argsEnum.ElementAt(0) is ArrayTypeSymbol arrayType
                ) return new NSLFunction.Signature
                {
                    name = "push",
                    desc = desc,
                    arguments = new TypeSymbol[] { arrayType, arrayType.ItemType },
                    result = arrayType
                };
                else return new NSLFunction.Signature
                {
                    name = "push",
                    desc = desc,
                    arguments = new TypeSymbol[] { PrimitiveTypes.neverType.ToArray(), PrimitiveTypes.neverType },
                    result = PrimitiveTypes.neverType
                };
            }, (argsEnum, state) =>
            {
                var arrayValue = argsEnum.ElementAt(0);
                if (
                    arrayValue.Value is IEnumerable<object> array &&
                    argsEnum.ElementAt(1).Value is object item
                )
                {
                    if (array is IList<object> list)
                    {
                        try
                        {
                            list.Add(item);
                        }
                        catch (NotSupportedException)
                        {
                            var result = array.Concat(new object[] { item }).ToList();
                            arrayValue.Value = result;
                        }
                    }
                    else
                    {
                        var result = array.Concat(new object[] { item }).ToList();
                        arrayValue.Value = result;
                    }

                    return arrayValue;
                }
                else throw new ImplWrongValueNSLException();
            }));

            registry.Add(new NSLFunction("index", argsEnum =>
            {
                var desc = "Get element at index in an array";
                if (
                    argsEnum.Count() == 2 &&
                    argsEnum.ElementAt(0) is ArrayTypeSymbol arrayType
                ) return new NSLFunction.Signature
                {
                    name = "index",
                    desc = desc,
                    arguments = new TypeSymbol[] { arrayType, PrimitiveTypes.numberType },
                    result = arrayType.ItemType
                };
                else return new NSLFunction.Signature
                {
                    name = "index",
                    desc = desc,
                    arguments = new TypeSymbol[] { PrimitiveTypes.neverType.ToArray(), PrimitiveTypes.numberType },
                    result = PrimitiveTypes.neverType
                };
            }, (argsEnum, state) =>
            {
                var arrayValue = argsEnum.ElementAt(0);
                if (
                    arrayValue.Value is IEnumerable<object> array &&
                    arrayValue.TypeSymbol is ArrayTypeSymbol arrayType &&
                    argsEnum.ElementAt(1).Value is double index
                )
                {
                    void throwError() => throw new UserNSLException("Index must be greather than zero and less than the length of the array");

                    return new CallbackValue(() =>
                    {
                        try
                        {

                            return array.ElementAt((int)index);
                        }
                        catch (IndexOutOfRangeException) { throwError(); return null; }
                        catch (ArgumentOutOfRangeException) { throwError(); return null; }

                    }, value =>
                    {
                        try
                        {
                            if (array is IList<object> list)
                            {
                                list[(int)index] = value!;
                            }
                            else
                            {
                                arrayValue.Value = array.Select((v, i) => i == (int)index ? value : v).ToArray();
                            }
                        }
                        catch (IndexOutOfRangeException) { throwError(); }
                        catch (ArgumentOutOfRangeException) { throwError(); }
                    }, arrayType.ItemType);
                }
                else throw new ImplWrongValueNSLException();
            }));

            registry.Add(new NSLFunction("length", argsEnum =>
            {
                var desc = "Get the length of an array";
                if (
                    argsEnum.Count() == 1 &&
                    argsEnum.ElementAt(0) is ArrayTypeSymbol arrayType
                ) return new NSLFunction.Signature
                {
                    name = "length",
                    desc = desc,
                    arguments = new TypeSymbol[] { arrayType },
                    result = PrimitiveTypes.numberType
                };
                else return new NSLFunction.Signature
                {
                    name = "length",
                    desc = desc,
                    arguments = new TypeSymbol[] { PrimitiveTypes.neverType.ToArray() },
                    result = PrimitiveTypes.numberType
                };
            }, (argsEnum, state) =>
            {
                var arrayValue = argsEnum.ElementAt(0);
                if (
                    arrayValue.Value is IEnumerable<object> array
                )
                {
                    return PrimitiveTypes.numberType.Instantiate(array.Count());
                }
                else throw new ImplWrongValueNSLException();
            }));

            registry.Add(new NSLFunction("contains", argsEnum =>
            {
                var desc = "Tests if the array contains the specified element";
                if (
                    argsEnum.Count() == 2 &&
                    argsEnum.ElementAt(0) is ArrayTypeSymbol arrayType
                ) return new NSLFunction.Signature
                {
                    name = "contains",
                    desc = desc,
                    arguments = new TypeSymbol[] { arrayType, arrayType.ItemType },
                    result = PrimitiveTypes.boolType
                };
                else return new NSLFunction.Signature
                {
                    name = "contains",
                    desc = desc,
                    arguments = new TypeSymbol[] { PrimitiveTypes.neverType.ToArray(), PrimitiveTypes.neverType },
                    result = PrimitiveTypes.boolType
                };
            }, (argsEnum, state) =>
            {
                var arrayValue = argsEnum.ElementAt(0);
                if (
                    arrayValue.Value is IEnumerable<object> array &&
                    argsEnum.ElementAt(1).Value is object item
                )
                {
                    return PrimitiveTypes.boolType.Instantiate(array.Contains(item));
                }
                else throw new ImplWrongValueNSLException();
            }));

            registry.Add(NSLFunction.MakeAuto<Func<double, IEnumerable<object>>>("range", (len) =>
            {
                var length = (int)(len);
                if (length < 0) throw new UserNSLException($"Length ({length}) must be greater than zero");
                return new object[(int)len].Select((v, i) => (object)((double)i)).ToArray();
            }, new Dictionary<int, TypeSymbol> { { -1, PrimitiveTypes.numberType.ToArray() } }, "Creates an array of the specified length"));

            registry.Add(NSLFunction.MakeAuto<Func<double, double, IEnumerable<object>>>("range", (val, len) =>
            {
                var length = (int)(len - val) + 1;
                if (length <= 0) throw new UserNSLException($"Value ({val}) must not be less than length ({len})");
                return new object[length].Select((v, i) => (object)(val + (double)i)).ToArray();
            }, new Dictionary<int, TypeSymbol> { { -1, PrimitiveTypes.numberType.ToArray() } }, "Creates an array of the specified length, starting with the start value"));

            // Operators
            registry.AddOperator("_->_", "index", -2);

            registry.AddOperator("_++", "incPost", -1);
            registry.AddOperator("_--", "decPost", -1);
            registry.AddOperator("_~~", "invPost", -1);
            registry.AddOperator("++_", "incPrev", -1);
            registry.AddOperator("--_", "decPrev", -1);
            registry.AddOperator("~~_", "invPrev", -1);

            registry.AddOperator("!_", "not", 0);
            registry.AddOperator("~_", "not", 0);

            registry.AddOperator("_**_", "pow", 1);

            registry.AddOperator("_*_", "mul", 1);
            registry.AddOperator("_/_", "div", 1);
            registry.AddOperator("_%_", "mod", 1);

            registry.AddOperator("_+_", "add", 4);
            registry.AddOperator("_-_", "sub", 4);

            registry.AddOperator("_<<_", "shl", 5);
            registry.AddOperator("_>>_", "shr", 5);
            registry.AddOperator("-_", "neg", 5);

            registry.AddOperator("_>_", "gt", 6);
            registry.AddOperator("_<_", "lt", 6);
            registry.AddOperator("_>=_", "gte", 6);
            registry.AddOperator("_<=_", "lte", 6);

            registry.AddOperator("_==_", "eq", 7);
            registry.AddOperator("_!=_", "neq", 7);

            registry.AddOperator("_&_", "and", 8);

            registry.AddOperator("_^_", "xor", 9);

            // Skip bitwise or because it collides with pipe, you can just use logical or

            registry.AddOperator("_&&_", "and", 11);

            registry.AddOperator("_||_", "or", 12);

            registry.AddOperator("_=_", "set", 13);
            registry.AddOperator("_~>_", "set", 13, reverse: true);


            return registry;
        }
    }
}