using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper
{
    class LCGenerator1
    {
        // The star for which fluxes will be modelled;
        private TSurface tsrf = null;
        // Delegate for Limb Darkening Function (mu, teff);
        private LimbDarkFunction_Mu_Teff limbDarkFunc;
        // Delegate for the function that return normal
        // intensity for given value of effective temperature;
        private NormIntensity_Teff normIntFunc;

        /// <summary>
        /// Constructor of the class
        /// </summary>
        /// <param name="tsrf">the surface for which light curve should be modelled</param>
        /// <param name="limbDarkFunc">delegate for the limb darkening function Ld(mu, Teff)</param>
        /// <param name="normIntFunc">delegate for the normal intensity function In(Teff)</param>
        public LCGenerator1(TSurface tsrf, 
            LimbDarkFunction_Mu_Teff limbDarkFunc, NormIntensity_Teff normIntFunc)
        {
            this.tsrf = tsrf;
            this.limbDarkFunc = limbDarkFunc;
            this.normIntFunc = normIntFunc;
        }

        /// <summary>
        /// Gets the array of fluxes
        /// </summary>
        /// <param name="phases">the array of phases for which fluxes must be computed</param>
        /// <param name="scale">the value that will be multiplied to flux values</param>
        /// <returns></returns>
        public double[] GetFluxes(double[] phases, double scale)
        {
            double[] fluxes = new double[phases.Length];
            double ld, mu, prArea, flux;
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
                            ld = this.limbDarkFunc(mu, this.tsrf.teff[i][j]);
                            prArea = this.tsrf.patch[i][j].ProjectedArea(phases[p], this.tsrf.GetInc());
                            flux += ld * prArea * this.normIntFunc(this.tsrf.teff[i][j]);
                        }
                    }
                }
                fluxes[p] = flux * scale;
            }
            return fluxes;
        }
    }
}
