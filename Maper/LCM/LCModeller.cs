using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLib;

namespace Maper
{
    /// <summary>
    /// Class that provides methods for light curve modelling.
    /// </summary>
    public class LCModeller
    {
        private Star star;

        private Spline31D spJ;

        public string ErrorString="";

        public LCModeller(Star star, Spline31D spJ)
        {
            this.star = star;
            this.spJ = spJ;
        }

        public Star StarM
        {
            get { return this.star; }
        }

        public double[] GetLightCurve(double[] phases, double xLDCPh, double xLDCSp, double scale)
        {
            // Normal speciefic intensity for a point of unspotted surface;
            double jPh = spJ.Interp(this.star.TeffPhot);
            // Array of normal speciefic intensities for spots;
            double[] jSp = new double[this.star.circSpots.Length];
            for (int i = 0; i < jSp.Length; i++)
            {
                jSp[i] = spJ.Interp(this.star.circSpots[i].Teff);
            }
            // Flux emitted by unspotted surface;
            double uniFlux = jPh * (Math.PI * (1 - xLDCPh / 3.0));
            double sinInc = Math.Sin(this.star.Inc);
            // Array of fluxes at different phases of star surface rotation;
            double[] flux = new double[phases.Length];
            double prArea;
            double ld;

            // overlap mask;
            int[][][] mask = new int[star.circSpots.Length][][];
            for (int s = 0; s < star.circSpots.Length; s++)
            {
                mask[s] = new int[star.circSpots[s].patch.Length][];
                for (int i = 0; i < star.circSpots[s].patch.Length; i++)
                {
                    mask[s][i] = new int[star.circSpots[s].patch[i].Length];
                }
            }

            // initialization of overlap mask;
            for (int s = 0; s < star.circSpots.Length; s++)
            {
                for (int i = 0; i < star.circSpots[s].patch.Length; i++)
                {
                    for (int j = 0; j < star.circSpots[s].patch[i].Length; j++)
                    {
                        mask[s][i][j] = -1; // not overlapped;
                    }
                }
            }

            for (int t = 0; t < star.circSpots.Length; t++)
            {
                for (int s = 0; s < t; s++)
                {
                    double deltaLambda = star.circSpots[t].PhiOfSpotCenterAtZeroPhase - star.circSpots[s].PhiOfSpotCenterAtZeroPhase;
                    // cosine of distance between centers of spots;
                    double cosR = Math.Cos(star.circSpots[s].ThetaOfSpotCenter) * Math.Cos(star.circSpots[t].ThetaOfSpotCenter) +
                                Math.Sin(star.circSpots[s].ThetaOfSpotCenter) * Math.Sin(star.circSpots[s].ThetaOfSpotCenter) *
                                Math.Cos(Math.Abs(deltaLambda));
                    if (cosR > 1) cosR = 1;
                    if (cosR < -1) cosR = -1;
                    // distance between centers of spots;
                    double R = Math.Acos(cosR);
                    if (R < 0 || R > 2 * Math.PI) this.ErrorString = this.ErrorString + "R is't correct; ";
                    if (R < (star.circSpots[s].Radius + star.circSpots[t].Radius))
                    {
                        for (int i = 0; i < star.circSpots[t].patch.Length; i++)
                        {
                            for (int j = 0; j < star.circSpots[t].patch[i].Length; j++)
                            {
                                double phi0=0;
                                if (R == 0)
                                {
                                    if (star.circSpots[t].Radius > star.circSpots[s].Radius) phi0 = Math.PI;
                                    else phi0 = 0;
                                }
                                else
                                {
                                    double sinPhi0 = Math.Sin(Math.Abs(deltaLambda)) *
                                        Math.Sin(star.circSpots[t].ThetaOfSpotCenter) / Math.Sin(R);
                                    if (sinPhi0 > 1) sinPhi0 = 1;
                                    if (sinPhi0 < -1) sinPhi0 = -1;
                                    phi0 = Math.Asin(sinPhi0);
                                }
                                if (phi0 < 0 || phi0 > 2 * Math.PI) 
                                    this.ErrorString = this.ErrorString + "phi0 is't correct; ";
                                double eta1 = star.circSpots[t].patch[i][j].FiCenter();
                                if (deltaLambda < 0) eta1 = 2 * Math.PI - eta1;
                                double ksi1 = star.circSpots[t].patch[i][j].ThetaCenter();
                                double phi1 = 0;
                                if (eta1 < phi0) phi1 = phi0 - eta1;
                                if (eta1 >= phi0) phi1 = eta1 - phi0;
                                if (phi1 > Math.PI) phi1 = 2 * Math.PI - phi1;
                                if (phi1 < 0 || phi1 > Math.PI) 
                                    this.ErrorString = this.ErrorString + "phi1 is't correct; ";
                                double cosr = cosR * Math.Cos(ksi1) + Math.Sin(R) * Math.Sin(ksi1) * Math.Cos(phi1);
                                if (cosr > 1) cosr = 1;
                                if (cosr < -1) cosr = -1;
                                double r = Math.Acos(cosr);
                                if (r < star.circSpots[s].Radius)
                                {
                                    mask[t][i][j] = s;
                                }
                            }
                        }
                    }
                }
            }

            double prAreaSum = 0;
            //
            double muCenter;
            double spotFluxForTph = 0, spotFluxForTsp = 0;
            for (int p = 0; p < phases.Length; p++)
            {
                spotFluxForTph = 0; spotFluxForTsp = 0;
                for (int t = 0; t < star.circSpots.Length; t++)
                {
                    muCenter = this.star.circSpots[t].MuOfCenter(phases[p], this.star.Inc);
                    for (int i = 0; i < star.circSpots[t].patch.Length; i++)
                    {
                        for (int j = 0; j < star.circSpots[t].patch[i].Length; j++)
                        {
                            double mu = this.star.circSpots[t].Mu(i, j, muCenter, this.star.Inc, phases[p]);
                            if (mu > 0)
                            {

                                prArea = this.star.circSpots[t].ProjectedArea(i, j, muCenter, phases[p], this.star.Inc);
                                prAreaSum += prArea;
                                if (mask[t][i][j]==-1)
                                {
                                    ld = (1 - xLDCPh * (1 - mu));
                                    spotFluxForTph = spotFluxForTph + ld * prArea * jPh;
                                }
                                else
                                {
                                    ld = (1 - xLDCSp * (1 - mu));
                                    spotFluxForTph = spotFluxForTph + ld * prArea * jSp[mask[t][i][j]];
                                }
                                spotFluxForTsp = spotFluxForTsp + ld * prArea * jSp[t];
                            }
                        }
                    }
                }
                flux[p] = uniFlux - spotFluxForTph + spotFluxForTsp;
                flux[p] = flux[p] * scale;
            }
            return flux;
        }
    }
}
