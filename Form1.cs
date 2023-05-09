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

namespace LABA3
{
    public partial class Form1 : Form
    {
        List<string> paths = new List<string> { };
        Bitmap currentBitmap;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.SelectedPath = @"D:\Study\4 sem\TechnicalVision\RoadSignsDataSet";

            if (folder.ShowDialog() == DialogResult.OK)
            {
                listBox1.Items.Clear();
                paths.Clear();

                string[] files = Directory.GetFiles(folder.SelectedPath);

                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file);

                    if (ext == ".bmp" || ext == ".png" || ext == ".jpg")
                    {
                        paths.Add(file);
                        listBox1.Items.Add(Path.GetFileName(file));
                    }
                }
            }
            folder.Dispose();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 0)
            {
                currentBitmap = new Bitmap(paths[listBox1.SelectedIndex]);
                pictureBox1.Height = currentBitmap.Height;
                pictureBox1.Width = currentBitmap.Width;
                pictureBox1.Image = currentBitmap;
            }

        }
    }
}
