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
    public partial class DiskViewer : Form
    {
        double[][] rmas = null;

        Point mapCenter;

        int mapRadius;

        public DiskViewer()
        {
            InitializeComponent();
        }

        private void DiskViewer_Load(object sender, EventArgs e)
        {
            this.mapCenter = new Point((int)(this.Width * 0.5), (int)(this.Height * 0.5));
            this.mapRadius = (int)(this.Width * 0.5 * 0.95);
        }

        public void Init(double[][] rmas)
        {
            this.rmas = rmas;
        }

        private void FillSector(Graphics g, double radIn, double radOut, double angleBegin, double angleEnd)
        {

        }

        private void DrawSector(Graphics g)
        {
            Pen pen = new Pen(Color.Blue, 2.0f);

            for (int i = 0; i < this.rmas.Length; i++)
            {
                float radOut = (float)(this.mapRadius * (double)(this.rmas[i][1]) / this.rmas[rmas.Length - 1][1]);
                float radIn = (float)(this.mapRadius * (double)(this.rmas[i][0]) / this.rmas[rmas.Length - 1][1]);
                if (radOut != 0)
                {
                    g.DrawArc(pen,
                        (float)this.mapCenter.X - radOut,
                        (float)this.mapCenter.Y - radOut,
                         radOut*2,
                         radOut*2,
                        (float)(this.rmas[i][2] * 180 / Math.PI),
                        (float)(this.rmas[i][3] * 180 / Math.PI));
                }
                if (radIn != 0)
                {
                    g.DrawArc(pen,
                        (float)this.mapCenter.X - radIn,
                        (float)this.mapCenter.Y - radIn,
                         radIn * 2,
                         radIn * 2,
                        (float)(this.rmas[i][2] * 180 / Math.PI),
                        (float)(this.rmas[i][3] * 180 / Math.PI));
                }

                g.DrawLine(pen, (float)(this.mapCenter.X + radIn * Math.Cos(rmas[i][2])),
                    (float)(this.mapCenter.Y + radIn * Math.Sin(rmas[i][2])),
                    (float)(this.mapCenter.X + radOut * Math.Cos(rmas[i][2])),
                    (float)(this.mapCenter.Y + radOut * Math.Sin(rmas[i][2])));
            }
        }



        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            this.DrawSector(g);
        }
    }
}
