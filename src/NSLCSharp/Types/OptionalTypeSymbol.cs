namespace NSL.Types
{
    public class OptionalTypeSymbol : TypeSymbol
    {
        protected TypeSymbol item;

        public OptionalTypeSymbol(TypeSymbol item) : base(item.GetName() + "?")
        {
            this.item = item;
        }

        public TypeSymbol GetItemType() => item;
    }
}