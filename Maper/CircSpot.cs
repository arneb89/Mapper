using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper
{
    /// <summary>
    /// Represents a circular spot on stellar surface.
    /// </summary>
    public class CircSpot
    {
        // Coordinates of the center;
        private double theta0, phi0;
        private double cosTheta0, sinTheta0;
        // Radius of the spot;
        private double radius;
        // Parameters of subdividing;
        private int n, m;
        // Array of patches;
        public SpotPatch[][] patch;

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="theta0">polar angle of the spot center (in radiants).</param>
        /// <param name="phi0">longitude of the spot center (in radiants).</param>
        /// <param name="radius">radius of the spot (in radiants).</param>
        /// <param name="n">number of the belts of subdiviation.</param>
        /// <param name="m">number of near-to-bound patches.</param>
        public CircSpot(double theta0, double phi0, double radius, int n, int m)
        {
            this.theta0 = theta0;
            this.cosTheta0 = Math.Cos(theta0);
            this.sinTheta0 = Math.Sin(theta0);
            this.phi0 = phi0;
            this.radius = radius;
            
            this.n = n;
            this.m = m;

            this.patch = new SpotPatch[n][];
            double dphiN = 2 * Math.PI * Math.Sin(radius) / (double)m;
            double dtheta = radius / (double)n;
            double beltSquare0 = 2 * Math.PI * (Math.Cos(radius - dtheta) - Math.Cos(radius));
            double pathSquare0 = beltSquare0 / (double)m;
            double beltSquare;

            int patchNum;
            double dphi;
            this.patch[n - 1] = new SpotPatch[m];
            for (int j = 0; j < m ; j++)
            {
                this.patch[n - 1][j] = new SpotPatch(j * dphiN, (j + 1) * dphiN, 
                    radius - dtheta, radius);
            }
            
            for (int i = 0; i < n; i++)
            {
                beltSquare = 2 * Math.PI * (Math.Cos(i * dtheta) - Math.Cos((i + 1) * dtheta));
                patchNum = (int) (beltSquare / pathSquare0);
                if (patchNum == 0) patchNum = 1;
                this.patch[i] = new SpotPatch[patchNum];
                dphi = 2 * Math.PI / (double)patchNum;
                for (int j = 0; j < patchNum; j++)
                {
                    this.patch[i][j] = new SpotPatch(j * dphi, (j + 1) * dphi, i * dtheta, 
                        (i + 1) * dtheta);
                }
            }
        }

        /// <summary>
        /// Resize the spot;
        /// </summary>
        /// <param name="newRadius">new radius of the spot [deg]</param>
        public void Resize(double newRadius)
        {
            double scale = newRadius / this.radius;
            this.radius = newRadius;
            
            for (int i = 0; i < this.patch.Length; i++)
            {
                for (int j = 0; j < this.patch[i].Length; j++)
                {
                    this.patch[i][j].ShiftInTheta(scale);
                }
            }
        }

        /// <summary>
        /// Gets cosine of the angle between normal to the spot's center and the line of sight.
        /// </summary>
        /// <param name="phase">phase of rotation of the star.</param>
        /// <param name="inc">inclination of the rotation axis to the line of sight.</param>
        /// <returns></returns>
        public double MuOfCenter(double phase, double inc)
        {
            return this.sinTheta0 * Math.Sin(this.PhiOfSpotCenter(phase)) * Math.Sin(inc) +
                this.cosTheta0 * Math.Cos(inc);
            //return Math.Sin(this.theta0) * Math.Sin(this.PhiOfSpotCenter(phase)) * Math.Sin(inc) +
            //    Math.Cos(this.theta0) * Math.Cos(inc);
        }

        private double GetOmega(double muCenter, double phase, double inc)
        {
            if (muCenter == 1.0 || muCenter == -1.0) return 0;
            if (this.theta0 == 0) return Math.PI;
            if (this.theta0 == Math.PI) return 0;

            double sinGammaCenter = Math.Sqrt(1.0 - muCenter * muCenter);
            double phiCenter = this.PhiOfSpotCenter(phase);
            double cosOmega, omega = 0;

            cosOmega = (Math.Cos(inc) - this.cosTheta0 * muCenter) /
                (this.sinTheta0 * sinGammaCenter);
            //cosOmega = (Math.Cos(inc) - Math.Cos(this.theta0) * muCenter) /
            //    (Math.Sin(this.theta0) * sinGammaCenter);

            if (phiCenter <= 0.5 * Math.PI && phiCenter > 1.5 * Math.PI)
            {
                omega = Math.Acos(cosOmega);
            }
            else
            {
                omega = 2 * Math.PI - Math.Acos(cosOmega);
            }

            return omega;
        }

        /// <summary>
        /// Change spot parameters
        /// </summary>
        /// <param name="phi">longitude of the spot center</param>
        /// <param name="theta">colatitude of the spot center</param>
        /// <param name="radius">radius of the spot</param>
        public void ChangeSpotParameters(double phi, double theta, double radius, double teff)
        {
            if (theta >= 0.0)
            {
                this.theta0 = theta;
                this.phi0 = phi;
            }
            else
            {
                this.theta0 = Math.Abs(theta);
                this.phi0 = phi + Math.PI;
            }
            this.cosTheta0 = Math.Cos(this.theta0);
            this.sinTheta0 = Math.Sin(this.theta0);

            if (this.phi0 > 2 * Math.PI)
            {
                this.phi0 = this.phi0 - Math.Floor(this.phi0 / (2 * Math.PI)) * 2 * Math.PI;
            }

            this.Resize(radius);
        }

        /// <summary>
        /// Gets projected area of the patch (i,j).
        /// </summary>
        /// <param name="i">number of belt in which the patch plased.</param>
        /// <param name="j">number of the patch in i-th belt.</param>
        /// <param name="muCenter">cosine of the angle between normal to the spot center and line of sight.</param>
        /// <returns></returns>
        public double ProjectedArea(int i, int j, double muCenter, double phase, double inc)
        {
            double term1;
            double term2;
            double area;

            double phi1 = this.patch[i][j].Phi10;
            double phi2 = this.patch[i][j].Phi20;
            double theta1 = this.patch[i][j].Theta1;
            double theta2 = this.patch[i][j].Theta2;

            double sinGammaCenter = Math.Sqrt(1 - muCenter * muCenter);

            double omega;

            omega = this.GetOmega(muCenter, phase, inc);

            if (omega < 0) omega = 2 * Math.PI + omega;

            double sin_theta1 = this.patch[i][j].SinTheta1;
            double sin_theta2 = this.patch[i][j].SinTheta2;
            double cos_theta1 = this.patch[i][j].CosTheta1;
            double cos_theta2 = this.patch[i][j].CosTheta2;

            term1 = 0.5 * muCenter * (phi2 - phi1) *
                (Math.Pow(sin_theta2, 2) - Math.Pow(sin_theta1, 2));

            term2 = sinGammaCenter * (Math.Sin(phi2 - omega) - Math.Sin(phi1 - omega)) *
                0.5 * ((theta2 - theta1) - (sin_theta2 * cos_theta2 - sin_theta1 * cos_theta1));

            //term1 = 0.5 * muCenter * (phi2 - phi1) * 
            //    (Math.Pow(Math.Sin(theta2), 2) - Math.Pow(Math.Sin(theta1), 2));

            //term2 = sinGammaCenter * (Math.Sin(phi2 - omega) - Math.Sin(phi1 - omega)) *
            //    (0.5 * (theta2 - theta1) - 0.25 * (Math.Sin(2 * theta2) - Math.Sin(2 * theta1)));

            area = term1 + term2;

            return area;
        }

        /// <summary>
        /// Gets cosine of the angle between center of the patch (i,j) and the line of sight.
        /// </summary>
        /// <param name="i">number of belt in which the patch plased.</param>
        /// <param name="j">number of the patch in i-th belt.</param>
        /// <param name="phase">phase of rotation of the star.</param>
        /// <param name="muCenter">cosine of the angle between normal to the spot center and line of sight.</param>
        /// <param name="sinInc">sine of the inclination of rotational axis to the line of sight.</param>
        /// <returns></returns>
        public double Mu(int i, int j, double muCenter, double inc, double phase)
        {
            //double zeta = this.patch[i][j].ThetaCenter();
            double sinThetaCenter = this.patch[i][j].SinThetaCenter;
            double cosThetaCenter = this.patch[i][j].CosThetaCenter;
            double sinGammaCenter = Math.Sqrt(1 - muCenter * muCenter);
            double mu;
            double omega, beta;

            omega = this.GetOmega(muCenter, phase, inc);

            beta = this.patch[i][j].FiCenter() - omega;

            mu = muCenter * cosThetaCenter + sinGammaCenter * sinThetaCenter * Math.Cos(beta);

            //mu = muCenter * Math.Cos(zeta) + sinGammaCenter * Math.Sin(zeta) * Math.Cos(beta);

            return mu;
        }

        /// <summary>
        /// Gets or sets colatitude of the spot's center.
        /// </summary>
        public double ThetaOfSpotCenter
        {
            get 
            {
                return this.theta0;
            }
            set
            {
                this.theta0 = value;
                //if (this.theta0 < 0) this.theta0 =0;
                //if (this.theta0 > Math.PI) this.theta0 = Math.PI;
                this.cosTheta0 = Math.Cos(value);
                this.sinTheta0 = Math.Sin(value);
            }
        }

        public double CosThetaOfTheSpotCenter
        {
            get { return this.cosTheta0; }
        }

        public double SinThetaOfTheSpotCenter
        {
            get { return this.sinTheta0; }
        }

        /// <summary>
        /// Gets or sets longitude of the spot's center at zero phase.
        /// </summary>
        public double PhiOfSpotCenterAtZeroPhase
        {
            get
            {
                return this.phi0;
            }
            set
            {
                this.phi0 = value;
                //if (this.phi0 < 0) this.phi0 = 0;
                //if (this.phi0 > 2 * Math.PI) this.phi0 = 2 * Math.PI;
            }
        }

        /// <summary>
        /// Gets longitude of the spot's center at given phase of stellar rotation.
        /// </summary>
        /// <param name="phase"></param>
        /// <returns></returns>
        public double PhiOfSpotCenter(double phase)
        {
            return this.phi0 + 2 * phase * Math.PI;
        }

        /// <summary>
        /// Gets radius of the spot.
        /// </summary>
        public double Radius
        {
            get { return this.radius; }
        }

        /// <summary>
        /// Gets the total number of patches.
        /// </summary>
        /// <returns></returns>
        public int GetPatchNumber()
        {
            int Ns, PN;
            Ns = this.patch.Length;
            PN = 0;
            for (int i = 0; i < Ns; i++)
            {
                PN = PN + patch[i].Length;
            }
            return PN;
        }

        public double[][] Coords()
        {
            int patchNum = this.GetPatchNumber();
            double[][] mas = new double[patchNum][];
            for (int i = 0; i < patchNum; i++)
            {
                mas[i] = new double[4];
            }

            int k = 0;
            for (int i = 0; i < patch.Length; i++)
            {
                for (int j = 0; j < patch[i].Length; j++)
                {
                    mas[k][0] = patch[i][j].Theta1;
                    mas[k][1] = patch[i][j].Theta2;
                    mas[k][2] = patch[i][j].Phi10;
                    mas[k][3] = patch[i][j].Phi20;
                    k++;
                }
            }
            return mas;
        }
    }
}
