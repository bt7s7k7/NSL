using System.Collections.Generic;

namespace NSL.Runtime
{
    public partial class Runner
    {
        public class State
        {
            protected Stack<Scope> scopeStack = new Stack<Scope>();
            protected Scope rootScope;
            public FunctionRegistry FunctionRegistry { get; }

            public void PushScope(string name, string? parentName)
            {
                if (name == "-1")
                {
                    if (parentName != null) throw new InternalNSLExcpetion("Root scope cannot have a parent");
                    scopeStack.Push(rootScope);
                    return;
                }

                Scope? parent = null;
                if (parentName != null)
                {
                    Scope currTop = GetTopScope();
                    if (currTop.Name != parentName)
                    {
                        throw new InternalNSLExcpetion($"Parent name must does not equal the name of the parent '{parentName}' != '{currTop.Name}'");
                    }
                    else
                    {
                        parent = currTop;
                    }
                }

                var newScope = new Scope(name, parent);
                scopeStack.Push(newScope);
            }

            public void PopScope()
            {
                scopeStack.Pop();
            }

            public Scope GetTopScope() => scopeStack.Peek();

            public State(FunctionRegistry functionRegistry, Scope rootScope)
            {
                FunctionRegistry = functionRegistry;
                this.rootScope = rootScope;
            }
        }
    }
}