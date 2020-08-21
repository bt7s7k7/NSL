using System;
using System.Collections.Generic;
using System.Text;
using NSL.Types;

namespace NSL.Executable
{
    public class NSLProgram
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

        protected IEnumerable<ExeInstruction> instructions;
        protected ReturnVariable? returnVariable = null;

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
                    .Append(inst.start.ToString())
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
                Logger.instance?.Message(new String(' ', indent * 2)).Message(inst.ToString() ?? inst.GetType().Name).Pos(inst.start).End();
                if (indentDelta > 0) indent += indentDelta;
            }
        }

        public ReturnVariable? GetReturnVariable() => returnVariable;

        public NSLProgram(IEnumerable<ExeInstruction> instructions, ReturnVariable? returnVariable)
        {
            this.instructions = instructions;
            this.returnVariable = returnVariable;
        }
    }
}