using System.Collections.Generic;
using NSL.Tokenization;
using NSL.Tokenization.General;

namespace NSL.Parsing
{
    abstract public class ASTNode
    {
        public List<ASTNode> children = new List<ASTNode>();
        public Position start;
        public Position end;

        public void AddChild(ASTNode child)
        {
            Logger.instance?.Source("PAR").Message("Added child").Name(child.GetType().Name).Pos(child.start).End();
            children.Add(child);
        }

        public void Execute(Parser.ParsingState state)
        {
            var next = state.Lookahead();

            if (next == null)
            {
                throw new InternalNSLExcpetion("Next node is null, despite not continued, this should not happen");
            }


        }

        protected virtual void OnToken(Token<NSLTokenizer.TokenType> next, Parser.ParsingState state)
        {
            state.diagnostics.Add(new Diagnostic($"Unexpected {next.type} node", next.start, next.end));
            Logger.instance?.Source("PAR").Error().Message("Unexpected").Name(next.type.ToString()).Message("node").Pos(next.start).End();
        }

        public abstract void Unbalanced(Parser.ParsingState state);

        public ASTNode(Position start, Position end)
        {
            this.start = start;
            this.end = end;
        }
    }
}