using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper
{
    class RectSpot
    {
        private double phi0, theta0;
        private double thetaWidth, phiWidth;

        private RectSpot(double phi0, double theta0, double thetaWidth, double phiWidth)
        {
            this.phi0 = phi0;
            this.theta0 = theta0;
            this.thetaWidth = thetaWidth;
            this.phiWidth = phiWidth;
        }

        public double PhiCenter
        {
            get
            {
                return this.phi0;
            }
        }

        public double ThetaCenter
        {
            get
            {
                return this.theta0;
            }
        }

        public double PhiWidth
        {
            get
            {
                return this.phiWidth;
            }
        }

        public double ThetaWidth
        {
            get
            {
                return this.thetaWidth;
            }
        }
    }
}
