using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Mages.Package
{
    public class MPK
    {
        public static byte[] MPKHeader = new byte[] { 0x4D, 0x50, 0x4B, 0x00, 0x00, 0x00, 0x02, 0x00 };

        public static MPK ReadFile(BinaryReader reader)
        {
            if (Encoding.UTF8.GetString(reader.ReadBytes(3)) != "MPK" || reader.ReadByte() != 0)
            {
                throw new InvalidDataException("MPK header mismatch");
            }
            reader.BaseStream.Position += 4;

            var result = new MPK();
            int count = reader.ReadInt32();
            if (count <= 0)
            {
                throw new InvalidDataException("MPK structure mismatch");
            }

            List<long> offsets = new List<long>();
            for (int i = 0; i < count; i++)
            {
                reader.BaseStream.Position += MPKEntry.PrePadding.Length;
                if (reader.ReadInt32() != i)
                {
                    throw new InvalidDataException("MPK index mismatch");
                }

                offsets.Add(reader.ReadInt64());
                long size = reader.ReadInt64();
                if (size < 0 || size > int.MaxValue || reader.ReadInt64() != size)
                {
                    throw new InvalidDataException("Unsupported MPK structure");
                }

                var name = Encoding.UTF8.GetString(reader.ReadBytes(MPKEntry.PostPadding.Length));
                int index = name.IndexOf('\0');
                if (index >= 0)
                {
                    name = name.Remove(index);
                }

                result.Entries.Add(new MPKEntry(name, (int)size));
            }
            for (int i = 0; i < count; i++)
            {
                reader.BaseStream.Position = offsets[i];
                result.Entries[i].Data = reader.ReadBytes(result.Entries[i].Size);
            }
            return result;
        }

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

                writer.Write((long)entry.Size);
                writer.Write((long)entry.Size); // Compressed size or sth?

                var name = Encoding.UTF8.GetBytes(entry.Name);
                writer.Write(name, 0, name.Length);
                writer.Write(MPKEntry.PostPadding, 0, MPKEntry.PostPadding.Length - name.Length);
            }

            foreach (var entry in Entries)
            {
                writer.Write(entry.Data, 0, entry.Size);
            }
        }
    }
}
