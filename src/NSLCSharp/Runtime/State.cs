using System.Collections.Generic;

namespace NSL.Runtime
{
    public partial class Runner
    {
        public class State
        {
            protected Stack<Scope> scopeStack = new Stack<Scope>(10);
            protected Scope rootScope;
            protected Scope topScope;
            public FunctionRegistry FunctionRegistry { get; }
            public Runner Runner { get; protected set; }

            public void PushScope(string name, string parentName)
            {
                if (name == "-1")
                {
                    if (parentName != null) throw new InternalNSLExcpetion("Root scope cannot have a parent");
                    PushScope(rootScope);
                    return;
                }

                Scope parent = null;
                if (parentName != null)
                {
                    Scope currTop = TopScope;
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
                PushScope(newScope);
            }

            public void PushScope(Scope scope)
            {
                scopeStack.Push(scope);
                topScope = scope;
            }

            public void PopScope()
            {
                scopeStack.Pop();
                if (scopeStack.TryPeek(out Scope top))
                {
                    topScope = top;
                }
                else
                {
                    topScope = rootScope;
                }
            }

            public Scope TopScope => topScope;

            public State(FunctionRegistry functionRegistry, Scope rootScope, Runner runner)
            {
                FunctionRegistry = functionRegistry;
                this.rootScope = rootScope;
                topScope = rootScope;
                Runner = runner;
            }
        }
    }
}