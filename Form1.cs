using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LABA3
{
    public partial class Form1 : Form
    {
        List<string> paths = new List<string> { };
        bool[,] visited;
        Bitmap currentBitmap, mask, bitmapForRect;
        readonly Pen penForRect = new Pen(Color.Red, 1);
        List<myPixel> pixels;
        List<myCluster> clusters;
        public Form1()
        {
            InitializeComponent();
        }
        public void DoMask(byte[] RGB, Bitmap inputBitmap, out Bitmap outMask, out List<myPixel> outputPixels)
        {
            outputPixels = new List<myPixel>();
            myPixel.CountPixels = 0;
            myPixel.CountClusters = -1;
            outMask = new Bitmap(inputBitmap.Width, inputBitmap.Height);

            var g = Graphics.FromImage(mask);
            g.Clear(Color.Black);
            g.Dispose();

            for (int y = 0; y < inputBitmap.Height; y++)
            {
                for (int x = 0; x < inputBitmap.Width; x++)
                {
                    var currentPixel = inputBitmap.GetPixel(x, y);

                    if (currentPixel.R > RGB[0] && currentPixel.R < RGB[1] &&
                        currentPixel.G > RGB[2] && currentPixel.G < RGB[3] &&
                        currentPixel.B > RGB[4] && currentPixel.B < RGB[5])
                    {
                        outputPixels.Add(new myPixel(x, y));
                        outMask.SetPixel(x, y, Color.White);
                    }
                }
            }

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
            if (listBox1.SelectedIndex != -1)
            {
                currentBitmap = new Bitmap(paths[listBox1.SelectedIndex]);
                pictureBox1.Height = currentBitmap.Height;
                pictureBox1.Width = currentBitmap.Width;
                pictureBox1.Image = currentBitmap;
                PanelForMask.Visible = true;
                panelForSearch.Visible = false;
                listBox2.Items.Clear();
                checkBox1.Checked = false;
            }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = checkBox1.Checked ? mask : currentBitmap;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            clusters = new List<myCluster>();
            myCluster.Count = -1;
            Clustering(int.Parse(textBox8.Text), int.Parse(textBox7.Text), pixels,ref clusters);
            for (int i = 0; i < clusters.Count; i++)
            {
                listBox2.Items.Add(clusters[i]);
            }
        }

        public void Clustering(int radiusMin, int radiusMax,List<myPixel> pixels,ref List<myCluster> clusters)
        {
            for (int i = 0; i < pixels.Count; i++)
            {
                if (pixels[i].cluster == -1)
                {
                    pixels[i].cluster = pixels[i].GetCurrentClusterId(true);
                    clusters.Add(new myCluster(pixels[i]));

                    for (int j = 0; j < pixels.Count; j++)
                    {
                        if (pixels[j].cluster == -1)
                        {
                            int len = clusters[myCluster.Count].LenToPoint(pixels[j]);

                            if (len <= radiusMin && len >= radiusMax)
                            {
                                pixels[j].cluster = pixels[j].GetCurrentClusterId();
                                clusters[myCluster.Count].CheckPoints(pixels[j]);
                            }
                        }
                    }
                }
            }
            clusters.RemoveAll(RemoveFakeClusters);
        }
        private bool RemoveFakeClusters(myCluster cluster)
        {
            return cluster.Height <= 5 || cluster.Width <= 5;
        }
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBox2.SelectedIndex;
            if (index != -1)
            {
                //bitmapForRect = (Bitmap)currentBitmap.Clone();
                bitmapForRect = (Bitmap)currentBitmap;
                Graphics g = Graphics.FromImage(bitmapForRect);
                g.DrawRectangle(penForRect, clusters[index].GetRectangle());
                pictureBox1.Image = bitmapForRect;
                g.Dispose();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    ChangeTextBox(new string[,] { { "100", "255" }, { "0", "60" }, { "0", "50" } });
                    break;
                case 1:
                    ChangeTextBox(new string[,] { { "0", "40" }, { "0", "80" }, { "70", "255" } });
                    break;
                case 2:
                    ChangeTextBox(new string[,] { { "100", "255" }, { "100", "255" }, { "0", "60" } });
                    break;
                default: break;
            }
        }
        private void ChangeTextBox(string[,] RGB)
        {
            textBox1.Text = RGB[0, 0]; //Rmin
            textBox2.Text = RGB[0, 1]; //Rmax

            textBox3.Text = RGB[1, 0]; //Gmin
            textBox4.Text = RGB[1, 1]; //Gmax

            textBox5.Text = RGB[2, 0]; //Bmin
            textBox6.Text = RGB[2, 1]; //Bmax
        }

        private void button4_Click(object sender, EventArgs e)
        {
            byte[] cr = new byte[6];

            if (byte.TryParse(textBox1.Text, out cr[0]) && byte.TryParse(textBox2.Text, out cr[1]) && byte.TryParse(textBox3.Text, out cr[2]) &&
                byte.TryParse(textBox4.Text, out cr[3]) && byte.TryParse(textBox5.Text, out cr[4]) && byte.TryParse(textBox6.Text, out cr[5]))
            {
                DoMask(cr, currentBitmap, out mask, out pixels);
                panelForSearch.Visible = true;
                checkBox1.Checked = false;

            }
            else
            {
                MessageBox.Show("Ошибка ввода данных!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            checkBox1.Visible = true;
            pictureBox1.Image = currentBitmap;
        }
    }
}
