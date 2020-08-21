using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class EndInstruction : InstructionBase
    {
        override public int GetIndentDiff() => -1;
        override public string ToString() => $"end";

        public EndInstruction(Position start, Position end) : base(start, end) { }
    }
}