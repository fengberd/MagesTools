using System.Text;

namespace Mages.Script.Tokens
{
    public class TextToken : Token
    {
        public string Value;

        public TextToken(SCXReader reader) : base(TokenType.TextMask)
        {
            var sb = new StringBuilder();
            while (true)
            {
                byte next = reader.ReadByte();
                if (next == (byte)TokenType.LineBreak)
                {
                    sb.Append("\n");
                    continue;
                }
                else if (next == (byte)TokenType.Terminator || (next & (byte)TokenType.TextMask) == 0)
                {
                    reader.BaseStream.Position--;
                    break;
                }
                sb.Append(reader.ReadChar(next));
            }
            Value = sb.ToString();
        }

        public override void Encode(SCXWriter target)
        {
            foreach (var c in Value)
            {
                if (c == '\n')
                {
                    target.Write(TokenType.LineBreak);
                    continue;
                }
                target.WriteChar(c);
            }
        }

        public override string ToString() => "\"" + Value.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\"", "\\\"") + "\"";
    }
}
