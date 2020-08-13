using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class EndInstruction : ExeInstruction
    {
        override public int GetIndentDiff() => -1;
        override public string ToString() => $"end";

        public EndInstruction(Position start, Position end) : base(start, end) { }
    }
}