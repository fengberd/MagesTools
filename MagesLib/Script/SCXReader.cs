using System;
using System.IO;
using System.Text;

using Mages.Script.Tokens;

namespace Mages.Script
{
    public class SCXReader : BinaryReader
    {
        public bool EOF => BaseStream.Position == BaseStream.Length;
        public readonly uint StringTable = 0, ReturnAddressTable = 0;

        public string Charset = null;
        public Encoding Encoding = Encoding.UTF8;

        public SCXReader(byte[] data, string charset) : this(new MemoryStream(data), charset) { }

        public SCXReader(Stream stream, string charset, string header = "SC3", Encoding encoding = null, bool leaveOpen = false) : base(stream, encoding ?? Encoding.UTF8, leaveOpen)
        {
            if (encoding != null)
            {
                Encoding = encoding;
            }
            if (Encoding.GetString(ReadBytes(header.Length)) != header || ReadByte() != 0)
            {
                throw new InvalidDataException("SCX header mismatch");
            }
            Charset = charset;
            StringTable = ReadUInt32();
            ReturnAddressTable = ReadUInt32();
        }

        public T ReadTable<T>(int offset, Func<T> callback)
        {
            long current = BaseStream.Position;
            BaseStream.Position = StringTable + offset * 4;
            BaseStream.Position = ReadUInt32();
            var result = callback();
            BaseStream.Position = current;
            return result;
        }

        public SCXString ReadString(int offset) => ReadTable(offset, () => new SCXString(this));

        public uint ReadReturnAddress(int offset) => ReadTable(offset, () => ReadUInt32());

        public char ReadChar(byte first) => Charset[((first & ~(byte)TokenType.TextMask) << 8) | ReadByte()];
    }
}
