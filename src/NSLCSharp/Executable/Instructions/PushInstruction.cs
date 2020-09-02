using NSL.Runtime;
using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class PushInstruction : InstructionBase
    {
        override public int IndentDiff => 1;

        protected string id;
        protected string? parentId;

        override public string ToString() => $"push {id} ← {parentId?.ToString() ?? "null"}";
        public override void Execute(Runner.State state)
        {
            state.PushScope(id, parentId);
        }

        public PushInstruction(Position start, Position end, int id, int? parentId) : base(start, end)
        {
            this.id = id.ToString();
            this.parentId = parentId?.ToString();
        }
    }
}