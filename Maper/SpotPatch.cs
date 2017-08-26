using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper
{
    /// <summary>
    /// Represents the patch of subdiviation of circular spot.
    /// </summary>
    public class SpotPatch
    {
        private double theta1, theta2;
        private double theta_mean;
        private double sin_theta_mean, cos_theta_mean;
        private double sin_theta1, sin_theta2, cos_theta1, cos_theta2;
        private double phi10, phi20;
        private double phi_mean;
        private double sin_phi_mean, cos_phi_mean;
        private double sin_phi1, sin_phi2, cos_phi1, cos_phi2;

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="fi1">longitude of the left bound.</param>
        /// <param name="fi2">longitude of the right bound.</param>
        /// <param name="theta1">latitude of the bound nearest to the north pole.</param>
        /// <param name="theta2">latitude of the near to equator bound.</param>
        public SpotPatch(double fi1, double fi2, double theta1, double theta2)
        {
            this.phi10 = fi1;
            this.phi20 = fi2;
            this.phi_mean = 0.5 * (fi1 + fi2);
            this.sin_phi_mean = Math.Sin(this.phi_mean);
            this.cos_phi_mean = Math.Cos(this.phi_mean);
            this.sin_phi1 = Math.Sin(fi1);
            this.cos_phi1 = Math.Cos(fi1);
            this.sin_phi2 = Math.Sin(fi2);
            this.cos_phi2 = Math.Cos(fi2);
            
            this.theta1 = theta1;
            this.theta2 = theta2;
            this.theta_mean = 0.5 * (theta1 + theta2);
            this.sin_theta_mean = Math.Sin(this.theta_mean);
            this.cos_theta_mean = Math.Cos(this.theta_mean);
            this.sin_theta1 = Math.Sin(theta1);
            this.sin_theta2 = Math.Sin(theta2);
            this.cos_theta1 = Math.Cos(theta1);
            this.cos_theta2 = Math.Cos(theta2);
        }

        /// <summary>
        /// Gets medium longitude of the patch.
        /// </summary>
        /// <returns></returns>
        public double FiCenter()
        {
            return this.phi_mean;
        }

        /// <summary>
        /// Gets medium latitude of the patch.
        /// </summary>
        /// <returns></returns>
        public double ThetaCenter()
        {
            return this.theta_mean;
        }

        /// <summary>
        /// Gets latitude of near to pole bound.
        /// </summary>
        public double Theta1
        {
            get { return this.theta1; }
        }

        /// <summary>
        /// Gets latitude of near to equator bound.
        /// </summary>
        public double Theta2
        {
            get { return this.theta2; }
        }

        /// <summary>
        /// Gets longitude of the left bound.
        /// </summary>
        public double Phi10
        {
            get { return this.phi10; }
        }

        /// <summary>
        /// Gets longitude of the right bound.
        /// </summary>
        public double Phi20
        {
            get { return this.phi20; }
        }

        /// <summary>
        /// Gets sine of the mean colatitude.
        /// </summary>
        public double SinThetaCenter
        {
            get { return this.sin_theta_mean; }
        }

        /// <summary>
        /// Gets cosine of the mean colatitude.
        /// </summary>
        public double CosThetaCenter
        {
            get { return this.cos_theta_mean; }
        }

        public double CosTheta1
        {
            get { return this.cos_theta1; }
        }

        public double CosTheta2
        {
            get { return this.cos_theta2; }
        }

        public double SinTheta1
        {
            get { return this.sin_theta1; }
        }

        public double SinTheta2
        {
            get { return this.sin_theta2; }
        }

        public double CosPhi1
        {
            get { return this.cos_phi1; }
        }

        public double CosPhi2
        {
            get { return this.cos_phi2; }
        }

        public double SinPhi1
        {
            get { return this.sin_phi1; }
        }

        public double SinPhi2
        {
            get { return this.sin_phi2; }
        }

        public double CosPhiCenter
        {
            get { return this.cos_phi_mean; }
        }

        public double SinPhiCenter
        {
            get { return this.sin_phi_mean; }
        }

        /// <summary>
        /// Shifts upper and lower bounds of the paths at factor scale.
        /// </summary>
        /// <param name="scale"></param>
        public void ShiftInTheta(double scale)
        {
            this.theta1 = this.theta1 * scale;
            this.theta2 = this.theta2 * scale;
            this.theta_mean = scale * this.theta_mean;
            this.sin_theta_mean = Math.Sin(this.theta_mean);
            this.cos_theta_mean = Math.Cos(this.theta_mean);
            this.sin_theta1 = Math.Sin(this.theta1);
            this.sin_theta2 = Math.Sin(this.theta2);
            this.cos_theta1 = Math.Cos(this.theta1);
            this.cos_theta2 = Math.Cos(this.theta2);
        }
    }
}
