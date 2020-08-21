using NSL.Runtime;
using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class PopInstruction : InstructionBase
    {
        override public int GetIndentDiff() => -1;
        override public string ToString() => $"pop";
        public override void Execute(Runner.State state)
        {
            state.PopScope();
        }

        public PopInstruction(Position start, Position end) : base(start, end) { }
    }
}