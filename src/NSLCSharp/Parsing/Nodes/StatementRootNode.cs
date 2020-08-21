using System.Collections.Generic;
using NSL.Tokenization.General;
using static NSL.Tokenization.NSLTokenizer;

namespace NSL.Parsing.Nodes
{
    public class StatementRootNode : ASTNodeBase
    {
        public StatementRootNode(Position start, Position end) : base(start, end)
        {
        }

        override protected void OnToken(Token<TokenType> next, Parser.ParsingState state)
        {
            if (next.type == TokenType.Keyword)
            {
                var statementNode = new StatementNode(next.content, next.start, next.end);
                AddChild(statementNode);
                state.Push(statementNode);
            }
            else if (next.type == TokenType.StatementEnd)
            {
                // Ignore
            }
            else if (next.type == TokenType.VariableDecl)
            {
                var variableNode = new VariableNode(next.start, next.end);
                AddChild(variableNode);
                state.Push(variableNode);
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