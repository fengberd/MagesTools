using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MPKTools
{
    public partial class MainForm : Form
    {
        static readonly byte[] MPKHeader = new byte[] { 0x4D, 0x50, 0x4B, 0x00, 0x00, 0x00, 0x02, 0x00 };

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        public void MPKPack(string input, string output)
        {
            using (var target = new BinaryWriter(File.Open(output, FileMode.Create)))
            {
                var files = Directory.GetFiles(input).OrderBy(f => f).ToArray();
                target.Write(MPKHeader);
                target.Write(files.Length);
                int i = 0;
                var lol = new byte[1780];
                long offset = MPKHeader.Length + 4 + files.Length * 256 + lol.Length;
                foreach (var file in files)
                {
                    target.Write(new byte[56], 0, 56);
                    target.Write(i++);

                    target.Write(offset);

                    long length = new FileInfo(file).Length;
                    offset += length;

                    target.Write(length);
                    // Can be compressed size or sth? ↓
                    target.Write(length);

                    var name = Encoding.UTF8.GetBytes(Path.GetFileName(file));
                    target.Write(name, 0, name.Length);
                    target.Write(new byte[172], 0, 172 - name.Length);
                }
                target.Write(lol, 0, lol.Length); // LOL
                foreach (var file in files)
                {
                    using (var read = File.OpenRead(file))
                    {
                        read.CopyTo(target.BaseStream);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MPKPack(textBox1.Text, textBox2.Text);
        }
    }
}
