using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLib;

namespace Maper
{
    class LCBox
    {
        private int lcNum=0;
        private Table1D[] lcMas;
        private bool[] usingSignature;
        private string[] filter;
        private int[] volume;

        private int ldMod;
        private double[] ldX;
        private double[] ldY;
        private double[] sigma;

        public void AddLightCurve(string path)
        {
            Table1D lc = new Table1D(path);
            
            //

            if (this.lcNum > 0)
            {
                Table1D[] lcMasCopy = this.lcMas;
                bool[] usingSignatureCopy = this.usingSignature;
                string[] filterCopy = this.filter;
                int[] volumeCopy = this.volume;
                double[] ldXCopy = this.ldX;
                double[] ldYCopy = this.ldY;
                double[] sigmaCopy = this.sigma;

                this.lcNum++;

                this.lcMas = new Table1D[this.lcNum];
                this.usingSignature = new bool[this.lcNum];
                this.filter = new string[this.lcNum];
                this.volume = new int[this.lcNum];
                this.ldX = new double[this.lcNum];
                this.ldY = new double[this.lcNum];
                this.sigma = new double[this.lcNum];

                for (int i = 0; i < this.lcNum - 1; i++)
                {
                    this.lcMas[i] = lcMasCopy[i];
                    this.usingSignature[i] = usingSignatureCopy[i];
                    this.filter[i] = filterCopy[i];
                    this.volume[i] = volumeCopy[i];
                    this.ldX[i] = ldXCopy[i];
                    this.ldY[i] = ldYCopy[i];
                    this.sigma[i] = sigmaCopy[i];
                }

                this.lcMas[this.lcNum - 1] = lc;
                this.usingSignature[this.lcNum - 1] = true;
                this.filter[this.lcNum - 1] = lc.Info;
                this.volume[this.lcNum - 1] = lc.XMas.Length;
                this.ldX[this.lcNum - 1] = 0.0;
                this.ldY[this.lcNum - 1] = 0.0;
                this.sigma[this.lcNum - 1] = 0.0;
            }

            else
            {
                this.lcNum = 1;

                this.lcMas = new Table1D[this.lcNum];
                this.usingSignature = new bool[this.lcNum];
                this.filter = new string[this.lcNum];
                this.volume = new int[this.lcNum];
                this.ldMod = 1;
                this.ldX = new double[this.lcNum];
                this.ldY = new double[this.lcNum];
                this.sigma = new double[this.lcNum];

                this.lcMas[0] = lc;
                this.usingSignature[0] = true;
                this.filter[0] = lc.Info;
                this.volume[0] = lc.XMas.Length;
                this.ldX[0] = 0.0;
                this.ldY[0] = 0.0;
                this.sigma[0] = 0.0;
            }
        }

        public void DelLightCurve(int num)
        {
            if (this.lcNum > 0)
            {
                Table1D[] lcMasCopy = this.lcMas;
                bool[] usingSignatureCopy = this.usingSignature;
                string[] filterCopy = this.filter;
                int[] volumeCopy = this.volume;
                double[] ldXCopy = this.ldX;
                double[] ldYCopy = this.ldY;
                double[] sigmaCopy = this.sigma;

                this.lcNum--;

                this.lcMas = new Table1D[this.lcNum];
                this.usingSignature = new bool[this.lcNum];
                this.filter = new string[this.lcNum];
                this.volume = new int[this.lcNum];
                this.ldX = new double[this.lcNum];
                this.ldY = new double[this.lcNum];
                this.sigma = new double[this.lcNum];

                int q = 0;
                for (int i = 0; i < this.lcNum; i++)
                {
                    if (i >= num) q = 1; 
                    this.lcMas[i] = lcMasCopy[i+q];
                    this.usingSignature[i] = usingSignatureCopy[i+q];
                    this.filter[i] = filterCopy[i+q];
                    this.volume[i] = volumeCopy[i+q];
                    this.ldX[i] = ldXCopy[i + q];
                    this.ldY[i] = ldYCopy[i + q];
                    this.sigma[i] = sigmaCopy[i + q];
                }
            }
        }

        public void SetLDX(int num, double value)
        {
            this.ldX[num] = value;
        }

        public void SetLDY(int num, double value)
        {
            this.ldY[num] = value;
        }

        public void SetSigma(int num, double value)
        {
            this.sigma[num] = value;
        }

        public void SetUseFlag(int num, bool value)
        {
            this.usingSignature[num] = value;
        }

        public Table1D[] GetAssignetLightCurves()
        {
            int masSize=0;
            for (int i = 0; i < this.lcNum; i++)
            {
                if(this.usingSignature[i]) masSize = masSize + 1;
            }

            Table1D[] lc = new Table1D[masSize];

            int k=0;
            for (int i = 0; i < this.lcNum; i++)
            {
                if (this.usingSignature[i])
                {
                    lc[k] = this.lcMas[i];
                    k++;
                }
            }

            return lc;
        }

        public int LightCurvesNumber
        {
            get
            {
                return this.lcNum;
            }
        }

        public int[] Volumes
        {
            get
            {
                return this.volume;
            }
        }

        public string[] Filters
        {
            get
            {
                return this.filter;
            }
        }

        public bool[] Signature
        {
            get
            {
                return this.usingSignature;
            }
        }

        public Table1D[] LightCurves
        {
            get
            {
                return this.lcMas;
            }
        }

        public double[] LDX
        {
            get
            {
                return this.ldX;
            }
        }

        public double[] LDY
        {
            get
            {
                return this.ldY;
            }
        }

        public double[] Sigma
        {
            get
            {
                return this.sigma;
            }
        }

        public int LDModel
        {
            get
            {
                return this.ldMod;
            }
            set
            {
                this.ldMod = value;
            }
        }
    }
}
