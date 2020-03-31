using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Mages.Script.Tokens
{
    public class ExpressionToken : Token
    {
        public List<byte> Value = new List<byte>();

        public ExpressionToken(TokenType type, SCXReader reader) : base(type)
        {
            var sb = new StringBuilder();
            while (true)
            {
                sbyte next = reader.ReadSByte();
                if (next == 0)
                {
                    break;
                }
                Value.Add((byte)next);
                Value.Add(reader.ReadByte());
                if (next > 0)
                {
                    continue;
                }
                switch (next & 0x60)
                {
                case 0:
                    break;
                case 0b0100000:
                    Value.Add(reader.ReadByte());
                    break;
                case 0b1000000:
                    Value.Add(reader.ReadByte());
                    Value.Add(reader.ReadByte());
                    break;
                case 0b1100000:
                    Value.Add(reader.ReadByte());
                    Value.Add(reader.ReadByte());
                    Value.Add(reader.ReadByte());
                    Value.Add(reader.ReadByte());
                    break;
                }
            }
        }

        public override void Encode(SCXWriter target)
        {
            base.Encode(target);
            target.Write(Value.ToArray());
            target.Write((byte)0);
        }

        public override string ToString() => "[" + Type + "," + string.Join(" ", Value.Select(s => s.ToString("X2"))) + "]";
    }
}
