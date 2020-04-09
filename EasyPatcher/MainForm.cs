using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.IO.Compression;
using System.Collections.Generic;

using fastJSON;

using Mages.Script;
using Mages.Package;

namespace EasyPatcher
{
    public partial class MainForm : Form
    {
        public const string PATCH_DIR = "berd/";

        public MainForm()
        {
            InitializeComponent();
            linkLabel_version.Text = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var meta = JSON.ToObject<Dictionary<string, dynamic>>(File.ReadAllText(PATCH_DIR + "meta.json"));
            Text += " - " + meta["name"];
            textBox_path.Text = meta["default_path"];
            pictureBox_main.ImageLocation = Path.GetFullPath(PATCH_DIR + meta["image"]);
            textBox_log.Text = (meta["notice"] as string).Replace("\n", Environment.NewLine);
        }

        public void Log(string data)
        {
            Invoke(new Action(() => textBox_log.AppendText(DateTime.Now.ToString() + " " + data + Environment.NewLine)));
        }

        public void Oops(string e)
        {
            Log(e);
            MessageBox.Show(e, "致命错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public bool patchSCX(Dictionary<string, MPKEntry> mpk, string charset, Dictionary<string, dynamic> scx)
        {
            Log("[SCX] 正在应用 SCX 补丁...");
            foreach (KeyValuePair<string, dynamic> kv in scx)
            {
                if (!mpk.ContainsKey(kv.Key))
                {
                    Oops("[SCX] 无法找到文件 " + kv.Key);
                    return false;
                }
                Log("[SCX] 正在对 " + kv.Key + " 应用补丁...");

                using (var ms = new MemoryStream())
                using (var reader = new SCXReader(mpk[kv.Key].Data, charset))
                using (var writer = new SCXWriter(ms, charset))
                {
                    var sb = new StringBuilder();
                    if (!SCX.ApplyPatch(kv.Value, reader, writer, sb))
                    {
                        Log(sb.ToString());
                        Oops("[SCX] 补丁应用失败");
                        return false;
                    }
                    mpk[kv.Key].SetData(ms.ToArray());
                }
            }
            return true;
        }

        public bool patchFile(Dictionary<string, MPKEntry> mpk, Dictionary<string, dynamic> data)
        {
            Log("[FILE] 正在应用文件补丁...");
            foreach (KeyValuePair<string, dynamic> kv in data)
            {
                if (!mpk.ContainsKey(kv.Key))
                {
                    Log("[FILE] 无法找到文件 " + kv.Key);
                    continue;
                }
                Log("[FILE] 正在替换文件 " + kv.Key + " ...");
                using (var ms = new MemoryStream(Convert.FromBase64String(kv.Value)))
                using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                using (var output = new MemoryStream())
                {
                    gzip.CopyTo(output);
                    mpk[kv.Key].SetData(output.ToArray());
                }
            }
            return true;
        }

        private void textBox_path_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBox_path_DragDrop(object sender, DragEventArgs e)
        {
            var box = sender as TextBox;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                box.Text = (e.Data.GetData(DataFormats.FileDrop) as string[])[0];
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                box.Text = e.Data.GetData(DataFormats.Text) as string;
            }
        }

        private void button_patch_Click(object sender, EventArgs e)
        {
            textBox_log.Clear();
            button_patch.Enabled = false;
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    var usrdir = Path.Combine(textBox_path.Text, "USRDIR");
                    Log("[BERD] 正在寻找 USRDIR...");
                    if (!Directory.Exists(usrdir))
                    {
                        Oops("[BERD] USRDIR 不存在, 请检查你的目录设置.");
                        Invoke(new Action(() => button_patch.Enabled = true));
                        return;
                    }

                    var bakdir = usrdir + ".bak";
                    if (!Directory.Exists(bakdir))
                    {
                        Log("[BERD] 备份文件夹不存在, 创建中...");
                        Directory.CreateDirectory(bakdir);
                    }

                    foreach (var patch in Directory.GetFiles(PATCH_DIR, "*.json").Select(p => JSON.ToObject<Dictionary<string, dynamic>>(File.ReadAllText(p))))
                    {
                        if (!patch.ContainsKey("file"))
                        {
                            continue;
                        }
                        string file = patch["file"];
                        if (!File.Exists(Path.Combine(bakdir, file)))
                        {
                            Log("[BERD] 正在备份 " + file + "...");
                            File.Copy(Path.Combine(usrdir, file), Path.Combine(bakdir, file));
                        }

                        MPK mpk = null;
                        Log("[MPK] 正在加载 " + file + "...");
                        using (var reader = new BinaryReader(File.OpenRead(Path.Combine(bakdir, file))))
                        {
                            mpk = MPK.ReadFile(reader);
                        }

                        var entries = mpk.Entries.ToDictionary(k => k.Name, v => v);
                        switch (patch["type"])
                        {
                        case "scx":
                            if (!patchSCX(entries, patch["charset_preset"] + patch["charset"], patch["data"]))
                            {
                                return;
                            }
                            break;
                        case "file":
                            if (!patchFile(entries, patch["data"]))
                            {
                                return;
                            }
                            break;
                        default:
                            Oops("未知补丁类型");
                            Invoke(new Action(() => button_patch.Enabled = true));
                            return;
                        }

                        Log("[MPK] 正在打包 " + file + "...");
                        using (var writer = new BinaryWriter(File.Open(Path.Combine(usrdir, file), FileMode.Create)))
                        {
                            mpk.Write(writer);
                            Log("[MPK] 打包成功: " + writer.BaseStream.Position);
                        }
                    }

                    MessageBox.Show("补丁应用完成, 请检查游戏是否能正常运行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void button_delete_bak_Click(object sender, EventArgs e)
        {
            try
            {
                var usrdir = Path.Combine(textBox_path.Text, "USRDIR");
                if (!Directory.Exists(usrdir))
                {
                    Oops("USRDIR 不存在, 请检查你的目录设置.");
                    return;
                }
                var bakdir = usrdir + ".bak";
                if (!Directory.Exists(bakdir))
                {
                    Oops("未找到备份文件夹.");
                    return;
                }
                if (MessageBox.Show("确认要删除备份文件夹吗?\n删除后要撤销补丁必须重新验证游戏完整性\n并且可能对未来的补丁覆盖造成影响", "操作确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                {
                    return;
                }
                Directory.Delete(bakdir, true);
                MessageBox.Show("备份文件夹已删除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Oops(ex.ToString());
            }
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
                textBox_path.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void linkLabel_version_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/fengberd/MagesTools");
        }
    }
}
