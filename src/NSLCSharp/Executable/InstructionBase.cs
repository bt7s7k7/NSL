using NSL.Runtime;
using NSL.Tokenization.General;

namespace NSL.Executable
{
    public interface IInstruction
    {
        public Position Start { get; }
        public Position End { get; }

        int GetIndentDiff();
        void Execute(Runner.State state);
    }

    public abstract class InstructionBase : IInstruction
    {
        public Position Start { get; protected set; }
        public Position End { get; protected set; }

        public virtual int GetIndentDiff() => 0;
        public abstract void Execute(Runner.State state);

        protected InstructionBase(Position start, Position end)
        {
            Start = start;
            End = end;
        }
    }
}