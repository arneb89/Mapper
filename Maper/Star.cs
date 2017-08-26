using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper
{
    /// <summary>
    /// Represents the model of the star with circular spots 
    /// </summary>
    public class Star
    {
        // Count of spots;
        private int spotsCount = 0;
        // Array of circular spots;
        public UniformCircSpot[] circSpots;
        // Effective temperature of the star's photosphere;
        private double tPh;
        // Logariphm of surface gravity acceleration;
        private double logg;
        // Inclination;
        private double inc;
        private double sin_inc, cos_inc;

        /// <summary>
        /// Constructor of the class
        /// </summary>
        /// <param name="inc">inclination of the star's rotation axis to the line of sight</param>
        /// <param name="tPh">temperature of unspotted photosphere</param>
        /// <param name="logg">logarithm of the acceleration of gravity</param>
        public Star(double inc, double tPh, double logg)
        {
            this.inc = inc;
            this.sin_inc = Math.Sin(inc);
            this.cos_inc = Math.Cos(inc);
            this.tPh = tPh;
            this.logg = logg;
        }

        public void AddUniformCircularSpot(double phi, double theta, double radius, double teff, int n, int m)
        {
            if (this.spotsCount == 0)
            {
                this.spotsCount++;
                this.circSpots = new UniformCircSpot[this.spotsCount];
                this.circSpots[this.spotsCount-1] = new UniformCircSpot(theta, phi, radius, teff, n, m);
            }
            else
            {
                
                UniformCircSpot[] buf = this.circSpots;
                this.spotsCount++;
                this.circSpots = new UniformCircSpot[this.spotsCount];
                for (int i = 0; i < this.spotsCount - 1; i++)
                {
                    this.circSpots[i] = buf[i];
                }
                this.circSpots[this.spotsCount - 1] = new UniformCircSpot(theta, phi, radius, teff, n, m);
            }
        }

        public void RemoveAllSpots()
        {
            this.spotsCount = 0;
            this.circSpots = null;
        }

        public double MuOfTheSpotCenter(int num, double phase)
        {
            return this.circSpots[num].SinThetaOfTheSpotCenter * Math.Sin(this.circSpots[num].PhiOfSpotCenter(phase)) * this.sin_inc +
                this.circSpots[num].CosThetaOfTheSpotCenter * this.cos_inc;

            //return this.sinTheta0 * Math.Sin(this.PhiOfSpotCenter(phase)) * Math.Sin(inc) +
            //    this.cosTheta0 * Math.Cos(inc);
        }

        public double MuOfTheSpotPathCenter(int s, int i, int j, double phase)
        {
            double eta;
            eta = this.circSpots[s].patch[i][j].FiCenter();
            double sin_eta = Math.Sin(eta);
            double cos_eta = Math.Cos(eta);
            double sin_ksi = this.circSpots[s].patch[i][j].SinThetaCenter;
            double cos_ksi = this.circSpots[s].patch[i][j].CosThetaCenter;
            double alpha = 2 * Math.PI * phase - Math.PI * 0.5;
            double sin_alpha = Math.Sin(alpha);
            double cos_alpha = Math.Cos(alpha);
            double cos_theta=this.circSpots[s].CosThetaOfTheSpotCenter;
            double sin_theta=this.circSpots[s].SinThetaOfTheSpotCenter;

            return sin_inc * (sin_alpha * sin_ksi * sin_eta - cos_alpha * cos_theta * sin_ksi * sin_eta + cos_alpha * sin_theta * cos_ksi) +
                cos_inc * (sin_theta * sin_ksi * cos_eta + cos_theta * cos_ksi);
        }

        /// <summary>
        /// Returns cosine of the angle between spot patch center and sub-Earth point
        /// </summary>
        /// <param name="s">number of the spot</param>
        /// <param name="i">number of subdiviation belt</param>
        /// <param name="j">number of the path in the belt</param>
        /// <param name="muCenter">cosine of the angle between spot's center and sub-Earth point</param>
        /// <param name="omega">angle between directions to the star's pole and to the sub-Earth point
        /// as it seems from spot's center</param>
        /// <returns></returns>
        public double MuOfTheSpotPatchCenter(int s, int i, int j, double muCenter, double omega)
        {
            double sinThetaCenter = this.circSpots[s].patch[i][j].SinThetaCenter;
            double cosThetaCenter = this.circSpots[s].patch[i][j].CosThetaCenter;
            double sinGammaCenter = Math.Sqrt(1 - muCenter * muCenter);
            double mu;
            double beta;

            beta = this.circSpots[s].patch[i][j].FiCenter() - omega;

            mu = muCenter * cosThetaCenter + sinGammaCenter * sinThetaCenter * Math.Cos(beta);

            return mu;
        }

        /// <summary>
        /// Returns cosine of the angle between spot patch center and sub-Earth point
        /// </summary>
        /// <param name="s">number of the spot</param>
        /// <param name="i">number of subdiviation belt</param>
        /// <param name="j">number of the path in the belt</param>
        /// <param name="muCenter">cosine of the angle between spot's center and sub-Earth point</param>
        /// <param name="sinGammaCenter">sine of the angle between spot's center and sub-Earth point</param>
        /// <param name="omega">angle between directions to the star's pole and to the sub-Earth point
        /// as it seems from spot's center</param>
        /// <returns></returns>
        public double MuOfTheSpotPatchCenter(int s, int i, int j, double muCenter, double sinGammaCenter, double omega)
        {
            double sinThetaCenter = this.circSpots[s].patch[i][j].SinThetaCenter;
            double cosThetaCenter = this.circSpots[s].patch[i][j].CosThetaCenter;
            double mu;
            double beta;

            beta = this.circSpots[s].patch[i][j].FiCenter() - omega;

            mu = muCenter * cosThetaCenter + sinGammaCenter * sinThetaCenter * Math.Cos(beta);

            return mu;
        }

        /// <summary>
        /// Returns cosine of the angle between spot patch center and sub-Earth point
        /// </summary>
        /// <param name="s">number of the spot</param>
        /// <param name="i">number of subdiviation belt</param>
        /// <param name="j">number of the path in the belt</param>
        /// <param name="muCenter">cosine of the angle between spot's center and sub-Earth point</param>
        /// <param name="sinGammaCenter">sine of the angle between spot's center and sub-Earth point</param>
        /// <param name="cosOmega">cosine of the angle between directions to the star's pole and to the sub-Earth point
        /// as it seems from spot's center</param>
        /// <param name="sinOmega">sine of the angle between directions to the star's pole and to the sub-Earth point
        /// as it seems from spot's center</param>
        /// <returns></returns>
        public double MuOfTheSpotPatchCenter(int s, int i, int j, double muCenter, double sinGammaCenter, double cosOmega, double sinOmega)
        {
            double sinThetaCenter = this.circSpots[s].patch[i][j].SinThetaCenter;
            double cosThetaCenter = this.circSpots[s].patch[i][j].CosThetaCenter;
            double sinPhiCenter = this.circSpots[s].patch[i][j].SinPhiCenter;
            double cosPhiCenter = this.circSpots[s].patch[i][j].CosPhiCenter;
            double mu;
            //double beta;
            //beta = this.circSpots[s].patch[i][j].FiCenter() - omega;
            //mu = muCenter * cosThetaCenter + sinGammaCenter * sinThetaCenter * Math.Cos(beta);

            mu = muCenter * cosThetaCenter + sinGammaCenter * sinThetaCenter *
                (cosPhiCenter * cosOmega + sinPhiCenter * sinOmega);

            return mu;
        }

        /// <summary>
        /// Returns angular distance between spot's center and sub-Earth point;
        /// </summary>
        /// <param name="s">spot's number</param>
        /// <param name="phase">phase of stellar rotation</param>
        /// <param name="muCenter">cosine of the angle between spot's center and sub-Earth point</param>
        /// <returns></returns>
        public double GetOmega(int s, double phase, double muCenter)
        {
            if (muCenter == 1.0 || muCenter == -1.0) return 0;
            if (this.circSpots[s].ThetaOfSpotCenter == 0) return Math.PI;
            if (this.circSpots[s].ThetaOfSpotCenter == Math.PI) return 0;

            double sinGammaCenter = Math.Sqrt(1.0 - muCenter * muCenter);
            double phiCenter = this.circSpots[s].PhiOfSpotCenter(phase);
            double cosOmega, omega = 0;

            cosOmega = (this.cos_inc - this.circSpots[s].CosThetaOfTheSpotCenter * muCenter) /
                (this.circSpots[s].SinThetaOfTheSpotCenter * sinGammaCenter);
            //cosOmega = (Math.Cos(inc) - Math.Cos(this.theta0) * muCenter) /
            //    (Math.Sin(this.theta0) * sinGammaCenter);

            omega = Math.Acos(cosOmega);

            if (phiCenter <= 0.5 * Math.PI && phiCenter > 1.5 * Math.PI)
            {
                return omega;
            }
            else
            {
                return 2 * Math.PI - omega;
            }

            return 0;
        }

        /// <summary>
        /// Returns cosine of the angular distance between spot's center and sub-Earth point;
        /// </summary>
        /// <param name="s">spot's number</param>
        /// <param name="phase">phase of stellar rotation</param>
        /// <param name="muCenter">cosine of the angle between spot's center and sub-Earth point</param>
        /// <returns></returns>
        public double GetCosOmega(int s, double phase, double muCenter)
        {
            if (muCenter == 1.0 || muCenter == -1.0) return 1.0;
            if (this.circSpots[s].ThetaOfSpotCenter == 0) return -1.0;
            if (this.circSpots[s].ThetaOfSpotCenter == Math.PI) return 1.0;

            double sinGammaCenter = Math.Sqrt(1.0 - muCenter * muCenter);
            double phiCenter = this.circSpots[s].PhiOfSpotCenter(phase);
            double cosOmega;

            cosOmega = (this.cos_inc - this.circSpots[s].CosThetaOfTheSpotCenter * muCenter) /
                (this.circSpots[s].SinThetaOfTheSpotCenter * sinGammaCenter);
            return cosOmega;
        }

        /// <summary>
        /// Returns projected area of the spot's patch to the plane of the sky
        /// </summary>
        /// <param name="s">spot's number</param>
        /// <param name="i">number of subdiviation belt</param>
        /// <param name="j">number of the path in the belt</param>
        /// <param name="muCenter">cosine of the angle between spot's center and sub-Earth point</param>
        /// <param name="omega">angle between directions to the star's pole and to the sub-Earth point
        /// as it seems from spot's center</param>
        /// <returns></returns>
        public double ProjectedAreaOfThePatch(int s, int i, int j, double muCenter, double omega)
        {
            double term1;
            double term2;
            double area;

            double phi1 = this.circSpots[s].patch[i][j].Phi10;
            double phi2 = this.circSpots[s].patch[i][j].Phi20;
            double theta1 = this.circSpots[s].patch[i][j].Theta1;
            double theta2 = this.circSpots[s].patch[i][j].Theta2;

            double sinGammaCenter = Math.Sqrt(1 - muCenter * muCenter);

            if (omega < 0) omega = 2 * Math.PI + omega;

            double sin_theta1 = this.circSpots[s].patch[i][j].SinTheta1;
            double sin_theta2 = this.circSpots[s].patch[i][j].SinTheta2;
            double cos_theta1 = this.circSpots[s].patch[i][j].CosTheta1;
            double cos_theta2 = this.circSpots[s].patch[i][j].CosTheta2;

            term1 = 0.5 * muCenter * (phi2 - phi1) *
                (Math.Pow(sin_theta2, 2) - Math.Pow(sin_theta1, 2));

            term2 = sinGammaCenter * (Math.Sin(phi2 - omega) - Math.Sin(phi1 - omega)) *
                0.5 * ((theta2 - theta1) - (sin_theta2 * cos_theta2 - sin_theta1 * cos_theta1));

            area = term1 + term2;

            return area;
        }

        /// <summary>
        /// Returns projected area of the spot's patch to the plane of the sky
        /// </summary>
        /// <param name="s">spot's number</param>
        /// <param name="i">number of subdiviation belt</param>
        /// <param name="j">number of the path in the belt</param>
        /// <param name="muCenter">cosine of the angle between spot's center and sub-Earth point</param>
        /// <param name="sinGammaCenter">sine of the angle between spot's center and sub-Earth point</param>
        /// <param name="omega">angle between directions to the star's pole and to the sub-Earth point
        /// as it seems from spot's center</param>
        /// <returns></returns>
        public double ProjectedAreaOfThePatch(int s, int i, int j, double muCenter, double sinGammaCenter, double omega)
        {
            double term1;
            double term2;
            double area;

            double phi1 = this.circSpots[s].patch[i][j].Phi10;
            double phi2 = this.circSpots[s].patch[i][j].Phi20;
            double theta1 = this.circSpots[s].patch[i][j].Theta1;
            double theta2 = this.circSpots[s].patch[i][j].Theta2;

            if (omega < 0) omega = 2 * Math.PI + omega;

            double sin_theta1 = this.circSpots[s].patch[i][j].SinTheta1;
            double sin_theta2 = this.circSpots[s].patch[i][j].SinTheta2;
            double cos_theta1 = this.circSpots[s].patch[i][j].CosTheta1;
            double cos_theta2 = this.circSpots[s].patch[i][j].CosTheta2;

            term1 = muCenter * (phi2 - phi1) *
                (Math.Pow(sin_theta2, 2) - Math.Pow(sin_theta1, 2));

            term2 = sinGammaCenter * (Math.Sin(phi2 - omega) - Math.Sin(phi1 - omega)) *
                 ((theta2 - theta1) - (sin_theta2 * cos_theta2 - sin_theta1 * cos_theta1));

            area = 0.5 * (term1 + term2);

            return area;
        }

        public double ProjectedAreaOfThePatch(int s, int i, int j, double muCenter, double sinGammaCenter, double cosOmega, double sinOmega)
        {
            double term1;
            double term2;
            double area;

            double phi1 = this.circSpots[s].patch[i][j].Phi10;
            double phi2 = this.circSpots[s].patch[i][j].Phi20;
            double theta1 = this.circSpots[s].patch[i][j].Theta1;
            double theta2 = this.circSpots[s].patch[i][j].Theta2;

            double sin_theta1 = this.circSpots[s].patch[i][j].SinTheta1;
            double sin_theta2 = this.circSpots[s].patch[i][j].SinTheta2;
            double cos_theta1 = this.circSpots[s].patch[i][j].CosTheta1;
            double cos_theta2 = this.circSpots[s].patch[i][j].CosTheta2;

            double sin_phi1 = this.circSpots[s].patch[i][j].SinPhi1;
            double sin_phi2 = this.circSpots[s].patch[i][j].SinPhi2;
            double cos_phi1 = this.circSpots[s].patch[i][j].CosPhi1;
            double cos_phi2 = this.circSpots[s].patch[i][j].CosPhi2;

            term1 = muCenter * (phi2 - phi1) *
                (Math.Pow(sin_theta2, 2) - Math.Pow(sin_theta1, 2));

            //term2 = sinGammaCenter * (Math.Sin(phi2 - omega) - Math.Sin(phi1 - omega)) *
            //     ((theta2 - theta1) - (sin_theta2 * cos_theta2 - sin_theta1 * cos_theta1));

            term2 = sinGammaCenter * (cosOmega * (sin_phi2 - sin_phi1) + sinOmega * (cos_phi1 - cos_phi2)) *
                 ((theta2 - theta1) - (sin_theta2 * cos_theta2 - sin_theta1 * cos_theta1));

            area = 0.5 * (term1 + term2);

            return area;
        }

        /// <summary>
        /// Returns temperature of the stellar photosphere
        /// </summary>
        public double TeffPhot
        {
            get { return this.tPh; }
        }

        /// <summary>
        /// Returns the inclination of the star's rotation axis to the line of sight
        /// </summary>
        public double Inc
        {
            get { return this.inc; }
        }

        /// <summary>
        /// Returns clone of the class
        /// </summary>
        /// <returns></returns>
        public Star Clone()
        {
            return (Star)this.MemberwiseClone();
        }
    }
}
