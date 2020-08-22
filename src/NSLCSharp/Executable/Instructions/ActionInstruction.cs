using NSL.Runtime;
using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class ActionInstruction : InstructionBase
    {
        protected string name;

        override public int GetIndentDiff() => 1;
        override public string ToString() => $"action {name}";
        public override void Execute(Runner.State state)
        {
            throw new System.NotImplementedException();
        }

        public ActionInstruction(Position start, Position end, string name) : base(start, end)
        {
            this.name = name;
        }
    }
}