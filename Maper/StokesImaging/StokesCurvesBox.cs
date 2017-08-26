using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper.StokesImaging
{
    class StokesCurve
    {
        public string filter;
        public string type;
        public double[] value = null;
        public double[] phases = null;
        public double sigma = 0;
    }
    class StokesCurvesBox
    {
        private StokesCurve[] stokesCurves = null;

        private int stokesCurvesNumber = 0;

        public void AddStokesCurve(string file)
        {
            MathLib.Table1D table = new MathLib.Table1D(file);
            string[] stringSeparators = new string[] { " ", "\t" };
            if (this.stokesCurvesNumber > 0)
            {
                StokesCurve[] lcCache = this.stokesCurves;

                this.stokesCurvesNumber++;

                this.stokesCurves = new StokesCurve[this.stokesCurvesNumber];

                for (int i = 0; i < this.stokesCurvesNumber - 1; i++)
                {
                    this.stokesCurves[i] = lcCache[i];
                }

                this.stokesCurves[this.stokesCurvesNumber - 1] = new StokesCurve();
                this.stokesCurves[this.stokesCurvesNumber - 1].value = table.FMas;
                this.stokesCurves[this.stokesCurvesNumber - 1].phases = table.XMas;
                string info = table.Info;
                string[] infoMas = info.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                this.stokesCurves[this.stokesCurvesNumber - 1].filter = infoMas[0];
                this.stokesCurves[this.stokesCurvesNumber - 1].type = infoMas[1];
            }

            else
            {
                this.stokesCurvesNumber = 1;
                this.stokesCurves = new StokesCurve[this.StokesCurvesNumber];
                this.stokesCurves[0] = new StokesCurve();
                this.stokesCurves[0].value = table.FMas;
                this.stokesCurves[0].phases = table.XMas;
                string info = table.Info;
                string[] infoMas = info.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                this.stokesCurves[0].filter = infoMas[0];
                this.stokesCurves[0].type = infoMas[1];
            }
        }

        public void DeleteStokesCurve(int num)
        {
            if (this.stokesCurvesNumber > 0)
            {
                StokesCurve[] lcCopy = this.stokesCurves;

                this.stokesCurvesNumber--;
                this.stokesCurves = new StokesCurve[this.stokesCurvesNumber];

                int q = 0;
                for (int i = 0; i < this.stokesCurvesNumber; i++)
                {
                    if (i >= num) q = 1;
                    this.stokesCurves[i] = lcCopy[i + q];
                }
            }
        }

        public StokesCurve[] StokesCurves
        {
            get { return this.stokesCurves; }
        }

        public int StokesCurvesNumber
        {
            get { return this.stokesCurvesNumber; }
        }
    }
}
