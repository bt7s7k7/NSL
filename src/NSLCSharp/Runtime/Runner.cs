using System.Collections.Generic;
using NSL.Executable;
using NSL.Types;

namespace NSL.Runtime
{
    public class Runner
    {
        public class Scope
        {
            protected Dictionary<string, NSLValue> variables = new Dictionary<string, NSLValue>();
            protected Scope? parent = null;

            public void Set(string name, NSLValue value)
            {
                variables[name] = value;
            }

            public NSLValue? Get(string name)
            {
                if (variables.TryGetValue(name, out NSLValue? value))
                {
                    return value;
                }
                else if (parent != null)
                {
                    return parent.Get(name);
                }
                else return null;
            }

            public Scope(Scope? parent)
            {
                this.parent = parent;
            }
        }


        public class State
        {
            protected Stack<Scope> scopeStack = new Stack<Scope>();
            protected Dictionary<string, Scope> scopes = new Dictionary<string, Scope>();

            public FunctionRegistry FunctionRegistry { get; }

            public void PushScope(string name, string? parentName)
            {
                if (scopes.ContainsKey(name)) throw new InternalNSLExcpetion($"Duplicate scope named '{name}'");
                Scope? parent = null;
                if (parentName != null)
                {
                    if (!scopes.TryGetValue(parentName, out parent))
                    {
                        throw new InternalNSLExcpetion($"Failed to find scope name '{parentName}' for parent of '{name}'");
                    }
                }
                var newScope = new Scope(parent);
                scopeStack.Push(newScope);
                scopes.Add(name, newScope);
            }

            public void PopScope()
            {
                scopeStack.Pop();
            }

            public Scope GetTopScope() => scopeStack.Peek();

            public State(FunctionRegistry functionRegistry)
            {
                FunctionRegistry = functionRegistry;
            }
        }

        protected FunctionRegistry functions;

        public NSLValue? Run(NSLProgram program)
        {
            var state = new State(functions);

            foreach (var inst in program)
            {
                inst.Execute(state);
            }

            return null;
        }

        public Runner(FunctionRegistry functions)
        {
            this.functions = functions;
        }
    }
}