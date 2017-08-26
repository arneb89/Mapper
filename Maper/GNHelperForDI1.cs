using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLib;

namespace Maper
{
    class GNHelperForDI1
    {
        private double[][] rline, rcont;
        private double[] bline, bcont;
        private double[] obsSpec;
        private int pldim, vpn;
        private Matrix DTr;
        private Matrix F;

        public GNHelperForDI1(double[][] rline, 
                             double[][] rcont, 
                             double[] bline, 
                             double[] bcont, 
                             double[][] obsSpec, 
                             int pldim,
                             int vpn)
        {
            this.pldim = pldim;
            this.vpn = vpn;
            this.rline = rline;
            this.rcont = rcont;
            this.bline = bline;
            this.bcont = bcont;
            //this.obsSpec = obsSpec;
            this.obsSpec = new double[pldim];
            int k = 0;
            for (int i = 0; i < obsSpec.Length; i++)
            {
                for (int j = 0; j < obsSpec[i].Length; j++)
                {
                    this.obsSpec[k] = obsSpec[i][j];
                    k++;
                }
            }
            this.DTr = new Matrix(vpn, pldim);
            this.F = new Matrix(pldim, 1);
        }

        public Matrix GetJacobyMatrixTr(Matrix x)
        {
            double suml = 0, sumc = 0;
            for (int i = 0; i < pldim; i++)
            {
                suml = 0;
                for (int k = 0; k < vpn; k++)
                {
                    suml = suml + rline[i][k] * 1.0 / (1.0 + Math.Exp(-x[k]));
                }
                sumc = 0;
                for (int k = 0; k < vpn; k++)
                {
                    sumc = sumc + rcont[i][k] * 1.0 / (1.0 + Math.Exp(-x[k]));
                }
                for (int j = 0; j < vpn; j++)
                {
                    this.DTr[j, i] = (rline[i][j] * (sumc + bcont[i]) - rcont[i][j] * (suml + bline[i])) *
                    (Math.Exp(-x[j]) / (Math.Pow(1.0 + Math.Exp(-x[j]), 2))) / Math.Pow(sumc + bcont[i], 2);
                }
            }

            return this.DTr;
        }

        public Matrix GetVectorFunction(Matrix x)
        {
            double suml = 0, sumc = 0;

            for (int i = 0; i < pldim; i++)
            {
                suml = 0;
                for (int k = 0; k < vpn; k++)
                {
                    suml = suml + rline[i][k] * 1.0 / (1.0 + Math.Exp(-x[k]));
                }
                sumc = 0;
                for (int k = 0; k < vpn; k++)
                {
                    sumc = sumc + rcont[i][k] * 1.0 / (1.0 + Math.Exp(-x[k]));
                }
                this.F[i] = (suml + this.bline[i]) / (sumc + this.bcont[i]);
            }

            return this.F;
        }
    }
}
