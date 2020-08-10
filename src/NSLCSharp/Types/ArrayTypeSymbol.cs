namespace NSL.Types
{
    public class ArrayTypeSymbol : TypeSymbol
    {
        protected TypeSymbol item;

        public ArrayTypeSymbol(TypeSymbol item) : base(item.GetName() + "[]")
        {
            this.item = item;
        }

        public TypeSymbol GetItemType() => item;
    }
}