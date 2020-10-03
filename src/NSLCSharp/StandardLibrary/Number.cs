using System;
using System.Globalization;
using System.Linq;
using NSL.Types;

namespace NSL
{
    public partial class FunctionRegistry
    {
        private static void RegisterNumber(FunctionRegistry registry)
        {
            foreach (var (name, callback) in new (string name, System.Func<double, double, double> callback)[] {
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
                registry.Add(NSLFunction.MakeAuto(name, callback));

                registry.Add(NSLFunction.MakeSimple(name + "Set", new TypeSymbol[] { PrimitiveTypes.numberType, PrimitiveTypes.numberType }, PrimitiveTypes.numberType, (argsEnum, state) =>
                {
                    var targetValue = argsEnum.ElementAt(0);
                    if (
                        targetValue.Value is double target &&
                        argsEnum.ElementAt(1).Value is double operand
                    )
                    {
                        targetValue.Value = callback(target, operand);
                        return targetValue;
                    }
                    else throw new ImplWrongValueNSLException();
                }, targetMustBeMutable: true));
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
                    return Convert.ToInt32(text, 10);
                }
                catch (ArgumentOutOfRangeException) { return Double.NaN; }
                catch (ArgumentException err) { throw new UserNSLException(err.Message); }
                catch (FormatException) { return Double.NaN; }
                catch (OverflowException) { return Double.NaN; }
            }));

            registry.Add(NSLFunction.MakeAuto<Func<string, double, double>>("parseInt", (text, baseDouble) =>
            {
                var baseInt = (int)baseDouble;
                var mul = 1;
                if (text[0] == '-')
                {
                    text = text.Substring(1);
                    mul = -1;
                }
                try
                {
                    return Convert.ToInt32(text, baseInt) * mul;
                }
                catch (ArgumentOutOfRangeException) { return Double.NaN; }
                catch (ArgumentException err) { throw new UserNSLException(err.Message); }
                catch (FormatException) { return Double.NaN; }
                catch (OverflowException) { return Double.NaN; }
            }));


            registry.Add(NSLFunction.MakeAuto<Func<string, double>>("parseFloat", text =>
            {
                try
                {
                    return Convert.ToDouble(text, CultureInfo.InvariantCulture);
                }
                catch (ArgumentOutOfRangeException) { return Double.NaN; }
                catch (ArgumentException err) { throw new UserNSLException(err.Message); }
                catch (FormatException) { return Double.NaN; }
                catch (OverflowException) { return Double.NaN; }
            }));

            registry.Add(NSLFunction.MakeAuto<Func<double, double, bool>>("lt", (a, b) => a < b));
            registry.Add(NSLFunction.MakeAuto<Func<double, double, bool>>("gt", (a, b) => a > b));
            registry.Add(NSLFunction.MakeAuto<Func<double, double, bool>>("lte", (a, b) => a <= b));
            registry.Add(NSLFunction.MakeAuto<Func<double, double, bool>>("gte", (a, b) => a >= b));
            registry.Add(NSLFunction.MakeAuto<Func<double, double, bool>>("eq", (a, b) => (Double.IsNaN(a) && Double.IsNaN(b)) || a == b));
            registry.Add(NSLFunction.MakeAuto<Func<double, double, bool>>("neq", (a, b) => !(Double.IsNaN(a) && Double.IsNaN(b)) && a != b));

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
                        result = PrimitiveTypes.numberType,
                        targetMustBeMutable = true
                    };
                }, (argsEnum, state) =>
                {
                    var value = (double)argsEnum.ElementAt(0).Value;
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
                        result = PrimitiveTypes.numberType,
                        targetMustBeMutable = true
                    };
                }, (argsEnum, state) =>
                {
                    var value = (double)argsEnum.ElementAt(0).Value;
                    argsEnum.ElementAt(0).Value = callback(value);
                    return PrimitiveTypes.boolType.Instantiate(argsEnum.ElementAt(0).Value);
                }));
            }
        }
    }
}