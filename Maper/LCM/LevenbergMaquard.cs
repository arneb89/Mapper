using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper.LCM
{
    class LevenbergMaquard
    {
        private VectorFunction vecFunc;
        private double[] xInit;
        private double[] xInitTrimmed;
        private bool[] isFixed;
        private int xTrimmedLength;
        private double[] stepForDerivat = null;
        private double[] xStepForDerivatTrimmed = null;
        private double tau = 1e-3;
        private int iterMax = 100;
        

        public double Tau
        {
            get
            {
                return this.tau;
            }
            set
            {
                this.tau = value;
            }
        }

        public int IterMax
        {
            get
            {
                return this.iterMax;
            }
            set
            {
                this.iterMax = value;
            }
        }

        public VectorFunction MinimizedFunction
        {
            set
            {
                this.vecFunc = value;
            }
        }

        public double[] vecFuncReduced(double[] xTrimmed)
        {
            double[] x = new double[xInit.Length];
            {
                int j = 0;
                for (int i = 0; i < x.Length; i++)
                {
                    if (!this.isFixed[i])
                    {
                        x[i] = xTrimmed[j];
                        j++;
                    }
                    else
                    {
                        x[i] = this.xInit[i];
                    }
                }
            }

            double[] res = this.vecFunc(x);

            return res;
        }

        public double[][] GetJacobyMatrix(double[] xTrimmed)
        {
            double[] xtrim = (double[])xTrimmed.Clone();
            double[] y0 = this.vecFuncReduced(xtrim);


            int jacobyColumnsNumber = xtrim.Length;
            int jacobyRowsNumber = y0.Length;

            double[][] jacoby = MathLib.Basic.MatrixConstructor(jacobyRowsNumber, jacobyColumnsNumber);

            for (int i = 0; i < jacobyColumnsNumber; i++)
            {
                double[] x1 = (double[])xtrim.Clone();
                x1[i] += this.xStepForDerivatTrimmed[i];
                double[] y1 = this.vecFuncReduced(x1);
                double[] deltaY = new double[y0.Length];
                for (int k = 0; k < deltaY.Length; k++) deltaY[k] = y1[k] - y0[k];

                for (int j = 0; j < jacobyRowsNumber; j++)
                {
                    jacoby[j][i] = deltaY[j] / this.xStepForDerivatTrimmed[i];
                }
            }

            return jacoby;
        }

        public double[] StepForDer
        {
            get
            {
                return this.stepForDerivat;
            }
            set
            {
                this.stepForDerivat = value;
            }
        }

        public double[] ComputeMin(double[] x0, bool[] isFixed, double[] lowLimits, double[] highLimits)
        {
            this.xInit = x0;
            this.isFixed = isFixed;
            
            this.xTrimmedLength = 0;
            for (int i = 0; i < isFixed.Length; i++)
            {
                if (!isFixed[i]) this.xTrimmedLength++;
            }
            

            this.xInitTrimmed = new double[this.xTrimmedLength];
            this.xStepForDerivatTrimmed = new double[this.xTrimmedLength];

            {
                int j = 0;
                for (int i = 0; i < isFixed.Length; i++)
                {
                    if (!isFixed[i])
                    {
                        this.xInitTrimmed[j] = x0[i];
                        this.xStepForDerivatTrimmed[j] = this.stepForDerivat[i];
                        j++;
                    }
                }
            }

            double[] xtrim0;
            double[] xtrim = this.xInitTrimmed;

            int iter = 0;

            double[] b = MathLib.Basic.VectorConstructor(xTrimmedLength);
            double[][] jacoby;
            double[][] jacobyTr;
            double[][] hessian = MathLib.Basic.MatrixConstructor(xTrimmedLength, xTrimmedLength);
            double[] func;
            double[] step;
            do
            {
                func = this.vecFuncReduced(xtrim);
                jacoby = this.GetJacobyMatrix(xtrim);

                jacobyTr = MathLib.Basic.MatrixConstructor(MathLib.Basic.ColCount(ref jacoby),
                    MathLib.Basic.RowCount(ref jacoby));
                for (int i = 0; i < jacobyTr.Length; i++)
                {
                    for (int j = 0; j < jacobyTr[i].Length; j++)
                    {
                        jacobyTr[i][j] = jacoby[j][i];
                    }
                }

                MathLib.Basic.ATrA(ref jacoby, ref hessian);

                double maxDiagElement = hessian[0][0];
                for (int i = 1; i < hessian.Length; i++)
                {
                    if (hessian[i][i] > maxDiagElement) maxDiagElement = hessian[i][i];
                }
                for (int i = 0; i < hessian.Length; i++)
                {
                    hessian[i][i]+= this.tau * maxDiagElement;
                }


                MathLib.Basic.AMultB(ref jacobyTr, ref func, ref b);
                MathLib.Basic.VAMultSC(ref b, -1.0);

                step = MathLib.LES_Solver.SolveWithGaussMethod(hessian, b);

                xtrim0 = xtrim;
                MathLib.Basic.VAplusVB(ref xtrim0, ref step, ref xtrim);

                // Checking for living of the minimization area;
                {
                    int j = 0;
                    for (int i = 0; i < x0.Length; i++)
                    {
                        if (!isFixed[i])
                        {
                            if (xtrim[j] > highLimits[i]) xtrim[j] = highLimits[i];
                            if (xtrim[j] < lowLimits[i]) xtrim[j] = lowLimits[i];
                            j++;
                        }
                    }
                }

                iter++;
            } while (iter < this.iterMax);

            double[] xResult = new double[x0.Length];
            {
                int j = 0;
                for (int i = 0; i < xResult.Length; i++)
                {
                    if (!isFixed[i])
                    {
                        xResult[i] = xtrim[j];
                        j++;
                    }
                    else
                    {
                        xResult[i] = x0[i];
                    }
                }
            }

            return xResult;
        }
    }
}
