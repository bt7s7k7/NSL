using System;
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

        public NSLValue Run(IProgram program) => Run(program, new State(functions, RootScope, this));
        public NSLValue Run(IProgram program, State state)
        {
            NSLValue? result = null;

            var returnVariable = program.GetReturnVariable();
            if (returnVariable != null)
            {
                state.PushScope("-1", null);
                state.GetTopScope().Set(returnVariable.varName, returnVariable.type.Instantiate(null));
                state.PopScope();
            }

            ActionBuilder? buildingAction = null;

            foreach (var inst in program)
            {
                if (buildingAction == null)
                {
                    if (inst is ActionInstruction action)
                    {
                        buildingAction = new ActionBuilder(state.GetTopScope(), action.ReturnVariable, action.ArgumentVariable, action.Name, state.GetTopScope());
                    }
                    else
                    {
                        inst.Execute(state);
                    }
                }
                else
                {
                    if (inst is ActionInstruction)
                    {
                        buildingAction.Depth++;
                    }
                    else if (inst is EndInstruction)
                    {
                        buildingAction.Depth--;
                    }

                    if (buildingAction.Depth == 0)
                    {
                        state.GetTopScope().Set(buildingAction.ActionVarName, buildingAction.Build());
                        buildingAction = null;
                    }
                    else
                    {
                        buildingAction.Add(inst);
                    }
                }
            }

            if (returnVariable != null)
            {
                state.PushScope("-1", null);
                result = state.GetTopScope().Get(returnVariable.varName);
                state.PopScope();
            }

            return result ?? PrimitiveTypes.voidType.Instantiate(null);
        }

        public NSLValue RunAction(NSLAction action)
        {
            var state = new State(functions, RootScope, this);
            state.PushScope(action.Scope);
            return Run(action, state);
        }

        public Runner(FunctionRegistry functions)
        {
            this.functions = functions;
        }
    }
}