using NSL.Tokenization.General;

namespace NSL.Executable
{
    public interface IInstruction
    {
        public Position Start { get; }
        public Position End { get; }

        int GetIndentDiff();
    }

    public abstract class InstructionBase : IInstruction
    {
        public Position Start { get; protected set; }
        public Position End { get; protected set; }
        public virtual int GetIndentDiff() => 0;

        protected InstructionBase(Position start, Position end)
        {
            Start = start;
            End = end;
        }
    }
}