using System.Collections.Generic;
using NSL.Tokenization.General;
using static NSL.Tokenization.NSLTokenizer;

namespace NSL.Parsing.Nodes
{
    public class StatementBlockNode : ASTNode
    {
        public StatementBlockNode(Position start, Position end) : base(start, end)
        {
        }

        override protected void OnToken(Token<TokenType> next, Parser.ParsingState state)
        {
            if (next.type == TokenType.Keyword)
            {

            }
            else
            {
                base.OnToken(next, state);
            }
        }

        public override void Unbalanced(Parser.ParsingState state)
        {
            throw new InternalNSLExcpetion("Unbalanced statement block node, this should be impossible");
        }
    }
}