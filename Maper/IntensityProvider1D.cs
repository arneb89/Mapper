using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLib;

namespace Maper
{
    class IntensityProvider1D
    {
        private Spline31D[] spliner1d;
        private string[] filterSeq;
        private double logg;

        private string fixedFilter = null;
        private int fixedFilterNumber = -1;

        private double[] teffSet;
        private double[] intenSet;

        public IntensityProvider1D(string file, string[] filterSeq, double logg)
        {
            this.filterSeq = filterSeq;
            this.logg = logg;
            IntensityProvider2D ip2d = new IntensityProvider2D(file, filterSeq);

            double[] intenArray, teffArray;

            teffArray = ip2d.TeffArray;
            intenArray = new double[ip2d.TeffArray.Length];

            this.spliner1d = new Spline31D[filterSeq.Length];
            for (int q = 0; q < filterSeq.Length; q++)
            {
                for (int t = 0; t < teffArray.Length; t++)
                {
                    intenArray[t] = ip2d.GetIntensity(logg, teffArray[t], filterSeq[q]);
                }
                this.spliner1d[q] = new Spline31D(teffArray, intenArray);
            }
        }

        public double GetIntensity(double teff, string filter)
        {
            int q = 0;

            for (q = 0; q <= filterSeq.Length; q++)
            {
                if (this.filterSeq[q] == filter) break;
            }
            // check for presence of the filter;
            if (q == this.filterSeq.Length) return -1;

            double res;
            res = this.spliner1d[q].Interp(teff);
            return res;
        }

        public string FixFilter
        {
            get { return this.fixedFilter; }
            set
            {
                this.fixedFilter = value;
                for (int q = 0; q < this.filterSeq.Length; q++)
                {
                    if (this.filterSeq[q] == this.fixedFilter)
                    {
                        this.fixedFilterNumber = q;
                        break;
                    }
                }
            }
        }

        public double GetIntensityForFixedFilter(double teff)
        {
            return this.spliner1d[this.fixedFilterNumber].Interp(teff);
        }

        public double[] TeffSet
        {
            get
            {
                return this.teffSet;
            }
            set
            {
                this.teffSet = value;
                this.intenSet = new double[this.teffSet.Length];
                for (int i = 0; i < this.teffSet.Length; i++)
                {
                    this.intenSet[i] = this.GetIntensityForFixedFilter(this.teffSet[i]);
                }
            }
        }

        public double GetIntensityForFixedFilterForTeffFromTeffSet(double teff)
        {
            for (int i = 0; i < this.teffSet.Length; i++)
            {
                if (teff == this.teffSet[i]) return this.intenSet[i];
            }
            return 0;
        }
    }
}
