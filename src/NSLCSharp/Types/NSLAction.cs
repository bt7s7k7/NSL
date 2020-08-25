using System.Collections;
using System.Collections.Generic;
using NSL.Executable;
using NSL.Runtime;

namespace NSL.Types
{
    public class NSLAction : IProgram, IEnumerable<IInstruction>
    {
        protected List<IInstruction> instructions;

        public IProgram.ReturnVariable? ReturnVariable { get; protected set; }
        public IProgram.ReturnVariable ArgumentVariable { get; protected set; }
        public Runner.Scope Scope { get; internal set; }

        public IEnumerator<IInstruction> GetEnumerator()
        {
            return ((IEnumerable<IInstruction>)instructions).GetEnumerator();
        }

        public IProgram.ReturnVariable? GetReturnVariable() => ReturnVariable;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)instructions).GetEnumerator();
        }

        public NSLAction(List<IInstruction> instructions, IProgram.ReturnVariable? returnVariable, IProgram.ReturnVariable argumentVariable, Runner.Scope scope)
        {
            this.instructions = instructions;
            this.ReturnVariable = returnVariable;
            this.ArgumentVariable = argumentVariable;
            Scope = scope;
        }
    }
}