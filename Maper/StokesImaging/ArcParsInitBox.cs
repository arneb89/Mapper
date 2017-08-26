using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper.StokesImaging
{
    public class ArcSpotPars
    {
        public double colatitude1 = 0;
        public double colatitude2 = 180;

        public double longitude1 = 0;
        public double longitude2 = 360;

        public double brightness = 0.0;
    }

    public class ArcParsInitBox
    {
        // spots count;
        private int spotsNum = 0;
        // spots array;
        public ArcSpotPars[] spots;

        /// <summary>
        /// Adds new spot to the collection;
        /// </summary>
        public void AddSpot()
        {
            if (this.spotsNum > 0)
            {
                ArcSpotPars[] spotsCopy = this.spots;

                this.spotsNum++;

                this.spots = new ArcSpotPars[this.spotsNum];

                for (int i = 0; i < this.spotsNum - 1; i++)
                {
                    this.spots[i] = spotsCopy[i];
                }

                this.spots[this.spotsNum - 1] = new ArcSpotPars();
            }

            else
            {
                this.spotsNum = 1;

                this.spots = new ArcSpotPars[this.spotsNum];

                this.spots[0] = new ArcSpotPars();
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
                ArcSpotPars[] spotsCopy = this.spots;

                this.spotsNum--;

                this.spots = new ArcSpotPars[this.spotsNum];

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


