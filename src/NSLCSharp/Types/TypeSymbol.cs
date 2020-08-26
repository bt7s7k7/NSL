using System;

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

        public TypeSymbol(string name)
        {
            this.name = name;
        }

        public ArrayTypeSymbol ToArray() => new ArrayTypeSymbol(this);
        public OptionalTypeSymbol ToOptional() => new OptionalTypeSymbol(this);

        public NSLValue Instantiate(object? value) => new NSLValue(value, this);

        override public bool Equals(object? obj)
        {
            return obj != null && obj is TypeSymbol symbol && symbol.ToString() == this.ToString();
        }

        public static bool operator ==(TypeSymbol? a, TypeSymbol? b) => a?.Equals(b) ?? Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null);
        public static bool operator !=(TypeSymbol? a, TypeSymbol? b) => !(a == b);

        public override int GetHashCode()
        {
            return HashCode.Combine(name);
        }
    }
}