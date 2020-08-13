namespace NSL.Types
{
    public class ActionTypeSymbol : TypeSymbol
    {
        protected TypeSymbol argument;
        protected TypeSymbol result;

        public ActionTypeSymbol(TypeSymbol argument, TypeSymbol result) : base(argument + " => " + result)
        {
            this.argument = argument;
            this.result = result;
        }

        public TypeSymbol GetArgument() => argument;
        public TypeSymbol GetResult() => result;
    }
}