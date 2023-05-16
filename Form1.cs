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
        List<myPoint> clust, trueClust;
        int countClusters = 0;
        int numberOfCycles = 0;
        public Form1()
        {
            InitializeComponent();
        }
        public void DoMask(byte[] RGB, Bitmap inputBitmap, out Bitmap outMask, out bool[,] visitedArr)
        {
            outMask = new Bitmap(inputBitmap.Width, inputBitmap.Height);
            var g = Graphics.FromImage(mask);
            visitedArr = new bool[mask.Height, mask.Width];
            g.Clear(Color.White);
            g.Dispose();
            for (int y = 0; y < inputBitmap.Height; y++)
            {
                for (int x = 0; x < inputBitmap.Width; x++)
                {
                    var currentPixel = inputBitmap.GetPixel(x, y);
                    if (currentPixel.R < RGB[0] || currentPixel.R > RGB[1] ||
                        currentPixel.G < RGB[2] || currentPixel.G > RGB[3] ||
                        currentPixel.B < RGB[4] || currentPixel.B > RGB[5])
                    {
                        // inputBitmap.SetPixel(x, y, Color.Black);
                        mask.SetPixel(x, y, Color.Black);
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
            clust = new List<myPoint>();
            trueClust = new List<myPoint>();
            countClusters = 0;

            for (int y = 0; y < mask.Height; y++)
            {
                for (int x = 0; x < mask.Width; x++)
                {
                    if (mask.GetPixel(x, y) != Color.FromArgb(0, 0, 0) && !visited[y, x])
                    {
                        clust.Add(new myPoint { X0 = x, X1 = x, Y0 = y, Y1 = y });
                        /*                        numberOfCycles = 0;
                                                SearchAround(x, y);*/
                        clust[countClusters] = CheckClusters(visited, mask, clust[countClusters], int.Parse(textBox9.Text), int.Parse(textBox7.Text), int.Parse(textBox8.Text));
                        listBox2.Items.Add(clust[countClusters].stringForListbox());
                        countClusters++;
                    }
                }
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex <= clust.Count)
            {
                bitmapForRect = (Bitmap)currentBitmap.Clone();
                Graphics g = Graphics.FromImage(bitmapForRect);
                g.DrawRectangle(penForRect, clust[listBox2.SelectedIndex].RetRect());
                pictureBox1.Image = bitmapForRect;
                g.Dispose();
            }
        }
        private void SearchAround(int x0, int y0, int n = 3)
        {
            if (numberOfCycles < 5000)
            {
                for (int y = y0 - n; y < y0 + n; y++)
                {
                    for (int x = x0 - n; x < x0 + n; x++)
                    {
                        if (x >= 0 && x < currentBitmap.Width && y >= 0 && y < currentBitmap.Height)
                        {
                            if (mask.GetPixel(x, y) == Color.FromArgb(255, 255, 255))
                            {
                                if (!visited[y, x])
                                {
                                    visited[y, x] = true;
                                    clust[countClusters].ChangeCoords(x, y);
                                    numberOfCycles++;
                                    SearchAround(x, y);
                                }
                            }
                        }
                    }
                }
            }
        }

        private myPoint CheckClusters(bool[,] data, Bitmap inputMaskBitmap, myPoint inputPoint, int density, int rMin, int rMax)
        {
            int n = 5;
            myPoint temp = new myPoint() { X0 = inputPoint.X0, X1 = inputPoint.X1, Y0 = inputPoint.Y0, Y1 = inputPoint.Y1 };

            for (int x = temp.X0 - n; x < temp.X1 + n; x++)
            {
                if (x >= 0 && x < inputMaskBitmap.Width)
                {
                    for (int y = temp.Y0 - n; y < temp.Y1 + n; y++)
                    {
                        if (y >= 0 && y < inputMaskBitmap.Height)
                        {
                            if (!data[y, x])
                            {
                                temp.CountPixel++;
                                data[y, x] = true;
                                if (inputMaskBitmap.GetPixel(x, y) == Color.FromArgb(255, 255, 255))
                                {
                                    temp.CountWhitePixel++;
                                    temp.ChangeCoords(x, y);
                                }
                            }
                        }
                    }
                }
            }
            return temp;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            byte[] cr = new byte[6];
            if (byte.TryParse(textBox1.Text, out cr[0]) && byte.TryParse(textBox2.Text, out cr[1]) && byte.TryParse(textBox3.Text, out cr[2]) &&
                byte.TryParse(textBox4.Text, out cr[3]) && byte.TryParse(textBox5.Text, out cr[4]) && byte.TryParse(textBox6.Text, out cr[5]))
            {
                DoMask(cr, currentBitmap, out mask, out visited);
                panelForSearch.Visible = true;

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
