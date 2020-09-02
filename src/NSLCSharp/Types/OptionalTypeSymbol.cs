namespace NSL.Types
{
    public class OptionalTypeSymbol : TypeSymbol
    {
        protected TypeSymbol item;

        public OptionalTypeSymbol(TypeSymbol item) : base(item.Name + "?")
        {
            this.item = item;
        }

        public TypeSymbol ItemType => item;
    }
}