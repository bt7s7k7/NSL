using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSL.Executable;
using NSL.Types;

namespace NSL.Runtime
{
    public class ActionBuilder
    {
        protected List<IInstruction> instructions = new List<IInstruction>();
        protected IProgram.VariableDefinition? returnVariable;
        protected IEnumerable<IProgram.VariableDefinition> argumentVariables;
        protected Runner.Scope scope;
        public string ActionVarName { get; protected set; }
        public Runner.Scope ParentScope { get; protected set; }
        public int Depth { get; set; } = 1;

        public void Add(IInstruction instruction) => instructions.Add(instruction);

        public IValue Build() =>
            new ActionTypeSymbol(argumentVariables.Select(v => v.type).ToArray(), returnVariable?.type ?? PrimitiveTypes.voidType)
            .Instantiate(new NSLAction(instructions, returnVariable, argumentVariables, scope));

        public ActionBuilder(Runner.Scope parentScope, IProgram.VariableDefinition? returnVariable, IEnumerable<IProgram.VariableDefinition> argumentVariables, string actionVarName, Runner.Scope scope)
        {
            ParentScope = parentScope;
            this.returnVariable = returnVariable;
            this.argumentVariables = argumentVariables;
            this.ActionVarName = actionVarName;
            this.scope = scope;
        }
    }
}