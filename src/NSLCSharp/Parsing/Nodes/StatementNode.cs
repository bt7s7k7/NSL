using System.Linq;
using NSL.Tokenization.General;
using static NSL.Tokenization.NSLTokenizer;

namespace NSL.Parsing.Nodes
{
    public class StatementNode : ASTNodeBase
    {
        public string name;
        public TokenType? terminator = null;
        public StatementNode(string name, Position start, Position end) : base(start, end)
        {
            this.name = name;
        }

        protected bool isPartOfDirectPipe = false;

        override protected void OnToken(Tokenization.General.Token<Tokenization.NSLTokenizer.TokenType> next, Parser.ParsingState state)
        {
            if (Parent == null) throw new InternalNSLExcpetion("StatementNode cannot be root");

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
            else if (next.type == TokenType.Operator)
            {
                AddChild(new OperatorNode(next.content, next.start, next.end));
            }
            else if (next.type == TokenType.Pipe)
            {
                state.Pop();
                var prevParent = Parent;
                Parent.RemoveChild(this);

                var afterPipe = state.Next();
                if (afterPipe != null)
                {
                    next = afterPipe;
                    if (
                        next.type == TokenType.Literal ||
                        next.type == TokenType.InlineStart ||
                        (next.type == TokenType.Keyword && next.content[0] == '$') ||
                        next.type == TokenType.Operator
                    )
                    {
                        var statementNode = new StatementNode("echo", next.start, next.end);
                        state.Push(statementNode);
                        statementNode.AddChild(this);
                        prevParent.AddChild(statementNode);
                        state.index--;
                    }
                    else if (next.type == TokenType.Keyword)
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
                    ILogger.instance?.Source("PAR").Error().Message("Unexpected EOF after pipe").Pos(next.start).End();
                }
            }
            else if (next.type == TokenType.DirectPipe)
            {
                var afterPipe = state.Next();
                if (afterPipe != null)
                {
                    next = afterPipe;
                    if (next.type == TokenType.Keyword)
                    {
                        var statementNode = new StatementNode(next.content, next.start, next.end);
                        statementNode.isPartOfDirectPipe = true;
                        if (Children.Count <= (isPartOfDirectPipe ? 1 : 0) || Start.Equals(Children[^1].Start))
                        {
                            state.Pop();
                            state.Push(statementNode);
                            var prevParent = Parent;
                            Parent.RemoveChild(this);
                            statementNode.AddChild(this);
                            prevParent.AddChild(statementNode);
                        }
                        else
                        {
                            var lastIndex = Children.Count - 1;
                            var lastChild = Children[lastIndex];
                            Children.RemoveAt(lastIndex);

                            statementNode.AddChild(lastChild);
                            AddChild(statementNode);
                        }
                    }
                    else
                    {
                        base.OnToken(next, state);
                    }
                }
                else
                {
                    state.diagnostics.Add(new Diagnostic($"Unexpected EOF after direct pipe", next.start, next.end));
                    ILogger.instance?.Source("PAR").Error().Message("Unexpected EOF after direct pipe").Pos(next.start).End();
                }
            }
            else if (next.type == TokenType.PipeForEach)
            {
                state.Pop();
                var prevParent = Parent;
                Parent.RemoveChild(this);

                var pipe = next;
                var afterPipe = state.Next();
                if (afterPipe != null)
                {
                    next = afterPipe;
                    if (
                        next.type == TokenType.Literal ||
                        next.type == TokenType.InlineStart ||
                        (next.type == TokenType.Keyword && next.content[0] == '$') ||
                        next.type == TokenType.Operator
                    )
                    {
                        var forEachNode = new ForEachNode(pipe.start, pipe.end);
                        prevParent.AddChild(forEachNode);
                        forEachNode.AddChild(this);

                        var statementNode = new StatementNode("echo", next.start, next.end);
                        state.Push(statementNode);
                        forEachNode.AddChild(statementNode);
                        state.index--;

                        statementNode.AddChild(new StatementNode("$_a", next.start, next.end));
                    }
                    else if (next.type == TokenType.Keyword)
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
                    ILogger.instance?.Source("PAR").Error().Message("Unexpected EOF after pipe").Pos(next.start).End();
                }
            }
            else if (next.type == TokenType.StatementEnd)
            {
                state.Pop();
            }
            else if (next.type == TokenType.ActionStart)
            {
                ActionNode actionNode;
                if (Children.Count > 0 && Children.Last() is ActionNode lastActionNode && !lastActionNode.HasBody)
                {
                    actionNode = lastActionNode;
                }
                else
                {
                    actionNode = new ActionNode(next.start, next.end);
                    AddChild(actionNode);
                }

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
                var prevParent = Parent;
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
                var prevParent = Parent;
                prevParent.RemoveChild(this);
                prevParent.AddChild(distNode);

                var distBlock = new StatementBlockNode(false, false, next.start, next.end);
                distBlock.pushedVarName = "$_a";
                state.Push(distBlock);
                distNode.AddChild(this);
                distNode.AddChild(distBlock);
            }
            else if (next.type == TokenType.ActionArgument)
            {
                if (Children.Count > 0)
                {
                    var lastChild = Children.Last();
                    Children.RemoveAt(Children.Count - 1);

                    var actionNode = new ActionNode(next.start, next.end);
                    AddChild(actionNode);

                    void addArgument(IASTNode argNode)
                    {
                        if (argNode is StatementNode statementArgument)
                        {

                            if (statementArgument.name[0] == '$')
                            {
                                actionNode.AddArgument(statementArgument.name);
                            }
                            else
                            {
                                state.diagnostics.Add(new Diagnostic("Argument does not match variable name format", statementArgument.Start, statementArgument.End));
                            }
                        }
                        else
                        {
                            state.diagnostics.Add(new Diagnostic("Invalid action argument specification", argNode.Start, argNode.End));
                        }
                    }

                    if (lastChild is StatementNode statementArgument)
                    {
                        addArgument(statementArgument);
                    }
                    else if (lastChild is StatementBlockNode blockArgument)
                    {
                        var children = blockArgument.Children;

                        if (blockArgument.Children.Count > 0 && blockArgument.Children[0] is StatementNode echoNode && echoNode.name == "echo") children = echoNode.Children;

                        foreach (var child in children)
                        {
                            addArgument(child);
                        }
                    }
                    else
                    {
                        state.diagnostics.Add(new Diagnostic("Invalid action argument specification", lastChild.Start, lastChild.End));
                    }
                }
                else
                {
                    state.diagnostics.Add(new Diagnostic("Action argument arrow expected only after action arguments", next.start, next.end));
                }



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