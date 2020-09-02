using NSL.Runtime;
using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class EndInstruction : InstructionBase
    {
        override public int IndentDiff => -1;

        override public string ToString() => $"end";
        public override void Execute(Runner.State state)
        {
            throw new System.NotImplementedException();
        }

        public EndInstruction(Position start, Position end) : base(start, end) { }
    }
}