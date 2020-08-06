using System.Text.Json;
using NSL.Tokenization.General;

namespace NSL.Parsing.Nodes
{
    public class LiteralNode : ASTNode
    {
        public object value;

        public LiteralNode(object value, Position start, Position end) : base(start, end)
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