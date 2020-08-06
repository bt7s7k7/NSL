using System;
using System.Collections.Generic;
using System.Text;
using NSL.Tokenization;
using NSL.Tokenization.General;

namespace NSL.Parsing
{
    abstract public class ASTNode
    {
        public List<ASTNode> children = new List<ASTNode>();
        public ASTNode? parent;
        public Position start;
        public Position end;

        public virtual void AddChild(ASTNode child)
        {
            Logger.instance?.Source("PAR").Message("Added child").Name(child.GetType().Name).Object(child.GetAdditionalInfo()).Pos(child.start).End()
                .Message("      → to").Name(GetType().Name).Pos(start).End();
            children.Add(child);

            child.parent = this;
        }

        public virtual void RemoveChild(ASTNode child)
        {
            Logger.instance?.Source("PAR").Message("Removed child").Name(child.GetType().Name).Object(child.GetAdditionalInfo()).Pos(child.start).End()
                .Message("      → from").Name(GetType().Name).Pos(start).End(); ;
            children.Remove(child);

            child.parent = null;
        }

        public virtual void Execute(Parser.ParsingState state)
        {
            var next = state.Next();

            if (next == null)
            {
                throw new InternalNSLExcpetion("Next node is null, despite not continued, this should not happen");
            }

            OnToken(next, state);
        }

        protected virtual void OnToken(Token<NSLTokenizer.TokenType> next, Parser.ParsingState state)
        {
            state.diagnostics.Add(new Diagnostic($"Unexpected {next.type} node ({GetType().Name})", next.start, next.end));
            Logger.instance?.Source("PAR").Error().Message("Unexpected").Name(next.type.ToString()).Message("node (").Name(GetType().Name).Message(")").Pos(next.start).End();
        }

        public abstract void Unbalanced(Parser.ParsingState state);

        public ASTNode(Position start, Position end)
        {
            this.start = start;
            this.end = end;
        }

        override public string ToString()
        {
            var builder = new StringBuilder();
            AppendToBuilder(builder, 0);
            return builder.ToString();
        }

        protected void AppendToBuilder(StringBuilder builder, int indent)
        {
            builder.Append(new String(' ', indent * 2));
            builder.Append(GetType().Name);
            builder.Append(' ');
            builder.Append(GetAdditionalInfo());
            builder.Append(" at ");
            builder.Append(this.start.ToString());
            builder.Append('\n');
            builder.Append(start.GetDebugLineArrow(indent));
            builder.Append('\n');
            foreach (var child in children)
            {
                child.AppendToBuilder(builder, indent + 1);
            }
        }

        public virtual string GetAdditionalInfo()
        {
            return "";
        }
    }
}