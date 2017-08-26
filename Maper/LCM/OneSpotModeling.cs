using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLib;
using DotNumerics;

namespace Maper.LCM
{
    public class OneSpotModeling
    {
        private Table1D[] lc;
        private LCModeller lcm;
        public double[][] lcComp;

        public OneSpotModeling(Table1D[] lc, Spline31D[] spInt)
        {
            this.lc = lc;
            Star star = new Star(45 * Math.PI / 180, 5000, 4.5);
            star.AddUniformCircularSpot(0.0, 0.0, 1.0, 5000, 30, 90);
            this.lcm = new LCModeller(star, spInt[0]);
        }

        /// <summary>
        /// Khi2
        /// </summary>
        /// <param name="x">
        /// x[0] --- longitude of the spot;
        /// x[1] --- 90 - latitude of the spot;
        /// x[2] --- radius of the spot.
        /// </param>
        /// <returns></returns>
        private double Khi2(double[] x)
        {
            this.lcm.StarM.circSpots[0] = new UniformCircSpot(x[1], x[0], x[2], 4000, 30, 30 * 3);
            double[] fluxes = this.lcm.GetLightCurve(this.lc[0].XMas, 0.725, 0.725, 1e-5);
            double khi2 = 0;
            for (int i = 0; i < fluxes.Length; i++)
            {
                khi2 = khi2 + Math.Pow(fluxes[i] - this.lc[0].FMas[i], 2);
            }
            return khi2;
        }

        public double[] Find(double phi, double theta, double radius)
        {
            double[] x = new double[3];
            x[0] = phi;
            x[1] = theta;
            x[2] = radius;
            DotNumerics.Optimization.Simplex simplex = new DotNumerics.Optimization.Simplex();

            x = simplex.ComputeMin(this.Khi2, x);


            this.lcComp = new double[2][];
            lcComp[0] = new double[100];
            lcComp[1] = new double[100];
            for (int p = 0; p < 100; p++) lcComp[0][p] = p * 0.01;

            this.lcm.StarM.circSpots[0] = new UniformCircSpot(x[1], x[0], x[2], 4000, 30, 30 * 3);
            this.lcComp[1] = this.lcm.GetLightCurve(this.lcComp[0], 0.725, 0.725, 1e-5);

            return x;
        }
    }
}
