using NSL.Runtime;
using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class ActionInstruction : InstructionBase
    {
        public string Name { get; protected set; }
        public IProgram.ReturnVariable? ReturnVariable { get; protected set; }
        public IProgram.ReturnVariable ArgumentVariable { get; protected set; }

        override public int GetIndentDiff() => 1;
        override public string ToString() => $"action {Name} : {(ReturnVariable == null ? "void" : $"{ReturnVariable.varName} = {ReturnVariable.type}")}";
        public override void Execute(Runner.State state)
        {
            throw new System.NotImplementedException();
        }

        public ActionInstruction(Position start, Position end, string name, IProgram.ReturnVariable? returnVariable, IProgram.ReturnVariable argumentVariable) : base(start, end)
        {
            Name = name;
            ReturnVariable = returnVariable;
            ArgumentVariable = argumentVariable;
        }
    }
}