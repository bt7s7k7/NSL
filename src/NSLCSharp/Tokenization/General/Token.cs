namespace NSL.Tokenization.General
{
    public class Token<T>
    {
        public T type;
        public string content;
        public object value;
        public Position start;
        public Position end;
    }
}