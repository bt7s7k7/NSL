using NSL.Tokenization.General;
using static NSL.Tokenization.NSLTokenizer;

namespace NSL.Parsing.Nodes
{
    public class VariableNode : StatementNode
    {
        public string? varName = null;

        public VariableNode(Position start, Position end) : base("var", start, end) { }

        override protected void OnToken(Token<Tokenization.NSLTokenizer.TokenType> next, Parser.ParsingState state)
        {
            if (varName == null)
            {
                if (next.type == TokenType.Keyword)
                {
                    varName = next.content;
                    var statementNode = new StatementNode(next.content, next.start, next.end);
                    AddChild(statementNode);
                    state.Pop();
                    state.Push(statementNode);
                }
                else
                {
                    base.OnToken(next, state);
                }
            }
        }

    }
}