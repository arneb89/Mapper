using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper.StokesImaging
{
    class LinInterpolator2D
    {
        private double[][] f = null;
        private double[] x = null;
        private double[] y = null;

        public LinInterpolator2D(double[] x, double[] y, double[][] f)
        {
            this.x = x;
            this.y = y;
            this.f = f;
        }

        public double Interp(double x0, double y0)
        {
            int i;
            for (i = 0; i < x.Length - 2; i++)
            {
                if (x0 <= x[i + 1])
                {
                    break;
                }
            }

            int j;
            for (j = 0; j < y.Length - 2; j++)
            {
                if (y0 <= y[j + 1])
                {
                    break;
                }
            }

            double z1 = (this.f[i+1][j] - this.f[i][j]) * (x0 - x[i]) / 
                (x[i + 1] - x[i]) + this.f[i][j];
            double z2 = (this.f[i + 1][j + 1] - this.f[i][j + 1]) * (x0 - x[i]) /
                (x[i + 1] - x[i]) + this.f[i][j + 1];

            double f0 = (z2 - z1) * (y0 - y[j]) / (y[j + 1] - y[j]) + z1;

            return f0;
        }
    }
}
