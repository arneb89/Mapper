using System;
using System.Collections.Generic;
using System.Text;

namespace Maper.StokesImaging
{
    public delegate double Stokes_B_Theta_Lambda(double magStr, double theta, double lambda);
    public delegate double Stokes_T_F_B_Theta_Lambda(string type, string filter, double magStr, double theta, double lambda);
}