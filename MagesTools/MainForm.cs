using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

using fastJSON;

using Mages.Script;
using Mages.Package;
using Mages.Script.Tokens;

namespace MagesTools
{
    public partial class MainForm : Form
    {
        public IDictionary<string, object> SCX_Patch = null;

        public MainForm()
        {
            InitializeComponent();
        }

        public void Log(string data)
        {
            textBox_log.AppendText(DateTime.Now.ToString("t") + " " + data + "\n");
        }

        public void Oops(Exception e)
        {
            Log(e.ToString());
            MessageBox.Show(e.ToString(), "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button_scx_export_Click(object sender, EventArgs e)
        {
            try
            {
                var files = new List<string>();
                if (Directory.Exists(textBox_scx_export.Text))
                {
                    files.AddRange(Directory.GetFiles(textBox_scx_export.Text)
                        .Where(f => f.EndsWith(".scx", StringComparison.CurrentCultureIgnoreCase)));
                }
                else
                {
                    files.Add(textBox_scx_export.Text);
                }
                var charset = File.ReadAllText(textBox_scx_charset.Text);
                var patch = new Dictionary<string, List<List<string>>>();
                foreach (var file in files)
                {
                    Log("[SCX Export] Exporting " + Path.GetFileName(file) + " ...");
                    using (var reader = new SCXReader(File.OpenRead(file), charset))
                    {
                        var result = new List<List<string>>();
                        try
                        {
                            for (int i = 0; !reader.EOF; i++)
                            {
                                var xd = reader.ReadString(i);
                                var row = new List<string>();
                                foreach (var t in xd.Tokens)
                                {
                                    if (t is TextToken text)
                                    {
                                        row.Add(text.Value);
                                    }
                                }
                                if (row.Count > 0)
                                {
                                    result.Add(row);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log(ex.Message);
                        }
                        patch.Add(Path.GetFileNameWithoutExtension(file), result);
                    }
                }
                var save = new SaveFileDialog
                {
                    Filter = "JSON|*.json",
                    DefaultExt = "log",
                    CheckPathExists = true
                };
                if (save.ShowDialog() == DialogResult.OK)
                {
                    using (var writer = new StreamWriter(save.OpenFile()))
                    {
                        writer.Write(JSON.ToNiceJSON(patch, new JSONParameters()
                        {
                            UseEscapedUnicode = false
                        }));
                    }
                }
                else
                {
                    textBox_log.Text = JSON.ToNiceJSON(patch, new JSONParameters()
                    {
                        UseEscapedUnicode = false
                    });
                }
            }
            catch (Exception ex) { Oops(ex); }
        }

        private void button_scx_apply_Click(object sender, EventArgs e)
        {
            try
            {
                var patch = JSON.ToObject<Dictionary<string, dynamic>>(File.ReadAllText(textBox_scx_patch.Text));
                if (!patch.ContainsKey("scx") || !(patch["scx"] is Dictionary<string, dynamic> scx))
                {
                    throw new Exception("Bad patch file: Key scx not found or not dictionary");
                }
                var sb = new StringBuilder();
                string charset = patch["charset_preset"] + patch["charset"];
                foreach (KeyValuePair<string, object> kv in scx)
                {
                    string target = Path.Combine(textBox_scx_target.Text, kv.Key + ".SCX");
                    if (!File.Exists(target + ".bak"))
                    {
                        continue;
                    }
                    sb.AppendLine("---------- " + kv.Key + " ----------");
                    using (var reader = new SCXReader(File.OpenRead(target + ".bak"), charset))
                    using (var writer = new SCXWriter(File.Open(target, FileMode.Create), charset))
                    {
                        if (!SCX.ApplyPatch((IList<object>)kv.Value, reader, writer, sb))
                        {
                            break;
                        }
                    }
                }
                sb.Append("\n");
                textBox_log.Text = sb.ToString();
                File.WriteAllText("R:/patch.log", sb.ToString());
            }
            catch (Exception ex) { Oops(ex); }
        }

        private void button_mpk_unpack_Click(object sender, EventArgs e)
        {
            try
            {
                var files = new List<string>();
                if (Directory.Exists(textBox_mpk_unpack_input.Text))
                {
                    files.AddRange(Directory.GetFiles(textBox_mpk_unpack_input.Text)
                        .Where(f => f.EndsWith(".mpk", StringComparison.CurrentCultureIgnoreCase)));
                }
                else
                {
                    files.Add(textBox_mpk_unpack_input.Text);
                }
                foreach (var file in files)
                {
                    Log("[MPK Unpack] Unpacking " + Path.GetFileName(file) + " ...");
                    var output = Path.Combine(textBox_mpk_unpack_output.Text, Path.GetFileNameWithoutExtension(file));
                    Directory.CreateDirectory(output);
                    using (var reader = new BinaryReader(File.OpenRead(file)))
                    {
                        var mpk = MPK.ReadFile(reader);
                        Log("[MPK Unpack] Found " + mpk.Entries.Count + " entries.");
                        foreach (var entry in mpk.Entries)
                        {
                            Log("[MPK Unpack] Unpacking " + entry.Name + "(" + entry.Size + ") ...");
                            File.WriteAllBytes(Path.Combine(output, entry.Name), entry.Data);
                        }
                    }
                }
            }
            catch (Exception ex) { Oops(ex); }
        }

        private void button_mpk_pack_Click(object sender, EventArgs e)
        {
            try
            {
                var mpk = new MPK();
                foreach (var file in Directory.GetFiles(textBox_mpk_pack_input.Text).OrderBy(f => f))
                {
                    if (checkBox_mpk_pack_ignore_bak.Checked && file.EndsWith(".bak"))
                    {
                        continue;
                    }
                    mpk.Entries.Add(new MPKEntry(Path.GetFileName(file), File.ReadAllBytes(file)));
                }
                Log("[MPK Pack] Created " + mpk.Entries.Count + " entries.");
                var target = textBox_mpk_pack_output.Text;
                if (Directory.Exists(target))
                {
                    target = Path.Combine(target, Path.GetFileName(textBox_mpk_pack_input.Text) + ".mpk");
                }
                using (var writer = new BinaryWriter(File.Open(target, FileMode.Create)))
                {
                    mpk.Write(writer);
                    Log("[MPK Pack] Write success, size: " + writer.BaseStream.Position + ", target: " + target);
                }
            }
            catch (Exception ex) { Oops(ex); }
        }
    }
}
