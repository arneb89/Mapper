using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper
{
    public class LightCurve
    {
        string band;
        double[] phases;
        double[] fluxes;

        double sigma;
        double fluxMax;
        double fluxMin;

        public LightCurve() { }

        public LightCurve(double[] phases, double[] fluxes, string band, double sigma)
        {
            this.phases = phases;
            this.fluxes = fluxes;
            this.band = band;
            this.sigma = sigma;
        }

        public double FluxMax
        {
            set { this.fluxMax = value; }
            get { return this.fluxMax; }
        }

        public double FluxMin
        {
            set { this.fluxMin = value; }
            get { return this.fluxMin; }
        }

        public double Sigma
        {
            set { this.sigma = value; }
            get { return this.sigma; }
        }

        public double[] Fluxes
        {
            set { this.fluxes = value; }
            get { return this.fluxes; }
        }

        public double[] Phases
        {
            set { this.phases = value; }
            get { return this.phases; }
        }

        public string Band
        {
            set { this.band = value; }
            get { return this.band; }
        }
    }
}
