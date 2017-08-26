using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLib;

namespace Maper
{
    public class LCGenerator
    {
        private TSurface tsrf;
        private Spline31D spJ;
        private Spline31D spLX;
        private Spline31D spLY;
        private double xLDC, yLDC;

        public LCGenerator(TSurface tsrf, Spline31D spJ, double xLDC, double yLDC)
        {
            this.tsrf = tsrf;
            this.spJ = spJ;
            this.xLDC = xLDC;
            this.yLDC = yLDC;
        }

        public LCGenerator(TSurface tsrf, Spline31D spJ, Spline31D spLX)
        {
            this.tsrf = tsrf;
            this.spJ = spJ;
            this.spLX = spLX;
        }

        public LCGenerator(TSurface tsrf, Spline31D spJ, Spline31D spLX, Spline31D spLY)
        {
            this.tsrf = tsrf;
            this.spJ = spJ;
            this.spLX = spLX;
            this.spLY = spLY;
        }

        public LCGenerator(TSurface tsrf, double xLDC, double yLDC)
        {
            this.tsrf = tsrf;
            this.xLDC = xLDC;
            this.yLDC = yLDC;
        }

        public double[] GetFluxMasForLinLD(double[] phases)
        {
            double[] fluxes = new double[phases.Length];
            double ldc, mu, flux;
            for (int p = 0; p < phases.Length; p++)
            {
                flux = 0;
                for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
                {
                    for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                    {
                        mu=this.tsrf.patch[i][j].Mu(phases[p], this.tsrf.GetInc());
                        if (mu>=0)
                        {
                            ldc = 1 - this.spLX.Interp(this.tsrf.teff[i][j]) * (1 - mu);
                            flux = flux + ldc * this.tsrf.patch[i][j].ProjectedArea(phases[p], this.tsrf.GetInc()) *
                                this.spJ.Interp(this.tsrf.teff[i][j]);
                        }
                    }
                }
                fluxes[p] = flux;
            }
            return fluxes;
        }

        public double[] GetFluxMasForLinLDForConstLDC(double[] phases)
        {
            double[] fluxes = new double[phases.Length];
            double ldc, mu, flux;
            for (int p = 0; p < phases.Length; p++)
            {
                flux = 0;
                for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
                {
                    for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                    {
                        mu = this.tsrf.patch[i][j].Mu(phases[p], this.tsrf.GetInc());
                        if (mu >= 0)
                        {
                            ldc = 1 - this.xLDC * (1 - mu);
                            flux = flux + ldc * this.tsrf.patch[i][j].ProjectedArea(phases[p], this.tsrf.GetInc()) *
                                this.spJ.Interp(this.tsrf.teff[i][j]);
                        }
                    }
                }
                fluxes[p] = flux;
            }
            return fluxes;
        }

        public double[] GetFluxMasForLinLDForIntSrfForConstLD(double[] phases)
        {
            double[] fluxes = new double[phases.Length];
            double ldc, mu, flux;
            for (int p = 0; p < phases.Length; p++)
            {
                flux = 0;
                for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
                {
                    for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                    {
                        mu = this.tsrf.patch[i][j].Mu(phases[p], this.tsrf.GetInc());
                        if (mu >= 0)
                        {
                            ldc = 1 - this.xLDC * (1 - mu);
                            flux = flux + ldc * this.tsrf.patch[i][j].ProjectedArea(phases[p], this.tsrf.GetInc()) *
                                this.tsrf.teff[i][j];
                        }
                    }
                }
                fluxes[p] = flux;
            }
            return fluxes;
        }
    }
}
