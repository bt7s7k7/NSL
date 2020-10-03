using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NSL.Types;

namespace NSL.Executable
{
    public class VariableDefinition
    {
        public TypeSymbol type;
        public string varName;

        public VariableDefinition(TypeSymbol type, string varName)
        {
            this.type = type;
            this.varName = varName;
        }
    }

    public interface IProgram
    {

        VariableDefinition ReturnVariable { get; }

        IEnumerator<IInstruction> GetEnumerator();
    }

    public class NSLProgram : IEnumerable<IInstruction>, IProgram
    {
        protected IEnumerable<IInstruction> instructions;
        protected VariableDefinition returnVariable = null;

        override public string ToString()
        {
            var builder = new StringBuilder();

            var indent = 0;
            foreach (var inst in instructions)
            {
                var indentDelta = inst.IndentDiff;
                if (indentDelta < 0) indent += indentDelta;
                builder.Append(new String(' ', indent * 2))
                    .Append(inst.ToString() ?? inst.GetType().Name)
                    .Append(" at ")
                    .Append(inst.Start.ToString())
                    .Append("\n");
                if (indentDelta > 0) indent += indentDelta;
            }

            return builder.ToString();
        }

        public void Log()
        {
            var indent = 0;
            foreach (var inst in instructions)
            {
                var indentDelta = inst.IndentDiff;
                if (indentDelta < 0) indent += indentDelta;
                LoggerProvider.instance?.Message(new String(' ', indent * 2)).Message(inst.ToString() ?? inst.GetType().Name).Pos(inst.Start).End();
                if (indentDelta > 0) indent += indentDelta;
            }
        }

        public VariableDefinition ReturnVariable => returnVariable;

        public IEnumerator<IInstruction> GetEnumerator()
        {
            return instructions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)instructions).GetEnumerator();
        }

        public NSLProgram(IEnumerable<IInstruction> instructions, VariableDefinition returnVariable)
        {
            this.instructions = instructions;
            this.returnVariable = returnVariable;
        }
    }
}