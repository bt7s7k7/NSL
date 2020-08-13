using NSL.Tokenization.General;
using static NSL.Tokenization.NSLTokenizer;

namespace NSL.Parsing.Nodes
{
    public class StatementNode : ASTNode
    {
        public string name;
        public TokenType? terminator = null;
        public StatementNode(string name, Position start, Position end) : base(start, end)
        {
            this.name = name;
        }

        override protected void OnToken(Tokenization.General.Token<Tokenization.NSLTokenizer.TokenType> next, Parser.ParsingState state)
        {
            if (parent == null) throw new InternalNSLExcpetion("StatementNode cannot be root");

            if (next.type == TokenType.Literal)
            {
                var literalNode = new LiteralNode(
                    next.value ?? throw new InternalNSLExcpetion($"Literal token {next.type} doesn't have a value at {next.start}"),
                    next.start,
                    next.end
                );

                AddChild(literalNode);
            }
            else if (next.type == TokenType.Keyword)
            {
                var inlineStatement = new StatementNode(next.content, next.start, next.end);

                AddChild(inlineStatement);
            }
            else if (next.type == TokenType.Pipe)
            {
                state.Pop();
                var prevParent = parent;
                parent.RemoveChild(this);

                var afterPipe = state.Next();
                if (afterPipe != null)
                {
                    next = afterPipe;
                    if (next.type == TokenType.Keyword)
                    {
                        var statementNode = new StatementNode(next.content, next.start, next.end);
                        state.Push(statementNode);
                        statementNode.AddChild(this);
                        prevParent.AddChild(statementNode);
                    }
                    else
                    {
                        base.OnToken(next, state);
                    }
                }
                else
                {
                    state.diagnostics.Add(new Diagnostic($"Unexpected EOF after pipe", next.start, next.end));
                    Logger.instance?.Source("PAR").Error().Message("Unexpected EOF after pipe").Pos(next.start).End();
                }
            }
            else if (next.type == TokenType.PipeForEach)
            {
                state.Pop();
                var prevParent = parent;
                parent.RemoveChild(this);

                var pipe = next;
                var afterPipe = state.Next();
                if (afterPipe != null)
                {
                    next = afterPipe;
                    if (next.type == TokenType.Keyword)
                    {
                        var forEachNode = new ForEachNode(pipe.start, pipe.end);
                        prevParent.AddChild(forEachNode);
                        forEachNode.AddChild(this);

                        var statementNode = new StatementNode(next.content, next.start, next.end);
                        state.Push(statementNode);
                        forEachNode.AddChild(statementNode);

                        statementNode.AddChild(new StatementNode("$_a", next.start, next.end));
                    }
                    else
                    {
                        base.OnToken(next, state);
                    }
                }
                else
                {
                    state.diagnostics.Add(new Diagnostic($"Unexpected EOF after pipe", next.start, next.end));
                    Logger.instance?.Source("PAR").Error().Message("Unexpected EOF after pipe").Pos(next.start).End();
                }
            }
            else if (next.type == TokenType.StatementEnd)
            {
                state.Pop();
            }
            else if (next.type == TokenType.ActionStart)
            {
                var actionNode = new ActionNode(next.start, next.end);
                AddChild(actionNode);

                var actionBlock = new StatementBlockNode(false, false, next.start, next.end);
                state.Push(actionBlock);
                actionNode.AddChild(actionBlock);
            }
            else if (next.type == TokenType.InlineStart)
            {
                var block = new StatementBlockNode(true, false, next.start, next.end);
                state.Push(block);
                AddChild(block);
            }
            else if (next.type == TokenType.PipeStart)
            {
                var prevParent = parent;
                prevParent.RemoveChild(this);

                var blockNode = new StatementBlockNode(false, true, next.start, next.end);
                state.Push(blockNode);

                var distNode = new DistributionNode(next.start, next.end);
                distNode.AddChild(this);
                distNode.AddChild(blockNode);

                prevParent.AddChild(distNode);
            }
            else if (next.type == TokenType.PipeForEachStart)
            {
                var distNode = new ForEachNode(next.start, next.end);
                var prevParent = parent;
                prevParent.RemoveChild(this);
                prevParent.AddChild(distNode);

                var distBlock = new StatementBlockNode(false, false, next.start, next.end);
                distBlock.pushedVarName = "$_a";
                state.Push(distBlock);
                distNode.AddChild(this);
                distNode.AddChild(distBlock);
            }
            else if (next.type == terminator)
            {
                state.Pop();
                state.index--;
            }
            else
            {
                base.OnToken(next, state);
            }
        }

        public override void Unbalanced(Parser.ParsingState state)
        {
            // They don't need it
        }

        override public string GetAdditionalInfo()
        {
            return name;
        }
    }
}