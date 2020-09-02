namespace NSL.Types
{
    public class ArrayTypeSymbol : TypeSymbol
    {
        public TypeSymbol ItemType { get; protected set; }

        public ArrayTypeSymbol(TypeSymbol item) : base(item.Name + "[]")
        {
            this.ItemType = item;
        }
    }
}