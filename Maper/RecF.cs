using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper
{
    class RecF
    {
        private TSurface tsrf = null;
        private double[] intenPhot;
        private double[] intenSpot;
        private LightCurve[] lcsObs = null;
        private LightCurve[] lcsMod = null;
        private double[] xLDCPhot;
        private double[] xLDCSpot;
        private double[] sigmas;
        private double std_dev = 0;

        private TSurface tsrfRes = null;

        public RecF(TSurface tsrf_, LightCurve[] lcsObs_, 
            double[] intenPhot_, double[] intenSpot_, 
            double[] xLDCPhot_, double[] xLDCSpot_,
            double[] sigmas_, double[] flx_max)
        {
            this.tsrf = tsrf_;
            this.lcsObs = lcsObs_;
            this.intenPhot = new double[intenPhot_.Length];
            this.intenSpot = new double[intenSpot_.Length];
            for (int q = 0; q < intenSpot_.Length; q++)
            {
                double ii = flx_max[q] / (Math.PI * (1.0 - xLDCPhot_[q] / 3.0));
                this.intenSpot[q] = intenSpot_[q] * ii / intenPhot_[q];
                this.intenPhot[q] = ii;
            }
            this.xLDCPhot = xLDCPhot_;
            this.xLDCSpot = xLDCSpot_;
            this.sigmas = sigmas_;
            this.std_dev = 0;
            this.lcsMod = null;
            this.tsrfRes = null;
        }

        public void Map(double lambda)
        {
            double sqrtLambda = Math.Sqrt(lambda);

            int filterCount = this.lcsObs.Length;
            // The count of patches that is able to be observed;
            int vpn = this.tsrf.GetNumberOfPatchesOfVisibleBelts();

            // The array of averange observed fluxes;
            double[] fluxAve = new double[filterCount];
            for (int q = 0; q < filterCount; q++)
            {
                double sum = 0;
                for (int p = 0; p < this.lcsObs[q].Phases.Length; p++)
                {
                    sum = sum + this.lcsObs[q].Fluxes[p];
                }
                fluxAve[q] = sum / (double)this.lcsObs[q].Phases.Length;
            }

            // Полное количество измерений интенсивности во всех фильтрах
            int lcPointsNumber = 0;
            for (int q = 0; q < filterCount; q++)
            {
                lcPointsNumber += lcsObs[q].Phases.Length;
            }

            double[][] H = new double[lcPointsNumber][];
            for (int p = 0; p < lcPointsNumber; p++)
            {
                H[p] = new double[vpn];
            }

            double[] B = new double[lcPointsNumber];

            double[] I = new double[lcPointsNumber];
            int k = 0;
            for (int q = 0; q < filterCount; q++)
            {
                for (int p = 0; p < this.lcsObs[q].Phases.Length; p++)
                {
                    I[k] = this.lcsObs[q].Phases[p] / sigmas[q];
                    k++;
                }
            }

            // Инициализация матриц H;
            k = 0;
            int s = 0;
            double ldc_phot = 0;
            double ldc_spot = 0;
            double mu;
            double sigma_inv;
            double pr_area;
            int visib;
            for (int q = 0; q < filterCount; q++)
            {
                sigma_inv = (1.0 / this.sigmas[q]);
                for (int p = 0; p < lcsObs[q].Phases.Length; p++)
                {
                    k = 0;
                    double sum = 0;
                    for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
                    {
                        for (int j = 0; j < this.tsrf.patch[i].Length; j++)
                        {
                            mu = this.tsrf.patch[i][j].Mu(this.lcsObs[q].Phases[p], this.tsrf.GetInc());
                            visib = this.tsrf.patch[i][j].Visible(this.lcsObs[q].Phases[p], this.tsrf.GetInc());
                            ldc_phot = 1.0 - xLDCPhot[q] * (1.0 - mu);
                            ldc_spot = 1.0 - xLDCSpot[q] * (1.0 - mu);
                            pr_area = this.tsrf.patch[i][j].ProjectedArea(this.lcsObs[q].Phases[p], this.tsrf.GetInc());
                            H[s][k] = sigma_inv * visib * pr_area * (ldc_spot * intenSpot[q] - ldc_phot * intenPhot[q]);
                            sum += visib * pr_area * ldc_phot * intenPhot[q];
                            k++;
                        }
                    }
                    B[s] = sigma_inv * sum;
                    s++;
                }
            }

            double[][] A = new double[lcPointsNumber + vpn][];
            for (int i = 0; i < A.Length; i++) A[i] = new double[vpn];
            

            for (int i = 0; i < lcPointsNumber; i++)
                for (int j = 0; j < vpn; j++) A[i][j] = H[i][j];
            
            for (int i = 0; i < vpn; i++)
            {
                A[i + lcPointsNumber][i] = sqrtLambda;
            }

            double[] C = new double[lcPointsNumber + vpn];
            k = 0;
            for (int q = 0; q < filterCount; q++)
            {
                for (int p = 0; p < this.lcsObs[q].Phases.Length; p++)
                {
                    C[k] = lcsObs[q].Fluxes[p] - B[k];
                    k++;
                }
            }

            double[][] ATA = MathLib.Basic.MatrixConstructor(vpn, vpn);
            double[][] AT = new double[vpn][];
            for (int i = 0; i < AT.Length; i++) AT[i] = new double[lcPointsNumber + vpn];
            for (int i = 0; i < AT.Length; i++)
                for (int j = 0; j < AT[i].Length; j++)
                    AT[i][j] = A[j][i];

            double[] x = new double[vpn];
            double[] x0 = new double[vpn];
            double[] ff = new double[vpn];
            double[] ffdx = new double[vpn];
            double[] g = new double[vpn+lcPointsNumber];
            double[] b = new double[vpn];
            double[] h = new double[vpn];
            for (int i = 0; i < x.Length; i++) x[i] = 0.0;

            double[][] JT = new double[vpn][];
            for (int i = 0; i < vpn; i++) JT[i] = new double[lcPointsNumber + vpn];

            double[][] hessian = new double[vpn][];
            for (int i = 0; i < vpn; i++) hessian[i] = new double[vpn];
            double ss = 10.0;
            double norm2;

            double[] ff0 = new double[vpn];
            double[] ff1 = new double[vpn];

            int iter2 = 0;
            do
            {
                for (int i = 0; i < vpn; i++) x0[i] = x[i];

                for (int i = 0; i < vpn; i++) ff[i] = 1.0 / (1.0 + Math.Exp(-x[i]));

                for (int i = 0; i < vpn; i++) ffdx[i] = Math.Pow(ff[i], 2) * Math.Exp(-x[i]);

                // Вычисление вектора g;

                k = 0;
                for (s = 0; s < A.Length; s++)
                {
                    double sum = 0;
                    for (int i = 0; i < A[i].Length; i++)
                    {
                        sum += A[s][i] * ff[i];
                    }
                    g[s] = sum - C[s];
                }

                for (s = 0; s < lcPointsNumber + vpn; s++)
                {
                    for (int i = 0; i < vpn; i++)
                    {
                        JT[i][s] = A[s][i] * ffdx[i];
                    }
                }

                // Computing of the Hessian matrix;
                for (int i = 0; i < vpn; i++)
                {
                    double sum;
                    for (int j = i; j < vpn; j++)
                    {
                        sum = 0;
                        for (int l = 0; l < lcPointsNumber + vpn; l++)
                        {
                            sum = sum + JT[i][l] * JT[j][l];
                        }
                        hessian[i][j] = sum;
                        hessian[j][i] = sum;
                    }
                }

                MathLib.Basic.AMultB(ref JT, ref g, ref b);

                MathLib.Basic.VAMultSC(ref b, -1.0);

                //h=MathLib.LES_Solver.SolveWithGaussMethod(hessian, b);

                MathLib.LES_Solver.ConvGradMethodPL(ref hessian, ref b, ref h, 0.000001);

                MathLib.Basic.VAplusVB(ref x0, ref h, ref x);

                //for (int i = 0; i < x.Length; i++)
                //{
                //    if (x[i] > 1.0) x[i] = 1.0;
                //    if (x[i] < 0.0) x[i] = 0.0;
                //}

                
                for (int i = 0; i < vpn; i++)
                {
                    if (x[i] > 20) x[i] = 10;
                    if (x[i] < -20) x[i] = -10;
                    ff0[i] = 1.0 / (1.0 + Math.Exp(-x0[i]));
                    ff1[i] = 1.0 / (1.0 + Math.Exp(-x[i]));
                }

                norm2 = 0;
                for (int i = 0; i < vpn; i++)
                {
                    norm2 = norm2 + Math.Pow(ff1[i] - ff0[i], 2);
                }
                norm2 = Math.Sqrt(norm2);

                iter2++;
            } while (norm2 > 0.01);

            //MathLib.Basic.AMultB(ref AT, ref A, ref ATA);

            //double[] U = new double[vpn];
            //MathLib.Basic.AMultB(ref AT, ref C, ref U);
            //double[] F = new double[vpn];
            //MathLib.LES_Solver.ConvGradMethodPL(ref ATA, ref U, ref F, 1e-9);

            

            this.tsrfRes = new TSurface(tsrf.GetN(), tsrf.GetM(), tsrf.GetInc(), 0);
            k = 0;
            for (int i = 0; i < tsrf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < tsrf.patch[i].Length; j++)
                {
                    tsrfRes.teff[i][j] = ff1[k];
                    k++;
                }
            }

            double[] phases = new double[100];
            for (int i = 0; i < phases.Length; i++) phases[i] = i * 0.01;
            double[][] flx_mod = new double[filterCount][];
            for (int q = 0; q < flx_mod.Length; q++) flx_mod[q] = new double[phases.Length];
            this.lcsMod = new LightCurve[filterCount];

            for (int q = 0; q < filterCount; q++)
            {
                for (int p = 0; p < flx_mod[q].Length; p++)
                {
                    flx_mod[q][p] = 0.0;
                    for (int i = 0; i < tsrfRes.GetNumberOfVisibleBelts(); i++)
                    {
                        for (int j = 0; j < tsrfRes.patch[i].Length; j++)
                        {
                            mu = this.tsrf.patch[i][j].Mu(phases[p], this.tsrf.GetInc());
                            visib = this.tsrf.patch[i][j].Visible(phases[p], this.tsrf.GetInc());
                            ldc_phot = 1.0 - xLDCPhot[q] * (1.0 - mu);
                            ldc_spot = 1.0 - xLDCSpot[q] * (1.0 - mu);
                            pr_area = this.tsrf.patch[i][j].ProjectedArea(phases[p], this.tsrf.GetInc());
                            flx_mod[q][p] += visib * pr_area * (tsrfRes.teff[i][j] * ldc_spot * intenSpot[q] 
                                + (1 - tsrfRes.teff[i][j]) * ldc_phot * intenPhot[q]);
                        }
                    }
                }
                lcsMod[q] = new LightCurve(phases, flx_mod[q], lcsObs[q].Band, 0);
            }
        }

        public TSurface ResSurface
        {
            get
            {
                return this.tsrfRes;
            }
        }

        public LightCurve[] ModelLigthCurves
        {
            get
            {
                return this.lcsMod;
            }
        }
    }
}
