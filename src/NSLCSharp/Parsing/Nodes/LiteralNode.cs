using System.Text.Json;
using NSL.Tokenization.General;
using NSL.Types;

namespace NSL.Parsing.Nodes
{
    public class LiteralNode : ASTNodeBase
    {
        public NSLValue value;

        public LiteralNode(NSLValue value, Position start, Position end) : base(start, end)
        {
            this.value = value;
        }

        public override void Unbalanced(Parser.ParsingState state)
        {
            throw new InternalNSLExcpetion("Literal node cannot be push on the stack");
        }

        public override void Execute(Parser.ParsingState state)
        {
            throw new InternalNSLExcpetion("Literal node cannot be push on the stack");
        }

        override public string GetAdditionalInfo()
        {
            return JsonSerializer.Serialize(value);
        }
    }
}