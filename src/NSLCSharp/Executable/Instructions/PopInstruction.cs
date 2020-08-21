using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class PopInstruction : InstructionBase
    {
        override public int GetIndentDiff() => -1;
        override public string ToString() => $"pop";

        public PopInstruction(Position start, Position end) : base(start, end) { }
    }
}