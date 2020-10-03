using NSL.Tokenization.General;

namespace NSL
{
    public class Diagnostic
    {
        public string text;
        public Position start;
        public Position end;

        public Diagnostic(string text, Position start, Position end)
        {
            this.text = text;
            this.start = start;
            this.end = end;
        }

        public override string ToString()
        {
            return text + " at " + start.ToString();
        }

        public void Log()
        {
            LoggerProvider.instance?.Source("/\\/").Message(text).Pos(start).End();
        }
    }
}