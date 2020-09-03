using System;
using System.Collections.Generic;
using System.Linq;
using NSL.Types;
using NSL.Types.Values;

namespace NSL
{
    public partial class FunctionRegistry
    {
        private static void RegisterArray(FunctionRegistry registry)
        {
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
        }
    }
}