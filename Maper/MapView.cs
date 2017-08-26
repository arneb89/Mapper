using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Maper
{
    public partial class MapView : Form
    {
        private int xBegin, yBegin;
        private int plotHeight, plotWidth;
        private string plotTitle, plotLabelX, plotLabelY;

        private Color gridsColor = Color.Black;
        private Color minColor;
        private Color maxColor;

        private int longLinesNumber = 4;
        private int latLinesNumber = 4;
        private double longLinesWidth = 1; // in deg
        private double latLinesWidth = 1;

        Bitmap bitmap;
        
        private Star star;
        
        public MapView()
        {
            InitializeComponent();
            this.xBegin = (int)(70);
            this.yBegin = (int)(40);
            this.plotWidth = (int)(this.Width - this.xBegin - 40);
            this.plotHeight = (int)(this.plotWidth * 0.5);
            
            this.plotTitle = "Map";
            this.plotLabelX = "Longitude";
            this.plotLabelY = "Latitude";

            this.maxColor = Color.White;
            this.minColor = Color.Black;
            this.panelColorMax.BackColor = this.maxColor;
            this.panelColorMin.BackColor = this.minColor;

            this.gridsColor = Color.Black;
            this.panelGridColor.BackColor = this.gridsColor;

            this.longLinesNumber = int.Parse(txtLongitudeLinesNumber.Text);
            this.latLinesNumber = int.Parse(txtLatitudeLinesNumber.Text);

            this.longLinesWidth = double.Parse(txtLongLinesWidth.Text.Replace(".", ","));
            this.latLinesWidth = double.Parse(txtLatLinesWidth.Text.Replace(".", ","));
        }

        public void SetStar(Star star)
        {
            this.star = star;
            this.CreateBitmap();
        }

        private void DrawLatitAxis(Graphics g, int x, int ymin, int ymax, int pointsNumber)
        {
            int axisWidth = ymax - ymin;
            Pen p = new Pen(Color.Black);
            g.DrawLine(p, x, ymin, x, ymax);
            Font tFont = new Font("Arial", 12, FontStyle.Regular);
            for (int i = 0; i < pointsNumber; i++)
            {
                g.DrawLine(p, x - 5, ymin + i * axisWidth / (pointsNumber - 1), x + 5, 
                    ymin + i * axisWidth / (pointsNumber - 1));
                g.DrawString(string.Format("{0}", 90-i * 180.0 / (pointsNumber - 1)), 
                    tFont, Brushes.Black, new PointF(x - 30, ymin + i * axisWidth / (pointsNumber - 1)));
            }
        }

        private void DrawLongitAxis(Graphics g, int y, int xmin, int xmax, int pointsNumber)
        {
            int axisWidth = xmax - xmin;
            Pen p = new Pen(Color.Black);
            g.DrawLine(p, xmin, y, xmax, y);
            Font tFont = new Font("Arial", 12, FontStyle.Italic);
            for (int i = 0; i < pointsNumber; i++)
            {
                g.DrawLine(p, xmin + i * axisWidth / (pointsNumber - 1), y - 5, xmin + i * axisWidth / (pointsNumber - 1), y + 5);
                g.DrawString(string.Format("{0}", i * 360.0 / (pointsNumber - 1)), tFont, Brushes.Black, new PointF(xmin + i * axisWidth / (pointsNumber - 1) - 5, y + 7));
            }
        }

        private void DrawSpots(Graphics g)
        {
            if (this.star == null) { return; }
            if (this.star.circSpots == null) { return; }

            double dth = Math.PI / this.plotHeight;
            double dph = Math.PI * 2 / this.plotWidth;
            double scale = (double)this.plotWidth / (2 * Math.PI);

            double theta, phi, cosr;
            Rectangle rect;
            for (int i = 0; i < this.plotHeight; i++)
            {
                for (int j = 0; j < this.plotWidth; j++)
                {
                    theta = i * dth + 0.5 * dth;
                    phi = j * dph + 0.5 * dph;
                    for (int s = 0; s < this.star.circSpots.Length; s++)
                    {
                        cosr = Math.Cos(theta) * Math.Cos(this.star.circSpots[s].ThetaOfSpotCenter) +
                            Math.Sin(theta) * Math.Sin(this.star.circSpots[s].ThetaOfSpotCenter) *
                            Math.Cos(this.star.circSpots[s].PhiOfSpotCenterAtZeroPhase - phi);
                        double dist = Math.Acos(cosr);
                        if (dist < this.star.circSpots[s].Radius)
                        {
                            rect = new Rectangle((int)(j * dph * scale) + this.xBegin, 
                                (int)(i * dth * scale) + this.yBegin,
                                1, 1);
                            g.DrawRectangle(Pens.Black, rect);
                        }
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            // title and labels;
            this.DrawTitle(txtTitle.Text);
            this.DrawXLabel(txtXLabel.Text);
            this.DrawYLabel(txtYLabel.Text);
            //
            this.DrawLatitAxis(g, this.xBegin - 10, this.yBegin, 
                this.yBegin + this.plotHeight, this.latLinesNumber);
            this.DrawLongitAxis(g, this.yBegin + this.plotHeight + 10, this.xBegin, 
                this.xBegin + this.plotWidth, this.longLinesNumber + 1);

            if (bitmap != null)
            {
                g.DrawImage(this.bitmap, xBegin, yBegin, plotWidth, plotHeight);
            }

            g.Dispose();
        }

        private void DrawGrid(Graphics g, int xGridsNum, int yGridsNum)
        {
            int z = 0;
            for (int i = 0; i < xGridsNum; i++)
            {
                z = this.xBegin + (int)(i * this.plotWidth / (double)(xGridsNum - 1));
                g.DrawLine(Pens.Black, z, this.yBegin, z, this.yBegin + this.plotHeight);
            }
            for (int i = 0; i < yGridsNum; i++)
            {
                z = this.yBegin + (int)(i * this.plotHeight / (double)(yGridsNum - 1));
                g.DrawLine(Pens.Black, this.xBegin, z, this.xBegin+this.plotWidth, z);
            }
            this.DrawLatitAxis(g, this.xBegin, this.yBegin, this.yBegin + this.plotHeight, yGridsNum);
            this.DrawLongitAxis(g, this.yBegin + this.plotHeight, this.xBegin, 
                this.xBegin + this.plotWidth, xGridsNum);

        }

        private double TMax()
        {
            double tmax;
            tmax = this.star.TeffPhot;
            for (int i = 0; i < this.star.circSpots.Length; i++)
            {
                if (this.star.circSpots[i].Teff >= tmax)
                {
                    tmax = this.star.circSpots[i].Teff;
                }
            }
            return tmax;
        }

        private double TMin()
        {
            double tmin;
            tmin = this.star.TeffPhot;
            for (int i = 0; i < this.star.circSpots.Length; i++)
            {
                if (this.star.circSpots[i].Teff < tmin)
                {
                    tmin = this.star.circSpots[i].Teff;
                }
            }
            return tmin;
        }

        private void CreateBitmap()
        {
            int height;
            int width;
            int longLinesCount;
            int latLinesCount;
            bool sphereTexture;
            try
            {
                height = int.Parse(txtHeight.Text);
                width = int.Parse(txtWidth.Text);
                longLinesCount = int.Parse(txtLongitudeLinesNumber.Text);
                latLinesCount = int.Parse(txtLatitudeLinesNumber.Text);
                sphereTexture = rbSphereTexture.Checked;
            }
            catch
            {
                MessageBox.Show("Error in input data format...", "Error...");
                return;
            }

            int xConstLinesNumber = longLinesCount;
            int yConstLinesNumber = latLinesCount;

            bitmap = new Bitmap(width, height);

            int[] xConstLines = new int[xConstLinesNumber];
            int[] yConstLines = new int[yConstLinesNumber];

            for (int i = 0; i < xConstLines.Length; i++)
            {
                xConstLines[i] = (int)(i * width / (double)xConstLinesNumber);
            }

            for (int i = 0; i < yConstLines.Length; i++)
            {
                yConstLines[i] = (int)(i * height / (double)(yConstLinesNumber-1));
            }

            double tmax, tmin;
            int color_r, color_g, color_b;

            tmin = tmax = 0;

            if (this.star.circSpots != null)
            {
                tmax = this.TMax();
                tmin = this.TMin();

                color_r = this.minColor.R + (int)((this.maxColor.R - this.minColor.R)
                    * (this.star.TeffPhot - tmin) / (tmax - tmin));
                color_g = this.minColor.G + (int)((this.maxColor.G - this.minColor.G)
                    * (this.star.TeffPhot - tmin) / (tmax - tmin));
                color_b = this.minColor.B + (int)((this.maxColor.B - this.minColor.B)
                    * (this.star.TeffPhot - tmin) / (tmax - tmin));
            }
            else
            {
                color_r = this.maxColor.R;
                color_g = this.maxColor.G;
                color_b = this.maxColor.B;
            }

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    bitmap.SetPixel(i, j, Color.FromArgb(color_r, color_g, color_b));
                }
            }

            if (this.star != null && this.star.circSpots != null)
            {
                double dth = Math.PI / height;
                double dph = Math.PI * 2 / width;
                double scale = (double)width / (2 * Math.PI);

                double theta, phi, cosr;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        theta = (i + 0.5) * dth;
                        phi = (j + 0.5) * dph;
                        for (int s = this.star.circSpots.Length-1; s >=0; s--)
                        {
                            cosr = Math.Cos(theta) * Math.Cos(this.star.circSpots[s].ThetaOfSpotCenter) +
                                Math.Sin(theta) * Math.Sin(this.star.circSpots[s].ThetaOfSpotCenter) *
                                Math.Cos(this.star.circSpots[s].PhiOfSpotCenterAtZeroPhase - phi);
                            double dist = Math.Acos(cosr);
                            if (dist < this.star.circSpots[s].Radius)
                            {
                                color_r = this.minColor.R + (int)((this.maxColor.R - this.minColor.R)
                                    * (this.star.circSpots[s].Teff - tmin) / (tmax - tmin));
                                color_g = this.minColor.G + (int)((this.maxColor.G - this.minColor.G)
                                    * (this.star.circSpots[s].Teff - tmin) / (tmax - tmin));
                                color_b = this.minColor.B + (int)((this.maxColor.B - this.minColor.B)
                                    * (this.star.circSpots[s].Teff - tmin) / (tmax - tmin));
                                bitmap.SetPixel(j, i, Color.FromArgb(color_r, color_g, color_b));
                            }
                        }
                    }
                }
            }

            double deltaLambda = this.longLinesWidth;
            double deltaTheta = this.latLinesWidth;

            for (int i = 0; i < xConstLines.Length; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    if (i != 0)
                    {
                        int k2 = 0;
                        if (j == 0)
                        {
                            k2 = width;
                        }
                        else
                        {
                            if (sphereTexture)
                            {
                                k2 = (int)((bitmap.Width / (double)360)
                                * (deltaLambda * Math.Abs(1.0 / Math.Sin(j * Math.PI / bitmap.Height))));
                            }
                            else
                            {
                                k2 = (int)((bitmap.Width / (double)360)
                                * (deltaLambda));
                            }
                        }
                        int i1 = 0;
                        for (int k = -k2; k < k2 + 1; k++)
                        {
                            i1 = xConstLines[i] + k;
                            if (i1 < bitmap.Width && i1 >= 0)
                            {
                                bitmap.SetPixel(i1, j, this.gridsColor);
                            }
                        }
                    }
                    else
                    {
                        int k2 = 0;
                        if (j == 0)
                        {
                            k2 = width;
                        }
                        else
                        {
                            if (sphereTexture)
                            {
                                k2 = (int)((bitmap.Width / (double)360)
                                * (deltaLambda * Math.Abs(1.0 / Math.Sin(j * Math.PI / bitmap.Height))));
                            }
                            else
                            {
                                k2 = (int)((bitmap.Width / (double)360)
                                * (deltaLambda));
                            }
                        }
                        int i1 = 0;
                        for (int k = 0; k < k2 + 1; k++)
                        {
                            i1 = 0 + k;
                            if (i1 < bitmap.Width && i1 >= 0)
                            {
                                bitmap.SetPixel(i1, j, this.gridsColor);
                            }
                        }
                        for (int k = -k2; k < 1; k++)
                        {
                            i1 = width - 1 + k;
                            if (i1 < bitmap.Width && i1 >= 0)
                            {
                                bitmap.SetPixel(i1, j, this.gridsColor);
                            }
                        }
                    }
                }
            }

            // Drawing of latitude belts;
            for (int j = 0; j < yConstLines.Length; j++)
            {
                int k1 = 0, k2 = 0;
                if (j != 0)
                {
                    k2 = (int)((bitmap.Height / (double)180) * deltaTheta);
                    k1 = -k2;
                }
                if (j == 0)
                {
                    k2 = (int)((bitmap.Height / (double)180) * deltaTheta);
                    k1 = 0;
                }
                if (j == yConstLines.Length - 1)
                {
                    k1 = -(int)((bitmap.Height / (double)180) * deltaTheta);
                    k2 = -1;
                }
                for (int k = k1; k <= k2; k++)
                {
                    for (int i = 0; i < bitmap.Width; i++)
                    {
                        bitmap.SetPixel(i, yConstLines[j] + k, this.gridsColor);
                    }
                }
            }
        }

        private void btnSavePicture_Click(object sender, EventArgs e)
        {
            try
            {
                string file;
                file = @txtOutputFile.Text;
                this.bitmap.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch
            {
                MessageBox.Show("Cannot save the image...", "Error...");
                return;
            }
        }

        private void MapView_Load(object sender, EventArgs e)
        {

        }

        private void btnPathToFile_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            try
            {
                txtOutputFile.Text = saveFileDialog1.FileName;
            }
            catch
            {
                MessageBox.Show("File name error...", "Error...");
                return;
            }
        }

        private void btnSetGridsColor_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            try
            {
                this.gridsColor = colorDialog1.Color;
                panelGridColor.BackColor = colorDialog1.Color;
            }
            catch
            {
                MessageBox.Show("Cannot set grids color...", "Error...");
                return;
            }
        }

        private void btnGridParsRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                this.longLinesNumber = int.Parse(txtLongitudeLinesNumber.Text);
                this.latLinesNumber = int.Parse(txtLatitudeLinesNumber.Text);
                this.longLinesWidth = double.Parse(txtLongLinesWidth.Text.Replace(".", ","));
                this.latLinesWidth = double.Parse(txtLatLinesWidth.Text.Replace(".", ","));
            }
            catch
            {
                MessageBox.Show("Wrong format of input data...", "Error...");
                return;
            }
        }

        private void DrawTitle(string title)
        {
            Graphics g = this.CreateGraphics();
            g.DrawString(title, new Font("Arial", 15, FontStyle.Regular), Brushes.Black,
                (float)(this.xBegin), (float)(this.yBegin - 30));
            g.Dispose();
        }

        private void DrawXLabel(string xlabel)
        {
            Graphics g = this.CreateGraphics();
            g.DrawString(txtXLabel.Text, new Font("Arial", 14, 
                FontStyle.Regular), Brushes.Black,
                (float)(this.xBegin + 0.5 * this.plotWidth - 40), 
                (float)(this.yBegin + this.plotHeight + 30));
            g.Dispose();
        }

        private void DrawYLabel(string ylabel)
        {
            Graphics g = this.CreateGraphics();
            g.RotateTransform(-90);
            g.DrawString(txtYLabel.Text, new Font("Arial", 14, FontStyle.Regular), Brushes.Black,
                (float)(-this.yBegin - this.plotHeight * 0.5 - 35/*string's semiwidth*/),
                (float)(this.xBegin - 60));
            g.Dispose();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            this.CreateBitmap();
            this.Refresh();
        }

        private void btnSetMaxColor_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            try
            {
                this.maxColor = colorDialog1.Color;
                panelColorMax.BackColor = colorDialog1.Color;
            }
            catch
            {
                MessageBox.Show("Cannot set grids color...", "Error...");
                return;
            }
        }

        private void btnSetMinColor_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            try
            {
                this.minColor = colorDialog1.Color;
                panelColorMin.BackColor = colorDialog1.Color;
            }
            catch
            {
                MessageBox.Show("Cannot set grids color...", "Error...");
                return;
            }
        }
    }
}
