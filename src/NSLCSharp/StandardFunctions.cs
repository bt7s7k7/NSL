using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NSL.Types;

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
                return new NSLFunction.Signature { name = "echo", arguments = new TypeSymbol[] { arg }, result = arg };
            }, argsEnum => argsEnum.First()));

            registry.Add(new NSLFunction(
                name: "void",
                signatureGenerator: argsEnum => new NSLFunction.Signature { name = "void", arguments = argsEnum.Select(v => v ?? PrimitiveTypes.neverType), result = PrimitiveTypes.voidType },
                impl: argsEnum => PrimitiveTypes.voidType.Instantiate(null)
            ));

            // String
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

            registry.Add(NSLFunction.MakeAuto<Func<string, string, bool>>("stringContains", (text, substr) => text.Contains(substr)));
            registry.Add(NSLFunction.MakeAuto<Func<string, string, double>>("stringIndexOf", (text, substr) => text.IndexOf(substr)));
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

            // Arrays
            registry.Add(new NSLFunction(
                name: "arr",
                signatureGenerator: argsEnum => new NSLFunction.Signature
                {
                    name = "arr",
                    arguments = argsEnum.Select(v => argsEnum.First() ?? PrimitiveTypes.neverType),
                    result = argsEnum.First()?.ToArray() ?? PrimitiveTypes.neverType
                },
                impl: argsEnum => argsEnum.First().GetTypeSymbol().ToArray().Instantiate(argsEnum.Select(v => v.GetValue()).ToArray())
            ));

            registry.Add(new NSLFunction(
                name: "filter",
                signatureGenerator: argsEnum =>
                {
                    if (
                        argsEnum.Count() == 2 &&
                        argsEnum.ElementAt(0) is ArrayTypeSymbol array &&
                        array.GetItemType() is TypeSymbol itemType &&
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
                impl: argsEnum => PrimitiveTypes.voidType.Instantiate(null)
            ));

            return registry;
        }
    }
}