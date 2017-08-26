using System;
using System.Collections.Generic;
using System.ComponentModel;
/*using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Maper
{
    public partial class SurfaceViewer3D : Form
    {
        private Device device = null;
        // Массив координат ячеек разбиения. 
        // [*][ ][ ] - номер ячейки, 
        // [ ][*][ ] - номер угла ячейки, 
        // [ ][ ][*] - номер декартовой координаты угла;
        private float[][][] coord;
        private double[] vals;
        private Color color0 = Color.Blue;
        private Color color1 = Color.Red;
        private double inc;
        private double phase;
        private double distance=30;

        public SurfaceViewer3D()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
            //InitializeGraphics();
        }

        public void InitializeGraphics()
        {
            PresentParameters presentParams = new PresentParameters();
            presentParams.Windowed = true;
            presentParams.SwapEffect = SwapEffect.Discard;
            device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
            device.DeviceResizing += new CancelEventHandler(this.CancelResize);
            DisplayMode dm=new DisplayMode();
            dm.Height=100;
            dm.Width=100;
        }

        private void CancelResize(object sender, CancelEventArgs e)
        {
            e.Cancel = false;
        }

        private void SetupCamera()
        {
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, 1.0f, 0.0f, 100.0f);
            device.Transform.View = Matrix.LookAtLH(
                new Vector3((float)(distance*Math.Sin(this.inc)*Math.Cos(-this.phase)), 
                            (float)(distance*Math.Sin(this.inc)*Math.Sin(-this.phase)), 
                            -(float)(distance*Math.Cos(this.inc))), 
                new Vector3(0,0,0), 
                new Vector3(0, 0, 1));
            device.RenderState.Lighting = false;
            device.Transform.World = Matrix.RotationZ(System.Environment.TickCount / 450.0f / (float)Math.PI);
        }

        private float[] SphereToDec(float[] spc)
        {
            float[] dec = new float[3];
            dec[0] = spc[0] * (float)Math.Sin(spc[1]) * (float)Math.Sin(spc[2]);
            dec[1] = spc[0] * (float)Math.Sin(spc[1]) * (float)Math.Cos(spc[2]);
            dec[2] = spc[0] * (float)Math.Cos(spc[1]);
            return dec;
        }

        private void Smoother(ref double[][] rects, ref double[] vals, int nX, int nY)
        {
            int size;
            size = rects.Length * nX * nY;
            double[][] rectsNew = new double[size][];
            double[] valsNew = new double[size];
            for (int i = 0; i < size; i++)
            {
                rectsNew[i] = new double[4];
            }

            double height, width;
            int n=0;
            for (int j = 0; j < vals.Length; j++)
            {
                height = (rects[j][1] - rects[j][0])/(double)nY;
                width = (rects[j][3] - rects[j][2])/(double)nX;
                for (int k = 0; k < nX; k++)
                {
                    for (int l = 0; l < nY; l++)
                    {
                        valsNew[n] = vals[j];
                        rectsNew[n][0] = rects[j][0] + l * height;
                        rectsNew[n][1] = rects[j][0] + (l + 1) * height;
                        rectsNew[n][2] = rects[j][2] + k * width;
                        rectsNew[n][3] = rects[j][2] + (k + 1) * width;
                        n++;
                    }
                }
            }
            vals = valsNew;
            rects = rectsNew;
        }

        public void Init(double[][] rects, double[] vals)
        {
            float[] dec = new float[3];

            this.Smoother(ref rects, ref vals, 5, 5);

            this.vals = vals;

            this.coord = new float[rects.Length][][];
            for (int i = 0; i < rects.Length; i++)
            {
                this.coord[i] = new float[4][];
                for (int j = 0; j < 4; j++)
                {
                    this.coord[i][j] = new float[3];
                }
            }

            float radius = 10f;

            for (int i = 0; i < rects.Length; i++)
            {
                dec = this.SphereToDec(new float[3] { radius, (float)rects[i][0], (float)rects[i][2] });
                this.coord[i][0] = dec;
                dec = this.SphereToDec(new float[3] { radius, (float)rects[i][0], (float)rects[i][3] });
                this.coord[i][1] = dec;
                dec = this.SphereToDec(new float[3] { radius, (float)rects[i][1], (float)rects[i][2] });
                this.coord[i][2] = dec;
                dec = this.SphereToDec(new float[3] { radius, (float)rects[i][1], (float)rects[i][3] });
                this.coord[i][3] = dec;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            device.Clear(ClearFlags.Target, Color.White, 1.0f, 0);
            
            this.SetupCamera();

            double iMax = vals.Max();
            double iMin = vals.Min();

            int R1, G1, B1, R0, G0, B0;

            R1 = color1.R; G1 = color1.G; B1 = color1.B;
            R0 = color0.R; G0 = color0.G; B0 = color0.B;

            double kR, kB, kG;
            if (iMin != iMax)
            {
                kR = (R1 - R0) / (iMax - iMin);
                kG = (G1 - G0) / (iMax - iMin);
                kB = (B1 - B0) / (iMax - iMin);
            }
            else
            {
                kR = 0; kG = 0; kB = 0;
            }

            int[] valsB = new int[this.vals.Length];
            int[] valsG = new int[this.vals.Length];
            int[] valsR = new int[this.vals.Length];

            for (int i = 0; i < vals.Length; i++)
            {
                valsR[i] = Convert.ToInt16(kR * (vals[i] - iMin) + R0);
                valsG[i] = Convert.ToInt16(kG * (vals[i] - iMin) + G0);
                valsB[i] = Convert.ToInt16(kB * (vals[i] - iMin) + B0);
            }
            
            CustomVertex.PositionColored[] verts = new CustomVertex.PositionColored[6*this.coord.Length];

            int p = 0;
            for (int i = 0; i < 6 * this.coord.Length; i = i + 6)
            {
                verts[i].Position = new Vector3(this.coord[p][0][0], this.coord[p][0][1], this.coord[p][0][2]);
                verts[i].Color = Color.FromArgb(valsR[p], valsG[p], valsB[p]).ToArgb();
                verts[i + 1].Position = new Vector3(this.coord[p][2][0], this.coord[p][2][1], this.coord[p][2][2]);
                verts[i + 1].Color = Color.FromArgb(valsR[p], valsG[p], valsB[p]).ToArgb();
                verts[i + 2].Position = new Vector3(this.coord[p][3][0], this.coord[p][3][1], this.coord[p][3][2]);
                verts[i + 2].Color = Color.FromArgb(valsR[p], valsG[p], valsB[p]).ToArgb();
                verts[i + 3].Position = new Vector3(this.coord[p][3][0], this.coord[p][3][1], this.coord[p][3][2]);
                verts[i + 3].Color = Color.FromArgb(valsR[p], valsG[p], valsB[p]).ToArgb();
                verts[i + 4].Position = new Vector3(this.coord[p][1][0], this.coord[p][1][1], this.coord[p][1][2]);
                verts[i + 4].Color = Color.FromArgb(valsR[p], valsG[p], valsB[p]).ToArgb();
                verts[i + 5].Position = new Vector3(this.coord[p][0][0], this.coord[p][0][1], this.coord[p][0][2]);
                verts[i + 5].Color = Color.FromArgb(valsR[p], valsG[p], valsB[p]).ToArgb();
                p++;
            }

            device.BeginScene();
            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.DrawUserPrimitives(PrimitiveType.TriangleList, 2*this.coord.Length, verts);
            device.EndScene();
            device.Present();

            this.Invalidate();
            //base.OnPaint(e);
        }

        private void SurfaceViewer3D_Load(object sender, EventArgs e)
        {

        }

        public Color ColorMin
        {
            set { this.color0 = value; }
        }

        public Color ColorMax
        {
            set { this.color1 = value; }
        }

        public double Distance
        {
            set { this.distance = value; }
        }

        public double Inclination
        {
            set { this.inc = value; }
        }

        public double Phase
        {
            set { this.phase = value; }
        }
    }
}*/
