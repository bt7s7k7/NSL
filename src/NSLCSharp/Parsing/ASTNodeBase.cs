using System;
using System.Collections.Generic;
using System.Text;
using NSL.Tokenization;
using NSL.Tokenization.General;

namespace NSL.Parsing
{
    public interface IASTNode
    {
        List<IASTNode> Children { get; }
        IASTNode? Parent { get; set; }
        Position Start { get; }
        Position End { get; }

        void AddChild(IASTNode child);
        void Execute(Parser.ParsingState state);
        string GetAdditionalInfo();
        void RemoveChild(IASTNode child);
        string ToString();
        void Unbalanced(Parser.ParsingState state);
    }

    abstract public class ASTNodeBase : IASTNode
    {
        public List<IASTNode> Children { get; protected set; } = new List<IASTNode>();
        public IASTNode? Parent { get; set; }
        public Position Start { get; protected set; }
        public Position End { get; protected set; }


        public virtual void AddChild(IASTNode child)
        {
            ILogger.instance?.Source("PAR").Message("Added child").Name(child.GetType().Name).Object(child.GetAdditionalInfo()).Pos(child.Start).End()
                .Message("      → to").Name(GetType().Name).Pos(Start).End();
            Children.Add(child);

            child.Parent = this;
        }

        public virtual void RemoveChild(IASTNode child)
        {
            ILogger.instance?.Source("PAR").Message("Removed child").Name(child.GetType().Name).Object(child.GetAdditionalInfo()).Pos(child.Start).End()
                .Message("      → from").Name(GetType().Name).Pos(Start).End(); ;
            Children.Remove(child);

            child.Parent = null;
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
            ILogger.instance?.Source("PAR").Error().Message("Unexpected").Name(next.type.ToString()).Message("node (").Name(GetType().Name).Message(")").Pos(next.start).End();
        }

        public abstract void Unbalanced(Parser.ParsingState state);

        public ASTNodeBase(Position start, Position end)
        {
            this.Start = start;
            this.End = end;
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
            builder.Append(this.Start.ToString());
            builder.Append('\n');
            builder.Append(Start.GetDebugLineArrow(indent));
            builder.Append('\n');
            foreach (var child in Children)
            {
                if (child is ASTNodeBase astNode)
                {
                    astNode.AppendToBuilder(builder, indent + 1);
                }
                else
                {
                    builder.AppendLine(child.ToString());
                }
            }
        }

        public virtual string GetAdditionalInfo()
        {
            return "";
        }
    }
}