using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper
{
    /// <summary>
    /// Represents an uniform circular spot on the stellar surface.
    /// </summary>
    public class UniformCircSpot: CircSpot
    {
        // Effective temperature of the spot;
        private double teff;

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="theta0">polar angle of the spot center;</param>
        /// <param name="phi0">longitude of the spot center;</param>
        /// <param name="radius">radius of the spot;</param>
        /// <param name="teff">temperature of the spot;</param>
        /// <param name="n">parameter of subdiviation, amount of belts of subdiviation;</param>
        /// <param name="m">parameter of subdiviation, amount of patches of the boundary belt.</param>
        public UniformCircSpot(double theta0, double phi0, double radius, double teff, int n, int m)
            : base(theta0, phi0, radius, n, m)
        {
            this.teff = teff;
        }
        
        /// <summary>
        /// Gets or sets effective temperature of the spot;
        /// </summary>
        public double Teff
        {
            get { return this.teff; }
            set { this.teff = value; }
        }
    }
}
