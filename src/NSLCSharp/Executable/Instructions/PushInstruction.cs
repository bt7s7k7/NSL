using NSL.Runtime;
using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class PushInstruction : InstructionBase
    {
        protected int id;
        protected int? parentId;

        override public int GetIndentDiff() => 1;
        override public string ToString() => $"push {id} ← {parentId?.ToString() ?? "null"}";
        public override void Execute(Runner.State state)
        {
            state.PushScope(id.ToString(), parentId?.ToString());
        }

        public PushInstruction(Position start, Position end, int id, int? parentId) : base(start, end)
        {
            this.id = id;
            this.parentId = parentId;
        }
    }
}