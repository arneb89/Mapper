using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Maper.StokesImaging
{
    public class StokesModeller
    {
        private Stokes_B_Theta_Lambda stokesI_b_theta_lambda;
        private Stokes_B_Theta_Lambda stokesV_b_theta_lambda;
        private Stokes_B_Theta_Lambda stokesQ_b_theta_lambda;
        private Stokes_B_Theta_Lambda stokesU_b_theta_lambda;

        private MagnetizedSurface magSrf = null;

        // The arrays contained stokes parameters;
        private double[] stokesI = null;
        private double[] stokesV = null;
        private double[] stokesQ = null;
        private double[] stokesU = null;
        //private double[] prAreaArray = null;

        // The arrays contained phases for stokes curves;
        private double[] phasesI = null;
        private double[] phasesV = null;
        private double[] phasesQ = null;
        private double[] phasesU = null;

        private string errorString = "";

        public StokesModeller(Stokes_B_Theta_Lambda stokesI_b_theta_lambda,
            Stokes_B_Theta_Lambda stokesV_b_theta_lambda,
            Stokes_B_Theta_Lambda stokesQ_b_theta_lambda,
            Stokes_B_Theta_Lambda stokesU_b_theta_lambda,
            MagnetizedSurface magSrf)
        {
            this.stokesI_b_theta_lambda = stokesI_b_theta_lambda;
            this.stokesV_b_theta_lambda = stokesV_b_theta_lambda;
            this.stokesQ_b_theta_lambda = stokesQ_b_theta_lambda;
            this.stokesU_b_theta_lambda = stokesU_b_theta_lambda;
            this.magSrf = magSrf;
        }

        public void StartStokesCurvesModelling(double[] phases, double scale, double poleOptDepth)
        {
            this.stokesI = new double[phases.Length];
            this.stokesQ = new double[phases.Length];
            this.stokesV = new double[phases.Length];
            this.stokesU = new double[phases.Length];

            double[] test_theta = new double[phases.Length];
            double[] test_koeff = new double[phases.Length];

            this.phasesI = phases;
            this.phasesQ = phases;
            this.phasesV = phases;
            this.phasesU = phases;

            //Parallel.For(0, phases.Length, p =>
            for (int p = 0; p < phases.Length; p++)
            {
                double muPole;
                double sinGammaCenter;
                double sinKhi, cosKhi;

                double sumI = 0;
                double sumV = 0;
                double sumQ = 0;
                double sumU = 0;

                muPole = this.magSrf.MuOfThePole(phases[p]);
                sinGammaCenter = Math.Sqrt(1 - muPole * muPole);
                if (sinGammaCenter > 1.0) sinGammaCenter = 1.0;
                if (sinGammaCenter < -1.0) sinGammaCenter = -1.0;

                cosKhi = this.magSrf.GetCosOmega(phases[p], muPole);

                double eps = this.magSrf.PhiOfThePole(phases[p])
                    - 2 * Math.PI * Math.Floor(this.magSrf.PhiOfThePole(phases[p]) / (2 * Math.PI));

                if (eps < 0.5*Math.PI  || eps > 1.5 * Math.PI)
                {
                    sinKhi = -Math.Sqrt(1 - cosKhi * cosKhi);
                }
                else
                {
                    sinKhi = Math.Sqrt(1 - cosKhi * cosKhi);
                }

                for (int i = 0; i < this.magSrf.Patches.Length; i++)
                {
                    // areas of patches in the same belt are equil
                    double area = (this.magSrf.Patches[i][0].Phi20 - this.magSrf.Patches[i][0].Phi10) *
                                (Math.Cos(this.magSrf.Patches[i][0].Theta1) - Math.Cos(this.magSrf.Patches[i][0].Theta2));
                    for (int j = 0; j < this.magSrf.Patches[i].Length; j++)
                    {
                        if (this.magSrf.BrightnessDensity[i][j] != 0)
                        {
                            double mu = magSrf.MuOfThePatchCenter(i, j, muPole, sinGammaCenter, cosKhi, sinKhi);
                            if (mu >= 0)
                            {
                                double cosAlpha = magSrf.GetCosAlpha(i, j, muPole, sinGammaCenter, cosKhi, sinKhi);
                                //double cosAlpha1 = magSrf.GetCosAlpha2(i, j, phases[p]);
                                double alpha = Math.Acos(cosAlpha);

                                test_theta[p] = alpha;

                                double scale1 = this.magSrf.BrightnessDensity[i][j] * area;                                

                                double ro;

                                ro = this.magSrf.GetRo(i, j, phases[p]);

                                double koeff_i = 0, koeff_v = 0, koeff_q = 0, koeff_u = 0;

                                // Stokes I integration;
                                if (alpha > 0.5 * Math.PI)
                                {
                                    koeff_i = this.stokesI_b_theta_lambda(
                                        this.magSrf.MagneticStrength(i, j) * 1e-6,
                                        Math.PI - alpha,
                                        this.magSrf.Lambda(i, j, poleOptDepth));
                                }
                                else
                                {
                                    koeff_i = this.stokesI_b_theta_lambda(
                                        this.magSrf.MagneticStrength(i, j) * 1e-6,
                                        alpha,
                                        this.magSrf.Lambda(i, j, poleOptDepth));
                                }
                                if (koeff_i < 0)
                                {
                                    koeff_i = 0;
                                }
                                sumI += koeff_i * scale1 * mu;


                                // Stokes V integration;
                                if (alpha > 0.5 * Math.PI)
                                {
                                    koeff_v =  -this.stokesV_b_theta_lambda(
                                         this.magSrf.MagneticStrength(i, j) * 1e-6,
                                         Math.PI - alpha,
                                         this.magSrf.Lambda(i, j, poleOptDepth));
                                }
                                else
                                {
                                    koeff_v = this.stokesV_b_theta_lambda(
                                        this.magSrf.MagneticStrength(i, j) * 1e-6,
                                         alpha,
                                        this.magSrf.Lambda(i, j, poleOptDepth));
                                }
                                sumV += koeff_v * scale1 * mu;
                                test_koeff[p] = koeff_v;

                                // Stokes Q integration;
                                if (alpha > 0.5 * Math.PI)
                                {
                                    koeff_q = this.stokesQ_b_theta_lambda(
                                        this.magSrf.MagneticStrength(i, j) * 1e-6,
                                        Math.PI - alpha,
                                        this.magSrf.Lambda(i, j, poleOptDepth));
                                }
                                else
                                {
                                    koeff_q = this.stokesQ_b_theta_lambda(
                                        this.magSrf.MagneticStrength(i, j) * 1e-6,
                                        alpha,
                                        this.magSrf.Lambda(i, j, poleOptDepth));
                                }
                                //if (koeff_q < 0) koeff_q = 0;
                                sumQ += Math.Cos(2 * ro) * koeff_q * scale1 * mu;

                                // Stokes U integration;
                                if (alpha > 0.5 * Math.PI)
                                {
                                    koeff_u = this.stokesQ_b_theta_lambda(
                                        this.magSrf.MagneticStrength(i, j) * 1e-6,
                                        Math.PI - alpha,
                                        this.magSrf.Lambda(i, j, poleOptDepth));
                                }
                                else
                                {
                                    koeff_u = this.stokesQ_b_theta_lambda(
                                        this.magSrf.MagneticStrength(i, j) * 1e-6,
                                        alpha,
                                        this.magSrf.Lambda(i, j, poleOptDepth));
                                }
                                //if (koeff_i < 0) koeff_i = 0;
                                sumU += -Math.Sin(2 * ro) * koeff_u * scale1 * mu;
                            }
                        }
                    }
                }

                //this.prAreaArray[p] = 1;

                this.stokesI[p] = sumI;
                this.stokesV[p] = sumV;
                this.stokesQ[p] = sumQ;
                this.stokesU[p] = sumU;

                this.stokesI[p] = stokesI[p] * scale;
                this.stokesV[p] = stokesV[p] * scale;
                this.stokesQ[p] = stokesQ[p] * scale;
                this.stokesU[p] = stokesU[p] * scale;
            }//);

            StreamWriter sw = new StreamWriter("D:\\stokes_modeller_out.dat");
            for (int i = 0; i < phases.Length; i++)
            {
                sw.WriteLine("{0:0.000}\t{1:0.000E000}", test_theta[i], test_koeff[i]);
                   
            }
            sw.Close();
            

        }

        public double[] StokesI
        {
            get { return this.stokesI; }
        }

        public double[] StokesV
        {
            get { return this.stokesV; }
        }

        public double[] StokesQ
        {
            get { return this.stokesQ; }
        }

        public double[] StokesU
        {
            get { return this.stokesU; }
        }

        public string ErrorString
        {
            get
            {
                if (this.errorString == "")
                    return "No problems, be happy!!!";
                return this.errorString;
            }
        }

        public double[] PhasesI
        {
            get { return this.phasesI; }
        }

        public double[] PhasesQ
        {
            get { return this.phasesQ; }
        }

        public double[] PhasesV
        {
            get { return this.phasesV; }
        }

        public double[] PhasesU
        {
            get { return this.phasesU; }
        }
    }
}
