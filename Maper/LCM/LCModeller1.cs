using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Maper.LCM
{
    class LCModeller1
    {
        // The star for which fluxes will be modelled;
        private Star star;
        // Delegate for Limb Darkening Function (mu, teff);
        private LimbDarkFunction_Mu_Teff limbDarkFunc;
        // Delegate for the function that return normal
        // intensity for given value of effective temperature;
        private NormIntensity_Teff normIntFunc;
        // unspotted brightness;
        private double unspottedBrightness;

        private string ErrorString;

        /// <summary>
        /// Constructor of the class;
        /// </summary>
        /// <param name="star">the star object for which light curve should be modelled</param>
        /// <param name="ldf">delegate for limb darkening function ld(mu, teff)</param>
        /// <param name="normIntFunc">delegate for normal intensity function In(teff)</param>
        /// <param name="unspottedBrightness">unspotted brightness of the star</param>
        public LCModeller1(Star star, LimbDarkFunction_Mu_Teff ldf, 
            NormIntensity_Teff normIntFunc, double unspottedBrightness)
        {
            this.star = star.Clone();
            this.limbDarkFunc = ldf;
            this.normIntFunc = normIntFunc;
            this.unspottedBrightness = unspottedBrightness;
        }

        /// <summary>
        /// Gets the private star object of the class. Gives the possibility for changing
        /// parameters of the star object.
        /// </summary>
        public Star StarM
        {
            get { return this.star; }
        }

        /// <summary>
        /// Gets adopted unspotted brightness of the star;
        /// </summary>
        public double UnspottedBrightness
        {
            get { return this.unspottedBrightness; }
        }

        /// <summary>
        /// Gets the array of fluxes;
        /// </summary>
        /// <param name="phases">the array of phases for which fluxes must be computed</param>
        /// <param name="unspottedBrightness">unspotted brigthness of the star;</param>
        /// <param name="scale">the value that will be multiplied to flux values;</param>
        /// <returns></returns>
        /// 
        public double[] GetFluxes(double[] phases, double scale)
        {
            // Normal speciefic intensity for a point of unspotted surface;
            double jPh = this.normIntFunc(this.star.TeffPhot);
            // Array of normal speciefic intensities for spots;
            double[] jSp = new double[this.star.circSpots.Length];
            for (int i = 0; i < jSp.Length; i++)
            {
                jSp[i] = this.normIntFunc(this.star.circSpots[i].Teff);
            }

            double sinInc = Math.Sin(this.star.Inc);
            // Array of fluxes at different phases of star surface rotation;
            double[] flux = new double[phases.Length];

            // overlap mask;
            bool[][][] overlapped = new bool[star.circSpots.Length][][];
            for (int s = 0; s < star.circSpots.Length; s++)
            {
                overlapped[s] = new bool[star.circSpots[s].patch.Length][];
                for (int i = 0; i < star.circSpots[s].patch.Length; i++)
                {
                    overlapped[s][i] = new bool[star.circSpots[s].patch[i].Length];
                }
            }

            // initialization of overlap mask;
            for (int s = 0; s < star.circSpots.Length; s++)
            {
                for (int i = 0; i < star.circSpots[s].patch.Length; i++)
                {
                    for (int j = 0; j < star.circSpots[s].patch[i].Length; j++)
                    {
                        overlapped[s][i][j] = false; // the element (i,j) is not overlapped any spot;
                    }
                }
            }

            for (int t = 0; t < star.circSpots.Length; t++)
            {
                for (int s = 0; s < t; s++)
                {
                    double deltaLambda = star.circSpots[t].PhiOfSpotCenterAtZeroPhase -
                        star.circSpots[s].PhiOfSpotCenterAtZeroPhase;
                    // cosine of distance between centers of spots;
                    double cosr12 = Math.Cos(star.circSpots[s].ThetaOfSpotCenter) *
                        Math.Cos(star.circSpots[t].ThetaOfSpotCenter) +
                                Math.Sin(star.circSpots[s].ThetaOfSpotCenter) *
                                Math.Sin(star.circSpots[t].ThetaOfSpotCenter) *
                                Math.Cos(Math.Abs(deltaLambda));
                    if (cosr12 > 1) cosr12 = 1;
                    if (cosr12 < -1) cosr12 = -1;
                    // distance between centers of spots;
                    double r12 = Math.Acos(cosr12);
                    if (r12 < 0 || r12 > 2 * Math.PI) this.ErrorString = this.ErrorString + "R is't correct; ";
                    if (r12 < (star.circSpots[s].Radius + star.circSpots[t].Radius))
                    {
                        double cosOmega;
                        double omega;

                        if (r12 == 0 || star.circSpots[t].ThetaOfSpotCenter == 0)
                        {
                            omega = 0;
                        }
                        else
                        {
                            cosOmega = (Math.Cos(star.circSpots[s].ThetaOfSpotCenter) -
                                cosr12 * Math.Cos(star.circSpots[t].ThetaOfSpotCenter)) /
                                (Math.Sin(r12) * Math.Sin(star.circSpots[t].ThetaOfSpotCenter));
                            if (cosOmega > 1.0) cosOmega = 1.0;
                            if (cosOmega < -1.0) cosOmega = -1.0;
                            if (this.star.circSpots[t].PhiOfSpotCenterAtZeroPhase >=
                            this.star.circSpots[s].PhiOfSpotCenterAtZeroPhase)
                            {
                                omega = Math.Acos(cosOmega);
                            }
                            else
                            {
                                omega = 2 * Math.PI - Math.Acos(cosOmega);
                            }
                        }

                        double beta;

                        for (int i = 0; i < star.circSpots[t].patch.Length; i++)
                        {
                            for (int j = 0; j < star.circSpots[t].patch[i].Length; j++)
                            {
                                double cosDelta;
                                double delta;
                                double eta_t = this.star.circSpots[t].patch[i][j].FiCenter();
                                double zeta_t = this.star.circSpots[t].patch[i][j].ThetaCenter();

                                beta = eta_t - omega;
                                cosDelta = cosr12 * Math.Cos(zeta_t) +
                                    Math.Sin(r12) * Math.Sin(zeta_t) * Math.Cos(beta);
                                if (cosDelta > 1.0) cosDelta = 1.0;
                                if (cosDelta < -1.0) cosDelta = -1.0;
                                delta = Math.Acos(cosDelta);
                                if (delta < this.star.circSpots[s].Radius)
                                {
                                    overlapped[t][i][j] = true;
                                }
                            }
                        }
                    }
                }
            }

            //StreamWriter sr1 = new StreamWriter("overlap1_.txt");
            //StreamWriter sr2 = new StreamWriter("overlap2_.txt");
            //for (int i = 0; i < overlapped[0].Length; i++)
            //{
            //    for (int j = 0; j < overlapped[0][i].Length; j++)
            //    {
            //        if (!overlapped[0][i][j]) sr1.Write("0");
            //        else sr1.Write("1");
            //    }
            //    sr1.Write("\r\n");
            //}
            //for (int i = 0; i < overlapped[1].Length; i++)
            //{
            //    for (int j = 0; j < overlapped[1][i].Length; j++)
            //    {
            //        if (!overlapped[1][i][j]) sr2.Write("0");
            //        else sr2.Write("1");
            //    }
            //    sr2.Write("\r\n");
            //}
            //sr1.Flush();
            //sr2.Flush();
            //sr1.Close();
            //sr2.Close();


            //double prArea;
            //double ld_ph;
            //double ld_sp;
            //double sum;
            
            //double muCenter;
            //double sinGammaCenter;
            //double khi;
            //double sinKhi, cosKhi;

            //StreamWriter sw_area = new StreamWriter("area.dat");
            //StreamWriter sw_ld = new StreamWriter("ld.dat");

            //phases = new double[2];
            //phases[0] = 10 / 36.0;
            //phases[1] = 26 / 36.0;

            //StreamWriter sw1 = new StreamWriter("spot1_.dat");
            //StreamWriter sw2 = new StreamWriter("spot2_.dat");

            Parallel.For(0, phases.Length, p =>
            //for (int p = 0; p < phases.Length; p++)
            {
                double prArea;
                double ld_ph;
                double ld_sp;
                double sum;

                double muCenter;
                double sinGammaCenter;
                double khi;
                double sinKhi, cosKhi;
                sum = 0;
                for (int t = 0; t < star.circSpots.Length; t++)
                {
                    muCenter = this.star.MuOfTheSpotCenter(t, phases[p]);
                    if (muCenter > 1.0) muCenter = 1.0;
                    if (muCenter < -1.0) muCenter = -1.0;
                    sinGammaCenter = Math.Sqrt(1.0 - muCenter * muCenter);
                    //khi = this.star.GetOmega(t, phases[p], muCenter);

                    cosKhi = this.star.GetCosOmega(t, phases[p], muCenter);
                    if (cosKhi > 1.0) cosKhi = 1.0;
                    if (cosKhi < -1.0) cosKhi = -1.0;

                    double eps = this.star.circSpots[t].PhiOfSpotCenter(phases[p]) - 2 * Math.PI * Math.Floor((this.star.circSpots[t].PhiOfSpotCenter(phases[p])) / (2 * Math.PI));
                    if (eps < Math.PI / 2.0 || eps > 1.5 * Math.PI)
                    {
                        sinKhi = -Math.Sqrt(1 - cosKhi * cosKhi);
                    }
                    else
                    {
                        sinKhi = Math.Sqrt(1 - cosKhi * cosKhi);
                    }
                    //muCenter = this.star.circSpots[t].MuOfCenter(phases[p], this.star.Inc);
                    for (int i = 0; i < star.circSpots[t].patch.Length; i++)
                    {
                        for (int j = 0; j < star.circSpots[t].patch[i].Length; j++)
                        {
                            if (!overlapped[t][i][j])
                            {
                                double mu = this.star.MuOfTheSpotPatchCenter(t, i, j, muCenter, sinGammaCenter, cosKhi, sinKhi);
                                //double mu = this.star.MuOfTheSpotPatchCenter(t, i, j, muCenter, khi);
                                //double mu = this.star.circSpots[t].Mu(i, j, muCenter, star.Inc, phases[p]);
                                if (mu >= 0) // the element is visible;
                                {
                                    prArea = this.star.ProjectedAreaOfThePatch(t, i, j, muCenter, sinGammaCenter, cosKhi, sinKhi);
                                    //prArea = this.star.ProjectedAreaOfThePatch(t, i, j, muCenter, sinGammaCenter, khi);
                                    //prArea = this.star.circSpots[t].ProjectedArea(i, j, muCenter, phases[p], this.star.Inc);
                                    ld_ph = this.limbDarkFunc(mu, star.TeffPhot);
                                    ld_sp = this.limbDarkFunc(mu, this.star.circSpots[t].Teff);
                                    sum += prArea * (ld_sp * jSp[t] - ld_ph * jPh);
                                    //if (t == 0) sw1.Write("{0:0.0000E000} ", -prArea * (ld_sp * jSp[t] - ld_ph * jPh));
                                    //if (t == 1) sw2.Write("{0:0.0000E000} ", -prArea * (ld_sp * jSp[t] - ld_ph * jPh));
                                }
                                else
                                {
                                    //if (t == 0) sw1.Write("{0:0.0000E000} ", 0);
                                    //if (t == 1) sw2.Write("{0:0.0000E000} ", 0);
                                }
                            }
                            else
                            {
                                //if (t == 0) sw1.Write("{0:0.0000E000} ", 0);
                                //if (t == 1) sw2.Write("{0:0.0000E000} ", 0);
                            }
                        }
                        //if (t == 0) sw1.Write("\r\n");
                        //if (t == 1) sw2.Write("\r\n");
                    }
                }
                flux[p] = this.unspottedBrightness + sum;
                flux[p] = flux[p] * scale;
            });
            //sw1.Flush();
            //sw2.Flush();
            //sw1.Close();
            //sw2.Close();
            return flux;
        }

        //public double[] GetFluxes(double[] phases, double scale)
        //{
        //    // Normal speciefic intensity for a point of unspotted surface;
        //    double jPh = this.normIntFunc(this.star.TeffPhot);
        //    // Array of normal speciefic intensities for spots;
        //    double[] jSp = new double[this.star.circSpots.Length];
        //    for (int i = 0; i < jSp.Length; i++)
        //    {
        //        jSp[i] = this.normIntFunc(this.star.circSpots[i].Teff);
        //    }

        //    double sinInc = Math.Sin(this.star.Inc);
        //    // Array of fluxes at different phases of star surface rotation;
        //    double[] flux = new double[phases.Length];

        //    double prArea;
        //    double ld;

        //    // overlap mask;
        //    int[][][] mask = new int[star.circSpots.Length][][];
        //    for (int s = 0; s < star.circSpots.Length; s++)
        //    {
        //        mask[s] = new int[star.circSpots[s].patch.Length][];
        //        for (int i = 0; i < star.circSpots[s].patch.Length; i++)
        //        {
        //            mask[s][i] = new int[star.circSpots[s].patch[i].Length];
        //        }
        //    }

        //    // initialization of overlap mask;
        //    for (int s = 0; s < star.circSpots.Length; s++)
        //    {
        //        for (int i = 0; i < star.circSpots[s].patch.Length; i++)
        //        {
        //            for (int j = 0; j < star.circSpots[s].patch[i].Length; j++)
        //            {
        //                mask[s][i][j] = -1; // the element (i,j) is not overlapped any spot;
        //            }
        //        }
        //    }

        //    for (int t = 0; t < star.circSpots.Length; t++)
        //    {
        //        for (int s = 0; s < t; s++)
        //        {
        //            double deltaLambda = star.circSpots[t].PhiOfSpotCenterAtZeroPhase -
        //                star.circSpots[s].PhiOfSpotCenterAtZeroPhase;
        //            // cosine of distance between centers of spots;
        //            double cosr12 = Math.Cos(star.circSpots[s].ThetaOfSpotCenter) * 
        //                Math.Cos(star.circSpots[t].ThetaOfSpotCenter) +
        //                        Math.Sin(star.circSpots[s].ThetaOfSpotCenter) * 
        //                        Math.Sin(star.circSpots[t].ThetaOfSpotCenter) *
        //                        Math.Cos(Math.Abs(deltaLambda));
        //            if (cosr12 > 1) cosr12 = 1;
        //            if (cosr12 < -1) cosr12 = -1;
        //            // distance between centers of spots;
        //            double r12 = Math.Acos(cosr12);
        //            if (r12 < 0 || r12 > 2 * Math.PI) this.ErrorString = this.ErrorString + "R is't correct; ";
        //            if (r12 < (star.circSpots[s].Radius + star.circSpots[t].Radius))
        //            {
        //                double cosOmega;
        //                double omega;

        //                if (r12 == 0 || star.circSpots[t].ThetaOfSpotCenter == 0)
        //                {
        //                    omega = 0;
        //                }
        //                else
        //                {
        //                    cosOmega = (Math.Cos(star.circSpots[s].ThetaOfSpotCenter) -
        //                        cosr12 * Math.Cos(star.circSpots[t].ThetaOfSpotCenter)) /
        //                        (Math.Sin(r12) * Math.Sin(star.circSpots[t].ThetaOfSpotCenter));
        //                    if (this.star.circSpots[t].PhiOfSpotCenterAtZeroPhase >=
        //                    this.star.circSpots[s].PhiOfSpotCenterAtZeroPhase)
        //                    {
        //                        omega = Math.Acos(cosOmega);
        //                    }
        //                    else
        //                    {
        //                        omega = 2 * Math.PI - Math.Acos(cosOmega);
        //                    }
        //                }

        //                double beta;               

        //                for (int i = 0; i < star.circSpots[t].patch.Length; i++)
        //                {
        //                    for (int j = 0; j < star.circSpots[t].patch[i].Length; j++)
        //                    {
        //                        double cosDelta;
        //                        double delta;
        //                        double eta_t = this.star.circSpots[t].patch[i][j].FiCenter();
        //                        double zeta_t = this.star.circSpots[t].patch[i][j].ThetaCenter();
                                
        //                        beta = eta_t - omega;
        //                        cosDelta = cosr12 * Math.Cos(zeta_t) + 
        //                            Math.Sin(r12) * Math.Sin(zeta_t) * Math.Cos(beta);
        //                        delta = Math.Acos(cosDelta);
        //                        if (delta < this.star.circSpots[s].Radius)
        //                        {
        //                            mask[t][i][j] = s;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    //
        //    double muCenter;
        //    double spotFluxForTph = 0, spotFluxForTsp = 0;
        //    for (int p = 0; p < phases.Length; p++)
        //    {
        //        spotFluxForTph = 0; spotFluxForTsp = 0;
        //        for (int t = 0; t < star.circSpots.Length; t++)
        //        {
        //            muCenter = this.star.MuOfTheSpotCenter(t, phases[p]);
        //            //muCenter = this.star.circSpots[t].MuOfCenter(phases[p], this.star.Inc);
        //            for (int i = 0; i < star.circSpots[t].patch.Length; i++)
        //            {
        //                for (int j = 0; j < star.circSpots[t].patch[i].Length; j++)
        //                {
        //                    double mu = this.star.circSpots[t].Mu(i, j, muCenter, star.Inc, phases[p]);
        //                    if (mu > 0) // the element is visible;
        //                    {
        //                        prArea = this.star.circSpots[t].ProjectedArea(i, j, muCenter, phases[p], this.star.Inc);
        //                        if (mask[t][i][j] == -1)
        //                        {
        //                            ld = this.limbDarkFunc(mu, star.TeffPhot);
        //                            spotFluxForTph = spotFluxForTph + ld * prArea * jPh;
        //                        }
        //                        else
        //                        {
        //                            ld = this.limbDarkFunc(mu, this.star.circSpots[mask[t][i][j]].Teff);
        //                            spotFluxForTph = spotFluxForTph + ld * prArea * jSp[mask[t][i][j]];
        //                        }
        //                        spotFluxForTsp = spotFluxForTsp + ld * prArea * jSp[t];
        //                    }
        //                }
        //            }
        //        }
        //        flux[p] = this.unspottedBrightness - spotFluxForTph + spotFluxForTsp;
        //        flux[p] = flux[p] * scale;
        //    }
        //    return flux;
        //}
    }
}
