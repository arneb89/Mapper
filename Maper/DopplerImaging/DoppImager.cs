using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLib;
using MathLib.Optimization;
using System.Threading.Tasks;
using System.Threading;


namespace Maper
{
    /// <summary>
    /// Class for filling factor imaging of spotted stars.
    /// </summary>
    class DoppImager
    {
        private Surface srf = null;
        private double[][] phIntLineGrid, spIntLineGrid, phIntContGrid, spIntContGrid, obsSpec, masObsLambda;
        private double[] masMu, masLambda, masObsPhase;
        
        private double[][] calcSpec;
        private TSurface calcSrf;

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="srf">
        /// init surface;</param>
        /// <param name="phIntLineGrid">
        /// grid of photosphere spectra with lines;</param>
        /// <param name="spIntLineGrid">
        /// grid of spot spectra with lines;</param>
        /// <param name="phIntContGrid">
        /// grid of photosphere spectra without lines;</param>
        /// <param name="spIntContGrid">
        /// grid of spot spectra without lines;</param>
        /// <param name="masMu">
        /// array of mu values (mu = cos(gamma), where gamma --- angle between 
        /// normal to the surface and line of sight);</param>
        /// <param name="masLambda">
        /// array of values of wavelengths for witch model spectra was calculated;</param>
        /// <param name="obsSpec">
        /// array of observed spectra;</param>
        /// <param name="masObsPhase">
        /// array of values of phases for witch spectra was observed;</param>
        /// <param name="masObsLambda">
        /// array of values of wavelengths for witch spectra was observed;</param>
        public DoppImager(Surface srf, 
            double[][] phIntLineGrid,  
            double[][] spIntLineGrid,
            double[][] phIntContGrid,
            double[][] spIntContGrid,
            double[] masMu,
            double[] masLambda,
            double[][] obsSpec,
            double[] masObsPhase,
            double[][] masObsLambda)
        {
            this.srf = srf;
            this.phIntLineGrid = phIntLineGrid;
            this.spIntLineGrid = spIntLineGrid;
            this.phIntContGrid = phIntContGrid;
            this.spIntContGrid = spIntContGrid;
            this.masMu = masMu;
            this.masLambda = masLambda;
            this.obsSpec = obsSpec;
            this.masObsPhase = masObsPhase;
            this.masObsLambda = masObsLambda;
            this.calcSpec = new double[obsSpec.Length][];
            for (int i = 0; i < obsSpec.Length; i++) this.calcSpec[i] = new double[obsSpec[i].Length];
            this.calcSrf = new TSurface(this.srf.GetN(), this.srf.GetM(), this.srf.GetInc(), 0.0);
        }

        private void ATmultA_PL(ref double[][] aTr, ref double[][] res, int t, int threadNum)
        {
            int n = res.Length;
            int upElNum = (n * n - n) / 2;
            int begin = (upElNum / threadNum) * t;
            int end = upElNum / threadNum * (t + 1);
            if (t == threadNum - 1) end = upElNum;

            int beginRow = 0, beginCol = 0;

            int k = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    if (k == begin)
                    {
                        beginRow = i;
                        beginCol = j;
                        k++;
                    }
                }
            }

            double sum;
            bool stop = false;
            k = 0;
            for (int i = beginRow; i < n; i++)
            {
                for (int j = beginCol; j < n; j++)
                {
                    sum = 0;
                    for (int s = 0; s < aTr[0].Length; s++)
                    {
                        sum = sum + aTr[i][s] * aTr[j][s];
                    }
                    res[i][j] = sum;
                    res[j][i] = sum;
                    k++;
                    if (k == end)
                    {
                        stop = true;
                        break;
                    }
                }
                if (stop) break;
                beginCol = i;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vSinI"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public void DoppImGo(double vSinI, double lambda)
        {
            int vpn = this.srf.GetNumberOfPatchesOfVisibleBelts();

            SpectrumInterpPL siplPhLine = new SpectrumInterpPL(this.phIntLineGrid, masMu, masLambda);

            SpectrumInterpPL siplPhCont = new SpectrumInterpPL(this.phIntContGrid, masMu, masLambda);
            
            SpectrumInterpPL siplSpLine = new SpectrumInterpPL(this.spIntLineGrid, masMu, masLambda);
            
            SpectrumInterpPL siplSpCont = new SpectrumInterpPL(this.spIntContGrid, masMu, masLambda);

            int pldim = 0;
            for (int p = 0; p < this.masObsPhase.Length; p++)
            {
                for (int l = 0; l < this.masObsLambda[p].Length; l++)
                {
                    pldim++;
                }
            }

            double[] bline = new double[pldim];
            double[] bcont = new double[pldim];
            double[][] rline = new double[pldim][];
            double[][] rcont = new double[pldim][];

            for (int i = 0; i < pldim; i++)
            {
                rline[i] = new double[vpn];
                rcont[i] = new double[vpn];
            }

            double vr;
            double deltaLambdaDopp;
            double prArea;
            double sumLine, sumCont;
            int m = 0;
            for (int p = 0; p < this.masObsPhase.Length; p++)
            {
                for (int l = 0; l < this.masObsLambda[p].Length; l++)
                {
                    sumLine = 0;
                    sumCont = 0;
                    for (int i = 0; i < this.srf.GetNumberOfVisibleBelts(); i++)
                    {
                        for (int j = 0; j < this.srf.patch[i].Length; j++)
                        {
                            if (this.srf.patch[i][j].Visible(this.masObsPhase[p], this.srf.GetInc())!=0)
                            {
                                vr = vSinI * Math.Sin(this.srf.patch[i][j].ThetaCenterOnStart()) *
                                    Math.Sin(this.srf.patch[i][j].FiCenterOnStart() + 2 * Math.PI * this.masObsPhase[p] - 0.5 * Math.PI);
                                deltaLambdaDopp = this.masObsLambda[p][l] * vr / 300000;
                                prArea = this.srf.patch[i][j].ProjectedArea(this.masObsPhase[p], this.srf.GetInc());
                                sumLine = sumLine + prArea * siplPhLine.Interp(
                                    this.srf.patch[i][j].Mu(this.masObsPhase[p],
                                    this.srf.GetInc()),
                                    this.masObsLambda[p][l] - deltaLambdaDopp);
                                sumCont = sumCont + prArea * siplPhCont.Interp(
                                    this.srf.patch[i][j].Mu(this.masObsPhase[p],
                                    this.srf.GetInc()),
                                    this.masObsLambda[p][l]);   
                            }
                        }
                    }
                    bline[m] = sumLine;
                    bcont[m] = sumCont;
                    m++;
                }
            }

            int k;
            m = 0;
            for (int p = 0; p < this.masObsPhase.Length; p++)
            {
                for (int l = 0; l < this.masObsLambda[p].Length; l++)
                {
                    k = 0;
                    for (int i = 0; i < this.srf.GetNumberOfVisibleBelts(); i++)
                    {
                        for (int j = 0; j < this.srf.patch[i].Length; j++)
                        {
                            if (this.srf.patch[i][j].Visible(this.masObsPhase[p], this.srf.GetInc()) != 0)
                            {
                                vr = vSinI * Math.Sin(this.srf.patch[i][j].ThetaCenterOnStart()) *
                                    Math.Sin(this.srf.patch[i][j].FiCenterOnStart() + 2 * Math.PI * this.masObsPhase[p] - 0.5 * Math.PI);
                                deltaLambdaDopp = this.masObsLambda[p][l] * vr / 300000;
                                prArea = this.srf.patch[i][j].ProjectedArea(this.masObsPhase[p], this.srf.GetInc());
                                rline[m][k] = prArea * (siplSpLine.Interp(
                                    this.srf.patch[i][j].Mu(this.masObsPhase[p], this.srf.GetInc()),
                                    this.masObsLambda[p][l] - deltaLambdaDopp)-
                                    siplPhLine.Interp(
                                    this.srf.patch[i][j].Mu(this.masObsPhase[p], this.srf.GetInc()),
                                    this.masObsLambda[p][l] - deltaLambdaDopp));
                                rcont[m][k] = prArea * ( 
                                    siplSpCont.Interp(
                                    this.srf.patch[i][j].Mu(this.masObsPhase[p], this.srf.GetInc()),
                                    this.masObsLambda[p][l])-
                                    siplPhCont.Interp(
                                    this.srf.patch[i][j].Mu(this.masObsPhase[p], this.srf.GetInc()),
                                    this.masObsLambda[p][l]));
                            }
                            k++;
                        }
                    }
                    m++;
                }
            }

            double[] obsSpec1 = new double[pldim];
            
            k = 0;
            for (int i = 0; i < obsSpec.Length; i++)
            {
                for (int j = 0; j < obsSpec[i].Length; j++)
                {
                    obsSpec1[k] = obsSpec[i][j];
                    k++;
                }
            }

            GNHelperForDI gnhdi = new GNHelperForDI();

            double[] x0 = new double[vpn];
            
            double[] x = new double[vpn];

            double[][] jacobyTr = new double[vpn][];
            
            for (int i = 0; i < vpn; i++) jacobyTr[i] = new double[pldim+vpn];

            double[][] hessian = new double[vpn][];
            
            for (int i = 0; i < vpn; i++) hessian[i] = new double[vpn];

            double[] b = new double[vpn];

            double[] f = new double[pldim + vpn];
            
            double[] h = new double[vpn];

            for (int i = 0; i < vpn; i++) x0[i] = 0.0;
            
            for (int i = 0; i < vpn; i++) x[i] = x0[i];

            double sqrtLambda = Math.Sqrt(lambda);
            
            /******************************************************************/
            /********************* Gauss-Newton cycle; ************************/
            /******************************************************************/

            int iter = 0;
            int iterMax = 100;
            int threadNum = 2;
            double norm2=0;
            
            do
            {
                for (int i = 0; i < vpn; i++) x0[i] = x[i];

                // Initialization of the transponse Jacoby matrix;
                Parallel.For(0, threadNum, t =>
                {
                    gnhdi.GetJacobyTrPL(ref rline,
                        ref rcont,
                        ref bline,
                        ref bcont,
                        ref jacobyTr,
                        ref x0,
                        sqrtLambda,
                        pldim,
                        vpn,
                        t,
                        threadNum
                        );
                });

                Parallel.For(0, threadNum, t =>
                {
                    gnhdi.GetVectorFunctionPL(ref rline,
                        ref rcont,
                        ref bline,
                        ref bcont,
                        ref obsSpec1,
                        ref x0,
                        ref f,
                        sqrtLambda,
                        pldim,
                        vpn,
                        t,
                        threadNum
                        );
                });

                // Initialization of the Hessian matrix;
                Parallel.For(0, vpn, i =>
                {
                    double sum;
                    for (int j = i; j < vpn; j++)
                    {
                        sum = 0;
                        for (int l = 0; l < pldim /*+ vpn*/; l++)
                        {
                            sum = sum + jacobyTr[i][l] * jacobyTr[j][l];
                        }
                        if (i == j) sum = sum + Math.Pow(jacobyTr[i][i + pldim], 2);
                        hessian[i][j] = sum;
                        hessian[j][i] = sum;
                    }
                });

                MathLib.Basic.AMultB(ref jacobyTr, ref f, ref b);

                MathLib.Basic.VAMultSC(ref b, -1.0);

                MathLib.LES_Solver.ConvGradMethodPL(ref hessian, ref b, ref h, 0.0000000001);

                MathLib.Basic.VAplusVB(ref x0, ref h, ref x);

                norm2 = 0;
                for (int i = 0; i < vpn; i++)
                {
                    norm2 = norm2 + Math.Pow(1.0 / (1.0 - Math.Exp(-x[i])) 
                        - 1.0 / (1.0 - Math.Exp(-x0[i])),2);
                }
                norm2 = Math.Sqrt(norm2);

                iter++;
                if (iter == iterMax) break;
            } while (norm2 > vpn * 0.01);

            /************************************************************************/
            /********************* End of Gauss-Newton cycle;************************/
            /************************************************************************/

            
            k = 0;
            for (int i = 0; i < srf.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < srf.patch[i].Length; j++)
                {
                    this.calcSrf.teff[i][j] = 1.0 / (1.0 + Math.Exp(-x[k]));
                    k++;
                }
            }

            double suml, sumc;
            k = 0;
            for (int i = 0; i < this.calcSpec.Length; i++)
            {
                for (int j = 0; j < this.calcSpec[i].Length; j++)
                {
                    suml = 0;
                    for (int s = 0; s < vpn; s++)
                    {
                        suml = suml + rline[k][s] * 1.0 / (1.0 + Math.Exp(-x[s]));
                    }
                    sumc = 0;
                    for (int s = 0; s < vpn; s++)
                    {
                        sumc = sumc + rcont[k][s] * 1.0 / (1.0 + Math.Exp(-x[s]));
                    }
                    this.calcSpec[i][j] = (suml + bline[k]) / (sumc + bcont[k]);
                    k++;
                }
            }
        }

        /// <summary>
        /// Gets the model spectrum set;
        /// </summary>
        public double[][] ModelSpectrumGrid
        {
            get
            {
                return this.calcSpec;
            }
        }

        /// <summary>
        /// Gets the restored surface;
        /// </summary>
        public TSurface RestoredSurface
        {
            get
            {
                return this.calcSrf;
            }
        }
    }
}