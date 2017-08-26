using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLib;
using DotNumerics.Optimization;

namespace Maper.LCM
{
    /// <summary>
    /// Provides methods for multispot mapping of the star;
    /// </summary>
    class MultiSpotMapper1
    {
        // collection of modellers for different bands;
        private LCModeller1[] modellers = null;
        // collection of observed light curves for different bands;
        private LightCurve[] lcObs = null;
        // collection of modelled light curves for different bands;
        private LightCurve[] lcMod = null;

        // Khi2 value for found solution;
        private double khi2;

        private bool[] fixedParsMask;
        
        /// <summary>
        /// Constructor of the class;
        /// </summary>
        /// <param name="modellers">collection of modellers for different bands.</param>
        /// <param name="lcObs">collection of observed light curves for different bands.</param>
        public MultiSpotMapper1(LCModeller1[] modellers, LightCurve[] lcObs)
        {
            this.modellers = modellers;
            this.lcObs = lcObs;
            this.khi2 = 0;
        }

        private double Khi2(double[] x)
        {
            double khi2 = 0;
            double[] fluxes;

            int spotNum = (x.Length + 1) / 4;

            for (int q = 0; q < this.lcObs.Length; q++)
            {
                for (int s = 0; s < spotNum; s++)
                {
                    this.modellers[q].StarM.circSpots[s].ThetaOfSpotCenter = x[1 + s * 4];
                    this.modellers[q].StarM.circSpots[s].PhiOfSpotCenterAtZeroPhase = x[0 + s * 4];
                    this.modellers[q].StarM.circSpots[s].Resize(x[2 + s * 4]);
                    this.modellers[q].StarM.circSpots[s].Teff = x[3 + s * 4];
                    //this.modellers[q].StarM.circSpots[s] =
                    //    new UniformCircSpot(x[1 + s * 4], x[0 + s * 4],
                    //        x[2 + s * 4], x[3 + s * 4], 50, 150);
                }
            }

            double khi2ForBand = 0;
            for (int q = 0; q < this.lcObs.Length; q++)
            {
                fluxes = this.modellers[q].GetFluxes(this.lcObs[q].Phases,
                    this.lcObs[q].FluxMax / this.modellers[q].UnspottedBrightness);

                khi2ForBand = 0;
                for (int p = 0; p < this.lcObs[q].Phases.Length; p++)
                {
                    khi2ForBand += Math.Pow(fluxes[p] - this.lcObs[q].Fluxes[p], 2);
                }
                khi2ForBand = khi2ForBand / Math.Pow(this.lcObs[q].Sigma, 2);
                khi2 += khi2ForBand;
            }
            return khi2;
        }

        private double[] ResudalVector(double[] x)
        {
            double[] fluxes;
            double[] resVector;
            int totalNumberOfObservedFluxes = 0;
            for (int i = 0; i < this.lcObs.Length; i++)
            {
                totalNumberOfObservedFluxes += this.lcObs[i].Fluxes.Length;
            }
            resVector = new double[totalNumberOfObservedFluxes];

            int spotNum = (x.Length + 1) / 4;

            for (int q = 0; q < this.lcObs.Length; q++)
            {
                for (int s = 0; s < spotNum; s++)
                {
                    //this.modellers[q].StarM.circSpots[s].ChangeSpotParameters(x[0 + s * 4], x[1 + s * 4], x[2 + s * 4], x[3 + s * 4]);
                    if (x[1 + s * 4] >= 0.0)
                    {
                        this.modellers[q].StarM.circSpots[s].ThetaOfSpotCenter = x[1 + s * 4];
                        this.modellers[q].StarM.circSpots[s].PhiOfSpotCenterAtZeroPhase = x[0 + s * 4];
                    }
                    else
                    {
                        this.modellers[q].StarM.circSpots[s].ThetaOfSpotCenter = Math.Abs(x[1 + s * 4]);
                        this.modellers[q].StarM.circSpots[s].PhiOfSpotCenterAtZeroPhase = x[0 + s * 4] + Math.PI;
                    }
                    this.modellers[q].StarM.circSpots[s].Resize(x[2 + s * 4]);
                    this.modellers[q].StarM.circSpots[s].Teff = x[3 + s * 4];
                    //this.modellers[q].StarM.circSpots[s] =
                    //    new UniformCircSpot(x[1 + s * 4], x[0 + s * 4],
                    //        x[2 + s * 4], x[3 + s * 4], 50, 150);
                }
            }
            {
                int j = 0;
                for (int q = 0; q < this.lcObs.Length; q++)
                {
                    fluxes = this.modellers[q].GetFluxes(this.lcObs[q].Phases,
                        this.lcObs[q].FluxMax / this.modellers[q].UnspottedBrightness);

                    for (int p = 0; p < this.lcObs[q].Phases.Length; p++)
                    {
                        resVector[j] += fluxes[p] - this.lcObs[q].Fluxes[p];
                        j++;
                    }
                }
            }
            return resVector;
        }

        private double[] GradKhi2(double[] x)
        {
            double[] grad = new double[x.Length];
            int spotNum = (x.Length + 1) / 4;

            double[] dist = new double[4] { 0.02, 0.01, 0.01, 0.1 };
            double[] x2 = new double[x.Length];

            double khi1, khi2;

            khi1 = this.Khi2(x);

            for (int s = 0; s < spotNum; s++)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (this.fixedParsMask[k + 4 * s]) 
                    { 
                        grad[k + 4 * s] = 0.0; 
                    }
                    else
                    {
                        for (int i = 0; i < x2.Length; i++) x2[i] = x[i];
                        x2[k + 4 * s] += dist[k];
                        khi2 = this.Khi2(x2);
                        grad[k + 4 * s] = (khi2 - khi1) / dist[k];
                    }
                }
            }

            return grad;
        }


        public double[] StartMapping(SpotParsInitBox spotParsInitBox, string minimizator, double tau)
        {
            OptBoundVariable[] x = new OptBoundVariable[spotParsInitBox.SpotsNumber * 4];

            for (int i = 0; i < x.Length; i++) x[i] = new OptBoundVariable();

            for (int q = 0; q < this.lcObs.Length; q++)
            {
                this.modellers[q].StarM.RemoveAllSpots();
                for (int s = 0; s < spotParsInitBox.SpotsNumber; s++)
                {
                    this.modellers[q].StarM.AddUniformCircularSpot(
                        spotParsInitBox.spots[s].longitude * Math.PI / 180,
                        spotParsInitBox.spots[s].colatutude * Math.PI / 180, 
                        spotParsInitBox.spots[s].radius * Math.PI / 180, 
                        spotParsInitBox.spots[s].teff,
                        spotParsInitBox.spots[s].beltsCount, 
                        spotParsInitBox.spots[s].nearEquatorialPatchesCount);
                }
            }

            this.fixedParsMask = new bool[x.Length];

            for (int s = 0; s < spotParsInitBox.SpotsNumber; s++)
            {
                x[0 + s * 4].InitialGuess = spotParsInitBox.spots[s].longitude * Math.PI / 180;
                x[0 + s * 4].UpperBound = spotParsInitBox.spots[s].longitudeUpperLimit * Math.PI / 180;
                x[0 + s * 4].LowerBound = spotParsInitBox.spots[s].longitudeLowerLimit * Math.PI / 180;
                x[0 + s * 4].Fixed = spotParsInitBox.spots[s].longitudeFixed;
                this.fixedParsMask[0 + s * 4] = spotParsInitBox.spots[s].longitudeFixed;

                x[1 + s * 4].InitialGuess = spotParsInitBox.spots[s].colatutude * Math.PI / 180;
                x[1 + s * 4].UpperBound = spotParsInitBox.spots[s].colatitudeUpperLimit * Math.PI / 180;
                x[1 + s * 4].LowerBound = spotParsInitBox.spots[s].colatitudeLowerLimit * Math.PI / 180;
                x[1 + s * 4].Fixed = spotParsInitBox.spots[s].colatitudeFixed;
                this.fixedParsMask[1 + s * 4] = spotParsInitBox.spots[s].colatitudeFixed;

                x[2 + s * 4].InitialGuess = spotParsInitBox.spots[s].radius * Math.PI / 180;
                x[2 + s * 4].UpperBound = spotParsInitBox.spots[s].radiusUpperLimit * Math.PI / 180;
                x[2 + s * 4].LowerBound = spotParsInitBox.spots[s].radiusLowerLimit * Math.PI / 180;
                x[2 + s * 4].Fixed = spotParsInitBox.spots[s].radiusFixed;
                this.fixedParsMask[2 + s * 4] = spotParsInitBox.spots[s].radiusFixed;

                x[3 + s * 4].InitialGuess = spotParsInitBox.spots[s].teff;
                x[3 + s * 4].UpperBound = spotParsInitBox.spots[s].teffUpperLimit;
                x[3 + s * 4].LowerBound = spotParsInitBox.spots[s].teffLowerLimit;
                x[3 + s * 4].Fixed = spotParsInitBox.spots[s].teffFixed;
                this.fixedParsMask[3 + s * 4] = spotParsInitBox.spots[s].teffFixed;
            }

            double[] res = new double[4];

            if (minimizator == "Simplex")
            {
                Simplex simplex = new Simplex();
                res = simplex.ComputeMin(this.Khi2, x);
            }
            if (minimizator == "TN")
            {
                //DotNumerics.Optimization.TruncatedNewton tn = new TruncatedNewton();
                DotNumerics.Optimization.L_BFGS_B bfgs = new L_BFGS_B();
                //tn.SearchSeverity = 1;
                
                //res = tn.ComputeMin(this.Khi2, this.GradKhi2, x);
                res = bfgs.ComputeMin(this.Khi2, this.GradKhi2, x);
            }
            if (minimizator == "LM")
            {
                LevenbergMaquard gn = new LevenbergMaquard();
                gn.MinimizedFunction = this.ResudalVector;
                double[] step = new double[x.Length];
                double[] xinit = new double[x.Length];
                bool[] isFixed = new bool[x.Length];
                double[] lowLimits = new double[x.Length];
                double[] highLimits = new double[x.Length];

                
                for (int i = 0; i < xinit.Length; i++)
                {
                    xinit[i] = x[i].InitialGuess;
                    isFixed[i] = x[i].Fixed;
                    lowLimits[i] = x[i].LowerBound;
                    highLimits[i] = x[i].UpperBound;
                    if (i != 0 && (i + 1) % 4 == 0)
                    {
                        step[i] = 10; // step for temperature;
                    }
                    else
                    {
                        step[i] = 0.02;
                    }
                }

                gn.StepForDer = step;
                gn.IterMax = 100;
                gn.Tau = tau;
                res = gn.ComputeMin(xinit, isFixed, lowLimits, highLimits);
            }

            this.khi2 = this.Khi2(res);

            this.GenerateModLightCurve();

            return res;
        }

        public void GenerateModLightCurve()
        {
            int phasesCount = 100;
            double[] phases = new double[phasesCount];
            double[] fluxes;

            this.lcMod = new LightCurve[lcObs.Length];
            for (int p = 0; p < phasesCount; p++)
            {
                phases[p] = (double)p / phasesCount;
            }

            double scale;
            for (int q = 0; q < this.lcMod.Length; q++)
            {
                scale = this.lcObs[q].FluxMax / this.modellers[q].UnspottedBrightness;
                fluxes = this.modellers[q].GetFluxes(phases, scale);
                this.lcMod[q] = new LightCurve(phases, fluxes, this.lcObs[q].Band, 0);
            }
        }

        public LightCurve[] GetModelLightCurves
        {
            get { return this.lcMod; }
        }

        public double Khi2Value
        {
            get { return this.khi2; }
        }
    }
}
