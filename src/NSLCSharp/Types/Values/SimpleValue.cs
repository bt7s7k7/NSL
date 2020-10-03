using System;

namespace NSL.Types.Values
{
    public class SimpleValue : NSLValueBase, IValue
    {
        public override object Value { get; set; }
        public override TypeSymbol TypeSymbol { get; protected set; }

        public SimpleValue(object value, TypeSymbol type)
        {
            this.Value = value;
            this.TypeSymbol = type;
        }
    }

    public class CallbackValue : NSLValueBase, IValue
    {
        public override object Value { get => getCallback(); set => setCallback(value); }
        public override TypeSymbol TypeSymbol { get; protected set; }

        protected Action<object> setCallback;
        protected Func<object> getCallback;

        public CallbackValue(Func<object> getCallback, Action<object> setCallback, TypeSymbol typeSymbol)
        {
            this.getCallback = getCallback;
            this.setCallback = setCallback ?? ((value) => throw new UserNSLException("This value is read only"));
            TypeSymbol = typeSymbol;
        }
    }
}