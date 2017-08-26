using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLib;
using DotNumerics.Optimization;

namespace Maper.LCM
{
    public class MultiSpotMapper
    {
        private Star star;
        private LightCurve[] lcObs;
        private LightCurve[] lcMod;
        private LCModeller[] modeller;
        private double[] scales;

        public MultiSpotMapper(Star star, Spline31D[] spInt, LightCurve[] lcObs)
        {
            this.star = star.Clone();
            this.lcObs = lcObs;
            this.modeller = new LCModeller[lcObs.Length];
            for (int q = 0; q < lcObs.Length; q++)
            {
                modeller[q] = new LCModeller(star.Clone(), spInt[q]);
            }
            this.lcMod = new LightCurve[lcObs.Length];

            this.scales = new double[lcObs.Length];
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
                    this.modeller[q].StarM.circSpots[s] = 
                        new UniformCircSpot(x[1 + s * 4], x[0 + s * 4], x[2 + s * 4], x[3 + s * 4], 30, 90);
                }
            }

            for (int q = 0; q < this.lcObs.Length; q++)
            {
                fluxes = this.modeller[q].GetLightCurve(this.lcObs[q].Phases, 0.794, 0.622, 5.94e-7/*1.58e-7 HII1883*/);
                
                for (int p = 0; p < this.lcObs[q].Phases.Length; p++ )
                {
                    khi2 += Math.Pow(fluxes[p] - this.lcObs[q].Fluxes[p], 2);
                }
            }
            return khi2;
        }

        private double[] GradKhi2(double[] x)
        {
            double[] grad = new double[x.Length];
            int spotNum = (x.Length + 1) / 4;

            double[] dist = new double[4] { 0.01, 0.01, 0.01, 1 };
            double[] x2 = new double[x.Length];

            double khi1, khi2;

            khi1 = this.Khi2(x);

            for (int s = 0; s < spotNum; s++)
            {
                for (int k = 0; k < 4; k++)
                {
                    for (int i = 0; i < x2.Length; i++) x2[i] = x[i];
                    x2[k + 4 * s] += dist[k];
                    khi2 = this.Khi2(x2);
                    grad[k + 4 * s] = (khi2 - khi1) / dist[k];
                }
            }

            return grad;
        }


        public double[] StartMapping(SpotParsInitBox spotParsInitBox)
        {
            OptBoundVariable[] x = new OptBoundVariable[spotParsInitBox.SpotsNumber * 4];

            for (int i = 0; i < x.Length; i++) x[i] = new OptBoundVariable();

            for (int q = 0; q < this.lcObs.Length; q++)
            {
                for (int s = 0; s < spotParsInitBox.SpotsNumber; s++)
                {
                    this.modeller[q].StarM.AddUniformCircularSpot(0.0, 0.0, 1.0, 4000, 30, 90);
                }
            }

            for (int s = 0; s < spotParsInitBox.SpotsNumber; s++)
            {
                x[0 + s * 4].InitialGuess = spotParsInitBox.spots[s].longitude * Math.PI / 180;
                x[0 + s * 4].UpperBound = spotParsInitBox.spots[s].longitudeUpperLimit * Math.PI / 180;
                x[0 + s * 4].LowerBound = spotParsInitBox.spots[s].longitudeLowerLimit * Math.PI / 180;
                x[0 + s * 4].Fixed = spotParsInitBox.spots[s].longitudeFixed;

                x[1 + s * 4].InitialGuess = spotParsInitBox.spots[s].colatutude * Math.PI / 180;
                x[1 + s * 4].UpperBound = spotParsInitBox.spots[s].colatitudeUpperLimit * Math.PI / 180;
                x[1 + s * 4].LowerBound = spotParsInitBox.spots[s].colatitudeLowerLimit * Math.PI / 180;
                x[1 + s * 4].Fixed = spotParsInitBox.spots[s].colatitudeFixed;

                x[2 + s * 4].InitialGuess = spotParsInitBox.spots[s].radius * Math.PI / 180;
                x[2 + s * 4].UpperBound = spotParsInitBox.spots[s].radiusUpperLimit * Math.PI / 180;
                x[2 + s * 4].LowerBound = spotParsInitBox.spots[s].radiusLowerLimit * Math.PI / 180;
                x[2 + s * 4].Fixed = spotParsInitBox.spots[s].radiusFixed;

                x[3 + s * 4].InitialGuess = spotParsInitBox.spots[s].teff;
                x[3 + s * 4].UpperBound = spotParsInitBox.spots[s].teffUpperLimit;
                x[3 + s * 4].LowerBound = spotParsInitBox.spots[s].teffLowerLimit;
                x[3 + s * 4].Fixed = spotParsInitBox.spots[s].teffFixed;
            }

            double[] res;

            Simplex simplex = new Simplex();

            DotNumerics.Optimization.TruncatedNewton tn = new TruncatedNewton();

            
            //res = simplex.ComputeMin(this.Khi2, x);

            res = tn.ComputeMin(this.Khi2, this.GradKhi2, x);

            this.GenerateModLightCurve();

            return res;
        }

        private void GenerateModLightCurve()
        {
            double[] phases = new double[100];
            double[][] fluxes = new double[this.lcObs.Length][];

            for (int p = 0; p < phases.Length; p++)
            {
                phases[p] = p / (double)phases.Length;
            }

            for (int q = 0; q < this.lcObs.Length; q++)
            {
                fluxes[q] = this.modeller[q].GetLightCurve(phases, 0.794, 0.622, 5.94e-7/*1.58e-7 HII1883*/);
            }

            for (int q = 0; q < this.lcMod.Length; q++)
            {
                this.lcMod[q] = new LightCurve();
                this.lcMod[q].Fluxes = fluxes[q];
                this.lcMod[q].Phases = phases;
            }
        }

        public LightCurve[] GetModLightCurves
        {
            get { return this.lcMod; }
        }
    }
}
