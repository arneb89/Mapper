using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper
{
    public delegate double SomeFunction(double x);
    public delegate double NormIntensity_Teff_Band(double teff, string band);
    public delegate double NormIntensity_Teff(double teff);
    public delegate double LimbDarkFunction_Mu_Teff(double mu, double teff);
}
