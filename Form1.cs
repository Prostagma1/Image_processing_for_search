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
        Bitmap currentBitmap, mask, bitmapForRect, zoomedRoadSignal, zoomedMask;
        readonly Pen penForRect = new Pen(Color.Green, 2);
        Bitmap[] teamplates = new Bitmap[5];
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
                panelForZoomed.Visible = false;
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

            Clustering(int.Parse(textBox8.Text), int.Parse(textBox7.Text), pixels, ref clusters);
            MergingClusters(ref clusters);
            DensityCalculation(mask, ref clusters);
            for (int i = 0; i < clusters.Count; i++)
            {
                listBox2.Items.Add($"{i + 1} кластер");
            }
        }

        public void Clustering(int radiusMin, int radiusMax, List<myPixel> pixels, ref List<myCluster> clusters)
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
        }
        public void MergingClusters(ref List<myCluster> clusters)
        {
            clusters.RemoveAll(RemoveVerySmallClusters);
            for (int i = 0; i < clusters.Count; i++)
            {
                for (int j = 0; j < clusters.Count; j++)
                {
                    if (i != j)
                    {
                        if ((clusters[j].Start.X >= clusters[i].Start.X && clusters[j].Start.X <= clusters[i].End.X) ||
                            (clusters[j].End.X >= clusters[i].Start.X && clusters[j].End.X <= clusters[i].End.X))
                        {
                            if ((clusters[j].Start.Y >= clusters[i].Start.Y && clusters[j].Start.Y <= clusters[i].End.Y) ||
                                (clusters[j].End.Y >= clusters[i].Start.Y && clusters[j].End.Y <= clusters[i].End.Y))
                            {
                                clusters[j].DeleteThisPoint = true;
                                clusters[i].CheckPoints(clusters[j].Start);
                                clusters[i].CheckPoints(clusters[j].End);
                            }
                        }
                    }
                }
                clusters.RemoveAll(RemovalOfSiftedClusters);
            }
        }
        public void DensityCalculation(Bitmap inputMaskBitmap, ref List<myCluster> clusters)
        {
            for (byte i = 0; i < clusters.Count; i++)
            {
                for (int x = clusters[i].Start.X; x <= clusters[i].End.X; x++)
                {
                    for (int y = clusters[i].Start.Y; y <= clusters[i].End.Y; y++)
                    {
                        var currentPixel = inputMaskBitmap.GetPixel(x, y);
                        if (currentPixel.R >= 10)
                        {
                            clusters[i].CountWhitePixel++;
                        }
                    }
                }
            }
            clusters.RemoveAll(RemoveClusterByDensity);
        }
        private bool RemoveVerySmallClusters(myCluster cluster)
        {
            return cluster.Height <= 3 || cluster.Width <= 3;
        }
        private bool RemovalOfSiftedClusters(myCluster cluster)
        {
            return cluster.DeleteThisPoint;
        }
        private bool RemoveClusterByDensity(myCluster cluster)
        {
            return (cluster.Density() <= (float.Parse(textBox9.Text) / 100));
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBox2.SelectedIndex;
            if (index != -1)
            {
                bitmapForRect = (Bitmap)currentBitmap.Clone();
                //bitmapForRect = (Bitmap)currentBitmap;

                Graphics g = Graphics.FromImage(bitmapForRect);
                g.DrawRectangle(penForRect, clusters[index].GetRectangle());
                pictureBox1.Image = bitmapForRect;
                g.Dispose();

                panelForZoomed.Visible = true;

                CopyAndZoomPic(currentBitmap, mask, clusters[index].GetRectangle(), out zoomedRoadSignal, out zoomedMask);

                if (checkBox2.Checked)
                {
                    SignDefinition();
                }
            }
        }
        private void CopyAndZoomPic(Bitmap inputBitmap, Bitmap inputMask, Rectangle rectangleForZoom, out Bitmap outputBitmap, out Bitmap outputMask)
        {
            Bitmap tempBitmap = new Bitmap(rectangleForZoom.Width + 1, rectangleForZoom.Height + 1);
            Bitmap tempBitmapMask = new Bitmap(rectangleForZoom.Width + 1, rectangleForZoom.Height + 1);

            for (int x = rectangleForZoom.X; x <= rectangleForZoom.X + rectangleForZoom.Width; x++)
            {
                for (int y = rectangleForZoom.Y; y <= rectangleForZoom.Y + rectangleForZoom.Height; y++)
                {
                    var colorPixel = inputBitmap.GetPixel(x, y);
                    var colorPixelMask = inputMask.GetPixel(x, y);
                    tempBitmap.SetPixel(x - rectangleForZoom.X, y - rectangleForZoom.Y, colorPixel);
                    tempBitmapMask.SetPixel(x - rectangleForZoom.X, y - rectangleForZoom.Y, colorPixelMask);
                }
            }
            outputBitmap = new Bitmap(tempBitmap, 128, 128);
            outputMask = new Bitmap(tempBitmapMask, 128, 128);
            for (int x = 0; x < outputMask.Width; x++)
            {
                for (int y = 0; y < outputMask.Height; y++)
                {
                    var colorPixelMask = outputMask.GetPixel(x, y).R > 10 ? Color.White : Color.Black;
                    outputMask.SetPixel(x, y, colorPixelMask);
                }
            }
        }
        private void SignDefinition()
        {
            float maxScore = 0;
            byte numMask = 0;
            Bitmap selectedRoadSign = new Bitmap(teamplates[0].Width, teamplates[0].Height);
            for (byte i = 0; i < teamplates.Length; i++)
            {
                float countPositive = 0;
                int allPixel = teamplates[i].Width * teamplates[i].Height;

                for (int x = 0; x < teamplates[i].Width; x++)
                {
                    for (int y = 0; y < teamplates[i].Height; y++)
                    {
                        var filterPixel = teamplates[i].GetPixel(x, y).R;
                        var maskPixel = zoomedMask.GetPixel(x, y).R;

                        if (maskPixel == 0)
                        {
                            switch (filterPixel)
                            {
                                case 5:
                                    selectedRoadSign.SetPixel(x, y, Color.FromArgb(0, 0, 139)); // Тёмно-синий – пиксель не учитывается, но он «черный»
                                    allPixel--;
                                    break;
                                case 255:
                                    selectedRoadSign.SetPixel(x, y, Color.FromArgb(121, 6, 4)); // Темно-красный – пиксель должен быть «светлым» (на шаблоне), но он «черный»
                                    break;
                                case 0:
                                    selectedRoadSign.SetPixel(x, y, Color.FromArgb(23, 114, 69)); // Темно-зелёный – пиксель должен быть «черным» (на шаблоне) и он «черный»
                                    countPositive++;
                                    break;
                            }
                        }
                        else
                        {
                            switch (filterPixel)
                            {
                                case 5:
                                    selectedRoadSign.SetPixel(x, y, Color.FromArgb(0, 0, 255)); // Светло-синий – пиксель не учитывается, но он «белый» (после фильтра)
                                    allPixel--;
                                    break;
                                case 255:
                                    selectedRoadSign.SetPixel(x, y, Color.FromArgb(144, 238, 144)); // Светло-зелёный – пиксель должен быть «белым» (на шаблоне) и он «белый»
                                    countPositive++;
                                    break;
                                case 0:
                                    selectedRoadSign.SetPixel(x, y, Color.FromArgb(255, 0, 0)); // Светло-красный – пиксель должен быть «темным» (на шаблоне), но он «белый»
                                    break;
                            }
                        }

                    }
                }
                if (countPositive / allPixel >= maxScore)
                {
                    maxScore = countPositive / allPixel;
                    zoomedRoadSignal = (Bitmap)selectedRoadSign.Clone();
                    numMask = i;
                }
            }
            pictureBox2.Image = zoomedRoadSignal;
            pictureBox3.Image = teamplates[numMask];
            pictureBox4.Image = zoomedMask;
            label6.Text = $"Степень совпадения с шаблоном = {maxScore*100}%";
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            panelForAuto.Visible = checkBox3.Checked;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    ChangeTextBox(new string[,] { { "100", "255" }, { "0", "60" }, { "0", "50" } }); // Для красных
                    break;
                case 1:
                    ChangeTextBox(new string[,] { { "0", "40" }, { "0", "100" }, { "70", "255" } }); // Для синих
                    break;
                case 2:
                    ChangeTextBox(new string[,] { { "100", "255" }, { "100", "255" }, { "0", "60" } }); // Для жёлтых 
                    break;
                default: break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 1; i < 6; i++)
            {
                teamplates[i - 1] = new Bitmap($@"D:\Study\4 sem\TechnicalVision\Template\{i}.png");
            }
            comboBox1.SelectedIndex = 1;
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
            panelForZoomed.Visible = false;

            pictureBox1.Image = currentBitmap;
        }
    }
}
