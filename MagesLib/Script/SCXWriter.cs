using System.IO;
using System.Text;

using Mages.Script.Tokens;

namespace Mages.Script
{
    public class SCXWriter : BinaryWriter
    {
        public bool EOF => BaseStream.Position == BaseStream.Length;
        public readonly uint StringTable = 0, ReturnAddressTable = 0;

        public string Charset;

        public SCXWriter(Stream stream, string charset, Encoding encoding = null, bool leaveOpen = false) : base(stream, encoding ?? Encoding.UTF8, leaveOpen)
        {
            Charset = charset;
        }

        public void Write(TokenType type) => Write((byte)type);

        public bool WriteChar(char c)
        {
            for (int i = 0; i < Charset.Length; i++)
            {
                if (Charset[i] == c)
                {
                    Write((byte)((i >> 8) | (byte)TokenType.TextMask));
                    Write((byte)i);
                    return true;
                }
            }
            return false;
        }
    }
}
