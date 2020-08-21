using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class ForEachInvokeInstruction : InstructionBase
    {
        protected string argVarName;
        protected string actionVarName;
        protected string arrayVarName;

        public ForEachInvokeInstruction(Position start, Position end, string arrayVarName, string argVarName, string actionVarName) : base(start, end)
        {
            this.argVarName = argVarName;
            this.actionVarName = actionVarName;
            this.arrayVarName = arrayVarName;
        }

        override public int GetIndentDiff() => 0;
        override public string ToString() => $"for {arrayVarName} : {argVarName} => {actionVarName}";
    }
}