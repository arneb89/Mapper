using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLib;

namespace Maper.LCM
{
    public class LCContainer
    {
        private LightCurve[] lc;
        private int lightCurvesNumber = 0;

        public void AddLightCurve(string file)
        {
            Table1D table = new Table1D(file);

            if (this.lightCurvesNumber > 0)
            {
                LightCurve[] lcCache = this.lc;

                this.lightCurvesNumber++;

                this.lc = new LightCurve[this.lightCurvesNumber];

                for (int i = 0; i < this.lightCurvesNumber - 1; i++)
                {
                    this.lc[i] = lcCache[i];
                }

                this.lc[this.lightCurvesNumber - 1] = new LightCurve();
                this.lc[this.lightCurvesNumber - 1].Fluxes = table.FMas;
                this.lc[this.lightCurvesNumber - 1].Phases = table.XMas;
                this.lc[this.lightCurvesNumber - 1].Band = table.Info;
            }

            else
            {
                this.lightCurvesNumber = 1;

                this.lc = new LightCurve[this.lightCurvesNumber];
                this.lc[0] = new LightCurve();
                this.lc[0].Fluxes = table.FMas;
                this.lc[0].Phases = table.XMas;
                this.lc[0].Band = table.Info;
            }
        }

        public void DelLightCurve(int num)
        {
            if (this.lightCurvesNumber > 0)
            {
                LightCurve[] lcCopy = this.lc;

                this.lightCurvesNumber--;
                this.lc = new LightCurve[this.lightCurvesNumber];

                int q = 0;
                for (int i = 0; i < this.lightCurvesNumber; i++)
                {
                    if (i >= num) q = 1;
                    this.lc[i] = lcCopy[i + q];
                }
            }
        }

        public int LightCurvesNumber
        {
            get { return this.lightCurvesNumber; }
        }

        public LightCurve[] LightCurves
        {
            get { return this.lc; }
            set { this.lc = value; }
        }
    }
}
