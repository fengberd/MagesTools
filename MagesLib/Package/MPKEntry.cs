using System;
using System.IO;

namespace Mages.Package
{
    public class MPKEntry
    {
        public const int EntrySize = 256;
        public static readonly byte[] PrePadding = new byte[56];
        public static readonly byte[] PostPadding = new byte[EntrySize - PrePadding.Length - 24 - 4];

        public long Size;
        public string Name;
        public Func<Stream> GetStream = null;

        public MPKEntry(long size, string name, Func<Stream> getStream)
        {
            Size = size;
            Name = name;
            GetStream = getStream;
        }
    }
}
