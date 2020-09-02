using System.Collections;
using System.Collections.Generic;
using NSL.Executable;
using NSL.Runtime;

namespace NSL.Types
{
    public class NSLAction : IProgram, IEnumerable<IInstruction>
    {
        protected List<IInstruction> instructions;

        public IProgram.VariableDefinition? ReturnVariable { get; protected set; }
        public IProgram.VariableDefinition ArgumentVariable { get; protected set; }
        public Runner.Scope Scope { get; internal set; }

        public IEnumerator<IInstruction> GetEnumerator()
        {
            return ((IEnumerable<IInstruction>)instructions).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)instructions).GetEnumerator();
        }

        public NSLAction(List<IInstruction> instructions, IProgram.VariableDefinition? returnVariable, IProgram.VariableDefinition argumentVariable, Runner.Scope scope)
        {
            this.instructions = instructions;
            this.ReturnVariable = returnVariable;
            this.ArgumentVariable = argumentVariable;
            Scope = scope;
        }

        public IValue Invoke(Runner runner, IValue argument)
        {
            Scope.Set(ArgumentVariable.varName, argument);
            return runner.RunAction(this);
        }
    }
}