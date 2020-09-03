using System;
using System.Collections.Generic;
using System.Linq;
using NSL.Parsing.Nodes;
using NSL.Tokenization;
using NSL.Tokenization.General;
using static NSL.FunctionRegistry;

namespace NSL.Parsing
{
    public static class Parser
    {
        public class ParsingResult
        {
            public List<Diagnostic> diagnostics;

            public ParsingResult(List<Diagnostic> diagnostics, StatementRootNode rootNode)
            {
                this.diagnostics = diagnostics;
                this.rootNode = rootNode;
            }

            public List<IASTNode> statements = new List<IASTNode>();

            public StatementRootNode rootNode;
        }

        public class ParsingState : ParsingResult
        {
            public Stack<IASTNode> stack = new Stack<IASTNode>();
            protected List<Token<NSLTokenizer.TokenType>> tokens;

            public ParsingState(List<Token<NSLTokenizer.TokenType>> tokens, List<Diagnostic> diagnostics) : base(diagnostics, new StatementRootNode(tokens[0].start, tokens[tokens.Count - 1].end))
            {
                this.tokens = tokens;
                this.stack.Push(rootNode);
            }

            public int index = 0;

            public Token<NSLTokenizer.TokenType>? Lookahead()
            {
                try
                {
                    return tokens[index];
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
            }

            public Token<NSLTokenizer.TokenType>? Next()
            {
                var token = Lookahead();
                index++;
                return token;
            }

            public void Push(IASTNode node)
            {
                ILogger.instance?.Source("PAR").Message("Pushed node").Name(node.GetType().Name).Pos(node.Start).End();
                stack.Push(node);
            }

            public void Pop()
            {
                var node = stack.Pop();
                ILogger.instance?.Source("PAR").Message("Popped node").Name(node.GetType().Name).Object(node.GetAdditionalInfo()).Pos(node.Start).End();
            }

            public IASTNode Top()
            {
                return stack.Peek();
            }

        }

        public static ParsingResult Parse(NSLTokenizer.TokenizationResult tokenized, FunctionRegistry functions)
        {
            var state = new ParsingState(
                diagnostics: tokenized.diagnostics,
                tokens: tokenized.tokens
            );

            if (state.diagnostics.Count > 0)
            {
                return state;
            }

            while (state.Lookahead() != null)
            {
                var index = state.index;
                var stackSize = state.stack.Count;
                var topNode = state.Top();
                topNode.Execute(state);
                if (index == state.index && stackSize == state.stack.Count) throw new InternalNSLExcpetion($"Top node ({topNode.GetType().Name}) failed to increment state or modify stack index at {state.Lookahead()?.start}");
            }

            if (state.stack.Count > 1)
            {
                state.Top().Unbalanced(state);
            }

            void visitNode(IASTNode node)
            {
                node.Children.ForEach(v => visitNode(v));

                foreach (var op in functions.Operators)
                {
                    var repeat = true;
                    while (repeat)
                    {
                        repeat = false;
                        foreach (var child in node.Children)
                        {
                            if (child is OperatorNode childOp && childOp.Match == op.match)
                            {
                                var statement = new StatementNode(op.function, childOp.Start, childOp.End);
                                var fail = false;

                                if (op.type.HasFlag(Operator.Type.Suffix))
                                {
                                    var index = node.Children.IndexOf(child);
                                    if (index > 0)
                                    {
                                        var targetIndex = index - 1;
                                        var targetNode = node.Children[targetIndex];
                                        if (targetNode is OperatorNode)
                                        {
                                            fail = true;
                                        }
                                        else
                                        {
                                            node.Children.RemoveAt(targetIndex);
                                            statement.AddChild(targetNode);
                                            repeat = true && !fail;
                                        }
                                    }
                                    else fail = true;
                                }

                                if (op.type.HasFlag(Operator.Type.Prefix) && !fail)
                                {
                                    var index = node.Children.IndexOf(child);
                                    if (index < node.Children.Count - 1)
                                    {
                                        var targetIndex = index + 1;
                                        var targetNode = node.Children[targetIndex];
                                        if (targetNode is OperatorNode)
                                        {
                                            fail = true;
                                        }
                                        else
                                        {
                                            node.Children.RemoveAt(targetIndex);
                                            statement.AddChild(targetNode);
                                            repeat = true && !fail;
                                        }
                                    }
                                    else fail = true;
                                }

                                if (repeat)
                                {
                                    if (op.reverse) statement.Children.Reverse();

                                    var index = node.Children.IndexOf(child);
                                    node.Children.RemoveAt(index);
                                    node.Children.Insert(index, statement);
                                    break;
                                }
                            }
                        }
                    }
                }

                foreach (var op in node.Children.Where(v => v is OperatorNode).Cast<OperatorNode>())
                {
                    state!.diagnostics.Add(new Diagnostic($"Failed to find matching definition for operator {op.Match}", op.Start, op.End));
                }
            }

            visitNode(state.rootNode);

            return state;
        }
    }
}