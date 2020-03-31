using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;

using fastJSON;

using Mages.Script;
using Mages.Package;

namespace EasyPatcher
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            linkLabel_version.Text = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public void Log(string data)
        {
            Invoke(new Action(() => textBox_log.AppendText(DateTime.Now.ToString() + " " + data + "\n")));
        }

        public void Oops(string e)
        {
            Log(e);
            MessageBox.Show(e, "致命错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button_patch_Click(object sender, EventArgs e)
        {
            textBox_log.Clear();
            button_patch.Enabled = false;
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    var usrdir = Path.Combine(textBox1.Text, "USRDIR");
                    Log("[MPK] 正在寻找 USRDIR...");
                    if (!Directory.Exists(usrdir))
                    {
                        Oops("[MPK] USRDIR 不存在, 请检查你的目录设置.");
                        Invoke(new Action(() => button_patch.Enabled = true));
                        return;
                    }

                    var bakdir = usrdir + ".bak";
                    if (!Directory.Exists(bakdir))
                    {
                        Log("[MPK] 备份文件夹不存在, 创建中...");
                        Directory.CreateDirectory(bakdir);
                    }

                    var MPKs = new Dictionary<string, MPK>();
                    var mpkFiles = new string[] { "script.mpk", "system.mpk" };
                    foreach (var file in mpkFiles)
                    {
                        if (!File.Exists(Path.Combine(bakdir, file)))
                        {
                            Log("[MPK] 正在备份 " + file + "...");
                            File.Copy(Path.Combine(usrdir, file), Path.Combine(bakdir, file));
                        }
                        Log("[MPK] 正在加载 " + file + "...");
                        using (var reader = new BinaryReader(File.OpenRead(Path.Combine(bakdir, file))))
                        {
                            MPKs.Add(file, MPK.ReadFile(reader));
                        }
                    }

                    Log("[SYS] 正在应用字库补丁...");
                    var system = MPKs["system.mpk"].Entries.ToDictionary(k => k.Name, v => v);
                    system["FONT.DDS"].SetData(File.ReadAllBytes("berd/FONT.DDS"));
                    system["FONT2.DDS"].SetData(File.ReadAllBytes("berd/FONT2.DDS"));
                    system["OPTION.DDS"].SetData(File.ReadAllBytes("berd/OPTION.DDS"));

                    Log("[SCX] 初始化 SCX 补丁过程...");
                    var patch = JSON.ToObject<Dictionary<string, dynamic>>(File.ReadAllText("berd/patch.json"));
                    if (!patch.ContainsKey("scx") || !(patch["scx"] is Dictionary<string, dynamic> scx))
                    {
                        Oops("[SCX] 补丁数据加载失败");
                        return;
                    }
                    var script = MPKs["script.mpk"].Entries.ToDictionary(k => k.Name, v => v);
                    string charset = patch["charset_preset"] + patch["charset"];
                    foreach (KeyValuePair<string, object> kv in scx)
                    {
                        if (!script.ContainsKey(kv.Key + ".SCX"))
                        {
                            Log("[SCX] 无法找到文件 " + kv.Key + ".SCX");
                            continue;
                        }
                        Log("[SCX] 正在对 " + kv.Key + ".SCX 应用补丁...");

                        using (var ms = new MemoryStream())
                        using (var reader = new SCXReader(script[kv.Key + ".SCX"].Data, charset))
                        using (var writer = new SCXWriter(ms, charset))
                        {
                            var sb = new StringBuilder();
                            if (!SCX.ApplyPatch((IList<object>)kv.Value, reader, writer, sb))
                            {
                                Log(sb.ToString());
                                Oops("[SCX] 补丁应用失败");
                                return;
                            }
                            script[kv.Key + ".SCX"].SetData(ms.ToArray());
                        }
                    }

                    foreach (var mpk in MPKs)
                    {
                        Log("[MPK] 正在重新打包 " + mpk.Key + "...");
                        using (var writer = new BinaryWriter(File.Open(Path.Combine(usrdir, mpk.Key), FileMode.Create)))
                        {
                            mpk.Value.Write(writer);
                            Log("[MPK] 打包成功: " + writer.BaseStream.Position);
                        }
                    }

                    MessageBox.Show("补丁操作完成, 请检查游戏是否能正常运行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Log("[FENGberd] 操作完成, 请检查游戏是否能正常运行");
                }
                catch (Exception ex)
                {
                    Oops(ex.ToString());
                    Log("[FENGberd] 发生致命错误");
                }
                Invoke(new Action(() => button_patch.Enabled = true));
            });
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            var save = new SaveFileDialog
            {
                Filter = "日志文件(*.log)|*.log",
                DefaultExt = "log",
                CheckPathExists = true
            };
            if (save.ShowDialog() == DialogResult.OK)
            {
                using (var writer = new StreamWriter(save.OpenFile()))
                {
                    writer.Write(textBox_log.Text);
                }
            }
        }

        private void button_select_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void linkLabel_version_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/fengberd/MagesTools");
        }
    }
}
