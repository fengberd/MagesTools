using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Mages.Package
{
    public class MPK
    {
        public static byte[] MPKHeader = new byte[] { 0x4D, 0x50, 0x4B, 0x00, 0x00, 0x00, 0x02, 0x00 };

        public List<MPKEntry> Entries = new List<MPKEntry>();

        public void Write(BinaryWriter writer)
        {
            writer.Write(MPKHeader);
            writer.Write(Entries.Count);

            int i = 0;
            long offset = MPKHeader.Length + 4 + Entries.Count * MPKEntry.EntrySize;

            foreach (var entry in Entries)
            {
                writer.Write(MPKEntry.PrePadding);
                writer.Write(i++);

                writer.Write(offset);

                offset += entry.Size;

                writer.Write(entry.Size);
                writer.Write(entry.Size); // Compressed size or sth?

                var name = Encoding.UTF8.GetBytes(entry.Name);
                writer.Write(name, 0, name.Length);
                writer.Write(MPKEntry.PostPadding, 0, MPKEntry.PostPadding.Length - name.Length);
            }

            foreach (var entry in Entries)
            {
                using (var stream = entry.GetStream())
                {
                    stream.CopyTo(writer.BaseStream);
                }
            }
        }
    }
}
