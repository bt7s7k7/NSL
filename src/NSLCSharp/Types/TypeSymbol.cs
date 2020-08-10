namespace NSL.Types
{
    public class TypeSymbol
    {
        protected string name;

        public string GetName() => name;

        override public string ToString()
        {
            return name;
        }

        protected TypeSymbol(string name)
        {
            this.name = name;
        }

        public ArrayTypeSymbol ToArray() => new ArrayTypeSymbol(this);
        public OptionalTypeSymbol ToOptional() => new OptionalTypeSymbol(this);

        public NSLValue Instantiate(object value) => new NSLValue(value, this);
    }
}