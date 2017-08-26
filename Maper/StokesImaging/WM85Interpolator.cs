using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Maper.StokesImaging
{
    public class WM85Interpolator
    {
        private double[][][] fT, fL, fC;
        private double[] xT, xL, xC;
        private double[] yT, yL, yC;
        private double[] zT, zL, zC;

        public WM85Interpolator()
        {
            MathLib.Table3D tableT = new MathLib.Table3D(Application.StartupPath + @"\Data\Stokes\MW85Original_kT10_TOTAL.dat");
            MathLib.Table3D tableL = new MathLib.Table3D(Application.StartupPath + @"\Data\Stokes\MW85Original_kT10_LINEAR.dat");
            MathLib.Table3D tableC = new MathLib.Table3D(Application.StartupPath + @"\Data\Stokes\MW85Original_kT10_CIRC.dat");

            this.xT = tableT.XMas; this.yT = tableT.YMas; this.zT = tableT.ZMas; this.fT = tableT.FMas;
            this.xL = tableL.XMas; this.yL = tableL.YMas; this.zL = tableL.ZMas; this.fL = tableL.FMas;
            this.xC = tableC.XMas; this.yC = tableC.YMas; this.zC = tableC.ZMas; this.fC = tableC.FMas;

            // form [deg] to [rad] and replacing 1 deg -> 0 deg and 89 deg -> 90 deg;

            for (int i = 0; i < this.yT.Length; i++)
            {
                this.yT[i] = this.yT[i] * Math.PI / 180;
            }
            this.yT[0] = 0;
            this.yT[this.yT.Length - 1] = 0.5 * Math.PI;

            for (int i = 0; i < this.yL.Length; i++)
            {
                this.yL[i] = this.yL[i] * Math.PI / 180;
            }
            this.yL[0] = 0;
            this.yL[this.yL.Length - 1] = 0.5 * Math.PI;

            for (int i = 0; i < this.yC.Length; i++)
            {
                this.yC[i] = this.yC[i] * Math.PI / 180;
            }
            this.yC[0] = 0;
            this.yC[this.yC.Length - 1] = 0.5 * Math.PI;
        }

        public double InterpTotal(double x, double y, double z)
        {
            double[] fForGrigZ = new double[this.zT.Length];

            for (int iZ = 0; iZ < fForGrigZ.Length; iZ++)
            {
                double[] fForGridY = new double[this.yT.Length];
                for (int iY = 0; iY < fForGridY.Length; iY++)
                {
                    MathLib.Spline31D splineX = new MathLib.Spline31D(this.xT, this.fT[iZ][iY]);
                    fForGridY[iY] = splineX.Interp(x);
                }
                MathLib.Spline31D splineY = new MathLib.Spline31D(this.yT, fForGridY, 0, 0);
                fForGrigZ[iZ] = splineY.Interp(y);
            }

            MathLib.PolynomInterpolator pi = new MathLib.PolynomInterpolator(this.zT, fForGrigZ);

            double val = pi.Interp(z);

            return val;
        }

        public double InterpLinearPart(double x, double y, double z)
        {
            double[] fForGrigZ = new double[this.zL.Length];

            for (int iZ = 0; iZ < fForGrigZ.Length; iZ++)
            {
                double[] fForGridY = new double[this.yL.Length];
                for (int iY = 0; iY < fForGridY.Length; iY++)
                {
                    MathLib.Spline31D splineX = new MathLib.Spline31D(this.xL, this.fL[iZ][iY]);
                    fForGridY[iY] = Math.Log10(splineX.Interp(x) + 100);
                }

                MathLib.Spline31D splineY = new MathLib.Spline31D(this.yL, fForGridY);//, 0, 0);
                fForGrigZ[iZ] = splineY.Interp(y);
            }

            MathLib.PolynomInterpolator pi = new MathLib.PolynomInterpolator(this.zL, fForGrigZ);

            double val = (Math.Pow(10, pi.Interp(z)) - 100) / 100;

            return val;
        }

        public double InterpCircularPart(double x, double y, double z)
        {
            double[] fForGrigZ = new double[this.zC.Length];

            for (int iZ = 0; iZ < fForGrigZ.Length; iZ++)
            {
                double[] fForGridY = new double[this.yC.Length];
                for (int iY = 0; iY < fForGridY.Length; iY++)
                {
                    MathLib.Spline31D splineX = new MathLib.Spline31D(this.xC, this.fC[iZ][iY]);
                    fForGridY[iY] = splineX.Interp(x);
                }
                MathLib.Spline31D splineY = new MathLib.Spline31D(this.yC, fForGridY, 0, 0);
                fForGrigZ[iZ] = splineY.Interp(y);
            }

            MathLib.PolynomInterpolator pi = new MathLib.PolynomInterpolator(this.zC, fForGrigZ);

            double val = pi.Interp(z) / 100;

            return val;
        }

        public double GetStokesI(double magStr, double theta, double optDepth)
        {
            double lambda = 9000;
            double c = 2.997e10;
            double me = 9.109e-28;
            double e = 4.803e-10;

            double omega = 2 * Math.PI * c / (lambda * 1e-8);
            double nuratio = omega / (e * magStr / (me * c));
            double inten_omega = Math.Pow(10, -0.4 * this.InterpTotal(nuratio, theta, optDepth));
            double inten_lambda = inten_omega * c / Math.Pow((lambda * 1e-8), 2);

            return inten_lambda * 1e15;
        }

        public double GetStokesV(double magStr, double theta, double optDepth)
        {
            double lambda = 9000;
            double c = 2.997e10;
            double me = 9.109e-28;
            double e = 4.803e-10;

            double omega = 2 * Math.PI * c / (lambda * 1e-8);
            double nuratio = omega / (e * magStr / (me * c));
            double inten_omega = Math.Pow(10, -0.4 * this.InterpTotal(nuratio, theta, optDepth)) *
                this.InterpCircularPart(nuratio, theta, optDepth);
            double inten_lambda = inten_omega * c / Math.Pow((lambda * 1e-8), 2);

            return inten_lambda * 1e15;
        }

        public double GetStokesQ(double magStr, double theta, double optDepth)
        {
            double lambda = 9000;
            double c = 2.997e10;
            double me = 9.109e-28;
            double e = 4.803e-10;

            double omega = 2 * Math.PI * c / (lambda * 1e-8);
            double nuratio = omega / (e * magStr / (me * c));
            double inten_omega = Math.Pow(10, -0.4 * this.InterpTotal(nuratio, theta, optDepth)) *
                this.InterpLinearPart(nuratio, theta, optDepth);
            double inten_lambda = inten_omega * c / Math.Pow((lambda * 1e-8), 2);

            return inten_lambda;
        }

        public double GetStokesU(double magStr, double theta, double optDepth)
        {
            return 0;
        }

        public double GetStokes(string stokes_type, string filter, double magStr, double theta, double optDepth)
        {
            double lambda=0;
            if (filter == "V") lambda = 5550;
            if (filter == "I") lambda = 9000;
            double c = 2.997e10;
            double me = 9.109e-28;
            double e = 4.803e-10;
            double omega = 2 * Math.PI * c / (lambda * 1e-8);
            double nuratio = omega / (e * magStr / (me * c));
            if (stokes_type == "I")
            {
                double inten_omega = Math.Pow(10, -0.4 * this.InterpTotal(nuratio, theta, optDepth));
                double inten_lambda = inten_omega * c / Math.Pow((lambda * 1e-8), 2);
                return inten_lambda;
            }
            if (stokes_type == "Q")
            {
                double inten_omega = Math.Pow(10, -0.4 * this.InterpTotal(nuratio, theta, optDepth)) *
                this.InterpLinearPart(nuratio, theta, optDepth);
                double inten_lambda = inten_omega * c / Math.Pow((lambda * 1e-8), 2);
                return inten_lambda;
            }
            if (stokes_type == "V")
            {
                double inten_omega = Math.Pow(10, -0.4 * this.InterpTotal(nuratio, theta, optDepth)) *
                this.InterpCircularPart(nuratio, theta, optDepth);
                double inten_lambda = inten_omega * c / Math.Pow((lambda * 1e-8), 2);
                return inten_lambda;
            }
            return 0;
        }

        public double[] GetNuRatSet
        {
            get { return this.xT; }
        }

        public double[] GetThetaSet
        {
            get { return this.yT; }
        }
    }
}
