using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MathLib;
using DotNumerics.LinearAlgebra.CSLapack;
using System.Threading.Tasks;

namespace Maper
{
    public class Reconstr
    {
        private TSurface tsrf = null;
        private Spline31D[] spInt = null;
        private Table1D[] lcMas = null;
        private LightCurve[] lcs = null;
        private LightCurve[] lcsMod = null;
        private double[] xLDC, yLDC;
        private int ldMod;
        private double[] sigma;
        private double khi2 = 0;

        public double critTurchin;
        public double critFillips;
        public double critCB;

        public double[][] fluxes;

        public void InitReconstr01(TSurface tsrf, Spline31D[] spInt, Table1D[] lcMas, 
            double[] xLDC, double[] yLDC, int ldModel, double[] sigma)
        {
            this.tsrf = tsrf;
            this.spInt = spInt;
            this.lcMas = lcMas;
            this.xLDC = xLDC;
            this.yLDC = yLDC;
            this.ldMod = ldModel;
            this.sigma = sigma;
        }

        public void InitReconstr02(TSurface tsrf, Spline31D[] spInt, LightCurve[] lcs,
            double[] xLDC)
        {
            this.tsrf = tsrf;
            this.spInt = spInt;
            this.lcs = lcs;
            this.xLDC = xLDC;
            this.lcMas = new Table1D[lcs.Length];
            this.sigma = new double[lcs.Length];
        }

        public void InitReconstrMLI(TSurface tsrf, Table1D lc, double xLDC, double yLDC, int ldModel, double sigma)
        {
            this.tsrf = tsrf;
            this.lcMas = new Table1D[1];
            this.xLDC = new double[1];
            this.yLDC = new double[1];
            this.ldMod = ldModel;
            this.sigma = new double[1];
            this.lcMas[0] = lc;
            this.xLDC[0] = xLDC;
            this.yLDC[0] = yLDC;
            this.sigma[0] = sigma;
        }

        //public TSurface Reconstr01(double lambda)
        //{
        //    double sqrtLambda = Math.Sqrt(lambda);

        //    // Определение и инициализация массива средних наблюдаемых потоков;
        //    double[] fluxAve = new double[this.lcMas.Length];
        //    for (int q = 0; q < fluxAve.Length; q++)
        //    {
        //        double sum=0;
        //        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
        //        {
        //            sum = sum + this.lcMas[q].FMas[p];
        //        }
        //        fluxAve[q] = sum / (double) this.lcMas[q].XMas.Length;
        //    }

            
        //    Matrix[] H = new Matrix[this.lcMas.Length];

        //    // Полное количество измерений интенсивности во всех фильтрах
        //    int lcPointsNumber = 0;
        //    for (int q = 0; q < lcMas.Length; q++)
        //    {
        //        lcPointsNumber = lcPointsNumber + lcMas[q].XMas.Length;
        //    }

        //    Matrix I = new Matrix(lcPointsNumber, 1);
        //    int k = 0;
        //    for (int q = 0; q < this.lcMas.Length; q++)
        //    {
        //        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
        //        {
        //            I.SetElement(k, 0, this.lcMas[q].FMas[p]);
        //            k++;
        //        }
        //    }

        //    int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();

        //    // Объявление матриц H;
        //    for (int q = 0; q < this.lcMas.Length; q++)
        //    {
        //        H[q] = new Matrix(this.lcMas[q].XMas.Length, vpn);
        //    }

        //    // Инициализация матриц H;
        //    k=0;
        //    for (int q = 0; q < this.lcMas.Length; q++)
        //    {
        //        for (int p = 0; p < lcMas[q].XMas.Length; p++)
        //        {
        //            k = 0;
        //            for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
        //            {
        //                int L = this.tsrf.patch[i].Length;
        //                for (int j = 0; j < L; j++)
        //                {
        //                    H[q].SetElement(p, k, this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc())*
        //                        (1-this.xLDC[q]*(1-this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc())))*
        //                        this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc()));
        //                    k++;
        //                }
        //            }
        //        }
        //    }

        //    // Инициализация вектора x;
        //    double[] x = new double[vpn + this.lcMas.Length];
        //    k = 0;
        //    for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
        //    {
        //        for (int j = 0; j < this.tsrf.patch[i].Length; j++)
        //        {
        //            x[k] = this.tsrf.teff[i][j];
        //            k++;
        //        }
        //    }

        //    for (int q = 0; q < this.lcMas.Length; q++)
        //    {
        //        double flux = this.spInt[q].Interp(this.tsrf.teff[0][0])*
        //            (Math.PI*(1-this.xLDC[q]/3.0));
        //        x[q+k] = fluxAve[q] / flux;
        //    }
            
        //    //
        //    Matrix Jacoby = new Matrix(lcPointsNumber + vpn, vpn + this.lcMas.Length);
        //    Matrix JacobyTr;
        //    Matrix F = new Matrix(lcPointsNumber + vpn, 1);
        //    Matrix G;
        //    Matrix B;
        //    //Matrix h = new Matrix(vpn + this.lcMas.Length, 1);
        //    double[] h = new double[vpn + this.lcMas.Length];

        //    // Объявление и инциализация сглаживающей матрицы; 
        //    double[][] W = new double[vpn][];
        //    for (int i = 0; i < vpn; i++)
        //    {
        //        W[i] = new double[vpn];
        //    }
        //    for (int i = 0; i < vpn; i++)
        //    {
        //        for (int j = 0; j < vpn; j++)
        //        {
        //            W[i][j] = 1 / (double)vpn;
        //        }
        //    }

        //    double[][] JD = new double[this.lcMas.Length][];
        //    for (int q = 0; q < this.lcMas.Length; q++)
        //    {
        //        JD[q] = new double[vpn];
        //    }


        //    int iter = 0;
        //    do
        //    {
        //        // Вычисление вектора F;
        //        k = 0;
        //        for (int q = 0; q < this.lcMas.Length; q++)
        //        {
        //            for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
        //            {
        //                double sum = 0;
        //                for (int i = 0; i < vpn; i++)
        //                {
        //                    sum = sum + H[q].GetElement(p, i) * this.spInt[q].Interp(x[i]);
        //                }
        //                F.SetElement(k, 0, (x[vpn + q] * sum - I.GetElement(k, 0)) / this.sigma[q]);
        //                k++;
        //            }
        //        }
        //        for (int i = 0; i < vpn; i++)
        //        {
        //            double sum=0;
        //            for(int j=0; j<vpn; j++)
        //            {
        //                sum = sum + W[i][j] * x[j];
        //            }

        //            F.SetElement(i + k, 0, sqrtLambda * (x[i] - sum));
        //        }


        //        // Вычисление производных норм.уд.интенсивностей по температуре;

        //        for (int q = 0; q < this.lcMas.Length; q++)
        //        {
        //            for (k = 0; k < x.Length - this.lcMas.Length; k++)
        //            {
        //                JD[q][k] = this.spInt[q].InterpD1(x[k]);
        //            }
        //        }

        //        // Вычисление компонент матрицы Якоби;
        //        k = 0;
        //        for (int q = 0; q < this.lcMas.Length; q++)
        //        {
        //            for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
        //            {
        //                for (int i = 0; i < vpn; i++)
        //                {
        //                    Jacoby.SetElement(k, i, x[vpn + q] * H[q].GetElement(p, i) * JD[q][i] / this.sigma[q]);
        //                }
        //                double sum = 0;
        //                for (int i = 0; i < vpn; i++)
        //                {
        //                    sum = sum + H[q].GetElement(p, i) * this.spInt[q].Interp(x[i]);
        //                }
        //                Jacoby.SetElement(k, vpn + q, sum / this.sigma[q]);
        //                k++;
        //            }
        //        }
        //        for (int i = 0; i < vpn; i++)
        //        {
        //            for (int j = 0; j < vpn; j++)
        //            {
        //                double delta = 0;
        //                if (i == j) delta = 1;
        //                Jacoby.SetElement(k + i, j, -sqrtLambda*( delta - W[i][j]));
        //            }
        //        }

        //        JacobyTr = Jacoby.Tr();
        //        G = JacobyTr * Jacoby;
        //        B = (JacobyTr * F)*(-1);

        //        Matrix G1 = new Matrix(G), B1 = new Matrix(B), E;

        //        //h = LES_Solver.SolveWithGaussMethod(G1, B1);
        //        h = LES_Solver.ConvGradMethod(G1, B1, new Matrix(vpn+this.lcMas.Length,1), 0.001);
        //        E = G * (new Matrix(h)) - B;
        //        for (int i = 0; i < vpn + this.lcMas.Length; i++)
        //        {
        //            //x[i] = x[i] + h.GetElement(i,0);
        //            x[i] = x[i] + h[i];
        //        }
        //        iter++;
        //    } while (iter < 7);

        //    this.fluxes = new double[100];

            

            
        //        for (int p = 0; p < this.lcMas[0].XMas.Length; p++)
        //        {
        //            double sum = 0;
        //            for (int j = 0; j < vpn; j++)
        //            {
        //                sum = sum + x[vpn]* H[0].GetElement(p, j) * this.spInt[0].Interp(x[j]);
        //            }
        //            fluxes[p] = sum;
        //        }
            

        //    k=0;
        //    for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
        //    {
        //        for (int j = 0; j < this.tsrf.patch[i].Length; j++)
        //        {
        //            this.tsrf.teff[i][j] = x[k];
        //            k++;
        //        }
        //    }

        //    return this.tsrf;
        //}

        private int[][] GetSmoothingMatrix(double averad)
        {
            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();
            int[][] Wmas = new int[vpn][];
            for (int i = 0; i < vpn; i++)
            {
                Wmas[i] = new int[vpn];
            }
            int ibelt = 0, jbelt = 0, ibeltel = 0, jbeltel = 0;
            int ibeltelsum = 0, jbeltelsum = 0;
            for (int i = 0; i < vpn; i++)
            {
                if (i == tsrf.patch[ibelt].Length + ibeltelsum)
                {
                    ibelt++;
                    ibeltelsum = ibeltelsum + tsrf.patch[ibelt - 1].Length;
                }
                ibeltel = i - ibeltelsum;
                jbeltelsum = 0;
                jbeltel = 0;
                jbelt = 0;
                for (int j = 0; j < vpn; j++)
                {
                    if (j == tsrf.patch[jbelt].Length + jbeltelsum)
                    {
                        jbelt++;
                        jbeltelsum = jbeltelsum + tsrf.patch[jbelt - 1].Length;
                    }
                    jbeltel = j - jbeltelsum;
                    Wmas[i][j] = this.tsrf.LocatedInNeighbourhoodOfPatch(
                        tsrf.patch[ibelt][ibeltel].FiCenterOnStart(),
                        tsrf.patch[ibelt][ibeltel].ThetaCenterOnStart(),
                        tsrf.patch[jbelt][jbeltel].FiCenterOnStart(),
                        tsrf.patch[jbelt][jbeltel].ThetaCenterOnStart(),
                        averad);
                }
            }

            int sum = 0;
            for (int i = 0; i < vpn; i++)
            {
                sum = 0;
                for (int j = 0; j < vpn; j++)
                {
                    sum = sum + Wmas[i][j];
                }
                for (int j = 0; j < vpn; j++)
                {
                    if (Wmas[i][j] != 0) Wmas[i][j] = sum;
                }
            }
            return Wmas;
        }

        public TSurface ReconstrMLI(double lambda, double bias)
        {
            int reg = 1;
            double averad=0.2;
            double sqrtLambda = Math.Sqrt(lambda)*sigma[0];
            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();
            int lcPointsNumber = this.lcMas[0].XMas.Length;

            Matrix H = new Matrix(lcPointsNumber, vpn);

            int k = 0;
            for (int p = 0; p < lcPointsNumber; p++)
            {
                k = 0;
                for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
                {
                    int l = this.tsrf.patch[i].Length;
                    for (int j = 0; j < l; j++)
                    {
                        H.SetElement(p, k, this.tsrf.patch[i][j].ProjectedArea(this.lcMas[0].XMas[p], this.tsrf.GetInc()) *
                            (1 - this.xLDC[0] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[0].XMas[p], this.tsrf.GetInc()))) *
                            (double)this.tsrf.patch[i][j].Visible(this.lcMas[0].XMas[p], this.tsrf.GetInc())/sigma[0]);
                        k++;
                    }
                }
            }

            Matrix I = new Matrix(this.lcMas[0].FMas);
            for (int i = 0; i < lcPointsNumber; i++) I.SetElement(i, 0, I.GetElement(i, 0) / sigma[0]);

            int[][] masW = this.GetSmoothingMatrix(averad);

            double sum = 0.0;

            Matrix W = new Matrix(vpn, vpn);

            Matrix J = new Matrix(vpn, 1);
            Matrix J0 = new Matrix(vpn, 1);
            Matrix B = H.Tr() * I;

            Matrix HTrH = H.Tr() * H;
            Matrix CC = new Matrix(vpn, 1);
            Matrix C;

            int iter=0;
            do
            {
                iter++;
                if (iter != 1)
                {
                    double jAve = 0;
                    for (int i = 0; i < vpn; i++)
                    {
                        jAve = jAve + J.GetElement(i, 0);
                    }
                    jAve = jAve / (double) vpn;
                    for (int i = 0; i < vpn; i++)
                    {
                        if (J.GetElement(i, 0) <= jAve) CC.SetElement(i, 0, 1.0);
                        else CC.SetElement(i, 0, bias);
                    }
                }
                else
                {
                    for (int i = 0; i < vpn; i++)
                    {
                        CC.SetElement(i, 0, 1.0);
                    }
                }

                if (reg == 0)
                {
                    for (int i = 0; i < vpn; i++)
                    {
                        for (int j = 0; j < vpn; j++)
                        {
                            if (i == j) W.SetElement(i, j, Math.Sqrt(CC.GetElement(i, 0)) * (1.0 - 1.0 / (double)vpn));
                            else W.SetElement(i, j, -Math.Sqrt(CC.GetElement(i, 0)) / (double)vpn);
                        }
                    }
                }
                if (reg == 1)
                {
                    for (int i = 0; i < vpn; i++)
                    {
                        for (int j = 0; j < vpn; j++)
                        {
                            if (masW[i][j] != 0)
                            {
                                if (i == j) W.SetElement(i, j, Math.Sqrt(CC.GetElement(i, 0)) * (1.0 - 1.0 / (double)masW[i][j]));
                                else W.SetElement(i, j, -Math.Sqrt(CC.GetElement(i, 0)) / (double)masW[i][j]);
                            }
                        }
                    }
                }

                C = W * W;

                J0 = J;

                Matrix A = HTrH + C * (lambda * sigma[0] * sigma[0]);
                J = new Matrix(MathLib.LES_Solver.ConvGradMethod(A, B, J, 0.05));
                //J = MathLib.LES_Solver.SolveWithGaussMethodM(A, B);

            } while ((J-J0).Norma()>0.01 && bias!=1.0);
            

            /*
            DGGSVD dggsvd = new DGGSVD();
            double[] gsvdB = new double[vpn*vpn], gsvdALPHA=new double[vpn], gsvdBETA=new double[vpn];
            double[] gsvdA = H.GetMasForLAPACK();
            double[] gsvdWORK= new double[vpn + Math.Max(3 * vpn, lcPointsNumber)];
            double[] gsvdU = new double[lcPointsNumber], gsvdV = new double[vpn], gsvdQ = new double[vpn];
            int[] iwork=new int[vpn];
            int gsvdK=vpn, gsvdL=vpn, offset_a=1, offset_b=1, offset_alpha=1, offset_beta=1, offset_u=1, offset_v=1, offset_q=1, offset_work=1, offset_iwork=1, info=0;
            dggsvd.Run("N", "N", "N", lcPointsNumber, vpn, vpn, ref gsvdK, ref gsvdL,
                ref gsvdA, offset_a, lcPointsNumber, ref gsvdB, offset_b, vpn,
                ref gsvdALPHA, offset_alpha, ref gsvdBETA, offset_beta, ref gsvdU, offset_u, 1,
                ref gsvdV, offset_v, 1, ref gsvdQ, offset_q, 1, ref gsvdWORK, offset_work,
                ref iwork, offset_iwork, ref info);*/
 

            // Вычисление критерия;
            //lambda = lambda / (sigma[0]*sigma[0]);
            /*
            double alpha = C.Max() / HTrH.Max();
            Matrix aHTrHC = HTrH * alpha + C;
            Matrix L = (new Cholesky(new Matrix(aHTrHC))).L;
            Matrix LInv = (new MatrixInvertor()).TriangleDown(new Matrix(L));
            
            Matrix Q = LInv * HTrH * LInv.Tr();

            EigenSymm es = new EigenSymm();
            es.ToTriagDiagMatrix(new Matrix(Q), true);
            es.TGQL();

            Matrix D = es.EigenValues;
            Matrix D1 = new Matrix(vpn, vpn);
            Matrix V = es.V;

            Matrix R = new Matrix(vpn, vpn);

            for (int i = 0; i < vpn; i++)
            {
                D1.SetElement(i, i, 1.0 / ((1 - alpha * lambda) * D.GetElement(i, 0) + lambda));
            }

            R = MatrixMult.DiagMatToMat(D1, V.Tr() * (LInv * H.Tr()));

            Matrix S = H * LInv.Tr() * V * R;

            double trace = S.Sp();*/
            
            double norm;
            norm = Math.Pow((H * J - I).Norma(), 2);
            
            this.critFillips = norm -  lcPointsNumber;
            //this.critTurchin = norm -  (lcPointsNumber - trace);
            //this.critCB = norm -  (lcPointsNumber - trace*trace);
            
            TSurface tsrf1 = this.tsrf;

            k = 0;
            for (int i = 0; i < tsrf1.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < tsrf1.patch[i].Length; j++)
                {
                    tsrf1.teff[i][j] = J.GetElement(k, 0);
                    k++;
                }
            }

            return tsrf1;
        }

        // Восстановление с использованием известного значения интенсивности фотосферы. 
        public TSurface ReconstrMLI2(double lambda, 
            double bias, 
            double fluxPhot, 
            double deltaFluxPhot, 
            double penalty)
        {
            double sqrtLambda = Math.Sqrt(lambda);
            
            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();
            int lcPointsNumber = this.lcMas[0].XMas.Length;

            Matrix H = new Matrix(lcPointsNumber, vpn);

            int k = 0;
            for (int p = 0; p < lcPointsNumber; p++)
            {
                k = 0;
                for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
                {
                    int l = this.tsrf.patch[i].Length;
                    for (int j = 0; j < l; j++)
                    {
                        H.SetElement(p, k, this.tsrf.patch[i][j].ProjectedArea(this.lcMas[0].XMas[p], this.tsrf.GetInc()) *
                            (1 - this.xLDC[0] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[0].XMas[p], this.tsrf.GetInc()))) *
                            (double)this.tsrf.patch[i][j].Visible(this.lcMas[0].XMas[p], this.tsrf.GetInc()));
                        k++;
                    }
                }
            }

            Matrix I = new Matrix(this.lcMas[0].FMas);
            Matrix IPh = new Matrix(lcPointsNumber, 1);
            for (int p = 0; p < lcPointsNumber; p++) IPh.SetElement(p, 0, fluxPhot);
            
            I = I - IPh;

            Matrix X = new Matrix(vpn, 1);
            Matrix X0 = new Matrix(vpn, 1);
            Matrix B = H.Tr() * I;

            Matrix HTrH = H.Tr() * H;
            Matrix CC = new Matrix(vpn, vpn);

            double intPhot = fluxPhot / (Math.PI * (1 - xLDC[0] / 3.0));
            double deltaIntPhot = deltaFluxPhot / (Math.PI * (1.0 - this.xLDC[0] / 3.0));

            int iter = 0;
            do
            {
                iter++;
                if (iter != 1)
                {
                    for (int i = 0; i < vpn; i++)
                    {
                        CC[i, i] = Math.Exp(Math.Pow(X[i,0], 2) / (2 * 0.08)) + Math.Exp(Math.Pow(X[i,0] - 2.0, 2) / (2 * 0.1));
                        //if (X.GetElement(i, 0) <= 0/*deltaIntPhot*/) CC[i, i] = 1.0;

                        //else CC[i, i] = bias;
                    }
                }
                else
                {
                    for (int i = 0; i < vpn; i++)
                    {
                        CC.SetElement(i, i, 1.0);
                    }
                }

                X0 = X;

                Matrix A = HTrH + CC * lambda;
                X = new Matrix(MathLib.LES_Solver.ConvGradMethod(A, B, X, 0.0001));
                //J = MathLib.LES_Solver.SolveWithGaussMethodM(A, B);

            } while ((X - X0).Norma() > 1.0 /*&& bias != 1.0*/);

            double norm;
            norm = Math.Pow((H * X - I).Norma(), 2);

            this.critFillips = norm - lcPointsNumber;
            //this.critTurchin = norm - (lcPointsNumber - trace);
            //this.critCB = norm - (lcPointsNumber - trace*trace);

            

            TSurface tsrf1 = this.tsrf;

            k = 0;
            for (int i = 0; i < tsrf1.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < tsrf1.patch[i].Length; j++)
                {
                    tsrf1.teff[i][j] = X.GetElement(k, 0) + intPhot;
                    k++;
                }
            }

            return tsrf1;
        }

        // Two temperature approach;
        public TSurface ReconstrTTA(double lambda,
            double jph,
            double jsp)
        {
            double sqrtLambda = Math.Sqrt(lambda);

            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();
            int lcPointsNumber = this.lcMas[0].XMas.Length;

            Matrix H = new Matrix(lcPointsNumber, vpn);

            int k = 0;
            for (int p = 0; p < lcPointsNumber; p++)
            {
                k = 0;
                for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
                {
                    int l = this.tsrf.patch[i].Length;
                    for (int j = 0; j < l; j++)
                    {
                        H.SetElement(p, k, this.tsrf.patch[i][j].ProjectedArea(this.lcMas[0].XMas[p], this.tsrf.GetInc()) *
                            (1 - this.xLDC[0] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[0].XMas[p], this.tsrf.GetInc()))) *
                            (double)this.tsrf.patch[i][j].Visible(this.lcMas[0].XMas[p], this.tsrf.GetInc()));
                        k++;
                    }
                }
            }

            Matrix I = new Matrix(this.lcMas[0].FMas);
            Matrix J = new Matrix(vpn, 1);

            for (int i = 0; i < vpn; i++) J[i] = jph;
            

            GNHelper gnh = new GNHelper(H, I, jph, jsp, sqrtLambda);
            MathLib.Optimization.GaussNewton gn = new MathLib.Optimization.GaussNewton();
            J = gn.FindMinPoint(gnh.GetJacobyMatrix, gnh.GetVectorFunction, J, 0.05);

            TSurface tsrf1 = this.tsrf;

            k = 0;
            for (int i = 0; i < tsrf1.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < tsrf1.patch[i].Length; j++)
                {
                    tsrf1.teff[i][j] = J[k];
                    k++;
                }
            }
            return tsrf1;
        }

        public TSurface ReconstrEND(double lambda, double bias, double tph, double tsp)
        {
            double sqrtLambda = Math.Sqrt(lambda);
            double sqrtBias = Math.Sqrt(bias);
            int filterCount = this.lcMas.Length;
            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();

            // Определение и инициализация массива средних наблюдаемых потоков;
            double[] fluxAve = new double[this.lcMas.Length];
            for (int q = 0; q < fluxAve.Length; q++)
            {
                double sum = 0;
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    sum = sum + this.lcMas[q].FMas[p];
                }
                fluxAve[q] = sum / (double)this.lcMas[q].XMas.Length;
            }

            // Initialization of reponse matrix;
            double[][][] H = new double[filterCount][][];
            for (int q = 0; q < filterCount; q++)
            {
                H[q] = new double[this.lcMas[q].XMas.Length][];
                for (int p = 0; p < H[q].Length; p++)
                {
                    H[q][p] = new double[vpn];
                }
            }

            // Полное количество измерений интенсивности во всех фильтрах
            int lcPointsNumber = 0;
            for (int q = 0; q < lcMas.Length; q++)
            {
                lcPointsNumber = lcPointsNumber + lcMas[q].XMas.Length;
            }


            double[] I = new double[lcPointsNumber];
            int k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    I[k] = this.lcMas[q].FMas[p] / sigma[q];
                    k++;
                }
            }

            // Инициализация матриц H;
            k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < lcMas[q].XMas.Length; p++)
                {
                    k = 0;
                    for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
                    {
                        int L = this.tsrf.patch[i].Length;
                        for (int j = 0; j < L; j++)
                        {
                            H[q][p][k] = (1.0 / this.sigma[q]) * this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                                (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                                this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc());
                            //H[q].SetElement(p, k, this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                            //    (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                            //    this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc()));
                            k++;
                        }
                    }
                }
            }

            // Инициализация вектора x;
            double[] x = new double[vpn + this.lcMas.Length];
            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    x[k] = this.tsrf.teff[i][j];
                    k++;
                }
            }

            for (int q = 0; q < this.lcMas.Length; q++)
            {
                double flux = this.spInt[q].Interp(this.tsrf.teff[0][0]) *
                    (Math.PI * (1 - this.xLDC[q] / 3.0));
                x[q + k] = fluxAve[q] / flux;
            }

            double[][] jacobyTr = new double[vpn + filterCount][];
            for (int i = 0; i < jacobyTr.Length; i++)
            {
                jacobyTr[i] = new double[lcPointsNumber + vpn];
            }

            double[] f = new double[lcPointsNumber + vpn];

            double[] b = new double[vpn+filterCount];

            // The Hessian matrix;
            double[][] hessian = new double[vpn + filterCount][];
            for (int i = 0; i < hessian.Length; i++)
            {
                hessian[i] = new double[vpn + filterCount];
            }

            // The step-vector for Gauss-Newton minimization algorithm;
            double[] h = new double[vpn + filterCount];

            double[] x0 = new double[vpn + filterCount];

            // The vector of first derivatives of normal specific intensities with respect to temperature;
            double[][] JD = new double[this.lcMas.Length][];
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                JD[q] = new double[vpn];
            }

            double[] patchAreas = new double[vpn];

            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    patchAreas[k] = (tsrf.patch[i][j].Phi20 - tsrf.patch[i][j].Phi10) *
                        (Math.Cos(tsrf.patch[i][j].Theta1) - Math.Cos(tsrf.patch[i][j].Theta2));
                    k++;
                }
            }

            // The vector of weight-coefficients;
            double[] c = new double[vpn];
            for (int i = 0; i < vpn; i++) c[i] = 1.0;

            double norm2;

            double ff = 0.07;

            int iter2 = 0;
            int iter1 = 0;

            // Wir sind geboren Taten zu volbringen,
            // zu uberwind ...
            do
            {
                if (iter1 != 0)
                {
                    double[] tvec = new double[vpn];

                    for (int i = 0; i < vpn; i++) tvec[i] = x[i];

                    for (int i = 0; i < vpn; i++) c[i] = 1.0;

                    double areaSrf = 2 * Math.PI * (Math.Sin(this.tsrf.GetInc()) + 1.0);

                    double areaSpot = ff * areaSrf;

                    double areaSpotCurr = 0;

                    int tt;
                    do
                    {
                        // Find next element that satisfy our conditions;
                        tt = 0; // index of the next element;
                        for (int i = 0; i < vpn; i++)
                        {
                            if (tvec[i] < tvec[tt] && c[i] == 1.0) tt = i;

                        }
                        // Calculation of the area of spot;

                        areaSpotCurr = areaSpotCurr + patchAreas[tt];

                        c[tt] = bias;
                        
                    } while (areaSpotCurr<areaSpot);
                }

                iter2 = 0;

                do
                {
                    for (int i = 0; i < vpn + filterCount; i++) x0[i] = x[i];

                    // Вычисление вектора f;
                    k = 0;
                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                        {
                            double sum = 0;
                            for (int i = 0; i < vpn; i++)
                            {
                                sum = sum + H[q][p][i] * this.spInt[q].Interp(x[i]);
                            }
                            f[k] = x[vpn + q] * sum - I[k];
                            k++;
                        }
                    }

                    for (int i = 0; i < vpn; i++)
                    {
                        //f[i + k] = c[i] * sqrtLambda * (x[i] - teffAve);
                        f[i + k] = sqrtLambda * c[i]*(x[i] - 5000);
                    }

                    // Вычисление производных норм.уд.интенсивностей по температуре;

                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int i = 0; i < vpn; i++)
                        {
                            JD[q][i] = this.spInt[q].InterpD1(x[i]);
                        }
                    }

                    // Вычисление компонент матрицы Якоби;
                    k = 0;
                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                        {
                            for (int i = 0; i < vpn; i++)
                            {
                                jacobyTr[i][k] = x[vpn + q] * H[q][p][i] * JD[q][i];
                            }

                            double sum = 0;
                            for (int i = 0; i < vpn; i++)
                            {
                                sum = sum + H[q][p][i] * this.spInt[q].Interp(x[i]);
                            }
                            jacobyTr[vpn + q][k] = sum;
                            k++;
                        }
                    }
                    for (int i = 0; i < vpn; i++)
                    {
                        for (int j = 0; j < vpn; j++)
                        {
                            //jacobyTr[j][k + i] = -sqrtLambda * c[i] / vpn;
                            //if (i == j) jacobyTr[i][k + i] = sqrtLambda * c[i]*(1.0 - 1.0 / vpn);
                            jacobyTr[i][k + i] = sqrtLambda*c[i];
                        }
                    }

                    // Computing of the Hessian matrix;
                    Parallel.For(0, vpn + filterCount, i =>
                    /*for (int i = 0; i < vpn + filterCount; i++)*/
                    {
                        double sum;
                        for (int j = i; j < vpn + filterCount; j++)
                        {
                            sum = 0;
                            for (int l = 0; l < lcPointsNumber /*+ vpn*/; l++)
                            {
                                sum = sum + jacobyTr[i][l] * jacobyTr[j][l];
                            }
                            if (i < vpn)
                            {
                                if (i == j) sum = sum + Math.Pow(jacobyTr[i][i + lcPointsNumber], 2);
                            }
                            hessian[i][j] = sum;
                            hessian[j][i] = sum;
                        }
                    });


                    //
                    double aa = hessian[0][0];
                    for (int i = 0; i < hessian.Length; i++)
                    {
                        if (hessian[i][i] > aa) aa = hessian[i][i];
                    }
                    double mu = 0.000000000000000000001 * aa;
                    for (int i = 0; i < hessian.Length; i++)
                    {
                        hessian[i][i] = hessian[i][i] + mu;
                    }

                    MathLib.Basic.AMultB(ref jacobyTr, ref f, ref b);

                    MathLib.Basic.VAMultSC(ref b, -1.0);

                    //h=MathLib.LES_Solver.SolveWithGaussMethod(hessian, b);

                    MathLib.LES_Solver.ConvGradMethodPL(ref hessian, ref b, ref h, 0.0000000001);

                    MathLib.Basic.VAplusVB(ref x0, ref h, ref x);

                    norm2 = 0;
                    for (int i = 0; i < vpn; i++)
                    {
                        norm2 = norm2 + Math.Pow(h[i], 2);
                    }
                    norm2 = Math.Sqrt(norm2);

                    iter2++;
                } while (norm2 > vpn * 0.01);

                iter1++;

            } while (iter1 < 10 && bias != 1.0);

            this.fluxes = new double[filterCount][];
            for (int q = 0; q < filterCount; q++)
            {
                this.fluxes[q] = new double[100];
            }

            for (int q = 0; q < filterCount; q++)                  // Be carefull, H is multiplied to sigma!
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    double sum = 0;
                    for (int j = 0; j < vpn; j++)
                    {
                        sum = sum + x[vpn + q] * H[q][p][j] * sigma[q] * this.spInt[q].Interp(x[j]);
                    }
                    fluxes[q][p] = sum;
                }
            }

            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    this.tsrf.teff[i][j] = x[k];
                    k++;
                }
            }

            return this.tsrf;
        }

        public TSurface ReconstrVictoria(double lambda, double ff, double tph, double tsp)
        {
            double sqrtLambda = Math.Sqrt(lambda);
            int filterCount = this.lcMas.Length;
            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();

            // Initialization of reponse matrix;
            double[][][] H = new double[filterCount][][];
            for (int q = 0; q < filterCount; q++)
            {
                H[q] = new double[this.lcMas[q].XMas.Length][];
                for (int p = 0; p < H[q].Length; p++)
                {
                    H[q][p] = new double[vpn];
                }
            }

            // Полное количество измерений интенсивности во всех фильтрах
            int lcPointsNumber = 0;
            for (int q = 0; q < lcMas.Length; q++)
            {
                lcPointsNumber = lcPointsNumber + lcMas[q].XMas.Length;
            }


            double[] I = new double[lcPointsNumber];
            int k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    I[k] = this.lcMas[q].FMas[p] / sigma[q];
                    k++;
                }
            }

            // Инициализация матриц H;
            k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < lcMas[q].XMas.Length; p++)
                {
                    k = 0;
                    for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
                    {
                        int L = this.tsrf.patch[i].Length;
                        for (int j = 0; j < L; j++)
                        {
                            H[q][p][k] = (1.0 / this.sigma[q]) * this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                                (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                                this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc());
                            //H[q].SetElement(p, k, this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                            //    (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                            //    this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc()));
                            k++;
                        }
                    }
                }
            }

            double isp=this.spInt[0].Interp(tsp)/1e5;
            double iph=this.spInt[0].Interp(tph)/1e5;

            Matrix S = new Matrix(this.lcMas[0].XMas.Length + 2 * vpn, vpn);

            for (int p = 0; p < this.lcMas[0].XMas.Length; p++)
            {
                for (int i = 0; i < vpn; i++)
                {
                    S[p,i] = H[0][p][i];
                }
            }
            for (int p = 0; p < vpn; p++)
            {
                S[p + this.lcMas[0].XMas.Length, p] = sqrtLambda * ff;
            }
            for (int p = 0; p < vpn; p++)
            {
                S[p + this.lcMas[0].XMas.Length + vpn, p] = sqrtLambda * (1 - ff);
            }

            Matrix B = new Matrix(this.lcMas[0].XMas.Length + 2 * vpn, 1);
            for (int p = 0; p < this.lcMas[0].XMas.Length; p++)
            {
                B[p] = I[p];
            }
            for (int p = 0; p < vpn; p++)
            {
                B[this.lcMas[0].XMas.Length + p] = isp;
            }
            for (int p = 0; p < vpn; p++)
            {
                B[this.lcMas[0].XMas.Length + vpn + p] = iph;
            }

            Matrix x = MathLib.LES_Solver.SolveWithGaussMethodM(S.Tr() * S, S.Tr() * B);

            this.fluxes = new double[filterCount][];
            for (int q = 0; q < filterCount; q++)
            {
                this.fluxes[q] = new double[100];
            }

            for (int q = 0; q < filterCount; q++)                  // Be carefull, H is multiplied to sigma!
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    double sum = 0;
                    for (int j = 0; j < vpn; j++)
                    {
                        sum = sum + H[q][p][j] * sigma[q] * this.spInt[q].Interp(x[j]);
                    }
                    fluxes[q][p] = sum;
                }
            }

            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    this.tsrf.teff[i][j] = x[k];
                    k++;
                }
            }

            return this.tsrf;
        }

        public TSurface ReconstrColor(double lambda, double bias, double tph, double tsp)
        {
            double sqrtLambda = Math.Sqrt(lambda);
            double sqrtBias = Math.Sqrt(bias);
            int filterCount = this.lcMas.Length;
            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();

            // Определение и инициализация массива средних наблюдаемых потоков;
            double fluxAve;
            {
                double sum = 0;
                for (int p = 0; p < this.lcMas[0].XMas.Length; p++)
                {
                    sum = sum + this.lcMas[0].FMas[p];
                }
                fluxAve = sum / (double)this.lcMas[0].XMas.Length;
            }


            double[][][] H = new double[filterCount][][];
            for (int q = 0; q < filterCount; q++)
            {
                H[q] = new double[this.lcMas[q].XMas.Length][];
                for (int p = 0; p < H[q].Length; p++)
                {
                    H[q][p] = new double[vpn];
                }
            }

            // Полное количество измерений интенсивности во всех фильтрах
            int lcPointsNumber = 0;
            for (int q = 0; q < lcMas.Length; q++)
            {
                lcPointsNumber = lcPointsNumber + lcMas[q].XMas.Length;
            }


            double[] I = new double[lcPointsNumber];
            int k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    I[k] = this.lcMas[q].FMas[p] / sigma[q];
                    k++;
                }
            }

            // Инициализация матриц H;
            k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < lcMas[q].XMas.Length; p++)
                {
                    k = 0;
                    for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
                    {
                        int L = this.tsrf.patch[i].Length;
                        for (int j = 0; j < L; j++)
                        {
                            H[q][p][k] = (1.0 / this.sigma[q]) * this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                                (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                                this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc());
                            //H[q].SetElement(p, k, this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                            //    (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                            //    this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc()));
                            k++;
                        }
                    }
                }
            }

            // Инициализация вектора x;
            double[] x = new double[vpn];
            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    x[k] = tph;
                    k++;
                }
            }

            double scale = 1e-5;

            double[][] jacobyTr = new double[vpn][];
            for (int i = 0; i < jacobyTr.Length; i++)
            {
                jacobyTr[i] = new double[lcPointsNumber + vpn];
            }

            double[] f = new double[lcPointsNumber + vpn];

            double[] b = new double[vpn];

            // The Hessian matrix;
            double[][] hessian = new double[vpn][];
            for (int i = 0; i < hessian.Length; i++)
            {
                hessian[i] = new double[vpn];
            }

            // The step-vector for Gauss-Newton minimization algorithm;
            double[] h = new double[vpn];

            double[] x0 = new double[vpn];

            // The vector of first derivatives of normal specific intensities with respect to temperature;
            double[][] JD = new double[this.lcMas.Length][];
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                JD[q] = new double[vpn];
            }

            // The vector of weight-coefficients;
            double[] c = new double[vpn];
            for (int i = 0; i < vpn; i++) c[i] = 1.0;

            double norm2;

            int iter2 = 0;
            int iter1 = 0;

            do
            {
                if (iter1 != 0)
                {
                    for (int i = 0; i < vpn; i++)
                    {
                        if (x[i] > tph) c[i] = sqrtBias;
                    }
                }

                iter2 = 0;

                do
                {
                    for (int i = 0; i < vpn; i++) x0[i] = x[i];

                    // Вычисление вектора f;
                    k = 0;
                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                        {
                            double sum = 0;
                            for (int i = 0; i < vpn; i++)
                            {
                                sum = sum + H[q][p][i] * this.spInt[q].Interp(x[i]);
                            }
                            f[k] = scale * sum - I[k];
                            k++;
                        }
                    }

                    for (int i = 0; i < vpn; i++)
                    {
                        f[i + k] = sqrtLambda * c[i] * (tph - x[i]);
                    }

                    // Вычисление производных норм.уд.интенсивностей по температуре;

                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int i = 0; i < vpn; i++)
                        {
                            JD[q][i] = this.spInt[q].InterpD1(x[i]);
                        }
                    }

                    // Вычисление компонент матрицы Якоби;
                    k = 0;
                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                        {
                            for (int i = 0; i < vpn; i++)
                            {
                                jacobyTr[i][k] = scale * H[q][p][i] * JD[q][i];
                            }

                            k++;
                        }
                    }

                    for (int i = 0; i < vpn; i++)
                    {
                        jacobyTr[i][k + i] = -sqrtLambda * c[i];
                    }

                    // Computing of the Hessian matrix;
                    for (int i = 0; i < vpn; i++)
                    {
                        double sum;
                        for (int j = i; j < vpn; j++)
                        {
                            sum = 0;
                            for (int l = 0; l < lcPointsNumber /*+ vpn*/; l++)
                            {
                                sum = sum + jacobyTr[i][l] * jacobyTr[j][l];
                            }

                            if (i < vpn)
                            {
                                if (i == j) sum = sum + Math.Pow(jacobyTr[i][i + lcPointsNumber], 2);
                            }
                            
                            hessian[i][j] = sum;
                            hessian[j][i] = sum;
                        }
                    }

                    MathLib.Basic.AMultB(ref jacobyTr, ref f, ref b);

                    MathLib.Basic.VAMultSC(ref b, -1.0);

                    //h=MathLib.LES_Solver.SolveWithGaussMethod(hessian, b);

                    MathLib.LES_Solver.ConvGradMethodPL(ref hessian, ref b, ref h, 0.00000000001);

                    MathLib.Basic.VAplusVB(ref x0, ref h, ref x);

                    norm2 = 0;
                    for (int i = 0; i < vpn; i++)
                    {
                        norm2 = norm2 + Math.Pow(x[i] - x0[i], 2);
                    }
                    norm2 = Math.Sqrt(norm2);

                    iter2++;
                } while (norm2>0.1*vpn);

                iter1++;

            } while (iter1<10);

            this.fluxes = new double[filterCount][];
            for (int q = 0; q < filterCount; q++)
            {
                this.fluxes[q] = new double[100];
            }

            for (int q = 0; q < filterCount; q++)                  // Be carefull, H is multiplied to sigma!
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    double sum = 0;
                    for (int j = 0; j < vpn; j++)
                    {
                        sum = sum + scale * H[q][p][j] * sigma[q] * this.spInt[q].Interp(x[j]);
                    }
                    fluxes[q][p] = sum;
                }
            }

            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    this.tsrf.teff[i][j] = x[k];
                    k++;
                }
            }

            return this.tsrf;
        }

        public TSurface ReconstrF(double lambda, double tph, double tsp)
        {
            double sqrtLambda = Math.Sqrt(lambda);
            
            int filterCount = this.lcMas.Length;
            // The count of patches that is able to be observed;
            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();

            // The array of averange observed fluxes;
            double[] fluxAve = new double[this.lcMas.Length];
            for (int q = 0; q < fluxAve.Length; q++)
            {
                double sum = 0;
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    sum = sum + this.lcMas[q].FMas[p];
                }
                fluxAve[q] = sum / (double)this.lcMas[q].XMas.Length;
            }


            double[][][] H = new double[filterCount][][];
            for (int q = 0; q < filterCount; q++)
            {
                H[q] = new double[this.lcMas[q].XMas.Length][];
                for (int p = 0; p < H[q].Length; p++)
                {
                    H[q][p] = new double[vpn];
                }
            }

            // Полное количество измерений интенсивности во всех фильтрах
            int lcPointsNumber = 0;
            for (int q = 0; q < lcMas.Length; q++)
            {
                lcPointsNumber = lcPointsNumber + lcMas[q].XMas.Length;
            }


            double[] I = new double[lcPointsNumber];
            int k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    I[k] = this.lcMas[q].FMas[p] / sigma[q];
                    k++;
                }
            }

            // Инициализация матриц H;
            k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < lcMas[q].XMas.Length; p++)
                {
                    k = 0;
                    for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
                    {
                        int L = this.tsrf.patch[i].Length;
                        for (int j = 0; j < L; j++)
                        {
                            H[q][p][k] = (1.0 / this.sigma[q]) * this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                                (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                                this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc());
                            //H[q].SetElement(p, k, this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                            //    (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                            //    this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc()));
                            k++;
                        }
                    }
                }
            }

            // The vector of spot's intensity values in various photometric bands;
            double[] isp = new double[filterCount];
            // The vector of photospheric intensity in various photometric bands;
            double[] iph = new double[filterCount];

            for (int i = 0; i < filterCount; i++)
            {
                isp[i] = this.spInt[i].Interp(tsp);
                iph[i] = this.spInt[i].Interp(tph);
            }


            double[][] B = new double[filterCount][];
            for (int q = 0; q < filterCount; q++)
            {
                B[q] = new double[lcMas[q].XMas.Length];
            }

            for (int q = 0; q < filterCount; q++)
            {
                for (int p = 0; p < lcMas[q].XMas.Length; p++)
                {
                    double sum = 0;
                    for (int i = 0; i < vpn; i++)
                    {
                        sum = sum + H[q][p][i];
                    }
                    B[q][p] = iph[q] * sum;
                }
            }

            for (int q = 0; q < filterCount; q++)
            {
                for (int p = 0; p < lcMas[q].XMas.Length; p++)
                {
                    for (int i = 0; i < vpn; i++)
                    {
                        H[q][p][i] = -(iph[q] - isp[q]) * H[q][p][i];
                    }
                }
            }

            // Initialization of vector x;
            double[] x = new double[vpn + this.lcMas.Length];
            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    x[k] = 0.0;
                    k++;
                }
            }

            for (int q = 0; q < this.lcMas.Length; q++)
            {
                double flux = iph[q] *
                    (Math.PI * (1 - this.xLDC[q] / 3.0));
                x[q + k] = fluxAve[q] / flux;
            }

            double[][] jacobyTr = new double[vpn + filterCount][];
            for (int i = 0; i < jacobyTr.Length; i++)
            {
                jacobyTr[i] = new double[lcPointsNumber + vpn];
            }

            double[] f = new double[lcPointsNumber + vpn];

            double[] b = new double[vpn + filterCount];

            // The Hessian matrix;
            double[][] hessian = new double[vpn + filterCount][];
            for (int i = 0; i < hessian.Length; i++)
            {
                hessian[i] = new double[vpn + filterCount];
            }

            // The vector of filling-factor values;
            double[] ff = new double[vpn];

            // The vector of first derevatives of filling-factor with respect to x-parameter;
            double[] ffdx = new double[vpn];

            // The step-vector for Gauss-Newton minimization algorithm;
            double[] h = new double[vpn + filterCount];

            double[] x0 = new double[vpn + filterCount];

            double norm2;

            int iter2 = 0;

            do
            {
                for (int i = 0; i < vpn + filterCount; i++) x0[i] = x[i];

                for (int i = 0; i < vpn; i++) ff[i] = 1.0 / (1.0 + Math.Exp(-x[i]));

                for (int i = 0; i < vpn; i++) ffdx[i] = Math.Pow(ff[i], 2) * Math.Exp(-x[i]);

                // Вычисление вектора f;
                k = 0;
                for (int q = 0; q < filterCount; q++)
                {
                    for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                    {
                        double sum = 0;
                        for (int i = 0; i < vpn; i++)
                        {
                            sum = sum + H[q][p][i] * ff[i];
                        }
                        f[k] = x[vpn + q] * (sum + B[q][p]) - I[k];
                        k++;
                    }
                }

                double fave = 0.0;
                for (int i = 0; i < vpn; i++)
                {
                    fave = fave + ff[i];
                }
                fave = fave / vpn;

                for (int i = 0; i < vpn; i++)
                {
                    f[i + k] = sqrtLambda * (ff[i] - fave);
                    //f[i + k] = sqrtLambda * (x[i] - 5000) * (x[i] - 4900);
                }

                // Вычисление компонент матрицы Якоби;
                k = 0;
                for (int q = 0; q < filterCount; q++)
                {
                    for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                    {
                        for (int i = 0; i < vpn; i++)
                        {
                            jacobyTr[i][k] = x[vpn + q] * H[q][p][i] * ffdx[i];
                        }

                        double sum = 0;
                        for (int i = 0; i < vpn; i++)
                        {
                            sum = sum + H[q][p][i] * ff[i];
                        }
                        jacobyTr[vpn + q][k] = sum + B[q][p];
                        k++;
                    }
                }
                for (int i = 0; i < vpn; i++)
                {
                    for (int j = 0; j < vpn; j++)
                    {
                        if (i == j)
                        {
                            jacobyTr[j][k + i] = sqrtLambda * (1.0 - 1.0 / vpn) * ffdx[i];
                        }
                        else
                        {
                            jacobyTr[j][k + i] = sqrtLambda * ( - 1.0 / vpn) * ffdx[j];
                        }
                    }
                }

                // Computing of the Hessian matrix;
                for (int i = 0; i < vpn + filterCount; i++)
                {
                    double sum;
                    for (int j = i; j < vpn + filterCount; j++)
                    {
                        sum = 0;
                        for (int l = 0; l < lcPointsNumber + vpn; l++)
                        {
                            sum = sum + jacobyTr[i][l] * jacobyTr[j][l];
                        }
                        hessian[i][j] = sum;
                        hessian[j][i] = sum;
                    }
                }

                MathLib.Basic.AMultB(ref jacobyTr, ref f, ref b);

                MathLib.Basic.VAMultSC(ref b, -1.0);

                //h=MathLib.LES_Solver.SolveWithGaussMethod(hessian, b);

                MathLib.LES_Solver.ConvGradMethodPL(ref hessian, ref b, ref h, 0.000001);

                MathLib.Basic.VAplusVB(ref x0, ref h, ref x);

                norm2 = 0;
                for (int i = 0; i < vpn; i++)
                {
                    norm2 = norm2 + Math.Pow(x[i] - x0[i], 2);
                }
                norm2 = Math.Sqrt(norm2);

                iter2++;
            } while (iter2<20);

            this.fluxes = new double[filterCount][];
            for (int q = 0; q < filterCount; q++)
            {
                this.fluxes[q] = new double[100];
            }

            // Computing of the synthetic light curves;                 //ACHTUNG!!!
            for (int q = 0; q < filterCount; q++)
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    double sum = 0;
                    for (int j = 0; j < vpn; j++)
                    {
                        sum = sum + H[q][p][j] * 1.0 / (1.0 + Math.Exp(-x[j]))*sigma[q];
                    }
                    fluxes[q][p] = x[vpn + q] * (sum + B[q][p] * sigma[q]);
                }
            }

            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    this.tsrf.teff[i][j] = 1.0 / (1.0 + Math.Exp(-x[k]));
                    k++;
                }
            }

            return this.tsrf;
        }

        public TSurface ReconstrF1(double lambda, double tph, double tsp)
        {
            double sqrtLambda = Math.Sqrt(lambda);

            int filterCount = this.lcMas.Length;
            // The count of patches that is able to be observed;
            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();

            // The array of averange observed fluxes;
            double[] fluxAve = new double[this.lcMas.Length];
            for (int q = 0; q < fluxAve.Length; q++)
            {
                double sum = 0;
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    sum = sum + this.lcMas[q].FMas[p];
                }
                fluxAve[q] = sum / (double)this.lcMas[q].XMas.Length;
            }


            double[][][] H = new double[filterCount][][];
            for (int q = 0; q < filterCount; q++)
            {
                H[q] = new double[this.lcMas[q].XMas.Length][];
                for (int p = 0; p < H[q].Length; p++)
                {
                    H[q][p] = new double[vpn];
                }
            }

            // Полное количество измерений интенсивности во всех фильтрах
            int lcPointsNumber = 0;
            for (int q = 0; q < lcMas.Length; q++)
            {
                lcPointsNumber = lcPointsNumber + lcMas[q].XMas.Length;
            }


            double[] I = new double[lcPointsNumber];
            int k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    I[k] = this.lcMas[q].FMas[p] / sigma[q];
                    k++;
                }
            }

            // Инициализация матриц H;
            k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < lcMas[q].XMas.Length; p++)
                {
                    k = 0;
                    for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
                    {
                        int L = this.tsrf.patch[i].Length;
                        for (int j = 0; j < L; j++)
                        {
                            H[q][p][k] = (1.0 / this.sigma[q]) * this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                                (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                                this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc());
                            //H[q].SetElement(p, k, this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                            //    (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                            //    this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc()));
                            k++;
                        }
                    }
                }
            }

            // The vector of spot's intensity values in various photometric bands;
            double[] isp = new double[filterCount];
            // The vector of photospheric intensity in various photometric bands;
            double[] iph = new double[filterCount];

            for (int i = 0; i < filterCount; i++)
            {
                isp[i] = this.spInt[i].Interp(tsp);
                iph[i] = this.spInt[i].Interp(tph);
            }


            double[][] B = new double[filterCount][];
            for (int q = 0; q < filterCount; q++)
            {
                B[q] = new double[lcMas[q].XMas.Length];
            }

            for (int q = 0; q < filterCount; q++)
            {
                for (int p = 0; p < lcMas[q].XMas.Length; p++)
                {
                    double sum = 0;
                    for (int i = 0; i < vpn; i++)
                    {
                        sum = sum + H[q][p][i];
                    }
                    B[q][p] = iph[q] * sum;
                }
            }

            for (int q = 0; q < filterCount; q++)
            {
                for (int p = 0; p < lcMas[q].XMas.Length; p++)
                {
                    for (int i = 0; i < vpn; i++)
                    {
                        H[q][p][i] = -(iph[q] - isp[q]) * H[q][p][i];
                    }
                }
            }

            // Initialization of vector x;
            double[] x = new double[vpn + this.lcMas.Length];
            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    x[k] = 0.0;
                    k++;
                }
            }

            for (int q = 0; q < this.lcMas.Length; q++)
            {
                double flux = iph[q] *
                    (Math.PI * (1 - this.xLDC[q] / 3.0));
                x[q + k] = fluxAve[q] / flux;
            }

            double[][] jacobyTr = new double[vpn + filterCount][];
            for (int i = 0; i < jacobyTr.Length; i++)
            {
                jacobyTr[i] = new double[lcPointsNumber + vpn];
            }

            double[] f = new double[lcPointsNumber + vpn];

            double[] b = new double[vpn + filterCount];

            // The Hessian matrix;
            double[][] hessian = new double[vpn + filterCount][];
            for (int i = 0; i < hessian.Length; i++)
            {
                hessian[i] = new double[vpn + filterCount];
            }

            // The vector of filling-factor values;
            double[] ff = new double[vpn];

            // The vector of first derevatives of filling-factor with respect to x-parameter;
            double[] ffdx = new double[vpn];

            // The step-vector for Gauss-Newton minimization algorithm;
            double[] h = new double[vpn + filterCount];

            double[] x0 = new double[vpn + filterCount];

            double norm2;

            int iter2 = 0;

            do
            {
                for (int i = 0; i < vpn + filterCount; i++) x0[i] = x[i];

                for (int i = 0; i < vpn; i++) ff[i] = 1.0 / (1.0 + Math.Exp(-x[i]));

                for (int i = 0; i < vpn; i++) ffdx[i] = Math.Pow(ff[i], 2) * Math.Exp(-x[i]);

                // Вычисление вектора f;
                k = 0;
                for (int q = 0; q < filterCount; q++)
                {
                    for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                    {
                        double sum = 0;
                        for (int i = 0; i < vpn; i++)
                        {
                            sum = sum + H[q][p][i] * ff[i];
                        }
                        f[k] = x[vpn + q] * (sum + B[q][p]) - I[k];
                        k++;
                    }
                }

                double fave = 0.0;
                for (int i = 0; i < vpn; i++)
                {
                    fave = fave + ff[i];
                }
                fave = fave / vpn;

                for (int i = 0; i < vpn; i++)
                {
                    f[i + k] = sqrtLambda * (ff[i] - fave);
                    //f[i + k] = sqrtLambda * (x[i] - 5000) * (x[i] - 4900);
                }

                // Вычисление компонент матрицы Якоби;
                k = 0;
                for (int q = 0; q < filterCount; q++)
                {
                    for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                    {
                        for (int i = 0; i < vpn; i++)
                        {
                            jacobyTr[i][k] = x[vpn + q] * H[q][p][i] * ffdx[i];
                        }

                        double sum = 0;
                        for (int i = 0; i < vpn; i++)
                        {
                            sum = sum + H[q][p][i] * ff[i];
                        }
                        jacobyTr[vpn + q][k] = sum + B[q][p];
                        k++;
                    }
                }
                for (int i = 0; i < vpn; i++)
                {
                    for (int j = 0; j < vpn; j++)
                    {
                        if (i == j)
                        {
                            jacobyTr[j][k + i] = sqrtLambda * (1.0 - 1.0 / vpn) * ffdx[i];
                        }
                        else
                        {
                            jacobyTr[j][k + i] = sqrtLambda * (-1.0 / vpn) * ffdx[j];
                        }
                    }
                }

                // Computing of the Hessian matrix;
                for (int i = 0; i < vpn + filterCount; i++)
                {
                    double sum;
                    for (int j = i; j < vpn + filterCount; j++)
                    {
                        sum = 0;
                        for (int l = 0; l < lcPointsNumber + vpn; l++)
                        {
                            sum = sum + jacobyTr[i][l] * jacobyTr[j][l];
                        }
                        hessian[i][j] = sum;
                        hessian[j][i] = sum;
                    }
                }

                MathLib.Basic.AMultB(ref jacobyTr, ref f, ref b);

                MathLib.Basic.VAMultSC(ref b, -1.0);

                //h=MathLib.LES_Solver.SolveWithGaussMethod(hessian, b);

                MathLib.LES_Solver.ConvGradMethodPL(ref hessian, ref b, ref h, 0.000001);

                MathLib.Basic.VAplusVB(ref x0, ref h, ref x);

                norm2 = 0;
                for (int i = 0; i < vpn; i++)
                {
                    norm2 = norm2 + Math.Pow(x[i] - x0[i], 2);
                }
                norm2 = Math.Sqrt(norm2);

                iter2++;
            } while (iter2 < 20);

            this.fluxes = new double[filterCount][];
            for (int q = 0; q < filterCount; q++)
            {
                this.fluxes[q] = new double[100];
            }

            // Computing of the synthetic light curves;                 //ACHTUNG!!!
            for (int q = 0; q < filterCount; q++)
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    double sum = 0;
                    for (int j = 0; j < vpn; j++)
                    {
                        sum = sum + H[q][p][j] * 1.0 / (1.0 + Math.Exp(-x[j])) * sigma[q];
                    }
                    fluxes[q][p] = x[vpn + q] * (sum + B[q][p] * sigma[q]);
                }
            }

            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    this.tsrf.teff[i][j] = 1.0 / (1.0 + Math.Exp(-x[k]));
                    k++;
                }
            }

            return this.tsrf;
        }

        //public TSurface ReconstrDISCRET(double lambda, double tph, double tsp)
        //{
            //double sqrtLambda = Math.Sqrt(lambda);
            //int filterCount = this.lcMas.Length;
            //int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();

            //// Определение и инициализация массива средних наблюдаемых потоков;
            //double[] fluxAve = new double[this.lcMas.Length];
            //for (int q = 0; q < fluxAve.Length; q++)
            //{
            //    double sum = 0;
            //    for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
            //    {
            //        sum = sum + this.lcMas[q].FMas[p];
            //    }
            //    fluxAve[q] = sum / (double)this.lcMas[q].XMas.Length;
            //}


            //double[][][] H = new double[filterCount][][];
            //for (int q = 0; q < filterCount; q++)
            //{
            //    H[q] = new double[this.lcMas[q].XMas.Length][];
            //    for (int p = 0; p < H[q].Length; p++)
            //    {
            //        H[q][p] = new double[vpn];
            //    }
            //}

            //// Полное количество измерений интенсивности во всех фильтрах
            //int lcPointsNumber = 0;
            //for (int q = 0; q < lcMas.Length; q++)
            //{
            //    lcPointsNumber = lcPointsNumber + lcMas[q].XMas.Length;
            //}


            //double[] I = new double[lcPointsNumber];
            //int k = 0;
            //for (int q = 0; q < this.lcMas.Length; q++)
            //{
            //    for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
            //    {
            //        I[k] = this.lcMas[q].FMas[p] / sigma[q];
            //        k++;
            //    }
            //}

            //// Инициализация матриц H;
            //k = 0;
            //for (int q = 0; q < this.lcMas.Length; q++)
            //{
            //    for (int p = 0; p < lcMas[q].XMas.Length; p++)
            //    {
            //        k = 0;
            //        for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
            //        {
            //            int L = this.tsrf.patch[i].Length;
            //            for (int j = 0; j < L; j++)
            //            {
            //                H[q][p][k] = (1.0 / this.sigma[q]) * this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
            //                    (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
            //                    this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc());
            //                k++;
            //            }
            //        }
            //    }
            //}

            //System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

            //Random rnd = new Random((int)DateTime.Now.Ticks);

            //// Инициализация вектора x;
            //double[] x = new double[vpn];
            //k = 0;
            //for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            //{
            //    for (int j = 0; j < this.tsrf.patch[i].Length; j++)
            //    {
            //        x[k] = tph;
            //        k++;
            //    }
            //}

            //double intenPh = this.spInt[0].Interp(tph);
            //double intenSp = this.spInt[0].Interp(tsp);

            //double[] c = new double[filterCount];

            //double[] x0 = new double[vpn];


            //double norm2;
            //int iter = 0;

            //do
            //{
            //    for (int i = 0; i < vpn; i++) x0[i] = x[i];

            //    for (int i = 0; i < vpn; i++)
            //    {
            //        x[i]
            //    }


            //    for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
            //    {
            //        double sum = 0;
            //        for (int j = 0; j < vpn; j++)
            //        {
            //            sum = sum + c[0] * H[0][p][j] * x[j];
            //        }
            //        fluxes[0][p] = sum;
            //    }

            //    {
            //        double sum1 = 0;
            //        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
            //        {
            //            sum1 = sum1 + I[p] * fluxes[0][p];
            //        }
            //        double sum2 = 0;
            //        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
            //        {
            //            sum2 = sum2 + fluxes[0][p] * fluxes[0][p];
            //        }
            //        c[0] = sum1 / sum2;
            //    }

            //    double term1 = 0;

            //    for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
            //    {
            //        term1 = term1 + Math.Pow(c[0] * fluxes[0][p] - I[p], 2);
            //    }

            //    double term2 = 0;

            //    for (int i = 0; i < vpn; i++)
            //    {
            //        term2 = term2 + Math.Pow(x[i] - tph, 2);
            //    }
            //    term2 = lambda * term2;

            //    double func = term2 + term1;

            //    iter++;
            //} while (iter < 4/*norm2 > vpn * 1*/);

            //this.fluxes = new double[filterCount][];
            //for (int q = 0; q < filterCount; q++)
            //{
            //    this.fluxes[q] = new double[100];
            //}

            //for (int q = 0; q < filterCount; q++)
            //{
            //    for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
            //    {
            //        double sum = 0;
            //        for (int j = 0; j < vpn; j++)
            //        {
            //            sum = sum + x[vpn+q] * H[0][p][j] * this.spInt[q].Interp(x[j]);
            //        }
            //        fluxes[q][p] = sum;
            //    }
            //}

            //k = 0;
            //for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            //{
            //    for (int j = 0; j < this.tsrf.patch[i].Length; j++)
            //    {
            //        this.tsrf.teff[i][j] = x[k];
            //        k++;
            //    }
            //}

            //return this.tsrf;
        //}

        public TSurface ReconstrEND1(double lambda, double bias, double tph, double tsp)
        {
            double sqrtLambda = Math.Sqrt(lambda);

            double sqrtBias = Math.Sqrt(bias);

            int filterCount = this.lcMas.Length;

            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();

            // Определение и инициализация массива средних наблюдаемых потоков;
            double[] fluxAve = new double[this.lcMas.Length];
            for (int q = 0; q < fluxAve.Length; q++)
            {
                double sum = 0;
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    sum = sum + this.lcMas[q].FMas[p];
                }
                fluxAve[q] = sum / (double)this.lcMas[q].XMas.Length;
            }

            // Initialization of response matrix;
            double[][][] H = new double[filterCount][][];
            for (int q = 0; q < filterCount; q++)
            {
                H[q] = new double[this.lcMas[q].XMas.Length][];
                for (int p = 0; p < H[q].Length; p++)
                {
                    H[q][p] = new double[vpn];
                }
            }

            // Полное количество измерений интенсивности во всех фильтрах
            int lcPointsNumber = 0;
            for (int q = 0; q < lcMas.Length; q++)
            {
                lcPointsNumber = lcPointsNumber + lcMas[q].XMas.Length;
            }


            double[] I = new double[lcPointsNumber];
            int k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    I[k] = this.lcMas[q].FMas[p] / sigma[q];
                    k++;
                }
            }

            // Инициализация матриц H;
            k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < lcMas[q].XMas.Length; p++)
                {
                    k = 0;
                    for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
                    {
                        int L = this.tsrf.patch[i].Length;
                        for (int j = 0; j < L; j++)
                        {
                            H[q][p][k] = (1.0 / this.sigma[q]) * this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                                (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                                this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc());
                            //H[q].SetElement(p, k, this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                            //    (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                            //    this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc()));
                            k++;
                        }
                    }
                }
            }

            // Инициализация вектора x;
            double[] x = new double[vpn];
            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    x[k] = this.tsrf.teff[i][j];
                    k++;
                }
            }

            double[] scale = new double[this.lcMas.Length];

            for (int q = 0; q < this.lcMas.Length; q++)
            {
                double flux = this.spInt[q].Interp(this.tsrf.teff[0][0]) *
                    (Math.PI * (1 - this.xLDC[q] / 3.0));
                scale[q] = 1e-5/*fluxAve[q] / flux*/;
            }

            double[][] jacobyTr = new double[vpn][];
            for (int i = 0; i < jacobyTr.Length; i++)
            {
                jacobyTr[i] = new double[lcPointsNumber + vpn];
            }

            double[] f = new double[lcPointsNumber + vpn];

            double[] b = new double[vpn];

            // The Hessian matrix;
            double[][] hessian = new double[vpn][];
            for (int i = 0; i < hessian.Length; i++)
            {
                hessian[i] = new double[vpn];
            }

            // The step-vector for Gauss-Newton minimization algorithm;
            double[] h = new double[vpn];

            double[] x0 = new double[vpn];

            // The vector of first derivatives of normal specific intensities with respect to temperature;
            double[][] JD = new double[this.lcMas.Length][];
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                JD[q] = new double[vpn];
            }

            double[] patchAreas = new double[vpn];

            // The vector of weight-coefficients;
            double[] c = new double[vpn];
            for (int i = 0; i < vpn; i++) c[i] = 1.0;

            double norm2;

            int iter2 = 0;
            int iter1 = 0;

            // Wir sind geboren Taten zu volbringen,
            // zu uberwind ...
            do
            {
                if (iter1 != 0)
                {
                    double jAve = 0;
                    for (int i = 0; i < vpn; i++)
                    {
                        jAve = jAve + x[i];
                    }
                    jAve = jAve / (double)vpn;
                    for (int i = 0; i < vpn; i++)
                    {
                        if (x[i] <= jAve) c[i] = 1.0;
                        else c[i] = sqrtBias;
                    }
                }

                iter2 = 0;

                do
                {
                    //if (iter2 != 0)
                    //{
                    //    double jAve = 0;
                    //    for (int i = 0; i < vpn; i++)
                    //    {
                    //        jAve = jAve + x[i];
                    //    }
                    //    jAve = jAve / (double)vpn;
                    //    for (int i = 0; i < vpn; i++)
                    //    {
                    //        if (x[i] <= jAve) c[i] = 1.0;
                    //        else c[i] = sqrtBias;
                    //    }
                    //}

                    for (int i = 0; i < vpn; i++) x0[i] = x[i];

                    // Вычисление вектора f;
                    k = 0;
                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                        {
                            double sum = 0;
                            for (int i = 0; i < vpn; i++)
                            {
                                sum = sum + H[q][p][i] * this.spInt[q].Interp(x[i]);
                            }
                            f[k] = scale[q] * sum - I[k];
                            k++;
                        }
                    }

                    double teffAve = x0.Average();
                    
                    for (int i = 0; i < vpn; i++)
                    {
                        f[i + k] = sqrtLambda * c[i] * (x[i] - teffAve);
                    }

                    // Вычисление производных норм.уд.интенсивностей по температуре;

                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int i = 0; i < vpn; i++)
                        {
                            JD[q][i] = this.spInt[q].InterpD1(x[i]);
                        }
                    }

                    // Вычисление компонент матрицы Якоби;
                    k = 0;
                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                        {
                            for (int i = 0; i < vpn; i++)
                            {
                                jacobyTr[i][k] = scale[q] * H[q][p][i] * JD[q][i];
                            }

                            k++;
                        }
                    }
                    for (int i = 0; i < vpn; i++)
                    {
                        for (int j = 0; j < vpn; j++)
                        {
                            jacobyTr[j][k + i] = -sqrtLambda * c[i] / vpn;
                            if (i == j) jacobyTr[i][k + i] = sqrtLambda * c[i]*(1.0 - 1.0 / vpn);
                            //jacobyTr[i][k + i] = sqrtLambda * c[i];
                        }
                    }

                    // Computing of the Hessian matrix;
                    Parallel.For(0, vpn, i =>
                    /*for (int i = 0; i < vpn + filterCount; i++)*/
                    {
                        double sum;
                        for (int j = i; j < vpn; j++)
                        {
                            sum = 0;
                            for (int l = 0; l < lcPointsNumber + vpn; l++)
                            {
                                sum = sum + jacobyTr[i][l] * jacobyTr[j][l];
                            }
                            
                            hessian[i][j] = sum;
                            hessian[j][i] = sum;
                        }
                    });



                    //double aa = hessian[0][0];
                    //for (int i = 0; i < hessian.Length; i++)
                    //{
                    //    if (hessian[i][i] > aa) aa = hessian[i][i];
                    //}
                    //double mu = 0.000000000000000000001 * aa;
                    //for (int i = 0; i < hessian.Length; i++)
                    //{
                    //    hessian[i][i] = hessian[i][i] + mu;
                    //}

                    MathLib.Basic.AMultB(ref jacobyTr, ref f, ref b);

                    MathLib.Basic.VAMultSC(ref b, -1.0);

                    //h=MathLib.LES_Solver.SolveWithGaussMethod(hessian, b);

                    MathLib.LES_Solver.ConvGradMethodPL(ref hessian, ref b, ref h, 1e-15);

                     //Line search;

                    double alpha = 1.0;
                    double alpha0;
                    double error;

                    do
                    {
                        alpha0 = alpha;
                        double eps = 0.00005;

                        double func1 = 0;
                        double func0 = 0;
                        double funcZ;

                        double funcDer;
                        double khi2 = 0;
                        double regul = 0;

                        //Calculation of the function and its first derivative;
                        for (int z = 0; z < 2; z++)
                        {
                            double alphaZ;
                            if (z == 0) alphaZ = alpha0;
                            else alphaZ = alpha0 + eps;

                            // Calculation of the first term;

                            k = 0;
                            khi2 = 0;
                            for (int q = 0; q < filterCount; q++)
                            {
                                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                                {
                                    double sum = 0;
                                    double sumd = 0;
                                    for (int i = 0; i < vpn; i++)
                                    {
                                        sum = sum + H[q][p][i] * this.spInt[q].Interp(x[i] + alphaZ * h[i]);
                                        sumd = sumd + H[q][p][i] * this.spInt[q].InterpD1(x[i] + alphaZ * h[i]) * h[i];
                                    }

                                    khi2 = khi2 + (scale[q] * sum - I[k]) * scale[q] * sumd;
                                    k++;
                                }
                            }

                            // Calculation of the regularization term;

                            double tAve = 0, hAve = 0;

                            for (int i = 0; i < vpn; i++)
                            {
                                tAve = tAve + x[i];
                                hAve = hAve + h[i];
                            }

                            tAve = tAve / vpn;
                            hAve = hAve / vpn;

                            double sum1 = 0;
                            for (int i = 0; i < vpn; i++)
                            {
                                sum1 = sum1 + c[i] * (x[i] + alphaZ * h[i] - tAve - alphaZ * hAve) * (h[i] - hAve);
                            }
                            regul = lambda * sum1;

                            funcZ = khi2 + regul;

                            if (z == 0) func0 = funcZ;
                            else func1 = funcZ;
                        }

                        funcDer = (func1 - func0) / eps;


                        alpha = alpha0 - func0 / funcDer;
                        

                        error = Math.Abs(alpha - alpha0);

                    } while (error > 0.005);

                     //End of line search;

                    MathLib.Basic.VAMultSC(ref h, alpha);

                    MathLib.Basic.VAplusVB(ref x0, ref h, ref x);

                    norm2 = 0;
                    for (int i = 0; i < vpn; i++)
                    {
                        norm2 = norm2 + Math.Pow(h[i], 2);
                    }
                    norm2 = Math.Sqrt(norm2);

                    
                    iter2++;
                } while (norm2 > vpn * 0.01);

                iter1++;

            } while (iter1<10);

            this.fluxes = new double[filterCount][];
            for (int q = 0; q < filterCount; q++)
            {
                this.fluxes[q] = new double[100];
            }

            for (int q = 0; q < filterCount; q++)                  // Be carefull, H is multiplied to sigma!
            {
                for (int p = 0; p < this.fluxes[q].Length; p++)
                {
                    double sum = 0;
                    for (int j = 0; j < vpn; j++)
                    {
                        sum = sum + scale[q] * H[q][p][j] * sigma[q] * this.spInt[q].Interp(x[j]);
                    }
                    fluxes[q][p] = sum;
                }
            }

            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    this.tsrf.teff[i][j] = x[k];
                    k++;
                }
            }

            return this.tsrf;
        }

        public TSurface ReconstrENDEnd(double lambda, double ff, double tph, double tsp)
        {
            double sqrtLambda = Math.Sqrt(lambda);
            int filterCount = this.lcMas.Length;
            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();

            // Определение и инициализация массива средних наблюдаемых потоков;
            double[] fluxAve = new double[this.lcMas.Length];
            for (int q = 0; q < fluxAve.Length; q++)
            {
                double sum = 0;
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    sum = sum + this.lcMas[q].FMas[p];
                }
                fluxAve[q] = sum / (double)this.lcMas[q].XMas.Length;
            }

            // Initialization of reponse matrix;
            double[][][] H = new double[filterCount][][];
            for (int q = 0; q < filterCount; q++)
            {
                H[q] = new double[this.lcMas[q].XMas.Length][];
                for (int p = 0; p < H[q].Length; p++)
                {
                    H[q][p] = new double[vpn];
                }
            }

            // Полное количество измерений интенсивности во всех фильтрах
            int lcPointsNumber = 0;
            for (int q = 0; q < lcMas.Length; q++)
            {
                lcPointsNumber = lcPointsNumber + lcMas[q].XMas.Length;
            }


            double[] I = new double[lcPointsNumber];
            int k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    I[k] = this.lcMas[q].FMas[p] / sigma[q];
                    k++;
                }
            }

            // Инициализация матриц H;
            k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < lcMas[q].XMas.Length; p++)
                {
                    k = 0;
                    for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
                    {
                        int L = this.tsrf.patch[i].Length;
                        for (int j = 0; j < L; j++)
                        {
                            H[q][p][k] = (1.0 / this.sigma[q]) * this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                                (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                                this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc());
                            //H[q].SetElement(p, k, this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                            //    (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                            //    this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc()));
                            k++;
                        }
                    }
                }
            }

            // Инициализация вектора x;
            double[] x = new double[vpn];
            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    x[k] = 4500;
                    k++;
                }
            }

            double[][] jacobyTr = new double[vpn][];
            for (int i = 0; i < jacobyTr.Length; i++)
            {
                jacobyTr[i] = new double[lcPointsNumber + 2];
            }

            double[] f = new double[lcPointsNumber + 2];

            double[] b = new double[vpn];

            // The Hessian matrix;
            double[][] hessian = new double[vpn][];
            for (int i = 0; i < hessian.Length; i++)
            {
                hessian[i] = new double[vpn];
            }

            // The step-vector for Gauss-Newton minimization algorithm;
            double[] h = new double[vpn];

            double[] x0 = new double[vpn];

            // The vector of first derivatives of normal specific intensities with respect to temperature;
            double[][] JD = new double[this.lcMas.Length][];
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                JD[q] = new double[vpn];
            }

            double[] patchAreas = new double[vpn];

            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    patchAreas[k] = (tsrf.patch[i][j].Phi20 - tsrf.patch[i][j].Phi10) *
                        (Math.Cos(tsrf.patch[i][j].Theta1) - Math.Cos(tsrf.patch[i][j].Theta2));
                    k++;
                }
            }

            double[] c = new double[filterCount];
            for (int i = 0; i < filterCount; i++)
            {
                c[i] = 1e-5;
            }

            double areaSrf = 2 * Math.PI * (Math.Sin(this.tsrf.GetInc()) + 1.0);

            double areaSpot = ff * areaSrf;

            double deltaTeff2 = Math.Pow(tph - tsp, 2);



            // The vector of weight-coefficients;

            double norm2;

            int iter2 = 0;
            int iter1 = 0;

            // Wir sind geboren Taten zu volbringen,
            // zu uberwind ...
            do
            {

                iter2 = 0;

                do
                {
                    for (int i = 0; i < vpn; i++) x0[i] = x[i];

                    // Вычисление вектора f;
                    k = 0;
                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                        {
                            double sum = 0;
                            for (int i = 0; i < vpn; i++)
                            {
                                sum = sum + H[q][p][i] * this.spInt[q].Interp(x[i]);
                            }
                            f[k] = c[q] * sum - I[k];
                            k++;
                        }
                    }

                    {
                        double sum = 0;
                        for (int i = 0; i < vpn; i++)
                        {
                            sum = sum + patchAreas[i] * Math.Pow(x[i] - tsp, 2);
                        }
                        sum = sum / deltaTeff2;
                        f[lcPointsNumber] = sqrtLambda * Math.Pow(sum - (1.0 - ff) * areaSrf, 2);

                        sum = 0;
                        for (int i = 0; i < vpn; i++)
                        {
                            sum = sum + patchAreas[i] * Math.Pow(x[i] - tph, 2);
                        }
                        sum = sum / deltaTeff2;
                        f[lcPointsNumber + 1] = sqrtLambda * Math.Pow(sum - ff * areaSrf, 2);
                    }

                    // Вычисление производных норм.уд.интенсивностей по температуре;

                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int i = 0; i < vpn; i++)
                        {
                            JD[q][i] = this.spInt[q].InterpD1(x[i]);
                        }
                    }

                    // Вычисление компонент матрицы Якоби;
                    k = 0;
                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                        {
                            for (int i = 0; i < vpn; i++)
                            {
                                jacobyTr[i][k] = c[q] * H[q][p][i] * JD[q][i];
                            }
                            k++;
                        }
                    }

                    {
                        for (int i = 0; i < vpn; i++)
                        {
                            jacobyTr[i][k] = sqrtLambda * 2 * patchAreas[i] * (x[i] - tsp) / deltaTeff2;
                        }
                        for (int i = 0; i < vpn; i++)
                        {
                            jacobyTr[i][k + 1] = sqrtLambda * 2 * patchAreas[i] * (x[i] - tph) / deltaTeff2;
                        }
                    }

                    // Computing of the Hessian matrix;
                    for (int i = 0; i < vpn; i++)
                    {
                        double sum;
                        for (int j = i; j < vpn; j++)
                        {
                            sum = 0;
                            for (int l = 0; l < lcPointsNumber + 2; l++)
                            {
                                sum = sum + jacobyTr[i][l] * jacobyTr[j][l];
                            }
                            hessian[i][j] = sum;
                            hessian[j][i] = sum;
                        }
                    };



                    double aa = hessian[0][0];
                    for (int i = 0; i < hessian.Length; i++)
                    {
                        if (hessian[i][i] > aa) aa = hessian[i][i];
                    }
                    double mu = 0.000000001 * aa;
                    for (int i = 0; i < hessian.Length; i++)
                    {
                        hessian[i][i] = hessian[i][i] + mu;
                    }

                    MathLib.Basic.AMultB(ref jacobyTr, ref f, ref b);

                    MathLib.Basic.VAMultSC(ref b, -1.0);

                    h=MathLib.LES_Solver.SolveWithGaussMethod(hessian, b);

                    //MathLib.LES_Solver.ConvGradMethodPL(ref hessian, ref b, ref h, 0.00000000000000001);

                    MathLib.Basic.VAplusVB(ref x0, ref h, ref x);

                    norm2 = 0;
                    for (int i = 0; i < vpn; i++)
                    {
                        norm2 = norm2 + Math.Pow(h[i], 2);
                    }
                    norm2 = Math.Sqrt(norm2);

                    iter2++;
                } while (norm2 > vpn * 0.01);

                iter1++;

            } while (false);

            this.fluxes = new double[filterCount][];
            for (int q = 0; q < filterCount; q++)
            {
                this.fluxes[q] = new double[100];
            }

            for (int q = 0; q < filterCount; q++)                  // Be carefull, H is multiplied to sigma!
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    double sum = 0;
                    for (int j = 0; j < vpn; j++)
                    {
                        sum = sum + c[q] * H[q][p][j] * sigma[q] * this.spInt[q].Interp(x[j]);
                    }
                    fluxes[q][p] = sum;
                }
            }

            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    this.tsrf.teff[i][j] = x[k];
                    k++;
                }
            }

            return this.tsrf;
        }


        public TSurface[] ReconstrLin(double lambda, double bias, double tph, double tsp, int iterMax, int scaleMode)
        {
            double sqrtLambda = Math.Sqrt(lambda);
            double sqrtBias = Math.Sqrt(bias);
            int filterCount = this.lcMas.Length;
            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();
            double gridScale = 1e0;

            // Определение и инициализация массива средних наблюдаемых потоков;
            double[] scale = new double[filterCount];
            double[] fluxAve = new double[this.lcs.Length];
            if (scaleMode == 0)
            {
                for (int q = 0; q < fluxAve.Length; q++)
                {
                    double sum = 0;
                    for (int p = 0; p < this.lcs[q].Phases.Length; p++)
                    {
                        sum = sum + this.lcs[q].Fluxes[p];
                    }
                    fluxAve[q] = sum / (double)this.lcs[q].Phases.Length;
                }

                for (int q = 0; q < filterCount; q++)
                {
                    scale[q] = 1.0e-5;
                    //scale[q] = fluxAve[q] /
                    //    (Math.PI * this.spInt[q].Interp(tph) *
                    //    gridScale * (1.0 - this.xLDC[q] / 3));
                }
            }
            if (scaleMode == 1)
            {
                for (int q = 0; q < filterCount; q++)
                {
                    scale[q]=this.lcs[q].FluxMax/
                        (Math.PI * this.spInt[q].Interp(tph) *
                        gridScale * (1.0 - this.xLDC[q] / 3));
                }
            }

            // Initialization of reponse matrix;
            double[][][] H = new double[filterCount][][];
            for (int q = 0; q < filterCount; q++)
            {
                H[q] = new double[this.lcs[q].Phases.Length][];
                for (int p = 0; p < H[q].Length; p++)
                {
                    H[q][p] = new double[vpn];
                }
            }

            // Полное количество измерений интенсивности во всех фильтрах
            int lpn = 0;
            for (int q = 0; q < lcs.Length; q++)
            {
                lpn = lpn + lcs[q].Phases.Length;
            }

            double[] I = new double[lpn];
            {
                int k = 0;
                for (int q = 0; q < this.lcs.Length; q++)
                {
                    for (int p = 0; p < this.lcs[q].Phases.Length; p++)
                    {
                        I[k] = this.lcs[q].Fluxes[p] / this.lcs[q].Sigma;
                        k++;
                    }
                }
            }

            // Инициализация матриц H;
            {
                int k = 0;
                for (int q = 0; q < this.lcs.Length; q++)
                {
                    for (int p = 0; p < lcs[q].Phases.Length; p++)
                    {
                        k = 0;
                        for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
                        {
                            for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                            {
                                H[q][p][k] = (1.0 / this.lcs[q].Sigma) * 
                                    this.tsrf.patch[i][j].ProjectedArea(this.lcs[q].Phases[p], this.tsrf.GetInc()) *
                                    (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcs[q].Phases[p], this.tsrf.GetInc()))) *
                                    this.tsrf.patch[i][j].Visible(this.lcs[q].Phases[p], this.tsrf.GetInc());
                                k++;
                            }
                        }
                    }
                }
            }

            double[] aMas = new double[filterCount];

            double[] bMas = new double[filterCount];

            double iph0, iph1, isp0, isp1;
            iph0 = this.spInt[0].Interp(tph) * gridScale;
            isp0 = this.spInt[0].Interp(tsp) * gridScale;
            
            
            aMas[0] = 1.0;
            bMas[0] = 0.0;

            if (filterCount > 1)
            {
                for (int q = 1; q < filterCount; q++)
                {
                    iph1 = this.spInt[q].Interp(tph)*scale[q];
                    isp1 = this.spInt[q].Interp(tsp)*scale[q];
                    aMas[q] = (iph1 - isp1) / (iph0*scale[0] - isp0*scale[0]);
                    bMas[q] = iph1 - aMas[q] * iph0*scale[0];
                }
            }
            

            double[][] A = new double[lpn + vpn][];
            for (int i = 0; i < A.Length; i++) A[i] = new double[vpn];

            double[] C = new double[vpn];

            double[] B = new double[lpn + vpn];

            double[][] AA = new double[vpn][];
            for (int i = 0; i < vpn; i++) AA[i] = new double[vpn];

            double[] BB = new double[vpn];

            double[] x = new double[vpn];

            

            for (int i = 0; i < vpn; i++) C[i] = 1.0;

            {
                int k = 0;
                for (int q = 0; q < filterCount; q++)
                {
                    for (int p = 0; p < this.lcs[q].Phases.Length; p++)
                    {
                        double sum = 0;
                        for (int i = 0; i < vpn; i++)
                        {
                            sum = sum + H[q][p][i] * bMas[q];
                        }
                        B[k] = I[k] - /*scale[q] */ sum;
                        k++;
                    }
                }
            }

            {
                int k = 0;
                for (int q = 0; q < filterCount; q++)
                {
                    double ss = /*scale[q] */ aMas[q];
                    for (int p = 0; p < this.lcs[q].Phases.Length; p++)
                    {
                        for (int i = 0; i < vpn; i++)
                        {
                            A[k][i] = ss * H[q][p][i];
                        }
                        k++;
                    }
                }
            }

            double vpnInvNegative = -1.0 / vpn;
            int iter = 0;
            do
            {
                if (iter != 0)
                {
                    double intAve = x.Average();
                    for (int i = 0; i < vpn; i++)
                    {
                        if (x[i] <= intAve) C[i] = 1.0;
                        else C[i] = sqrtBias;
                    }
                }

                for (int i = 0; i < vpn; i++)
                {
                    for (int j = 0; j < vpn; j++)
                    {
                        A[lpn + i][j] = sqrtLambda * C[i] * vpnInvNegative;
                        if (i == j) A[lpn + i][j] = 
                            sqrtLambda * C[i] * (1.0 + vpnInvNegative);
                    }
                }

                //MathLib.Basic.ATrA(ref A, ref AA);

                double sum1 = 0;
                for (int i = 0; i < vpn; i++)
                {
                    sum1 += Math.Pow(C[i] * sqrtLambda * vpnInvNegative, 2);
                }

                //double[][] SS = new double[AA.Length][];
                //for (int i = 0; i < SS.Length; i++) SS[i] = new double[AA[i].Length];

                Parallel.For(0, AA.Length, i =>
                //for (int i = 0; i < AA.Length; i++)
                {
                    for (int j = i; j < AA[i].Length; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < lpn; k++)
                        {
                            sum = sum + A[k][i] * A[k][j];
                        }
                        AA[i][j] = sum + sum1 - Math.Pow(C[i] * sqrtLambda * vpnInvNegative, 2) -
                            Math.Pow(C[j] * sqrtLambda * vpnInvNegative, 2);
                        if (i == j)
                        {
                            AA[i][j] += Math.Pow(C[i] * sqrtLambda * (1 + vpnInvNegative), 2);
                        }
                        else
                        {
                            AA[i][j] += C[i] * sqrtLambda * (1 + vpnInvNegative) * C[i] * sqrtLambda * vpnInvNegative +
                                C[j] * sqrtLambda * (1 + vpnInvNegative) * C[j] * sqrtLambda * vpnInvNegative;
                        }
                        AA[j][i] = AA[i][j];

                        //double sum2 = 0;
                        //for (int k = 0; k < A.Length; k++)
                        //{
                        //    sum2 = sum2 + A[k][i] * A[k][j];
                        //}
                        //SS[i][j] = sum2;
                        //SS[j][i] = sum2;
                    }
                });

                for (int i = 0; i < vpn; i++)
                {
                    double sum = 0;
                    for (int k = 0; k < lpn; k++)
                    {
                        sum = sum + A[k][i] * B[k];
                    }
                    BB[i] = sum;
                }
                x=MathLib.LES_Solver.SolveWithGaussMethod(AA, BB);
                
                //MathLib.LES_Solver.ConvGradMethodPL(ref AA, ref BB, ref x, 1e-50);
                iter++;

            } while (iter < iterMax && bias != 1.0);

            TSurface[] tsrfIntenRes = new TSurface[filterCount];

            for (int q = 0; q < filterCount; q++)
            {
                tsrfIntenRes[q] = new TSurface(this.tsrf.GetN(), this.tsrf.GetM(), this.tsrf.GetInc(), 0.0);
            }

            for (int q = 0; q < tsrfIntenRes.Length; q++)
            {
                int k=0;
                for (int i = 0; i < tsrfIntenRes[q].GetNumberOfVisibleBelts(); i++)
                {
                    for (int j = 0; j < tsrfIntenRes[q].patch[i].Length; j++)
                    {
                        tsrfIntenRes[q].teff[i][j] = (x[k] * aMas[q] + bMas[q]) /* scale[q]*/;
                        k++;
                    }
                }
            }

            this.lcsMod = new LightCurve[filterCount];

            double[] phases = new double[100];
            for (int p = 0; p < phases.Length; p++)
            {
                phases[p] = p / 100.0;
            }

            for (int q = 0; q < this.lcsMod.Length; q++)
            {
                LCGenerator lcg = new LCGenerator(tsrfIntenRes[q], this.xLDC[q], 0.0);
                this.lcsMod[q] = new LightCurve();
                this.lcsMod[q].Phases = phases;
                this.lcsMod[q].Sigma = 0.0;
                this.lcsMod[q].Band = this.lcs[q].Band;
                this.lcsMod[q].Fluxes = lcg.GetFluxMasForLinLDForIntSrfForConstLD(phases);
            }

            this.khi2 = 0;
            for (int q = 0; q < filterCount; q++)
            {
                double khiSum = 0;
                LCGenerator lcg = new LCGenerator(tsrfIntenRes[q], this.xLDC[q], 0.0);
                double[] flxs = lcg.GetFluxMasForLinLDForIntSrfForConstLD(this.lcs[q].Phases);
                for (int p = 0; p < this.lcs[q].Phases.Length; p++)
                {
                    khiSum += Math.Pow(flxs[p] - this.lcs[q].Fluxes[p], 2);
                }
                this.khi2 += khiSum / Math.Pow(this.lcs[q].Sigma, 2);
            }
            return tsrfIntenRes;
        }

        public double Khi2
        {
            get { return this.khi2; }
        }

        public LightCurve[] ModelLightCurves
        {
            get { return this.lcsMod; }
        }


        public TSurface ReconstrEND2(double lambda, double tph, double pow)
        {
            double sqrtLambda = Math.Sqrt(lambda);
            int filterCount = this.lcMas.Length;
            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();

            // Определение и инициализация массива средних наблюдаемых потоков;
            double[] fluxAve = new double[this.lcMas.Length];
            for (int q = 0; q < fluxAve.Length; q++)
            {
                double sum = 0;
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    sum = sum + this.lcMas[q].FMas[p];
                }
                fluxAve[q] = sum / (double)this.lcMas[q].XMas.Length;
            }

            // Initialization of reponse matrix;
            double[][][] H = new double[filterCount][][];
            for (int q = 0; q < filterCount; q++)
            {
                H[q] = new double[this.lcMas[q].XMas.Length][];
                for (int p = 0; p < H[q].Length; p++)
                {
                    H[q][p] = new double[vpn];
                }
            }

            // Полное количество измерений интенсивности во всех фильтрах
            int lcPointsNumber = 0;
            for (int q = 0; q < lcMas.Length; q++)
            {
                lcPointsNumber = lcPointsNumber + lcMas[q].XMas.Length;
            }


            double[] I = new double[lcPointsNumber];
            int k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    I[k] = this.lcMas[q].FMas[p] / sigma[q];
                    k++;
                }
            }

            // Инициализация матриц H;
            k = 0;
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                for (int p = 0; p < lcMas[q].XMas.Length; p++)
                {
                    k = 0;
                    for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
                    {
                        int L = this.tsrf.patch[i].Length;
                        for (int j = 0; j < L; j++)
                        {
                            H[q][p][k] = (1.0 / this.sigma[q]) * this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                                (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                                this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc());
                            //H[q].SetElement(p, k, this.tsrf.patch[i][j].ProjectedArea(this.lcMas[q].XMas[p], this.tsrf.GetInc()) *
                            //    (1 - this.xLDC[q] * (1 - this.tsrf.patch[i][j].Mu(this.lcMas[q].XMas[p], this.tsrf.GetInc()))) *
                            //    this.tsrf.patch[i][j].Visible(this.lcMas[q].XMas[p], this.tsrf.GetInc()));
                            k++;
                        }
                    }
                }
            }

            // Инициализация вектора x;
            double[] x = new double[vpn + this.lcMas.Length];
            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    x[k] = 5000;
                    k++;
                }
            }

            for (int q = 0; q < this.lcMas.Length; q++)
            {
                double flux = this.spInt[q].Interp(this.tsrf.teff[0][0]) *
                    (Math.PI * (1 - this.xLDC[q] / 3.0));
                x[q + k] = fluxAve[q] / flux;
            }

            double[][] jacobyTr = new double[vpn + filterCount][];
            for (int i = 0; i < jacobyTr.Length; i++)
            {
                jacobyTr[i] = new double[lcPointsNumber + vpn];
            }

            double[] f = new double[lcPointsNumber + vpn];

            double[] b = new double[vpn + filterCount];

            // The Hessian matrix;
            double[][] hessian = new double[vpn + filterCount][];
            for (int i = 0; i < hessian.Length; i++)
            {
                hessian[i] = new double[vpn + filterCount];
            }

            // The step-vector for Gauss-Newton minimization algorithm;
            double[] h = new double[vpn + filterCount];

            double[] x0 = new double[vpn + filterCount];

            // The vector of first derivatives of normal specific intensities with respect to temperature;
            double[][] JD = new double[this.lcMas.Length][];
            for (int q = 0; q < this.lcMas.Length; q++)
            {
                JD[q] = new double[vpn];
            }

            double norm2;

            int iter2 = 0;
            int iter1 = 0;

            double[] c = new double[vpn];
            for (int i = 0; i < vpn; i++) c[i] = 1.0;

            // Wir sind geboren Taten zu volbringen,
            // zu uberwind ...
            do
            {
                if (iter1 != 0)
                {
                    for (int i = 0; i < vpn; i++)
                    {
                        if (x[i] >= 5000) c[i] = 1.0;
                        if (x[i] <= 5000 && x[i] > 4000) c[i] = (1 - pow) * (x[i] - 4000) / 1000 + pow;
                        if (x[i] < 4000) c[i] = pow;
                    }
                }
                do
                {
                    for (int i = 0; i < vpn + filterCount; i++) x0[i] = x[i];

                    // Вычисление вектора f;
                    k = 0;
                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                        {
                            double sum = 0;
                            for (int i = 0; i < vpn; i++)
                            {
                                sum = sum + H[q][p][i] * this.spInt[q].Interp(x[i]);
                            }
                            f[k] = x[vpn + q] * sum - I[k];
                            k++;
                        }
                    }

                    for (int i = 0; i < vpn; i++)
                    {
                        //f[i + k] = c[i] * sqrtLambda * (x[i] - teffAve);
                        f[i + k] = sqrtLambda * c[i] * (x[i] - 5000);
                    }

                    // Вычисление производных норм.уд.интенсивностей по температуре;

                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int i = 0; i < vpn; i++)
                        {
                            JD[q][i] = this.spInt[q].InterpD1(x[i]);
                        }
                    }

                    // Вычисление компонент матрицы Якоби;
                    k = 0;
                    for (int q = 0; q < filterCount; q++)
                    {
                        for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                        {
                            for (int i = 0; i < vpn; i++)
                            {
                                jacobyTr[i][k] = x[vpn + q] * H[q][p][i] * JD[q][i];
                            }

                            double sum = 0;
                            for (int i = 0; i < vpn; i++)
                            {
                                sum = sum + H[q][p][i] * this.spInt[q].Interp(x[i]);
                            }
                            jacobyTr[vpn + q][k] = sum;
                            k++;
                        }
                    }
                    for (int i = 0; i < vpn; i++)
                    {
                        jacobyTr[i][k + i] = sqrtLambda * c[i];
                    }

                    // Computing of the Hessian matrix;
                    Parallel.For(0, vpn + filterCount, i =>
                    /*for (int i = 0; i < vpn + filterCount; i++)*/
                    {
                        double sum;
                        for (int j = i; j < vpn + filterCount; j++)
                        {
                            sum = 0;
                            for (int l = 0; l < lcPointsNumber /*+ vpn*/; l++)
                            {
                                sum = sum + jacobyTr[i][l] * jacobyTr[j][l];
                            }
                            if (i < vpn)
                            {
                                if (i == j) sum = sum + Math.Pow(jacobyTr[i][i + lcPointsNumber], 2);
                            }
                            hessian[i][j] = sum;
                            hessian[j][i] = sum;
                        }
                    });


                    //
                    //double aa = hessian[0][0];
                    //for (int i = 0; i < hessian.Length; i++)
                    //{
                    //    if (hessian[i][i] > aa) aa = hessian[i][i];
                    //}
                    //double mu = 0.000000000000000000001 * aa;
                    //for (int i = 0; i < hessian.Length; i++)
                    //{
                    //    hessian[i][i] = hessian[i][i] + mu;
                    //}

                    MathLib.Basic.AMultB(ref jacobyTr, ref f, ref b);

                    MathLib.Basic.VAMultSC(ref b, -1.0);

                    //h=MathLib.LES_Solver.SolveWithGaussMethod(hessian, b);

                    MathLib.LES_Solver.ConvGradMethodPL(ref hessian, ref b, ref h, 0.00000001);

                    MathLib.Basic.VAplusVB(ref x0, ref h, ref x);

                    norm2 = 0;
                    for (int i = 0; i < vpn; i++)
                    {
                        norm2 = norm2 + Math.Pow(h[i], 2);
                    }
                    norm2 = Math.Sqrt(norm2);

                    iter2++;
                } while (norm2 > vpn * 0.01);

                iter1++;

            } while (iter1 < 10);

            this.fluxes = new double[filterCount][];
            for (int q = 0; q < filterCount; q++)
            {
                this.fluxes[q] = new double[100];
            }

            for (int q = 0; q < filterCount; q++)                  // Be carefull, H is multiplied to sigma!
            {
                for (int p = 0; p < this.lcMas[q].XMas.Length; p++)
                {
                    double sum = 0;
                    for (int j = 0; j < vpn; j++)
                    {
                        sum = sum + x[vpn + q] * H[q][p][j] * sigma[q] * this.spInt[q].Interp(x[j]);
                    }
                    fluxes[q][p] = sum;
                }
            }

            k = 0;
            for (int i = 0; i < this.tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                {
                    this.tsrf.teff[i][j] = x[k];
                    k++;
                }
            }

            return this.tsrf;
        }
    }
}