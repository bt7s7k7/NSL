using NSL.Runtime;
using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class ActionInstruction : InstructionBase
    {
        override public int IndentDiff => 1;

        public string Name { get; protected set; }
        public IProgram.VariableDefinition? ReturnVariable { get; protected set; }
        public IProgram.VariableDefinition ArgumentVariable { get; protected set; }

        override public string ToString() => $"action {Name} : {(ReturnVariable == null ? "void" : $"{ReturnVariable.varName} = {ReturnVariable.type}")}";

        public override void Execute(Runner.State state)
        {
            throw new System.NotImplementedException();
        }

        public ActionInstruction(Position start, Position end, string name, IProgram.VariableDefinition? returnVariable, IProgram.VariableDefinition argumentVariable) : base(start, end)
        {
            Name = name;
            ReturnVariable = returnVariable;
            ArgumentVariable = argumentVariable;
        }
    }
}