namespace NSL.Types
{
    public class ConstexprTypeSymbol : TypeSymbol
    {
        public IValue Value { get; protected set; }
        public TypeSymbol Base { get; protected set; }

        public override IValue Instantiate(object value)
        {
            return base.Instantiate(Value.Value);
        }

        public ConstexprTypeSymbol(IValue value) : base($"{value.TypeSymbol}(={ToStringUtil.ToString(value.Value)})")
        {
            Value = value;
            Base = value.TypeSymbol;
        }
    }
}