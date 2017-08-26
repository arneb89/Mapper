using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper
{
    class LinearLimbDarkeningLow
    {
        IntensityProvider1D ldcp1d = null;

        public LinearLimbDarkeningLow(IntensityProvider1D ldcp1d)
        {
            this.ldcp1d = ldcp1d;
        }

        public string FixedFilter
        {
            get
            {
                return this.ldcp1d.FixFilter;
            }
            set
            {
                this.ldcp1d.FixFilter = value;
            }
        }

        public double GetLinerLimbDarkeningCoefficient(double mu, double teff)
        {
            return 1.0 - this.ldcp1d.GetIntensityForFixedFilter(teff)*(1 - mu);
        }

        public double GetLinerLimbDarkeningCoefficientForTeffFromTeffSet(double mu, double teff)
        {
            return 1.0 - this.ldcp1d.GetIntensityForFixedFilterForTeffFromTeffSet(teff) * (1 - mu);
        }
    }
}
