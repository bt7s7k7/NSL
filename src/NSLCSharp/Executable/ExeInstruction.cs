using NSL.Tokenization.General;

namespace NSL.Executable
{
    public abstract class ExeInstruction
    {
        public Position start;
        public Position end;
        public virtual int GetIndentDiff() => 0;

        protected ExeInstruction(Position start, Position end)
        {
            this.start = start;
            this.end = end;
        }
    }
}