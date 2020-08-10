namespace NSL.Types
{
    public class NSLValue
    {
        protected object value;
        protected TypeSymbol type;

        public NSLValue(object value, TypeSymbol type)
        {
            this.value = value;
            this.type = type;
        }

        public TypeSymbol GetTypeSymbol() => type;
        public object GetValue() => value;
    }
}