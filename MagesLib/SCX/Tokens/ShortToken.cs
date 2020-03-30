using System.IO;

namespace Mages.SCX.Tokens
{
    class ShortToken : Token
    {
        public short Value;

        public ShortToken(TokenType type, short value = 0) : base(type)
        {
            Value = value;
        }

        public override void Encode(BinaryWriter target)
        {
            base.Encode(target);
            target.Write(Value);
        }

        public override string ToString() => "[" + Type.ToString() + "," + Value + "]";
    }
}
