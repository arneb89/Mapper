using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper.StokesImaging
{
    public class MagnetizedSurface
    {
        private double magStrPole; // magnetic strength at the pole;
        private double[][] magStrength = null;
        private double[][] magLineTheta = null;
        private double[][] brightnessDensity = null;
        private Patch[][] patch = null;
        private double inc, beta, lambda;
        private double cos_inc, sin_inc, cos_beta, sin_beta;

        public MagnetizedSurface(double inc, double beta, double lambda, double magStr, int n, int m)
        {
            this.inc = inc;
            this.cos_inc = Math.Cos(inc);
            this.sin_inc = Math.Sin(inc);
            this.beta = beta;
            this.cos_beta = Math.Cos(beta);
            this.sin_beta = Math.Sin(beta);
            this.lambda = lambda;
            this.magStrPole = magStr;

            this.patch = new Patch[n][];
            this.magStrength = new double[n][];
            this.magLineTheta = new double[n][];
            this.brightnessDensity = new double[n][];
            for (int i = 0; i < n; i++)
            {
                this.patch[i] = new Patch[m];
                this.magStrength[i] = new double[m];
                this.magLineTheta[i] = new double[m];
                this.brightnessDensity[i] = new double[m];
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    this.patch[i][j] = new Patch(j * (2 * Math.PI / m), (j + 1) * (2 * Math.PI / m),
                        i * (Math.PI / n), (i + 1) * (Math.PI / n));
                    this.brightnessDensity[i][j] = 0.0;
                    this.magStrength[i][j] = 0.5 * magStr * Math.Sqrt(3 * Math.Pow(Math.Cos(this.patch[i][j].ThetaCenterOnStart()), 2) + 1);
                    this.magLineTheta[i][j] = (2.0 - 3 * Math.Pow(Math.Sin(this.patch[i][j].ThetaCenterOnStart()), 2))/(3 * Math.Sin(this.patch[i][j].ThetaCenterOnStart()) * Math.Cos(this.patch[i][j].ThetaCenterOnStart()));
                    if (this.patch[i][j].ThetaCenterOnStart() < 0.5 * Math.PI)
                    {
                        this.magLineTheta[i][j] = 0.5 * Math.PI - Math.Atan(this.magLineTheta[i][j]);
                    }
                    else
                    {
                        this.magLineTheta[i][j] = 1.5 * Math.PI - Math.Atan(this.magLineTheta[i][j]);
                    }
                }
            }
        }

        public double PhiOfThePole(double phase)
        {
            return this.lambda + 2 * phase * Math.PI - 
                Math.Floor((this.lambda + 2 * phase * Math.PI) / (2 * Math.PI)) * (2 * Math.PI);
        }

        public double MuOfThePole(double phase)
        {
            double mu = this.sin_beta * Math.Sin(this.PhiOfThePole(phase)) * this.sin_inc +
                this.cos_beta * this.cos_inc;
            if (mu > 1.0) mu = 1.0;
            if (mu < -1.0) mu = -1.0;
            return mu;
        }

        public double MagneticStrength(int i, int j)
        {
            return this.magStrength[i][j];
        }

        public double GetCosOmega(double phase, double muCenter)
        {
            double phiCenter = this.PhiOfThePole(phase);
            if (muCenter == 1.0 || muCenter == -1.0) return 1.0;
            if (this.beta == 0)
            {
                if (phiCenter <= 0.5 * Math.PI)
                    return Math.Cos(0.5 * Math.PI + phiCenter);
                if (phiCenter < 1.5 * Math.PI && phiCenter >= 0.5 * Math.PI)
                    return Math.Cos(1.5 * Math.PI - phiCenter);
                if (phiCenter <= 2 * Math.PI && phiCenter >= 1.5 * Math.PI)
                    return Math.Cos(phiCenter - 1.5 * Math.PI);
            }
            if (this.beta == Math.PI) return 1.0;

            double sinGammaCenter = Math.Sqrt(1.0 - muCenter * muCenter);
            
            double cosOmega;

            cosOmega = (Math.Cos(this.inc) - Math.Cos(this.beta) * muCenter) /
                (Math.Sin(this.beta) * sinGammaCenter);
            if (cosOmega > 1.0) cosOmega = 1.0;
            if (cosOmega < -1.0) cosOmega = -1.0;
            return cosOmega;
        }

        public double MuOfThePatchCenter(int i, int j, double muCenter, double sinGammaCenter, double cosOmega, double sinOmega)
        {
            double sinThetaCenter = Math.Sin(this.patch[i][j].ThetaCenterOnStart());
            double cosThetaCenter = Math.Cos(this.patch[i][j].ThetaCenterOnStart());
            double sinPhiCenter = Math.Sin(this.patch[i][j].FiCenterOnStart());
            double cosPhiCenter = Math.Cos(this.patch[i][j].FiCenterOnStart());
            double mu;

            mu = muCenter * cosThetaCenter + sinGammaCenter * sinThetaCenter *
                (cosPhiCenter * cosOmega + sinPhiCenter * sinOmega);

            if (mu > 1.0) mu = 1.0;
            if (mu < -1.0) mu = -1.0;

            return mu;
        }

        public double ProjectedAreaOfThePatch(int i, int j, double muCenter, double sinGammaCenter, double cosOmega, double sinOmega)
        {
            double term1;
            double term2;
            double area;

            double phi1 = this.patch[i][j].Phi10;
            double phi2 = this.patch[i][j].Phi20;
            double theta1 = this.patch[i][j].Theta1;
            double theta2 = this.patch[i][j].Theta2;

            double sin_theta1 = Math.Sin(theta1);
            double sin_theta2 = Math.Sin(theta2);
            double cos_theta1 = Math.Cos(theta1);
            double cos_theta2 = Math.Cos(theta2);

            double sin_phi1 = Math.Sin(phi1);
            double sin_phi2 = Math.Sin(phi2);
            double cos_phi1 = Math.Cos(phi1);
            double cos_phi2 = Math.Cos(phi2);

            term1 = muCenter * (phi2 - phi1) *
                (Math.Pow(sin_theta2, 2) - Math.Pow(sin_theta1, 2));

            //term2 = sinGammaCenter * (Math.Sin(phi2 - omega) - Math.Sin(phi1 - omega)) *
            //     ((theta2 - theta1) - (sin_theta2 * cos_theta2 - sin_theta1 * cos_theta1));

            term2 = sinGammaCenter * (cosOmega * (sin_phi2 - sin_phi1) + sinOmega * (cos_phi1 - cos_phi2)) *
                 ((theta2 - theta1) - (sin_theta2 * cos_theta2 - sin_theta1 * cos_theta1));

            area = 0.5 * (term1 + term2);

            return area;
        }
        
        public double GetCosAlpha(int i, int j, double muPole, double sinGammaPole, double cosOmega, double sinOmega)
        {
            double theta, phi;
            theta = this.magLineTheta[i][j];
            phi=this.patch[i][j].FiCenterOnStart();
            if (theta > Math.PI)
            {
                theta = 2 * Math.PI - theta;
                if (phi <= Math.PI) phi = phi + Math.PI;
                else phi = phi - Math.PI;
            }

            double sinThetaCenter = Math.Sin(theta);
            double cosThetaCenter = Math.Cos(theta);
            double sinPhiCenter = Math.Sin(phi);
            double cosPhiCenter = Math.Cos(phi);
            double mu;

            mu = muPole * cosThetaCenter + sinGammaPole * sinThetaCenter *
                (cosPhiCenter * cosOmega + sinPhiCenter * sinOmega);

            if (mu > 1.0) mu = 1.0;
            if (mu < -1.0) mu = -1.0;

            return mu;
        }

        public double Lambda(int i, int j, double lambdaPole)
        {
            double magStr = this.magStrength[i][j];
            return lambdaPole * this.magStrPole / magStr;
        }

        public double GetRo(int i, int j, double phase)
        {
            double zeta = this.patch[i][i].ThetaCenterOnStart();
            double eta = this.patch[i][j].FiCenterOnStart();
            double cos_zeta = Math.Cos(zeta);
            double sin_zeta = Math.Sin(zeta);
            double cos_eta = Math.Cos(eta);
            double sin_eta = Math.Sin(eta);
            double alpha = Math.PI * 0.5 - this.inc;
            double phi = this.lambda + 2 * Math.PI * phase - Math.PI * 0.5;
            double cos_alpha = Math.Cos(alpha);
            double sin_alpha = Math.Sin(alpha);
            double cos_phi = Math.Cos(phi);
            double sin_phi = Math.Sin(phi);

            double x111, y111, z111;
            x111 = -3 * Math.Pow(sin_zeta, 2) * cos_zeta * cos_eta;
            y111 = -3 * Math.Pow(sin_zeta, 2) * cos_zeta * sin_eta;
            z111 = 2 * sin_zeta * Math.Pow(cos_zeta, 2) - Math.Pow(sin_zeta, 3);

            double y, z;

            y = (cos_beta * sin_phi) * x111 +
                (cos_phi) * y111 +
                (sin_beta * sin_phi) * z111;

            z = (-cos_beta * cos_phi * sin_alpha - sin_beta * cos_alpha) * x111 +
                (sin_phi * sin_alpha) * y111 +
                (-cos_phi * sin_alpha * sin_beta + cos_beta * cos_alpha) * z111;

            if (y == 0) y = 1e-30;

            double tan_angle = y / z;
            double angle = 0;

            if (z > 0 && y >= 0) angle = Math.Atan(tan_angle);
            if (z > 0 && y < 0) angle = /*2 * Math.PI +*/ Math.Atan(tan_angle);
            if (z < 0 && y >= 0) angle =  Math.PI + Math.Atan(tan_angle);
            if (z < 0 && y < 0) angle = Math.PI + Math.Atan(tan_angle);

            return angle;
        }

        public double GetCosAlpha2(int i, int j, double phase)
        {
            double zeta = this.patch[i][i].ThetaCenterOnStart();
            double eta = this.patch[i][j].FiCenterOnStart();
            double cos_zeta = Math.Cos(zeta);
            double sin_zeta = Math.Sin(zeta);
            double cos_eta = Math.Cos(eta);
            double sin_eta = Math.Sin(eta);
            double alpha = Math.PI * 0.5 - this.inc;
            double phi = this.lambda + 2 * Math.PI * phase - Math.PI * 0.5;
            double cos_alpha = Math.Cos(alpha);
            double sin_alpha = Math.Sin(alpha);
            double cos_phi = Math.Cos(phi);
            double sin_phi = Math.Sin(phi);

            double x111, y111, z111;
            x111 = -3 * Math.Pow(sin_zeta, 2) * cos_zeta * cos_eta;
            y111 = -3 * Math.Pow(sin_zeta, 2) * cos_zeta * sin_eta;
            z111 = 2 * sin_zeta * Math.Pow(cos_zeta, 2) - Math.Pow(sin_zeta, 3);

            double x, y, z;

            x = (cos_beta * cos_phi * cos_alpha - sin_beta * sin_alpha) * x111 +
                (-sin_phi * cos_alpha) * y111 +
                (sin_beta * cos_phi * cos_alpha + cos_beta * sin_alpha) * z111;

            y = (cos_beta * sin_phi) * x111 +
                (cos_phi) * y111 +
                (sin_beta * sin_phi) * z111;

            z = (-cos_beta * cos_phi * sin_alpha - sin_beta * cos_alpha) * x111 +
                (sin_phi * sin_alpha) * y111 +
                (-cos_phi * sin_alpha * sin_beta + cos_beta * cos_alpha) * z111;

            double cos_angle = x / Math.Sqrt(x * x + y * y + z * z);

            if (cos_angle > 1.0) cos_angle = 1.0;
            if (cos_angle < -1.0) cos_angle = -1.0;

            return cos_angle; 
        }

        public int GetNumberOfObservablePatches()
        {
            double thetaMax = this.inc + this.beta + 0.5 * Math.PI;
            int count = 0;
            for (int i = 0; i < this.patch.Length; i++)
            {
                if (this.patch[i][0].ThetaCenterOnStart() <= thetaMax)
                {
                    count += this.patch[i].Length;
                }
                else
                {
                    break;
                }
            }
            return count;
        }

        public int GetNumberOfObservableLatBelts()
        {
            double thetaMax = this.inc + this.beta + 0.5 * Math.PI;
            int count = 0;
            for (int i = 0; i < this.patch.Length; i++)
            {
                if (this.patch[i][0].ThetaCenterOnStart() <= thetaMax)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            return count;
        }

        public double[][] GetPatchCoordMas()
        {
            int patchNum = this.patch.Length * this.patch[0].Length;
            double[][] mas = new double[patchNum][];
            for (int i = 0; i < patchNum; i++)
            {
                mas[i] = new double[4];
            }

            int k = 0;
            for (int i = 0; i < patch.Length; i++)
            {
                for (int j = 0; j < patch[i].Length; j++)
                {
                    mas[k][0] = patch[i][j].Theta1;
                    mas[k][1] = patch[i][j].Theta2;
                    mas[k][2] = patch[i][j].Phi10;
                    mas[k][3] = patch[i][j].Phi20;
                    k++;
                }
            }
            return mas;
        }

        public double[] GetBMas()
        {
            int patchNum = this.patch.Length * this.patch[0].Length;
            double[] mas = new double[patchNum];
            int k = 0;
            for (int i = 0; i < this.patch.Length; i++)
            {
                for (int j = 0; j < this.patch[i].Length; j++)
                {
                    mas[k] = this.magStrength[i][j];
                    k++;
                }
            }
            return mas;
        }

        public double[] GetBrightnessDensityMas()
        {
            int patchNum = this.patch.Length * this.patch[0].Length;
            double[] mas = new double[patchNum];
            int k = 0;
            for (int i = 0; i < this.patch.Length; i++)
            {
                for (int j = 0; j < this.patch[i].Length; j++)
                {
                    mas[k] = this.brightnessDensity[i][j];
                    k++;
                }
            }
            return mas;
        }

        public double[] GetThetaMas()
        {
            int patchNum = this.patch.Length * this.patch[0].Length;
            double[] mas = new double[patchNum];
            int k = 0;
            for (int i = 0; i < this.patch.Length; i++)
            {
                for (int j = 0; j < this.patch[i].Length; j++)
                {
                    mas[k] = this.magLineTheta[i][j];
                    k++;
                }
            }
            return mas;
        }

        public void AddCircSpot(double theta, double phi, double radius, double density)
        {
            double cosDist;
            double cosRadius;
            double thetaP, fiP;

            cosRadius = Math.Cos(radius);

            for (int i = 0; i < this.brightnessDensity.Length; i++)
            {
                for (int j = 0; j < this.brightnessDensity[i].Length; j++)
                {
                    fiP = this.patch[i][j].FiCenterOnStart();
                    thetaP = this.patch[i][j].ThetaCenterOnStart();
                    cosDist = Math.Cos(theta) * Math.Cos(thetaP) +
                        Math.Sin(theta) * Math.Sin(thetaP) * Math.Cos(fiP - phi);

                    if (cosDist >= cosRadius)
                    {
                        this.brightnessDensity[i][j] = density;
                    }
                }
            }
        }

        public void SetDensity(int i, int j, double density)
        {
            this.brightnessDensity[i][j] = density;
        }

        public void AddRectSpot(double theta1, double theta2, double phi1, double phi2, double density)
        {
            for (int i = 0; i < this.brightnessDensity.Length; i++)
            {
                for (int j = 0; j < this.brightnessDensity[i].Length; j++)
                {
                    if (this.patch[i][j].ThetaCenterOnStart() >= theta1 &&
                        this.patch[i][j].ThetaCenterOnStart() <= theta2 &&
                        this.patch[i][j].FiCenterOnStart() >= phi1 &&
                        this.patch[i][j].FiCenterOnStart() <= phi2)
                    {
                        this.brightnessDensity[i][j] = density;
                    }
                }
            }
        }

        public void ClearBrightnessDensityArray()
        {
            for (int i = 0; i < this.brightnessDensity.Length; i++)
            {
                for (int j = 0; j < this.brightnessDensity[i].Length; j++)
                {
                    this.brightnessDensity[i][j] = 0.0;
                }
            }
        }

        public Patch[][] Patches
        {
            get
            {
                return this.patch;
            }
        }

        public double[][] BrightnessDensity
        {
            get
            {
                return this.brightnessDensity;
            }
        }

        public double InclinationOfRotationAxis
        {
            get { return this.inc; }
        }

        public double LatitudeDipolOffset
        {
            get { return this.beta; }
        }

        /// <summary>
        /// Returns longitude dipol offset.
        /// </summary>
        public double LongitudeDipolOffset
        {
            get { return this.lambda; }
        }
        
        /// <summary>
        /// Returns pole's magnetic field strength.
        /// </summary>
        public double PolesMagneticField
        {
            get { return this.magStrPole; }
        }
    }
}
