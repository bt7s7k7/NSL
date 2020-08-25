using System.Collections.Generic;
using System.Linq;
using NSL.Executable;
using NSL.Executable.Instructions;
using NSL.Types;

namespace NSL.Runtime
{
    public partial class Runner
    {
        protected FunctionRegistry functions;
        public Scope RootScope { get; protected set; } = new Scope("-1", null);

        public NSLValue Run(NSLProgram program)
        {
            NSLValue? result = null;
            var state = new State(functions, RootScope);

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

        public Runner(FunctionRegistry functions)
        {
            this.functions = functions;
        }
    }
}