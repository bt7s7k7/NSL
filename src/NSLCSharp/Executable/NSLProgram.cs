using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NSL.Types;

namespace NSL.Executable
{
    public interface IProgram
    {
        public class ReturnVariable
        {
            public TypeSymbol type;
            public string varName;

            public ReturnVariable(TypeSymbol type, string varName)
            {
                this.type = type;
                this.varName = varName;
            }
        }

        IEnumerator<IInstruction> GetEnumerator();
        IProgram.ReturnVariable? GetReturnVariable();
    }

    public class NSLProgram : IEnumerable<IInstruction>, IProgram
    {
        protected IEnumerable<IInstruction> instructions;
        protected IProgram.ReturnVariable? returnVariable = null;

        override public string ToString()
        {
            var builder = new StringBuilder();

            var indent = 0;
            foreach (var inst in instructions)
            {
                var indentDelta = inst.GetIndentDiff();
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
                var indentDelta = inst.GetIndentDiff();
                if (indentDelta < 0) indent += indentDelta;
                ILogger.instance?.Message(new String(' ', indent * 2)).Message(inst.ToString() ?? inst.GetType().Name).Pos(inst.Start).End();
                if (indentDelta > 0) indent += indentDelta;
            }
        }

        public IProgram.ReturnVariable? GetReturnVariable() => returnVariable;

        public IEnumerator<IInstruction> GetEnumerator()
        {
            return instructions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)instructions).GetEnumerator();
        }

        public NSLProgram(IEnumerable<IInstruction> instructions, IProgram.ReturnVariable? returnVariable)
        {
            this.instructions = instructions;
            this.returnVariable = returnVariable;
        }
    }
}