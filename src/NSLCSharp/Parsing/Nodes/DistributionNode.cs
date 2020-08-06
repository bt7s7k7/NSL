using NSL.Tokenization.General;

namespace NSL.Parsing.Nodes
{
    public class DistributionNode : ASTNode
    {
        public DistributionNode(Position start, Position end) : base(start, end) { }

        public override void Unbalanced(Parser.ParsingState state)
        {
            throw new InternalNSLExcpetion("ForEach node cannot be push on the stack");
        }

        public override void Execute(Parser.ParsingState state)
        {
            throw new InternalNSLExcpetion("ForEach node cannot be push on the stack");
        }

    }
}