namespace Mages.Package
{
    public class MPKEntry
    {
        public const int EntrySize = 256;
        public static readonly byte[] PrePadding = new byte[56];
        public static readonly byte[] PostPadding = new byte[EntrySize - PrePadding.Length - 24 - 4];

        public int Size;
        public string Name;
        public byte[] Data;

        public MPKEntry(string name, int size)
        {
            Size = size;
            Name = name;
            Data = null;
        }

        public MPKEntry(string name, byte[] data)
        {
            Name = name;
            Data = data;
            Size = data.Length;
        }
    }
}
