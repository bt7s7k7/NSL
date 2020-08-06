using NSL.Tokenization.General;
using static NSL.Tokenization.NSLTokenizer;

namespace NSL.Parsing.Nodes
{
    public class StatementBlockNode : StatementRootNode
    {
        public bool isInline;
        public ASTNode? pushedArgument = null;
        public StatementBlockNode(bool isInline, Position start, Position end) : base(start, end)
        {
            this.isInline = isInline;
        }

        public static int pushedVariableId = 0;

        public override void AddChild(ASTNode child)
        {
            if (child is StatementNode statement)
            {
                statement.terminator = GetTerminationTokenType();
            }
            base.AddChild(child);
        }

        override protected void OnToken(Token<Tokenization.NSLTokenizer.TokenType> next, Parser.ParsingState state)
        {
            if (next.type == GetTerminationTokenType())
            {
                if (pushedArgument != null)
                {
                    var id = pushedVariableId++;

                    var varName = "$_" + id;
                    var variableNode = new VariableNode(start, end);
                    variableNode.varName = varName;

                    var varAssignment = new StatementNode(varName, start, end);
                    varAssignment.AddChild(pushedArgument);
                    variableNode.AddChild(varAssignment);

                    children.Insert(0, variableNode);


                    foreach (var child in children)
                    {
                        var varDeref = new StatementNode(varName, child.start, child.end);
                        child.children.Insert(0, varDeref);
                    }
                }

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
            state.diagnostics.Add(new Diagnostic($"Unbalanced block, expected {(isInline ? ")" : "}")}, {start} →", state.rootNode.end, state.rootNode.end));
            Logger.instance?.Source("PAR").Error().Message("Unbalanced block, expected").Object(isInline ? ")" : "}").Pos(state.rootNode.end).End()
                .Message("      Started at").Pos(start).End();
            ;
        }
    }
}