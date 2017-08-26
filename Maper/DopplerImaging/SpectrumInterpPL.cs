using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLib;

namespace Maper
{
    class SpectrumInterpPL
    {
        double[][] specGrid;
        double[] mu, lambda;

        public SpectrumInterpPL(double[][] specGrid, double[] mu, double[] lambda)
        {
            this.specGrid = specGrid;
            this.mu = mu;
            this.lambda = lambda;
        }

        public double Interp(double mu0, double lambda0)
        {
            PolynomInterpolator pi;

            double deltaLambda = this.lambda[1] - this.lambda[0];

            int i0 = (int)((lambda0 - this.lambda[0]) / deltaLambda);
            int i1 = i0 + 1;

            double[] intenSet0 = new double[3];
            double[] intenSet1 = new double[3];

            intenSet0[0] = this.specGrid[0][i0];
            intenSet0[1] = this.specGrid[1][i0];
            intenSet0[2] = this.specGrid[2][i0];

            intenSet1[0] = this.specGrid[0][i1];
            intenSet1[1] = this.specGrid[1][i1];
            intenSet1[2] = this.specGrid[2][i1];

            pi = new PolynomInterpolator(this.mu, intenSet0);

            double inten0 = pi.Interp(mu0);
            double inten1 = pi.Interp(mu0);

            return inten0 + (inten1 - inten0) * (lambda0 - this.lambda[i0]) / deltaLambda;
        }
    }
}
