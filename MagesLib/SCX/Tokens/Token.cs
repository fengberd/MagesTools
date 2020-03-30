using System.IO;

namespace Mages.SCX.Tokens
{
    public class Token
    {
        public TokenType Type { get; set; }

        public Token(TokenType type)
        {
            Type = type;
        }

        public virtual void Encode(BinaryWriter target) => target.Write((byte)Type);

        public override string ToString() => "[" + Type.ToString() + "]";
    }
}
