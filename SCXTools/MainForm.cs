using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

using fastJSON;

using Mages.SCX;
using Mages.SCX.Tokens;

namespace Mages
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var sb = new System.Text.StringBuilder();
            var patch = JSON.ToObject<dynamic>(File.ReadAllText("D:/VMShared/sg-phngrm-1.2-pc/game/src/patch.json"));
            TextToken.CharsetEncode = patch["charset_preset"] + patch["charset"];
            foreach (KeyValuePair<string, object> kv in patch["scx"])
            {
                if (!File.Exists("D:/Workspace/SG/script/" + kv.Key + "_old.SCX"))
                {
                    continue;
                }
                var data = (IList<object>)kv.Value;
                sb.AppendLine("-------------- File " + kv.Key + " --------------");
                using (var reader = new SCXReader(File.OpenRead("D:/Workspace/SG/script/" + kv.Key + "_old.SCX")))
                using (var writer = new BinaryWriter(File.Open("D:/Workspace/SG/script/" + kv.Key + ".SCX", FileMode.Create)))
                {
                    long pos = reader.BaseStream.Position;
                    reader.BaseStream.Position = 0;
                    reader.BaseStream.CopyTo(writer.BaseStream);
                    reader.BaseStream.Position = pos;

                    var offset = new List<uint>();
                    int i = 0;
                    foreach (IList<object> transform in data)
                    {
                        sb.AppendLine(i + ":");
                        foreach (var t in transform)
                        {
                            sb.AppendLine(t.ToString());
                        }
                        var str = reader.ReadString(i++);
                        while (str.Tokens.Count == 0)
                            str = reader.ReadString(i++);
                        sb.AppendLine(str.ToString());
                        int j = 0;
                        foreach (var t in str.Tokens)
                        {
                            if (t is TextToken txt)
                            {
                                if (transform.Count == j)
                                {
                                    sb.AppendLine("-------------- File " + kv.Key + " --------------");
                                    sb.AppendLine("Conflict!");
                                    return;
                                }
                                txt.Value = (string)transform[j++];
                            }
                        }
                        offset.Add((uint)writer.BaseStream.Position);
                        str.Encode(writer);
                    }
                    writer.BaseStream.Position = reader.StringTable;
                    foreach (var o in offset)
                    {
                        writer.Write(o);
                    }
                }
            }
            File.WriteAllText("R:/patch.log", sb.ToString());
        }
    }
}
