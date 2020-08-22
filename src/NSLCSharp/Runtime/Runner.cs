using System.Collections.Generic;
using System.Linq;
using NSL.Executable;
using NSL.Executable.Instructions;
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

            public IEnumerable<(string key, NSLValue value)> GetAllVariables() => variables.Select(v => (v.Key, v.Value));

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
                if (name != "-1" && scopes.ContainsKey(name)) throw new InternalNSLExcpetion($"Duplicate scope named '{name}'");
                Scope? parent = null;
                if (parentName != null)
                {
                    if (!scopes.TryGetValue(parentName, out parent))
                    {
                        throw new InternalNSLExcpetion($"Failed to find scope name '{parentName}' for parent of '{name}'");
                    }
                }
                var newScope = scopes.ContainsKey(name) ? scopes[name] : new Scope(parent);
                scopeStack.Push(newScope);
                scopes[name] = newScope;
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
        protected State state;

        public NSLValue Run(NSLProgram program)
        {
            NSLValue? result = null;

            var returnVariable = program.GetReturnVariable();
            if (returnVariable != null)
            {
                state.PushScope("-1", null);
                state.GetTopScope().Set(returnVariable.varName, returnVariable.type.Instantiate(null));
                state.PopScope();
            }

            foreach (var inst in program)
            {
                inst.Execute(state);
            }

            if (returnVariable != null)
            {
                state.PushScope("-1", null);
                result = state.GetTopScope().Get(returnVariable.varName);
                state.PopScope();
            }

            return result ?? PrimitiveTypes.voidType.Instantiate(null);
        }

        public Scope GetRootScope()
        {
            state.PushScope("-1", null);
            var ret = state.GetTopScope();
            state.PopScope();
            return ret;
        }

        public Runner(FunctionRegistry functions)
        {
            this.functions = functions;
            this.state = new State(functions);
        }
    }
}