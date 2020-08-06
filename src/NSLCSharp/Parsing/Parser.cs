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

            public ParsingResult(List<Diagnostic> diagnostics)
            {
                this.diagnostics = diagnostics;
            }

            public List<ASTNode> statements = new List<ASTNode>();
        }

        public class ParsingState : ParsingResult
        {
            public Stack<ASTNode> stack = new Stack<ASTNode>();
            protected List<Token<NSLTokenizer.TokenType>> tokens;

            public ParsingState(List<Token<NSLTokenizer.TokenType>> tokens, List<Diagnostic> diagnostics) : base(diagnostics)
            {
                this.tokens = tokens;
            }

            public int index = 0;

            public Token<NSLTokenizer.TokenType>? Lookahead()
            {
                return tokens[index];
            }

            public Token<NSLTokenizer.TokenType>? Next()
            {
                var token = Lookahead();
                index++;
                return token;
            }

            public void Push(ASTNode node)
            {
                stack.Push(node);
            }

            public void Pop()
            {
                stack.Pop();
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

            state.Push(new StatementBlockNode(tokenized.tokens[0].start, tokenized.tokens[tokenized.tokens.Count - 1].end));

            while (state.Lookahead() != null)
            {
                var index = state.index;
                var topNode = state.Top();
                topNode.Execute(state);
                if (index == state.index) throw new InternalNSLExcpetion($"Top node ({topNode.GetType().Name}) failed to increment state index at {state.Lookahead()?.start}");
            }

            if (state.stack.Count > 1)
            {
                state.Top().Unbalanced(state);
            }

            return state;
        }
    }
}