using System;
using System.Collections.Generic;
using NSL.Parsing.Nodes;
using NSL.Tokenization;
using NSL.Tokenization.General;

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

            public List<ASTNode> statements = new List<ASTNode>();

            public StatementRootNode rootNode;
        }

        public class ParsingState : ParsingResult
        {
            public Stack<ASTNode> stack = new Stack<ASTNode>();
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

            public void Push(ASTNode node)
            {
                Logger.instance?.Source("PAR").Message("Pushed node").Name(node.GetType().Name).Pos(node.start).End();
                stack.Push(node);
            }

            public void Pop()
            {
                var node = stack.Pop();
                Logger.instance?.Source("PAR").Message("Popped node").Name(node.GetType().Name).Object(node.GetAdditionalInfo()).Pos(node.start).End();
            }

            public ASTNode Top()
            {
                return stack.Peek();
            }

        }

        public static ParsingResult Parse(NSLTokenizer.TokenizationResult tokenized)
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

            return state;
        }
    }
}