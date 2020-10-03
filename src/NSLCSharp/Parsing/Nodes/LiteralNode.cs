using NSL.Tokenization.General;
using NSL.Types;

namespace NSL.Parsing.Nodes
{
    public class LiteralNode : ASTNodeBase
    {
        public IValue value;

        public LiteralNode(IValue value, Position start, Position end) : base(start, end)
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
            return ToStringUtil.ToString(value);
        }
    }
}