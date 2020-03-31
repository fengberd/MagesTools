using System.Text;
using System.Collections.Generic;

using Mages.Script.Tokens;

namespace Mages.Script
{
    public static class SCX
    {
        public static bool ApplyPatch(IList<object> data, SCXReader reader, SCXWriter writer, StringBuilder logger = null)
        {
            long pos = reader.BaseStream.Position;
            reader.BaseStream.Position = 0;
            reader.BaseStream.CopyTo(writer.BaseStream);
            reader.BaseStream.Position = pos;

            int i = 0;
            var offset = new List<uint?>();
            foreach (IList<object> transform in data)
            {
                SCXString str = reader.ReadString(i++);
                while (str.IsEmpty)
                {
                    offset.Add(null);
                    logger?.AppendLine(i + ":").AppendLine(str.ToString()).AppendLine("------(Skipped)");
                    str = reader.ReadString(i++);
                }
                if (transform == null)
                {
                    offset.Add(null);
                    logger?.AppendLine(i + ":").AppendLine(str.ToString()).AppendLine("------(Null Transform)");
                    continue;
                }
                if (logger != null)
                {
                    logger.AppendLine(i + ":");
                    foreach (var t in transform)
                    {
                        logger.Append('"').Append(t.ToString()).Append("\" ");
                    }
                    logger.AppendLine().AppendLine(str.ToString());
                }
                int j = 0;
                foreach (var t in str.Tokens)
                {
                    if (t is TextToken txt)
                    {
                        if (transform.Count == j)
                        {
                            logger?.AppendLine("---------- Conflict: Transform < Data ----------");
                            return false;
                        }
                        txt.Value = (string)transform[j++];
                    }
                }
                if (transform.Count != j)
                {
                    logger?.AppendLine("---------- Conflict: Transform > Data ----------");
                    return false;
                }
                offset.Add((uint)writer.BaseStream.Position);
                str.Encode(writer);
                logger?.AppendLine("------");
            }
            writer.BaseStream.Position = reader.StringTable;
            foreach (var o in offset)
            {
                if (o == null)
                {
                    writer.BaseStream.Seek(4, System.IO.SeekOrigin.Current);
                }
                else
                {
                    writer.Write(o.Value);
                }
            }
            return true;
        }
    }
}
