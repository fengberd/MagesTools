using System.IO;
using System.Text;

namespace Mages.SCX.Tokens
{
    public class TextToken : Token
    {
        public static string Charset = null;
        public static string CharsetEncode = null;

        static TextToken()
        {
            Charset = File.ReadAllText("R:/charset.utf8", Encoding.UTF8);
        }

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
                int code = ((next & ~(byte)TokenType.TextMask) << 8) | reader.ReadByte();
                sb.Append(Charset[code]);
            }
            Value = sb.ToString();
        }

        public override void Encode(BinaryWriter target)
        {
            // TODO: \n
            foreach (var c in Value)
            {
                for (int i = 0; i < CharsetEncode.Length; i++)
                {
                    if (CharsetEncode[i] == c)
                    {
                        target.Write((byte)((i >> 8) | (byte)TokenType.TextMask));
                        target.Write((byte)i);
                        break;
                    }
                }
            }
        }

        public override string ToString() => "\"" + Value.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\"", "\\\"") + "\"";
    }
}
