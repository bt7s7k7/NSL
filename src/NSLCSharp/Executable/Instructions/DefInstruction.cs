using NSL.Runtime;
using NSL.Tokenization.General;
using NSL.Types;

namespace NSL.Executable.Instructions
{
    public class DefInstruction : InstructionBase
    {
        override public int IndentDiff => 0;

        protected string varName;
        protected TypeSymbol type;
        protected object value;

        override public string ToString() => $"def {varName} {type} {ToStringUtil.ToString(value)}";
        public override void Execute(Runner.State state)
        {
            state.TopScope.Set(varName, type.Instantiate(value));
        }

        public DefInstruction(Position start, Position end, string varName, TypeSymbol type, object value) : base(start, end)
        {
            this.varName = varName;
            this.type = type;
            this.value = value;
        }
    }
}