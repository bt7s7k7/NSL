using NSL.Tokenization.General;
using static NSL.Tokenization.NSLTokenizer;

namespace NSL.Parsing.Nodes
{
    public class StatementBlockNode : StatementRootNode
    {
        public bool isInline;
        public string pushedVarName;

        public static int nextVarId = 0;

        public StatementBlockNode(bool isInline, bool doPushVariable, Position start, Position end) : base(start, end)
        {
            this.isInline = isInline;
            if (doPushVariable)
            {
                pushedVarName = "$_s" + nextVarId++;
            }
        }

        public override void AddChild(IASTNode child)
        {
            if (child is StatementNode statement)
            {
                statement.terminator = GetTerminationTokenType();

                if (pushedVarName != null && statement.Children.Count == 0)
                {
                    var varDeref = new StatementNode(pushedVarName, child.Start, child.End);
                    child.Children.Insert(0, varDeref);
                }
            }
            base.AddChild(child);
        }

        override protected void OnToken(Token<Tokenization.NSLTokenizer.TokenType> next, Parser.ParsingState state)
        {
            if (next.type == GetTerminationTokenType())
            {
                state.Pop();
            }
            else
            {
                base.OnToken(next, state);
            }
        }

        public TokenType GetTerminationTokenType()
        {
            return isInline ? TokenType.InlineEnd : TokenType.BlockEnd;
        }

        override public void Unbalanced(Parser.ParsingState state)
        {
            state.diagnostics.Add(new Diagnostic($"Unbalanced block, expected {(isInline ? ")" : "}")}, {Start} →", state.rootNode.End, state.rootNode.End));
            ILogger.instance?.Source("PAR").Error().Message("Unbalanced block, expected").Object(isInline ? ")" : "}").Pos(state.rootNode.End).End()
                .Message("      Started at").Pos(Start).End();
            ;
        }
    }
}