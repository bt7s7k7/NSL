using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSL.Executable;
using NSL.Runtime;

namespace NSL.Types
{
    public class NSLAction : IProgram, IEnumerable<IInstruction>
    {
        protected List<IInstruction> instructions;

        public IProgram.VariableDefinition ReturnVariable { get; protected set; }
        public IEnumerable<IProgram.VariableDefinition> ArgumentVariables { get; protected set; }
        public Runner.Scope Scope { get; internal set; }

        public IEnumerator<IInstruction> GetEnumerator()
        {
            return ((IEnumerable<IInstruction>)instructions).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)instructions).GetEnumerator();
        }

        public NSLAction(List<IInstruction> instructions, IProgram.VariableDefinition returnVariable, IEnumerable<IProgram.VariableDefinition> argumentVariables, Runner.Scope scope)
        {
            this.instructions = instructions;
            this.ReturnVariable = returnVariable;
            this.ArgumentVariables = argumentVariables;
            Scope = scope;
        }

        public IValue Invoke(Runner runner, IEnumerable<IValue> arguments)
        {
            if (arguments.Count() < ArgumentVariables.Count()) throw new ActionCallNSLException("Length of arguments doesn't match the length of the action's arguments");
            for (int i = 0, len = ArgumentVariables.Count(); i < len; i++)
            {
                var name = ArgumentVariables.ElementAt(i).varName;
                var value = arguments.ElementAt(i);

                Scope.Set(name, value);
            }
            return runner.RunAction(this);
        }
    }
}