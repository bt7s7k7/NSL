using NSL.Tokenization.General;

namespace NSL.Parsing.Nodes
{
    public class OperatorNode : ASTNodeBase
    {
        public string Match { get; protected set; }

        public OperatorNode(string match, Position start, Position end) : base(start, end)
        {
            Match = match;
        }

        public override void Unbalanced(Parser.ParsingState state)
        {
            throw new InternalNSLExcpetion("OperatorNode node cannot be pushed on the stack");
        }

        public override void Execute(Parser.ParsingState state)
        {
            throw new InternalNSLExcpetion("OperatorNode node cannot be pushed on the stack");
        }

    }
}