using System.Collections;
using System.Collections.Generic;
using NSL.Executable;
using NSL.Types;

namespace NSL.Runtime
{
    public class ActionBuilder
    {
        protected List<IInstruction> instructions = new List<IInstruction>();
        protected IProgram.ReturnVariable? returnVariable;
        protected IProgram.ReturnVariable argumentVariable;
        protected Runner.Scope scope;
        public string ActionVarName { get; protected set; }
        public Runner.Scope ParentScope { get; protected set; }
        public int Depth { get; set; } = 1;

        public void Add(IInstruction instruction) => instructions.Add(instruction);

        public IValue Build() =>
            new ActionTypeSymbol(argumentVariable.type, returnVariable?.type ?? PrimitiveTypes.voidType)
            .Instantiate(new NSLAction(instructions, returnVariable, argumentVariable, scope));

        public ActionBuilder(Runner.Scope parentScope, IProgram.ReturnVariable? returnVariable, IProgram.ReturnVariable argumentVariable, string actionVarName, Runner.Scope scope)
        {
            ParentScope = parentScope;
            this.returnVariable = returnVariable;
            this.argumentVariable = argumentVariable;
            this.ActionVarName = actionVarName;
            this.scope = scope;
        }
    }
}