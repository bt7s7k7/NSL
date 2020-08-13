using System.Security.Cryptography.X509Certificates;
using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class ActionInstruction : ExeInstruction
    {
        protected string name;

        override public int GetIndentDiff() => 1;
        override public string ToString() => $"action {name}";

        public ActionInstruction(Position start, Position end, string name) : base(start, end)
        {
            this.name = name;
        }
    }
}