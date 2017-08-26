using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper.LCM
{
    public class spotpars
    {
        public double colatutude = 0;
        public double colatitudeUpperLimit = 180;
        public double colatitudeLowerLimit = 0;
        public bool colatitudeFixed = false;

        public double longitude = 0;
        public double longitudeUpperLimit = 360;
        public double longitudeLowerLimit = 0;
        public bool longitudeFixed = false;

        public double radius = 20;
        public double radiusUpperLimit = 90;
        public double radiusLowerLimit = 0;
        public bool radiusFixed = false;

        public double teff = 4000;
        public double teffUpperLimit = 5000;
        public double teffLowerLimit = 4000;
        public bool teffFixed = false;

        public int beltsCount = 30;
        public int nearEquatorialPatchesCount = 90;
    }

    public class SpotParsInitBox
    {
        // spots count;
        private int spotsNum = 0;
        // spots array;
        public spotpars[] spots;

        /// <summary>
        /// Adds new spot to the collection;
        /// </summary>
        public void AddSpot()
        {

            if (this.spotsNum > 0)
            {
                spotpars[] spotsCopy = this.spots;

                this.spotsNum++;

                this.spots = new spotpars[this.spotsNum];

                for (int i = 0; i < this.spotsNum - 1; i++)
                {
                    this.spots[i] = spotsCopy[i];
                }

                this.spots[this.spotsNum - 1] = new spotpars();
            }

            else
            {
                this.spotsNum = 1;

                this.spots = new spotpars[this.spotsNum];

                this.spots[0] = new spotpars();
            }
        }

        /// <summary>
        /// deletes the spot from the collection
        /// </summary>
        /// <param name="num">number of the spot in the collection</param>
        public void DelSpot(int num)
        {
            if (this.spotsNum > 0)
            {
                spotpars[] spotsCopy = this.spots;

                this.spotsNum--;

                this.spots = new spotpars[this.spotsNum];

                int q = 0;
                for (int i = 0; i < this.spotsNum; i++)
                {
                    if (i >= num) q = 1;
                    this.spots[i] = spotsCopy[i + q];
                }
            }
        }

        /// <summary>
        /// returns count of spots in the collection
        /// </summary>
        public int SpotsNumber
        {
            get
            {
                return this.spotsNum;
            }
        }
    }
}
