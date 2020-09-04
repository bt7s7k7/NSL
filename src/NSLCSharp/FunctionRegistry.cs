using System;
using System.Collections.Generic;
using System.Linq;
using NSL.Types;

namespace NSL
{
    public partial class FunctionRegistry
    {
        public class Operator
        {
            public enum Type
            {
                Prefix = 1,
                Suffix = 2,
                Infix = Prefix | Suffix
            }

            public readonly string match;
            public readonly string definition;
            public readonly string function;
            public readonly int priority;
            public readonly Type type;
            public readonly bool reverse;

            public Operator(string match, string definition, string function, int priority, bool reverse)
            {
                this.match = match;
                this.definition = definition;
                this.function = function;
                this.priority = priority;

                var type = 0;

                if (definition[0] == '_') type |= (int)Type.Suffix;
                if (definition.Last() == '_') type |= (int)Type.Prefix;

                if (type == 0) throw new OperatorNSLException($"Definition ({definition}) specifies neither prefix or suffix, atleast one required");

                this.type = (Type)type;
                this.reverse = reverse;
            }
        }

        protected Dictionary<string, List<NSLFunction>> functions = new Dictionary<string, List<NSLFunction>>();
        protected Dictionary<string, NSLFunction> specificFunctions = new Dictionary<string, NSLFunction>();
        protected List<Operator> operators = new List<Operator>();
        public IEnumerable<Operator> Operators { get => operators; }

        public void Add(NSLFunction function)
        {
            if (!functions.ContainsKey(function.Name))
            {
                functions[function.Name] = new List<NSLFunction> { function };
                specificFunctions.Add(function.Name + "`" + 0, function);
            }
            else
            {
                List<NSLFunction> functionsList = functions[function.Name];
                specificFunctions.Add(function.Name + "`" + functionsList.Count, function);
                functionsList.Add(function);
            }
        }

        public IEnumerable<NSLFunction> Find(string name)
        {
            if (functions.TryGetValue(name, out List<NSLFunction>? functionsList))
            {
                return functionsList;
            }
            else return new List<NSLFunction>();
        }

        public NSLFunction FindSpecific(string name)
        {
            if (specificFunctions.TryGetValue(name, out NSLFunction? function))
            {
                return function;
            }
            else throw new InternalNSLExcpetion($"Specific function {name} not found");
        }

        public static NSLFunction MakeVariableDefinitionFunction(string varName, bool isConst)
        {
            if (isConst) return new NSLFunction(varName, argsEnum =>
          {
              var type = (argsEnum.Count() == 0 ? null : argsEnum.First()) ?? PrimitiveTypes.neverType;
              return new NSLFunction.Signature
              {
                  name = varName,
                  arguments = new TypeSymbol[] { type },
                  result = type,
                  useConstexpr = true
              };
          }, (argsEnum, state) =>
          {
              return argsEnum.First();
          });
            else return new NSLFunction(varName, argsEnum =>
           {
               var type = (argsEnum.Count() == 0 ? null : argsEnum.First()) ?? PrimitiveTypes.neverType;
               if (type is ConstexprTypeSymbol constType) type = constType.Base;
               return new NSLFunction.Signature
               {
                   name = varName,
                   arguments = new TypeSymbol[] { type },
                   result = type
               };
           }, (argsEnum, state) =>
           {
               return argsEnum.First();
           });
        }

        public void AddOperator(string definition, string function, int priority, bool reverse = false)
        {
            var match = definition.Replace("_", "");
            operators.Add(new Operator(match, definition, function, priority, reverse));

            operators.Sort((a, b) => a.priority - b.priority);
        }
    }
}