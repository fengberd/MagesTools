using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

using fastJSON;

using Mages.Script;

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
            textBox_log.AppendText(data);
        }

        private void button_scx_load_Click(object sender, EventArgs e)
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
                    if (!File.Exists("D:/Workspace/SG/script/" + kv.Key + "_old.SCX"))
                    {
                        continue;
                    }
                    sb.AppendLine("---------- " + kv.Key + " ----------");
                    using (var reader = new SCXReader(File.OpenRead("D:/Workspace/SG/script/" + kv.Key + "_old.SCX"), charset))
                    using (var writer = new SCXWriter(File.Open("D:/Workspace/SG/script/" + kv.Key + ".SCX", FileMode.Create), charset))
                    {
                        if (!SCX.ApplyPatch((IList<object>)kv.Value, reader, writer, sb))
                        {
                            break;
                        }
                    }
                }
                File.WriteAllText("R:/patch.log", sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button_mpk_pack_Click(object sender, EventArgs e)
        {

        }
    }
}
