using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLib;

namespace Maper
{
    class IntensityProvider2D
    {
        private Spline32D[] spliner2d;
        private string[] filterSeq;
        private double[] teffArray;
        private double[] loggArray;

        public IntensityProvider2D(string file, string[] filterSeq)
        {
            this.filterSeq = filterSeq;
            
            Table3D tabInt = new Table3D(file);

            this.teffArray = tabInt.YMas;
            this.loggArray = tabInt.XMas;

            this.spliner2d = new Spline32D[filterSeq.Length];
            for (int q = 0; q < filterSeq.Length; q++)
            {
                this.spliner2d[q] = new Spline32D(tabInt.YMas, tabInt.XMas, tabInt.FMas[q]);
            }
        }

        public double GetIntensity(double logg, double teff, string filter)
        {
            int q = 0;

            for (q = 0; q <= filterSeq.Length; q++)
            {
                if (this.filterSeq[q] == filter) break;
            }
            // check for presence of the filter;
            if (q == this.filterSeq.Length) return -1;

            double res = this.spliner2d[q].Interp(teff, logg);

            return res;
        }

        public double[] LoggArray
        {
            get { return this.loggArray; }
        }

        public double[] TeffArray
        {
            get { return this.teffArray; }
        }
    }
}
