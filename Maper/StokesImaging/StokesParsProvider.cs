using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Maper.StokesImaging
{
    class StokesParsProvider
    {
        private double[] lambdas, thetas, mags;

        private MathLib.Table3D stokesI, stokesQ, stokesV;

        private MathLib.Spline32D[] splineI = null, splineQ = null, splineV = null;

        private LinInterpolator2D[] liI = null, liQ = null, liV = null;

        private string interpFlag = "spline";

        public StokesParsProvider(string stockesIGridFile, string stockesQGridFile, string stockesVGridFile)
        {
            this.stokesI = new MathLib.Table3D(Application.StartupPath + stockesIGridFile);
            this.stokesQ = new MathLib.Table3D(Application.StartupPath + stockesQGridFile);
            this.stokesV = new MathLib.Table3D(Application.StartupPath + stockesVGridFile);

            this.lambdas = new double[this.stokesI.ZMas.Length];
            this.thetas = new double[this.stokesI.YMas.Length];
            this.mags = new double[this.stokesI.XMas.Length];

            for (int i = 0; i < this.lambdas.Length; i++) 
                this.lambdas[i] = this.stokesI.ZMas[i];

            for (int i = 0; i < this.thetas.Length; i++) 
                this.thetas[i] = this.stokesI.YMas[i];

            for (int i = 0; i < this.mags.Length; i++) 
                this.mags[i] = this.stokesI.XMas[i];

            for (int i = 0; i < this.stokesQ.ZMas.Length; i++)
            {
                for (int j = 0; j < this.stokesQ.YMas.Length; j++)
                {
                    for (int k = 0; k < this.stokesQ.XMas.Length; k++)
                    {
                        if (this.stokesI.FMas[i][j][k] != 0)
                        {
                            this.stokesQ.FMas[i][j][k] = 
                                this.stokesQ.FMas[i][j][k] / this.stokesI.FMas[i][j][k];
                        }
                        else
                        {
                            this.stokesQ.FMas[i][j][k] = 0;
                        }
                    }
                }
            }

            for (int i = 0; i < this.stokesV.ZMas.Length; i++)
            {
                for (int j = 0; j < this.stokesV.YMas.Length; j++)
                {
                    for (int k = 0; k < this.stokesV.XMas.Length; k++)
                    {
                        if (this.stokesI.FMas[i][j][k] != 0)
                        {
                            this.stokesV.FMas[i][j][k] = 
                                this.stokesV.FMas[i][j][k] / this.stokesI.FMas[i][j][k];
                        }
                        else
                        {
                            this.stokesV.FMas[i][j][k] = 0;
                        }
                    }
                }
            }

            // saving circular polarization grid to the file;
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\circ_pol_tmp.dat");
            for (int i = 0; i < this.stokesV.ZMas.Length; i++)
            {
                sw.WriteLine(this.stokesV.ZMas[i]);
                for (int j = 0; j < this.stokesV.YMas.Length; j++)
                {
                    for (int k = 0; k < this.stokesV.XMas.Length; k++)
                    {
                        sw.Write("{0:0000.000}\t", this.stokesV.FMas[i][j][k].ToString());
                    }
                    sw.Write("\r\n");
                }
            }
            sw.Flush();
            sw.Close();
            
            // end of saving;

            for (int i = 0; i < this.stokesI.ZMas.Length; i++)
            {
                for (int j = 0; j < this.stokesI.YMas.Length; j++)
                {
                    for (int k = 0; k < this.stokesI.XMas.Length; k++)
                    {
                        if (this.stokesI.FMas[i][j][k] != 0)
                        {
                            this.stokesI.FMas[i][j][k] = Math.Log10(Math.Abs(this.stokesI.FMas[i][j][k]));
                        }
                        else
                        {
                            this.stokesI.FMas[i][j][k] = -50;
                        }
                    }
                }
            }

            int lambdaNumber = this.stokesI.ZMas.Length;

            this.splineI = new MathLib.Spline32D[lambdaNumber];
            this.splineQ = new MathLib.Spline32D[lambdaNumber];
            this.splineV = new MathLib.Spline32D[lambdaNumber];

            this.liI = new LinInterpolator2D[lambdaNumber];
            this.liQ = new LinInterpolator2D[lambdaNumber];
            this.liV = new LinInterpolator2D[lambdaNumber];

            for (int i = 0; i < this.liI.Length; i++)
            {
                this.splineI[i] = new MathLib.Spline32D(this.stokesI.YMas, this.stokesI.XMas, this.stokesI.FMas[i]);
                this.splineQ[i] = new MathLib.Spline32D(this.stokesQ.YMas, this.stokesQ.XMas, this.stokesQ.FMas[i]);
                this.splineV[i] = new MathLib.Spline32D(this.stokesV.YMas, this.stokesV.XMas, this.stokesV.FMas[i]);

                this.liI[i] = new LinInterpolator2D(this.stokesI.YMas, this.stokesI.XMas, this.stokesI.FMas[i]);
                this.liQ[i] = new LinInterpolator2D(this.stokesQ.YMas, this.stokesQ.XMas, this.stokesQ.FMas[i]);
                this.liV[i] = new LinInterpolator2D(this.stokesV.YMas, this.stokesV.XMas, this.stokesV.FMas[i]);
            }
        }

        public double StokesI(double magStr, double theta, double lambda)
        {
            double[] xSet = new double[this.stokesI.ZMas.Length];
            double[] ySet = new double[xSet.Length];

            for (int i = 0; i < xSet.Length; i++)
            {
                xSet[i] = Math.Log10(this.stokesI.ZMas[i]);
                if (this.interpFlag == "spline")
                {
                    ySet[i] = this.splineI[i].Interp(theta, magStr);
                }

                if (this.interpFlag == "linear")
                {
                    ySet[i] = this.liI[i].Interp(theta, magStr);
                }
            }
            double res = 0;
            if (this.interpFlag == "spline")
            {
                MathLib.Spline31D sp = new MathLib.Spline31D(xSet, ySet);
                res = Math.Pow(10, sp.Interp(Math.Log10(lambda)));
            }

            if (this.interpFlag == "linear")
            {
                LinInterpolator li = new LinInterpolator(xSet, ySet);
                res = Math.Pow(10, li.Interp(Math.Log10(lambda)));
            }
            return res;
        }

        public double StokesQ(double magStr, double theta, double lambda)
        {
            double[] xSet = new double[this.stokesQ.ZMas.Length];
            double[] ySet = new double[xSet.Length];

            for (int i = 0; i < xSet.Length; i++)
            {
                //ySet[i] = this.splineQ[i].Interp(theta, magStr);
                xSet[i] = Math.Log10(this.stokesQ.ZMas[i]);
                ySet[i] = this.liQ[i].Interp(theta, magStr);
            }

            LinInterpolator li = new LinInterpolator(xSet, ySet);

            double stI = StokesI(magStr, theta, lambda);

            double res = li.Interp(Math.Log10(lambda)) * stI;

            return res;
        }

        public double StokesV(double magStr, double theta, double lambda)
        {
            double[] xSet = new double[this.stokesV.ZMas.Length];
            double[] ySet = new double[xSet.Length];

            for (int i = 0; i < xSet.Length; i++)
            {
                ySet[i] = this.liV[i].Interp(theta, magStr);
                //ySet[i] = this.splineV[i].Interp(theta, magStr);
                xSet[i] = Math.Log10(this.stokesV.ZMas[i]);
                //ySet[i] = this.liV[i].Interp(theta, magStr);
            }



            LinInterpolator li = new LinInterpolator(xSet, ySet);

            double stI = StokesI(magStr, theta, lambda);

            double part = li.Interp(Math.Log10(lambda));

            double res = part * stI;

            return res;
        }

        public double StokesU(double magStr, double theta, double lambda)
        {
            return 0;
        }

        public double GiveStokesPar(string type, string anyString, double magStr, double theta, double optDepth)
        {
            if (type == "I")
            {
                return this.StokesI(magStr, theta, optDepth);
            }
            else if (type == "V")
            {
                return this.StokesV(magStr, theta, optDepth);
            }
            else if (type == "Q")
            {
                return this.StokesQ(magStr, theta, optDepth);
            }
            else if (type == "U")
            {
                return this.StokesU(magStr, theta, optDepth);
            }
            return 0;
        }
    }
}
