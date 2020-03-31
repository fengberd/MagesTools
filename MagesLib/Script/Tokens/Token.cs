namespace Mages.Script.Tokens
{
    public class Token
    {
        public TokenType Type { get; set; }

        public Token(TokenType type)
        {
            Type = type;
        }

        public virtual void Encode(SCXWriter target) => target.Write(Type);

        public override string ToString() => "[" + Type.ToString() + "]";
    }
}
