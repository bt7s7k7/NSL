namespace NSL.Types
{
    public class ActionTypeSymbol : TypeSymbol
    {
        public TypeSymbol Argument { get; protected set; }
        public TypeSymbol Result { get; protected set; }

        public ActionTypeSymbol(TypeSymbol argument, TypeSymbol result) : base(argument + " → " + result)
        {
            this.Argument = argument;
            this.Result = result;
        }

        public TypeSymbol GetArgument() => Argument;
        public TypeSymbol GetResult() => Result;
    }
}