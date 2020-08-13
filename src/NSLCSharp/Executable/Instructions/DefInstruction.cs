using System.Text.Json;
using NSL.Tokenization.General;
using NSL.Types;

namespace NSL.Executable.Instructions
{
    public class DefInstruction : ExeInstruction
    {
        protected string varName;
        protected TypeSymbol type;
        protected object? value;

        override public int GetIndentDiff() => 0;
        override public string ToString() => $"def {varName} {type} {JsonSerializer.Serialize(value)}";

        public DefInstruction(Position start, Position end, string varName, TypeSymbol type, object? value) : base(start, end)
        {
            this.varName = varName;
            this.type = type;
            this.value = value;
        }
    }
}