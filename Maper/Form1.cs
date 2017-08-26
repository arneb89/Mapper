using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NPlot;
using System.Threading;
using MathLib;
using Maper.DopplerImaging;

namespace Maper
{
    public partial class Form1 : Form
    {        
        public Form1()
        {
            InitializeComponent();
            
            plotLCI.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotLCI.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotLCI.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plot2.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plot2.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plot2.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotMod2.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotMod2.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotMod2.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotILCM.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotILCM.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotILCM.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotSM_StokesI.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotSM_StokesI.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotSM_StokesI.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotSM_StokesQ.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotSM_StokesQ.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotSM_StokesQ.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotSM_StokesV.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotSM_StokesV.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotSM_StokesV.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotSM_StokesU.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotSM_StokesU.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotSM_StokesU.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotSM_PrArea.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotSM_PrArea.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotSM_PrArea.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));
        }

        /**********************************************************************************/
        /****************************Light Curve Inversion*********************************/
        /**********************************************************************************/

        #region LightCurveInversion

        private LCBox lcBox = new LCBox();

        private LightCurve[] lci_ModelLightCurves = null;

        private LCM.LCContainer lci_LightCurveBox = null;

        private TSurface[] intenSrfResult = null;

        private TSurface teffSrfResult = null;

        private LightCurve[] modelLightCurves = null;

        private Spline31D[] splineIntensity = null;

        private double[] lci_ldcX = null;

        /******************************** Functions ***************************************/

        private void btnAddLightCurve_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (this.lci_LightCurveBox == null)
            {
                this.lci_LightCurveBox = new Maper.LCM.LCContainer();
            }
            string path = openFileDialog1.FileName;
            if (path == null) return;
            try
            {
                this.lci_LightCurveBox.AddLightCurve(path);
            }
            catch
            {
                MessageBox.Show("No such file or error in file structure...", "Error");
            }

            lbLCI_LightCurveBox.Items.Clear();

            for (int i = 0; i < this.lci_LightCurveBox.LightCurvesNumber; i++)
            {
                lbLCI_LightCurveBox.Items.Add("Light Curve # " + (i + 1).ToString());
            }

            this.lbLCI_LightCurveBox.SelectedIndexChanged += new EventHandler(lbLCI_LightCurveBox_SelectedIndexChanged);
            this.txtLCI_Sigma.LostFocus += new EventHandler(LCI_LightCurveParsChanged);
            this.txtLCI_MaximumFlux.LostFocus += new EventHandler(LCI_LightCurveParsChanged);
        }

        void LCI_LightCurveParsChanged(object sender, EventArgs e)
        {
            try
            {
                int curveNumber = lbLCI_LightCurveBox.SelectedIndex;
                if (curveNumber == -1) return;

                this.lci_LightCurveBox.LightCurves[curveNumber].Sigma =
                    double.Parse(txtLCI_Sigma.Text.Replace(".", ","));
                this.lci_LightCurveBox.LightCurves[curveNumber].FluxMax=
                    double.Parse(txtLCI_MaximumFlux.Text.Replace(".", ","));
            }
            catch
            {
                MessageBox.Show("Wrong format...", "Error...");
                return;
            }  
        }

        void lbLCI_LightCurveBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int curveNumber = lbLCI_LightCurveBox.SelectedIndex;
            if (curveNumber == -1) return;

            this.txtLCI_Sigma.LostFocus -= new EventHandler(LCI_LightCurveParsChanged);
            this.txtLCI_MaximumFlux.LostFocus -= new EventHandler(LCI_LightCurveParsChanged);

            this.txtLCI_Sigma.Text = this.lci_LightCurveBox.LightCurves[curveNumber].Sigma.ToString();
            this.txtLCI_MaximumFlux.Text = this.lci_LightCurveBox.LightCurves[curveNumber].FluxMax.ToString();
            this.txtLCI_Filter.Text = this.lci_LightCurveBox.LightCurves[curveNumber].Band;
            this.txtLCI_Size.Text = this.lci_LightCurveBox.LightCurves[curveNumber].Phases.Length.ToString();

            this.txtLCI_Sigma.LostFocus += new EventHandler(LCI_LightCurveParsChanged);
            this.txtLCI_MaximumFlux.LostFocus += new EventHandler(LCI_LightCurveParsChanged);
        }

        private void btnDeleteLightCurve_Click(object sender, EventArgs e)
        {
            if (this.lci_LightCurveBox == null) return;
            if (this.lci_LightCurveBox.LightCurvesNumber == 0) return;

            int num = lbLCI_LightCurveBox.SelectedIndex;

            if (num != -1)
            {
                this.lci_LightCurveBox.DelLightCurve(num);
            }

            lbLCI_LightCurveBox.Items.Clear();

            for (int i = 0; i < this.lci_LightCurveBox.LightCurvesNumber; i++)
            {
                lbLCI_LightCurveBox.Items.Add("Ligth Curve #" + (i + 1).ToString());
            }

            this.lbLCI_LightCurveBox_SelectedIndexChanged(sender, e);
        }

        private void btnShowLightCurve_Click(object sender, EventArgs e)
        {
            int curveNum = lbLCI_LightCurveBox.SelectedIndex;
            if (curveNum == -1) return;
            PointPlot pp = new PointPlot(new Marker(Marker.MarkerType.FilledCircle, 3, Color.Black));
            pp.OrdinateData = this.lci_LightCurveBox.LightCurves[curveNum].Fluxes;
            pp.AbscissaData = this.lci_LightCurveBox.LightCurves[curveNum].Phases;
            plotLCI.Add(pp);
            plotLCI.XAxis1.Label = "Phase";
            plotLCI.YAxis1.Label = "Flux";
            plotLCI.Refresh();
        }

        private void btnReconstrEND_Click(object sender, EventArgs e)
        {
            double logg;
            double regpar;
            double biaspar;
            int n;
            int m;
            double inc;
            double teffMax, teffMin, tph;
            int iterMax;
            
            try
            {
                regpar = double.Parse(txtRegPar.Text.Replace(".", ","));
                biaspar = double.Parse(txtBias.Text.Replace(".", ","));
                n = int.Parse(txtN.Text.Replace(".", ","));
                m = int.Parse(txtM.Text.Replace(".", ","));
                inc = double.Parse(txtInc.Text.Replace(".", ","));
                inc = inc * Math.PI / 180.0;
                logg = double.Parse(txtLCI_Logg.Text.Replace(".", ","));
                teffMax = double.Parse(txtLCI_TeffMax.Text.Replace(".", ","));
                teffMin = double.Parse(txtLCI_TeffMin.Text.Replace(".", ","));
                iterMax = int.Parse(txtLCI_IterMax.Text);
                tph = double.Parse(txtLCI_Tph.Text.Replace(".", ","));
            }
            catch
            {
                MessageBox.Show("Error in format of input data...", "Error...");
                return;
            }

            Spline32D spliner2d;
            this.splineIntensity = new Spline31D[lci_LightCurveBox.LightCurvesNumber];
            Table3D tabInt = new Table3D(Application.StartupPath + @"\Data\NormSpInt\UBVRIRcIcJK.dat");
            for (int q = 0; q < lci_LightCurveBox.LightCurvesNumber; q++)
            {
                int filterID = 0;
                switch (lci_LightCurveBox.LightCurves[q].Band)
                {
                    case "U":  filterID = 0; break;
                    case "B":  filterID = 1; break;
                    case "V":  filterID = 2; break;
                    case "R":  filterID = 3; break;
                    case "I":  filterID = 4; break;
                    case "Rc": filterID = 5; break;
                    case "Ic": filterID = 6; break;
                }
                spliner2d = new Spline32D(tabInt.YMas, tabInt.XMas, tabInt.FMas[filterID]);
                double[] teffInt, intInt;
                teffInt = new double[tabInt.YMas.Length];
                intInt = new double[tabInt.YMas.Length];
                for (int i = 0; i < tabInt.YMas.Length; i++)
                {
                    intInt[i] = spliner2d.Interp(tabInt.YMas[i], logg);
                    teffInt[i] = tabInt.YMas[i];
                }
                splineIntensity[q] = new Spline31D(teffInt, intInt);
            }

            MathLib.Spline31D[] splineLDC = new Spline31D[lci_LightCurveBox.LightCurvesNumber];
            Table3D tabLDC = new Table3D(Application.StartupPath + @"\Data\LDC\UBVRI.dat");
            for (int q = 0; q < lci_LightCurveBox.LightCurvesNumber; q++)
            {
                int filterID = 0;
                switch (lci_LightCurveBox.LightCurves[q].Band)
                {
                    case "U": filterID = 0; break;
                    case "B": filterID = 1; break;
                    case "V": filterID = 2; break;
                    case "R": filterID = 3; break;
                    case "I": filterID = 4; break;
                    case "Rc": filterID = 5; break;
                    case "Ic": filterID = 6; break;
                }
                spliner2d = new Spline32D(tabLDC.YMas, tabLDC.XMas, tabLDC.FMas[filterID]);
                double[] teffLDC, ldcLDC;
                teffLDC = new double[tabLDC.YMas.Length];
                ldcLDC = new double[tabLDC.YMas.Length];
                for (int i = 0; i < tabLDC.YMas.Length; i++)
                {
                    ldcLDC[i] = spliner2d.Interp(tabLDC.YMas[i], logg);
                    teffLDC[i] = tabLDC.YMas[i];
                }
                splineLDC[q] = new Spline31D(teffLDC, ldcLDC);
            }

            txtLCI_Results.Text += "\r\nLight curves data:";
            for (int q = 0; q < lci_LightCurveBox.LightCurves.Length; q++)
            {
                txtLCI_Results.Text += string.Format("\r\nFilter {0}: MaxFlux = {1}; Sigma = {2}",
                    lci_LightCurveBox.LightCurves[q].Band, 
                    lci_LightCurveBox.LightCurves[q].FluxMax,
                    lci_LightCurveBox.LightCurves[q].Sigma);
            }
            
            this.lci_ldcX = new double[lci_LightCurveBox.LightCurvesNumber];
            
            for (int q = 0; q < lci_ldcX.Length; q++)
            {
                lci_ldcX[q] = splineLDC[q].Interp(tph);
                txtLCI_Results.Text += string.Format("\r\nLinear limb darkening parameter for {0} band: {1:0.0000}",
                    lci_LightCurveBox.LightCurves[q].Band, lci_ldcX[q]);
            }

            double[] sigmaMas = new double[lci_LightCurveBox.LightCurvesNumber];
            for (int q = 0; q < sigmaMas.Length; q++)
            {
                sigmaMas[q] = lci_LightCurveBox.LightCurves[q].Sigma;
            }

            TSurface tsrf = new TSurface(n, m, inc, 5000);
            
            Reconstr rcstr = new Reconstr();

            Table1D[] tabInten = new Table1D[lci_LightCurveBox.LightCurves.Length];
            for (int i = 0; i < tabInten.Length; i++)
            {
                tabInten[i] = new Table1D();
                tabInten[i].XMas = lci_LightCurveBox.LightCurves[i].Phases;
                tabInten[i].FMas = lci_LightCurveBox.LightCurves[i].Fluxes;
            }

            txtLCI_Results.Text += "\r\nInitialization of the reconstructor...";
            rcstr.InitReconstr02(tsrf, splineIntensity, lci_LightCurveBox.LightCurves, lci_ldcX);
            
            //tsrf = rcstr.ReconstrEND1(regpar, biaspar, 5000, 4000);
            //tsrf = rcstr.ReconstrColor(regpar, biaspar, 5000, 4000);
            //tsrf = rcstr.ReconstrVictoria(regpar, 0.03, 5000, 4000);
            //tsrf = rcstr.ReconstrENDEnd(regpar, 0.0354, 5000, 4000);
            Counter counter = new Counter(); // Thanks to Хабр
            counter.Start();
            txtLCI_Results.Text += "\r\nStart surface reconstruction process...";
            txtLCI_Results.Text += string.Format("\r\nSmooth. = {0}, Bias = {1}", regpar, biaspar);
            this.intenSrfResult = rcstr.ReconstrLin(regpar, biaspar, teffMax, teffMin, iterMax, 1);
            counter.Stop();
            txtLCI_Results.AppendText(string.Format("\r\nEnd of computations. Computings duration is {0:0.000} sec:",
                counter.TotalSeconds));
            txtLCI_Results.Text += string.Format("\r\nKhi^2 = {0:0.0000E000}", rcstr.Khi2);

            //tsrf = rcstr.ReconstrEND2(regpar, 5000, biaspar);
            this.lci_ModelLightCurves = rcstr.ModelLightCurves;
        }

        private void btnLCI_ShowTeffMap_Click(object sender, EventArgs e)
        {
            if (splineIntensity == null) return;
            double phase;
            double teff;
            try
            {
                phase = double.Parse(txtLCI_PhaseOfMaximumBrightness.Text.Replace(".", ","));
                teff = double.Parse(txtLCI_AverangeTeffAtMaxPhase.Text.Replace(".", ","));
            }
            catch
            {
                return;
            }
            TSurface tsrf = new TSurface(this.intenSrfResult[0].GetN(), this.intenSrfResult[0].GetM(),
                this.intenSrfResult[0].GetInc(), 0.0);
            double aveInt;
            double sum = 0;
            int patchNum = 0;
            for (int i = 0; i < this.intenSrfResult[0].GetN(); i++)
            {
                for (int j = 0; j < this.intenSrfResult[0].patch[i].Length; j++)
                {
                    if (this.intenSrfResult[0].patch[i][j].Mu(phase, this.intenSrfResult[0].GetInc())>0)
                    {
                        patchNum++;
                        sum += this.intenSrfResult[0].teff[i][j];
                    }
                }
            }
            aveInt = sum / patchNum;
            double modelIntensity = splineIntensity[0].Interp(teff);

            Spline31D spline = new Spline31D(this.splineIntensity[0].GetY, this.splineIntensity[0].GetX);

            for (int i = 0; i < tsrf.teff.Length; i++)
            {
                for (int j = 0; j < tsrf.teff[i].Length; j++)
                {
                    tsrf.teff[i][j] = spline.Interp(this.intenSrfResult[0].teff[i][j] * modelIntensity / aveInt);
                }
            }

            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.Blue;
            svf.color1 = Color.Red;
            svf.Init(tsrf.GetPatchCoordMas(), tsrf.GetTeffMas());
            svf.Show();

            // saving

            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\teff_map.dat");
            sw.Write(tsrf.ToText());
            sw.Flush();
            sw.Close();
        }

        private void btnLCI_ShowIntSurface_Click(object sender, EventArgs e)
        {
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.Blue;
            svf.color1 = Color.Red;
            svf.Init(intenSrfResult[0].GetPatchCoordMas(), intenSrfResult[0].GetTeffMas());
            svf.Show();

            // saving

            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\inten_map.dat");
            sw.Write(intenSrfResult[0].ToText());
            sw.Flush();
            sw.Close();
        }

        private void btnReconstrF_Click(object sender, EventArgs e)
        {
            double regpar = double.Parse(txtRegPar.Text.Replace(".", ","));
            double biaspar = double.Parse(txtBias.Text.Replace(".", ","));
            int n_lc = lci_LightCurveBox.LightCurvesNumber;

            Spline32D spliner2d;
            Spline31D[] splineInt = new Spline31D[n_lc];
            Table3D tabInt = new Table3D(Application.StartupPath + @"\Data\NormSpInt\UBVRI.dat");
            for (int q = 0; q < n_lc; q++)
            {
                int filterID=0;
                
                switch (lci_LightCurveBox.LightCurves[q].Band)
                {
                    case "U": filterID = 0; break;
                    case "B": filterID = 1; break;
                    case "V": filterID = 2; break;
                    case "R": filterID = 3; break;
                    case "I": filterID = 4; break;
                }
                spliner2d = new Spline32D(tabInt.YMas, tabInt.XMas, tabInt.FMas[filterID]);
                double[] teffInt, intInt;
                teffInt = new double[tabInt.YMas.Length];
                intInt = new double[tabInt.YMas.Length];
                for (int i = 0; i < tabInt.YMas.Length; i++)
                {
                    intInt[i] = spliner2d.Interp(tabInt.YMas[i], 4.5);
                    teffInt[i] = tabInt.YMas[i];
                }
                splineInt[q] = new Spline31D(teffInt, intInt);
            }

            //Table1D tabLC = new Table1D(Application.StartupPath + @"\Data\LC\Sample.txt");
            //Table1D[] tabLCMas = new Table1D[1];
            //tabLCMas[0] = tabLC;
            //Table3D tabInt = new Table3D(Application.StartupPath + @"\Data\NormSpInt\UBVRI.dat");
            TSurface tsrf = new TSurface(20, 40, 0.78, 5000);
            //Spline32D spliner2d = new Spline32D(tabInt.YMas, tabInt.XMas, tabInt.FMas[2]);
            //double[] teffInt, intInt;
            //teffInt = new double[tabInt.YMas.Length];
            //intInt = new double[tabInt.YMas.Length];
            //for (int i = 0; i < tabInt.YMas.Length; i++)
            //{
            //    intInt[i] = spliner2d.Interp(tabInt.YMas[i], 4.5);
            //    teffInt[i] = tabInt.YMas[i];
            //}
            //Spline31D splineInt;
            //splineInt = new Spline31D(teffInt, intInt);
            Reconstr rcstr = new Reconstr();
            //rcstr.InitReconstr01(tsrf, splineInt, lci_LightCurveBox.LightCurves, lcBox.LDY, lcBox.LDY, lcBox.LDModel, lcBox.Sigma);
            tsrf = rcstr.ReconstrF(regpar, 5000, 4000);
            //this.modelLC = rcstr.fluxes;
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.Blue;
            svf.color1 = Color.Red;
            svf.Init(tsrf.GetPatchCoordMas(), tsrf.GetTeffMas());
            svf.Show(); 
        }

        private void btnLCI_SaveLightCurve_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();

            string path = saveFileDialog1.FileName;

            StreamWriter sw;

            int lcNum = 0;

            try
            {
                sw = new StreamWriter(path);
                lcNum = int.Parse(txtLCI_ModelLightCurveNumber.Text);
            }
            catch
            {
                return;
            }

            for (int i = 0; i < this.lci_ModelLightCurves[lcNum].Phases.Length; i++)
            {
                sw.WriteLine("{0:0.000000} {1:0.00000E000}", this.lci_ModelLightCurves[lcNum].Phases[i],
                    this.lci_ModelLightCurves[lcNum].Fluxes[i]);
            }
            sw.Flush();
            sw.Close();
        }

        //private void btnAccept_Click(object sender, EventArgs e)
        //{
        //    int num = lbLCI_LightCurveBox.SelectedIndex;
        //    if (num != -1)
        //    {
        //        this.lcBox.LDModel = int.Parse(txtLDM.Text);
        //        this.lcBox.SetLDX(num, double.Parse(txtLDCX.Text.Replace(".", ",")));
        //        this.lcBox.SetSigma(num, double.Parse(txtLCI_Sigma.Text.Replace(".", ",")));
        //        this.ShowDataSetSettings(new object(), new EventArgs());
        //    }
        //}

        private void button5_Click(object sender, EventArgs e)
        {
            int N = int.Parse(txtN.Text);
            int M = int.Parse(txtM.Text);
            double inc = double.Parse(txtInc.Text)*Math.PI/180.0;
            double sigma=double.Parse(txtLCI_Sigma.Text);
            double regpar = double.Parse(txtRegPar.Text);
            double bias = double.Parse(txtBias.Text);
            Table1D lc = lcBox.LightCurves[0];
            TSurface tsrf=new TSurface(N, M, inc, 0);
            Reconstr rec = new Reconstr();
            // Recostruction of surface;
            txtLCI_Results.AppendText("Initialization of ReconstrMLI...\r\n");
            rec.InitReconstrMLI(tsrf, lc, lcBox.LDX[0], lcBox.LDY[0], 1, sigma);
            txtLCI_Results.AppendText("Run ReconstrMLI...\r\n");
            tsrf = rec.ReconstrMLI(regpar, bias);
            txtLCI_Results.AppendText("Recostruction is complited.\r\n");
            txtLCI_Results.AppendText(string.Format("CritFillips = {0}.\r\n", rec.critFillips));
            txtLCI_Results.AppendText(string.Format("CritTurchin = {0}.\r\n", rec.critTurchin));
            txtLCI_Results.AppendText(string.Format("CritCB = {0}.\r\n", rec.critCB));
            // Visualization of reconstructed surface;
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.Init(tsrf.GetPatchCoordMas(), tsrf.GetTeffMas());
            svf.color0 = Color.Black; svf.color1 = Color.White;
            svf.Show();
            // Visualization of synthetic light curve;
            LCGenerator lcg = new LCGenerator(tsrf, lcBox.LDX[0], lcBox.LDY[0]);
            double[] phases = new double[100];
            for (int i = 0; i < phases.Length; i++) phases[i] = i / 100.0;
            double[] fluxes = lcg.GetFluxMasForLinLDForIntSrfForConstLD(phases);
            LinePlot lp = new LinePlot(fluxes, phases);
            plotLCI.Add(lp);
            plotLCI.Refresh();
        }

        private void btnReconstrMLI2_Click(object sender, EventArgs e)
        {
            int N = int.Parse(txtN.Text);
            int M = int.Parse(txtM.Text);
            double inc = double.Parse(txtInc.Text) * Math.PI / 180.0;
            double sigma = double.Parse(txtLCI_Sigma.Text);
            double regpar = double.Parse(txtRegPar.Text);
            double bias = double.Parse(txtBias.Text);
            double fluxMax = double.Parse(txtfluxMax.Text);
            double deltaFluxMax = double.Parse(txtDeltaFluxMax.Text);
            Table1D lc = lcBox.LightCurves[0];
            TSurface tsrf = new TSurface(N, M, inc, 0);
            Reconstr rec = new Reconstr();
            // Recostruction of surface;
            txtLCI_Results.AppendText("Initialization of ReconstrMLI2...\r\n");
            rec.InitReconstrMLI(tsrf, lc, lcBox.LDX[0], lcBox.LDY[0], 1, sigma);
            txtLCI_Results.AppendText("Run ReconstrMLI2...\r\n");
            tsrf = rec.ReconstrMLI2(regpar, bias, fluxMax, deltaFluxMax, 0);
            txtLCI_Results.AppendText("Recostruction is complited.\r\n");
            txtLCI_Results.AppendText(string.Format("CritFillips = {0}.\r\n", rec.critFillips));
            txtLCI_Results.AppendText(string.Format("CritTurchin = {0}.\r\n", rec.critTurchin));
            txtLCI_Results.AppendText(string.Format("CritCB = {0}.\r\n", rec.critCB));
            // Visualization of reconstructed surface;
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.Init(tsrf.GetPatchCoordMas(), tsrf.GetTeffMas());
            svf.color0 = Color.Black; svf.color1 = Color.White;
            svf.Show();
            //Visualization of synthetic light curve;
            
            LCGenerator lcg = new LCGenerator(tsrf, lcBox.LDX[0], lcBox.LDY[0]);
            double[] phases = new double[100];
            for (int i = 0; i < phases.Length; i++) phases[i] = i / 100.0;
            double[] fluxes = lcg.GetFluxMasForLinLDForIntSrfForConstLD(phases);
            LinePlot lp = new LinePlot(fluxes, phases);
            plotLCI.Add(lp);
            plotLCI.Refresh();
        }

        private void btnReconstrTTA_Click(object sender, EventArgs e)
        {
            int N = int.Parse(txtN.Text);
            int M = int.Parse(txtM.Text);
            double inc = double.Parse(txtInc.Text) * Math.PI / 180.0;
            double sigma = double.Parse(txtLCI_Sigma.Text);
            double regpar = double.Parse(txtRegPar.Text);
            double jph = double.Parse(txtJPh.Text);
            double jsp = double.Parse(txtJSp.Text);

            Table1D lc = lcBox.LightCurves[0];
            TSurface tsrf = new TSurface(N, M, inc, 0);
            Reconstr rec = new Reconstr();
            // Recostruction of surface;
            txtLCI_Results.AppendText("Initialization of ReconstrTTA...\r\n");
            rec.InitReconstrMLI(tsrf, lc, lcBox.LDX[0], lcBox.LDY[0], 1, sigma);
            txtLCI_Results.AppendText("Run ReconstrTTA...\r\n");
            tsrf = rec.ReconstrTTA(regpar, jph, jsp);
            txtLCI_Results.AppendText("Recostruction is complited.\r\n");
            txtLCI_Results.AppendText(string.Format("CritFillips = {0}.\r\n", rec.critFillips));
            txtLCI_Results.AppendText(string.Format("CritTurchin = {0}.\r\n", rec.critTurchin));
            txtLCI_Results.AppendText(string.Format("CritCB = {0}.\r\n", rec.critCB));
            // Visualization of reconstructed surface;
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.Init(tsrf.GetPatchCoordMas(), tsrf.GetTeffMas());
            svf.color0 = Color.Black; svf.color1 = Color.White;
            svf.Show();
            //Visualization of synthetic light curve;

            LCGenerator lcg = new LCGenerator(tsrf, lcBox.LDX[0], lcBox.LDY[0]);
            double[] phases = new double[100];
            for (int i = 0; i < phases.Length; i++) phases[i] = i / 100.0;
            double[] fluxes = lcg.GetFluxMasForLinLDForIntSrfForConstLD(phases);
            LinePlot lp = new LinePlot(fluxes, phases);
            plotLCI.Add(lp);
            plotLCI.Refresh();
        }

        private void btnShowModelLightCurve_Click(object sender, EventArgs e)
        {
            if (this.intenSrfResult == null) return;
            int num;
            try
            {
                num = int.Parse(txtLCI_ModelLightCurveNumber.Text);
            }
            catch
            {
                return;
            }
            if (num < 0 || num > this.intenSrfResult.Length - 1) return;

            //Visualization of synthetic light curve;

            LinePlot lp = new LinePlot(this.lci_ModelLightCurves[num].Fluxes, 
                this.lci_ModelLightCurves[num].Phases);
            plotLCI.Add(lp);
            plotLCI.Title = "Light Curve";
            plotLCI.XAxis1.Label = "Phase";
            plotLCI.YAxis1.Label = "Flux";
            plotLCI.Refresh();
        }

        private void btnLCI_SaveIntMap_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string file = openFileDialog1.FileName;

            StreamWriter sw = new StreamWriter(file);

            sw.WriteLine(intenSrfResult[0].GetN().ToString() + " " + intenSrfResult[0].GetM().ToString());

            for (int i = 0; i < this.intenSrfResult[0].GetN(); i++)
            {
                for (int j = 0; j < this.intenSrfResult[0].patch[i].Length; i++)
                {
                    sw.Write(this.intenSrfResult[0].teff[i][j].ToString() + "\t");
                }
                sw.Write("\r\n");
            }
        }

        private void btmLCI_FR_Click(object sender, EventArgs e)
        {
            double logg;
            double regpar;
            double biaspar;
            int n;
            int m;
            double inc;
            double teffMax, teffMin, tph;
            int iterMax;

            try
            {
                regpar = double.Parse(txtRegPar.Text.Replace(".", ","));
                biaspar = double.Parse(txtBias.Text.Replace(".", ","));
                n = int.Parse(txtN.Text.Replace(".", ","));
                m = int.Parse(txtM.Text.Replace(".", ","));
                inc = double.Parse(txtInc.Text.Replace(".", ","));
                inc = inc * Math.PI / 180.0;
                logg = double.Parse(txtLCI_Logg.Text.Replace(".", ","));
                teffMax = double.Parse(txtLCI_TeffMax.Text.Replace(".", ","));
                teffMin = double.Parse(txtLCI_TeffMin.Text.Replace(".", ","));
                iterMax = int.Parse(txtLCI_IterMax.Text);
                tph = double.Parse(txtLCI_Tph.Text.Replace(".", ","));
            }
            catch
            {
                MessageBox.Show("Error in format of input data...", "Error...");
                return;
            }

            Spline32D spliner2d;
            this.splineIntensity = new Spline31D[lci_LightCurveBox.LightCurvesNumber];
            Table3D tabInt = new Table3D(Application.StartupPath + @"\Data\NormSpInt\UBVRIRcIcJK.dat");
            for (int q = 0; q < lci_LightCurveBox.LightCurvesNumber; q++)
            {
                int filterID = 0;
                switch (lci_LightCurveBox.LightCurves[q].Band)
                {
                    case "U": filterID = 0; break;
                    case "B": filterID = 1; break;
                    case "V": filterID = 2; break;
                    case "R": filterID = 3; break;
                    case "I": filterID = 4; break;
                    case "Rc": filterID = 5; break;
                    case "Ic": filterID = 6; break;
                }
                spliner2d = new Spline32D(tabInt.YMas, tabInt.XMas, tabInt.FMas[filterID]);
                double[] teffInt, intInt;
                teffInt = new double[tabInt.YMas.Length];
                intInt = new double[tabInt.YMas.Length];
                for (int i = 0; i < tabInt.YMas.Length; i++)
                {
                    intInt[i] = spliner2d.Interp(tabInt.YMas[i], logg);
                    teffInt[i] = tabInt.YMas[i];
                }
                splineIntensity[q] = new Spline31D(teffInt, intInt);
            }

            MathLib.Spline31D[] splineLDC = new Spline31D[lci_LightCurveBox.LightCurvesNumber];
            Table3D tabLDC = new Table3D(Application.StartupPath + @"\Data\LDC\UBVRI.dat");
            for (int q = 0; q < lci_LightCurveBox.LightCurvesNumber; q++)
            {
                int filterID = 0;
                switch (lci_LightCurveBox.LightCurves[q].Band)
                {
                    case "U": filterID = 0; break;
                    case "B": filterID = 1; break;
                    case "V": filterID = 2; break;
                    case "R": filterID = 3; break;
                    case "I": filterID = 4; break;
                    case "Rc": filterID = 5; break;
                    case "Ic": filterID = 6; break;
                }
                spliner2d = new Spline32D(tabLDC.YMas, tabLDC.XMas, tabLDC.FMas[filterID]);
                double[] teffLDC, ldcLDC;
                teffLDC = new double[tabLDC.YMas.Length];
                ldcLDC = new double[tabLDC.YMas.Length];
                for (int i = 0; i < tabLDC.YMas.Length; i++)
                {
                    ldcLDC[i] = spliner2d.Interp(tabLDC.YMas[i], logg);
                    teffLDC[i] = tabLDC.YMas[i];
                }
                splineLDC[q] = new Spline31D(teffLDC, ldcLDC);
            }

            txtLCI_Results.Text += "\r\nLight curves data:";
            for (int q = 0; q < lci_LightCurveBox.LightCurves.Length; q++)
            {
                txtLCI_Results.Text += string.Format("\r\nFilter {0}: MaxFlux = {1}; Sigma = {2}",
                    lci_LightCurveBox.LightCurves[q].Band,
                    lci_LightCurveBox.LightCurves[q].FluxMax,
                    lci_LightCurveBox.LightCurves[q].Sigma);
            }

            this.lci_ldcX = new double[lci_LightCurveBox.LightCurvesNumber];

            for (int q = 0; q < lci_ldcX.Length; q++)
            {
                lci_ldcX[q] = splineLDC[q].Interp(tph);
                txtLCI_Results.Text += string.Format("\r\nLinear limb darkening parameter for {0} band: {1:0.0000}",
                    lci_LightCurveBox.LightCurves[q].Band, lci_ldcX[q]);
            }

            TSurface tsrf = new TSurface(n, m, inc, 5000);

            int n_filt=lci_LightCurveBox.LightCurvesNumber;
            double[] intenPhot = new double[n_filt];
            double[] intenSpot = new double[n_filt];
            double[] xLDCPhot=new double[n_filt];
            double[] xLDCSpot = new double[n_filt];
            double[] sigmaMas = new double[n_filt];
            double[] flx_max = new double[n_filt];
            for (int q = 0; q < n_filt; q++)
            {
                intenPhot[q] = splineIntensity[q].Interp(teffMax);
                intenSpot[q] = splineIntensity[q].Interp(teffMin);
                xLDCPhot[q] = splineLDC[q].Interp(teffMax);
                xLDCSpot[q] = splineLDC[q].Interp(teffMin);
                sigmaMas[q] = lci_LightCurveBox.LightCurves[q].Sigma;
                flx_max[q] = lci_LightCurveBox.LightCurves[q].FluxMax;
            }

            txtLCI_Results.Text += "\r\nInitialization of the reconstructor...";
            RecF rf = new RecF(tsrf, lci_LightCurveBox.LightCurves, intenPhot, intenSpot, xLDCPhot, xLDCSpot, sigmaMas, flx_max);

            Counter counter = new Counter(); // Thanks to Хабр
            counter.Start();
            txtLCI_Results.Text += "\r\nStart surface reconstruction process...";
            txtLCI_Results.Text += string.Format("\r\nSmooth. = {0}", regpar);
            rf.Map(regpar);
            this.teffSrfResult = rf.ResSurface;
            counter.Stop();
            txtLCI_Results.AppendText(string.Format("\r\nEnd of computations. Computings duration is {0:0.000} sec:",
                counter.TotalSeconds));
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.Blue;
            svf.color1 = Color.Red;
            svf.Init(this.teffSrfResult.GetPatchCoordMas(), this.teffSrfResult.GetTeffMas());
            svf.Show();
            
            //txtLCI_Results.Text += string.Format("\r\nKhi^2 = {0:0.0000E000}", rcstr.Khi2);

            //tsrf = rcstr.ReconstrEND2(regpar, 5000, biaspar);
            //this.lci_ModelLightCurves = rcstr.ModelLightCurves;
            for (int q = 0; q < n_filt; q++)
            {
                LinePlot lp = new LinePlot();
                lp.AbscissaData = rf.ModelLigthCurves[q].Phases;
                lp.OrdinateData = rf.ModelLigthCurves[q].Fluxes;
                plotLCI.Add(lp);
            }
            plotLCI.Refresh();
        }

        #endregion

        /**********************************************************************************/
        /*************************Light Curve Generation Code******************************/
        /**********************************************************************************/

        #region LightCurveGeneration

        private TSurface srfMod;
        private double[] phases, fluxes;

        private void btnCreateSurface_Click(object sender, EventArgs e)
        {
            int n = int.Parse(txtLCG_N.Text);
            int m = int.Parse(txtLCG_M.Text);
            double logg = double.Parse(txtLCG_Logg.Text.Replace(".", ","));
            double tph = double.Parse(txtLCG_TeffPh.Text.Replace(".", ","));
            double inc = double.Parse(txtLCG_Inc.Text.Replace(".", ",")) * Math.PI / 180.0;
            this.srfMod = new TSurface(n, m, inc, tph);
        }

        private void btnShowSurfaceMod_Click(object sender, EventArgs e)
        {
            if (this.srfMod == null) return;
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.Blue;
            svf.color1 = Color.Red;
            svf.Init(this.srfMod.GetPatchCoordMas(), this.srfMod.GetTeffMas());
            svf.ShowDialog();
        }

        private void btnAddSpot_Click(object sender, EventArgs e)
        {
            double phi = double.Parse(txtLCG_Phi.Text.Replace(".", ",")) * Math.PI / 180.0;
            double theta = double.Parse(txtLCG_Theta.Text.Replace(".", ",")) * Math.PI / 180.0;
            double radius = double.Parse(txtLCG_Radius.Text.Replace(".", ",")) * Math.PI / 180;
            double tsp = double.Parse(txtLCG_TeffSp.Text.Replace(".", ","));
            this.srfMod.AddCircularSpot(phi, theta, radius, tsp);
        }

        private void btnGenerateLightCurve_Click(object sender, EventArgs e)
        {
            string filter = txtLCG_Filter.Text;
            int ldmod = int.Parse(txtLCG_LDModel.Text);
            int pointcount = int.Parse(txtLCG_PointsCount.Text);
            double sigma =  Convert.ToDouble(txtLCG_Sigma.Text.Replace(".", ","));
            double logg=double.Parse(txtLCG_Logg.Text.Replace(".", ","));
            double scale = double.Parse(txtLCG_Scale.Text.Replace(".", ","));

            // Achtung!!! Gleich filterID fur Flussen und Koeffizienten asnutzen.
            int filterID=0;
            switch (filter)
            {
                case "U": filterID = 0; break;
                case "B": filterID = 1; break;
                case "V": filterID = 2; break;
                case "R": filterID = 3; break;
                case "I": filterID = 4; break;
            }


            Table3D tabInt = new Table3D(Application.StartupPath + @"\Data\NormSpInt\UBVRI.dat");
            Table3D tabLDC = new Table3D(Application.StartupPath + @"\Data\LDC\UBVRI.dat");
            Spline32D spliner2d = new Spline32D(tabInt.YMas, tabInt.XMas, tabInt.FMas[filterID]);
            double[] teffInt, intInt;
            teffInt = new double[tabInt.YMas.Length];
            intInt = new double[tabInt.YMas.Length];
            for (int i = 0; i < tabInt.YMas.Length; i++)
            {
                intInt[i] = spliner2d.Interp(tabInt.YMas[i], logg);
                teffInt[i] = tabInt.YMas[i];
            }
            spliner2d = new Spline32D(tabLDC.YMas, tabLDC.XMas, tabLDC.FMas[filterID]);
            double[] teffLDC, ldcLDC;
            teffLDC = new double[tabLDC.YMas.Length];
            ldcLDC = new double[tabLDC.YMas.Length];
            for (int i = 0; i < tabLDC.YMas.Length; i++)
            {
                ldcLDC[i] = spliner2d.Interp(tabLDC.YMas[i], logg);
                teffLDC[i] = tabLDC.YMas[i];
            }
            Spline31D splineInt, splineLDC;
            splineInt = new Spline31D(teffInt, intInt);
            splineLDC = new Spline31D(teffLDC, ldcLDC);
            LCGenerator lcg = new LCGenerator(this.srfMod, splineInt, splineLDC);

            phases = new double[pointcount];
            for (int i = 0; i < phases.Length; i++)
            {
                phases[i] = i / (double)pointcount;
            }

            fluxes = lcg.GetFluxMasForLinLD(phases);

            for (int i = 0; i < fluxes.Length; i++)
            {
                fluxes[i] = fluxes[i] / scale;
            }

            double[] fluxesCert = fluxes;
            

            fluxes = Noiser.AddNormNoise(fluxes, sigma, 1);

            LinePlot lp = new LinePlot(fluxes, phases);

            plot2.Add(lp);
            plot2.Title = "Light Curve";
            plot2.XAxis1.Label = "Phase";
            plot2.YAxis1.Label = "Intensity";
            plot2.Refresh();

            if (sigma != 0.0)
            {
                double[][] gist = this.gistmake(fluxes, fluxesCert, 6 * sigma, 10);
                NPlot.HistogramPlot hp = new HistogramPlot();
                hp.Color = Color.White;
                hp.Filled = true;
                hp.AbscissaData = gist[0];
                hp.OrdinateData = gist[1];
                plotGist.Clear();
                plotGist.Add(hp);
                plotGist.Refresh();
            }
        }

        private void btnGenerateLightCurve2_Click(object sender, EventArgs e)
        {
            string filter = "";
            int ldmod;
            int pointcount = 0;
            double sigma = 0.0;
            double logg = 0.0;
            double scale = 1;

            try
            {
                filter = txtLCG_Filter.Text;
                ldmod = int.Parse(txtLCG_LDModel.Text);
                pointcount = int.Parse(txtLCG_PointsCount.Text);
                sigma = Convert.ToDouble(txtLCG_Sigma.Text.Replace(".", ","));
                logg = double.Parse(txtLCG_Logg.Text.Replace(".", ","));
                scale = double.Parse(txtLCG_Scale.Text.Replace(".", ","));
            }
            catch
            {
                MessageBox.Show("Wrong format of input data...", "Error...");
            }

            IntensityProvider1D ip1d = new IntensityProvider1D(Application.StartupPath + @"\Data\NormSpInt\UBVRIRcIcJK.dat",
                    new string[5] { "U", "B", "V", "R", "I" }, logg);
            ip1d.FixFilter = filter;
            IntensityProvider1D ldcp1d = new IntensityProvider1D(Application.StartupPath + @"\Data\LDC\UBVRI.dat",
                    new string[5] { "U", "B", "V", "R", "I" }, logg);
            ldcp1d.FixFilter = filter;
            LinearLimbDarkeningLow lldl = new LinearLimbDarkeningLow(ldcp1d);
            lldl.FixedFilter = filter;

            phases = new double[pointcount];
            for (int i = 0; i < phases.Length; i++)
            {
                phases[i] = i / (double)pointcount;
            }

            LCGenerator1 lcg = new LCGenerator1(this.srfMod, lldl.GetLinerLimbDarkeningCoefficient, ip1d.GetIntensityForFixedFilter);

            this.fluxes = lcg.GetFluxes(phases, scale);

            this.fluxes = Noiser.AddNormNoise(this.fluxes, sigma, 1);

            LinePlot lp = new LinePlot(fluxes, phases);

            

            plot2.Add(lp);
            plot2.Title = "Light Curve";
            plot2.XAxis1.Label = "Phase";
            plot2.YAxis1.Label = "Intensity";
            plot2.Refresh();


        }

        private double[][] gistmake(double[] xnoised, double[] xcertain, double width, int n)
        {
            // I now, it's seems ugly, but I did not have time to beatify this code;
            double[][] y = new double[2][];
            y[0] = new double[n];
            y[1] = new double[n];

            for (int i = 0; i < n; i++) y[0][i] = i * width / n - 0.5 * width;

            double diff;
            for (int i = 0; i < xnoised.Length; i++)
            {
                diff = xnoised[i] - xcertain[i];
                for (int j = 0; j < y[0].Length; j++)
                {
                    if (diff > y[0][j] && diff <= y[0][j] + width / n)
                    {
                        y[1][j] = y[1][j] + 1.0;
                    }
                }
            }

            for (int i = 0; i < y[0].Length; i++)
            {
                y[0][i] = y[0][i] + 0.5 * width / n;
            }

            return y;
        }

        private void btnSaveLightCurve_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            
            string path = saveFileDialog1.FileName;

            if (path == null || path=="") return;

            StreamWriter sw = new StreamWriter(path);

            sw.WriteLine(string.Format("S {0}", this.phases.Length));

            for (int p = 0; p < this.phases.Length; p++)
            {
                sw.WriteLine(string.Format("{0} {1}", this.phases[p], this.fluxes[p]));
            }

            sw.Flush();

            sw.Close();
        }

        private void btnSaveSurface_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            string file;
            try
            {
                file = saveFileDialog1.FileName;
            }
            catch
            {
                return;
            }

            StreamWriter sw = new StreamWriter(file);

            sw.Write(this.srfMod.ToText());
            sw.Flush();
            sw.Close();
        }

        private void btnClearPlotMod_Click(object sender, EventArgs e)
        {
            plot2.Clear();
            plot2.Refresh();
        }

        private void btnClearPlot_Click(object sender, EventArgs e)
        {
            plotLCI.Clear();
            plotLCI.Refresh();
        }


        private void btnAddGaussSpot_Click(object sender, EventArgs e)
        {
            double phi = double.Parse(txtLCG_Phi.Text.Replace(".", ",")) * Math.PI / 180.0;
            double theta = double.Parse(txtLCG_Theta.Text.Replace(".", ",")) * Math.PI / 180.0;
            double radius = double.Parse(txtLCG_Radius.Text.Replace(".", ",")) * Math.PI / 180;
            double tsp = double.Parse(txtLCG_TeffSp.Text.Replace(".", ","));
            this.srfMod.AddGaussSpot(phi, theta, radius, tsp);
        }

        private void btnSaveSurfaceForOrigin_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            string path = saveFileDialog1.FileName;
            StreamWriter sw = new StreamWriter(path);
            
            for (int i = 0; i < this.srfMod.GetNumberOfVisibleBelts(); i++)
            {
                for (int j = 0; j < this.srfMod.patch[i].Length; j++)
                {
                    sw.WriteLine(string.Format("{0}\t{1}\t{2}", 
                        this.srfMod.patch[i][j].FiCenterOnStart(),
                        this.srfMod.patch[i][j].ThetaCenterOnStart(),
                        this.srfMod.teff[i][j]).Replace(".",","));
                }
            }

            sw.Flush();
            sw.Close();
        }

        #endregion

        /*********************************************************************************/
        /***************************Light Curve Modelling*********************************/
        /*********************************************************************************/

        #region LightCurveModeling

        Star star;

        LightCurve lcMod2;

        LCM.SpotParsInitBox spotInitBoxMod2 = null;

        private void btnCreateUniformSurface_Click(object sender, EventArgs e)
        {
            double inc = double.Parse(txtIncMod2.Text.Replace(".", ",")) * Math.PI / 180;
            double tph = double.Parse(txtTphMod2.Text.Replace(".", ","));
            double logg = double.Parse(txtLoggMod2.Text.Replace(".", ","));
            this.star = new Star(inc, tph, logg);
        }

        private void ShowSpotsParametersMod2(object sender, EventArgs e)
        {
            int spotNumber = lbSpotsStackMod2.SelectedIndex;
            if (spotNumber == -1) return;

            this.txtNMod2.LostFocus -= new EventHandler(txtLCM_SpotParameterChanged);
            this.txtMMod2.LostFocus -= new EventHandler(txtLCM_SpotParameterChanged);
            this.txtPhiMod2.LostFocus -= new EventHandler(txtLCM_SpotParameterChanged);
            this.txtThetaMod2.LostFocus -= new EventHandler(txtLCM_SpotParameterChanged);
            this.txtRadiusMod2.LostFocus -= new EventHandler(txtLCM_SpotParameterChanged);
            this.txtTSpMod2.LostFocus -= new EventHandler(txtLCM_SpotParameterChanged);

            txtPhiMod2.Text = this.spotInitBoxMod2.spots[spotNumber].longitude.ToString();
            txtThetaMod2.Text = this.spotInitBoxMod2.spots[spotNumber].colatutude.ToString();
            txtRadiusMod2.Text = this.spotInitBoxMod2.spots[spotNumber].radius.ToString();
            txtTSpMod2.Text = this.spotInitBoxMod2.spots[spotNumber].teff.ToString();
            txtNMod2.Text = this.spotInitBoxMod2.spots[spotNumber].beltsCount.ToString();
            txtMMod2.Text = this.spotInitBoxMod2.spots[spotNumber].nearEquatorialPatchesCount.ToString();

            this.txtNMod2.LostFocus += new EventHandler(txtLCM_SpotParameterChanged);
            this.txtMMod2.LostFocus += new EventHandler(txtLCM_SpotParameterChanged);
            this.txtPhiMod2.LostFocus += new EventHandler(txtLCM_SpotParameterChanged);
            this.txtThetaMod2.LostFocus += new EventHandler(txtLCM_SpotParameterChanged);
            this.txtRadiusMod2.LostFocus += new EventHandler(txtLCM_SpotParameterChanged);
            this.txtTSpMod2.LostFocus += new EventHandler(txtLCM_SpotParameterChanged);
        }

        private void txtLCM_SpotParameterChanged(object sender, EventArgs e)
        {
            try
            {
                int spotNumber = lbSpotsStackMod2.SelectedIndex;
                if (lbSpotsStackMod2.SelectedIndex == -1) return;
                this.spotInitBoxMod2.spots[spotNumber].longitude = double.Parse(txtPhiMod2.Text);
                this.spotInitBoxMod2.spots[spotNumber].colatutude = double.Parse(txtThetaMod2.Text);
                this.spotInitBoxMod2.spots[spotNumber].radius = double.Parse(txtRadiusMod2.Text);
                this.spotInitBoxMod2.spots[spotNumber].teff = double.Parse(txtTSpMod2.Text);
                this.spotInitBoxMod2.spots[spotNumber].beltsCount = int.Parse(txtNMod2.Text);
                this.spotInitBoxMod2.spots[spotNumber].nearEquatorialPatchesCount = int.Parse(txtMMod2.Text);
            }
            catch
            {
                MessageBox.Show("Wrong format...", "Error...");
                return;
            }
        }

        private void btnAddUniformCircSpotMod2_Click(object sender, EventArgs e)
        {
            if (this.star == null)
            {
                MessageBox.Show("The Surface is not created.", "Error...");
                return;
            }
            if (this.spotInitBoxMod2 == null)
            {
                this.spotInitBoxMod2 = new Maper.LCM.SpotParsInitBox();
            }
            spotInitBoxMod2.AddSpot();

            lbSpotsStackMod2.Items.Clear();

            for (int i = 0; i < this.spotInitBoxMod2.SpotsNumber; i++)
            {
                lbSpotsStackMod2.Items.Add("Spot " + (i + 1).ToString());
            }

            this.lbSpotsStackMod2.SelectedIndexChanged += new EventHandler(ShowSpotsParametersMod2);
            this.txtNMod2.LostFocus += new EventHandler(txtLCM_SpotParameterChanged);
            this.txtMMod2.LostFocus += new EventHandler(txtLCM_SpotParameterChanged);
            this.txtPhiMod2.LostFocus += new EventHandler(txtLCM_SpotParameterChanged);
            this.txtThetaMod2.LostFocus += new EventHandler(txtLCM_SpotParameterChanged);
            this.txtRadiusMod2.LostFocus += new EventHandler(txtLCM_SpotParameterChanged);
            this.txtTSpMod2.LostFocus += new EventHandler(txtLCM_SpotParameterChanged);

            //double phi = double.Parse(txtPhiMod2.Text.Replace(".", ",")) * Math.PI / 180;
            //double theta = double.Parse(txtThetaMod2.Text.Replace(".", ",")) * Math.PI / 180;
            //double radius = double.Parse(txtRadiusMod2.Text.Replace(".", ",")) * Math.PI / 180;
            //double tsp = double.Parse(txtTSpMod2.Text);
            //int n = int.Parse(txtNMod2.Text);
            //int m = int.Parse(txtMMod2.Text);
            //this.star.AddUniformCircularSpot(phi, theta, radius, tsp, n, m);
        }

        private void btnDeleteSpotMod2_Click(object sender, EventArgs e)
        {
            int num = lbSpotsStackMod2.SelectedIndex;
            if (num != -1)
            {
                this.spotInitBoxMod2.DelSpot(num);
            }

            lbSpotsStackMod2.Items.Clear();

            for (int i = 0; i < this.spotInitBoxMod2.SpotsNumber; i++)
            {
                lbSpotsStackMod2.Items.Add("Spot " + (i + 1).ToString());
            }

            this.ShowSpotsParametersMod2(sender, e);
        }

        private void btnGenerateLightCurveMod2_Click(object sender, EventArgs e)
        {
            // Addition of spots to the surface;
            for (int s = 0; s < this.spotInitBoxMod2.SpotsNumber; s++)
            {
                this.star.AddUniformCircularSpot(
                    this.spotInitBoxMod2.spots[s].longitude * Math.PI / 180,
                    this.spotInitBoxMod2.spots[s].colatutude * Math.PI / 180,
                    this.spotInitBoxMod2.spots[s].radius * Math.PI / 180,
                    this.spotInitBoxMod2.spots[s].teff,
                    this.spotInitBoxMod2.spots[s].beltsCount,
                    this.spotInitBoxMod2.spots[s].nearEquatorialPatchesCount);
            }

            string filter = txtFilterMod2.Text;
            //int ldmod = int.Parse(txtLDModel.Text);
            int pointcount = int.Parse(txtPointsCountMod2.Text);
            double sigma = Convert.ToDouble(txtLCG_Sigma.Text.Replace(".", ","));
            double logg = double.Parse(txtLoggMod2.Text.Replace(".", ","));
            //double scale = double.Parse(txtScale.Text.Replace(".", ","));

            // Logging
            txtMMod2.AppendText(string.Format("\r\nGeneral parameters: Inc[deg] = {0:0.000}; Tph[K] = {1:00000.00}; logg[dex] = {2:00.00}",
                this.star.Inc, this.star.TeffPhot, logg));


            // Achtung!!! Gleich filterID fur Flussen und Koeffizienten asnutzen.
            int filterID = 0;
            switch (filter)
            {
                case "U": filterID = 0; break;
                case "B": filterID = 1; break;
                case "V": filterID = 2; break;
                case "R": filterID = 3; break;
                case "I": filterID = 4; break;
            }

            Table3D tabInt = new Table3D(Application.StartupPath + @"\Data\NormSpInt\UBVRI.dat");
            //Table3D tabLDC = new Table3D(Application.StartupPath + @"\Data\LDC\UBVRI.dat");
            Spline32D spliner2d = new Spline32D(tabInt.YMas, tabInt.XMas, tabInt.FMas[filterID]);
            double[] teffInt, intInt;
            teffInt = new double[tabInt.YMas.Length];
            intInt = new double[tabInt.YMas.Length];
            for (int i = 0; i < tabInt.YMas.Length; i++)
            {
                intInt[i] = spliner2d.Interp(tabInt.YMas[i], logg);
                teffInt[i] = tabInt.YMas[i];
            }

            Spline31D splineInt;
            splineInt = new Spline31D(teffInt, intInt);

            phases = new double[pointcount];
            for (int i = 0; i < phases.Length; i++)
            {
                phases[i] = i / (double)pointcount;
            }

            LCModeller lcm = new LCModeller(this.star, splineInt);

            this.lcMod2 = new LightCurve();
            this.lcMod2.Fluxes = lcm.GetLightCurve(phases, 0.725, 0.725, 1.0);
            this.lcMod2.Phases = phases;

            LinePlot lp = new LinePlot(this.lcMod2.Fluxes, this.lcMod2.Phases);
            plotMod2.Add(lp);
            plotMod2.Title = "Light Curve";
            plotMod2.XAxis1.Label = "Phase";
            plotMod2.YAxis1.Label = "Flux";
            plotMod2.Refresh();
        }

        private void btnGenerateLightCurve2Mod2_Click(object sender, EventArgs e)
        {
            string filter;
            double logg;
            int pointsCount;
            double sigma;

            try
            {
                filter = this.txtFilterMod2.Text;
                sigma = double.Parse(txtLCG_Sigma.Text.Replace(".", ","));
                pointsCount = int.Parse(txtPointsCountMod2.Text.Replace(".", ","));
                logg = double.Parse(txtLoggMod2.Text.Replace(".", ","));
            }
            catch
            {
                MessageBox.Show("Some error in input data format...", "Error...");
                return;
            }

            // Logging
            txtResultMod2.AppendText(string.Format("\r\nGeneral parameters: Inc[deg] = {0:0.000}; Tph[K] = {1:0.00}; logg[dex] = {2:0.00}",
                this.star.Inc * 180 / Math.PI, this.star.TeffPhot, logg));

            // Addition of spots to the surface;
            this.star.RemoveAllSpots();
            for (int s = 0; s < this.spotInitBoxMod2.SpotsNumber; s++)
            {
                this.star.AddUniformCircularSpot(
                    this.spotInitBoxMod2.spots[s].longitude * Math.PI / 180,
                    this.spotInitBoxMod2.spots[s].colatutude * Math.PI / 180,
                    this.spotInitBoxMod2.spots[s].radius * Math.PI / 180,
                    this.spotInitBoxMod2.spots[s].teff,
                    this.spotInitBoxMod2.spots[s].beltsCount,
                    this.spotInitBoxMod2.spots[s].nearEquatorialPatchesCount);
            }

            // Logging;
            for (int s = 0; s < this.star.circSpots.Length; s++)
            {
                txtResultMod2.AppendText(string.Format("\r\nSpot {0} parameters:", s + 1));
                txtResultMod2.AppendText(string.Format("\r\nPhi[deg] = {0:0.000}; Theta[deg] = {1:0.000}; Radius[deg] = {2:0.000}; Teff[K] = {3:0.00}",
                    this.star.circSpots[s].PhiOfSpotCenterAtZeroPhase * 180 / Math.PI,
                    this.star.circSpots[s].ThetaOfSpotCenter * 180 / Math.PI,
                    this.star.circSpots[s].Radius * 180 / Math.PI,
                    this.star.circSpots[s].Teff));
            }
            IntensityProvider1D ip1d = new IntensityProvider1D(Application.StartupPath + @"\Data\NormSpInt\UBVRIRcIcJK.dat",
                new string[9] { "U", "B", "V", "R", "I", "Rc", "Ic", "J", "K" }, logg);
            IntensityProvider1D ldcp1d = new IntensityProvider1D(Application.StartupPath + @"\Data\LDC\UBVRI.dat",
                new string[7] { "U", "B", "V", "R", "I", "Rc", "Ic" }, logg);
            LinearLimbDarkeningLow lldl = new LinearLimbDarkeningLow(ldcp1d);

            ldcp1d.FixFilter = filter;
            ip1d.FixFilter = filter;
            lldl.FixedFilter = filter;

            double unspottedBrightness = Math.PI * ip1d.GetIntensityForFixedFilter(this.star.TeffPhot) *
                (1.0 - ldcp1d.GetIntensityForFixedFilter(this.star.TeffPhot) / 3.0);

            LCM.LCModeller1 lcm = new Maper.LCM.LCModeller1(this.star, lldl.GetLinerLimbDarkeningCoefficient,
                ip1d.GetIntensityForFixedFilter, unspottedBrightness);


            double[] phases = new double[pointsCount];
            for (int p = 0; p < pointsCount; p++)
            {
                phases[p] = (double)p / pointsCount;
            }

            this.lcMod2 = new LightCurve();

            Counter counter = new Counter();
            counter.Start();
            this.lcMod2.Fluxes = lcm.GetFluxes(phases, 1.0);
            counter.Stop();

            txtResultMod2.AppendText(string.Format(
                "\r\nComputing of the light curve was completed in {0} sec.", counter.TotalSeconds));

            this.lcMod2.Phases = phases;

            LinePlot lp = new LinePlot(this.lcMod2.Fluxes, this.lcMod2.Phases);
            plotMod2.Add(lp);
            plotMod2.Title = "Light Curve";
            plotMod2.XAxis1.Label = "Phase";
            plotMod2.YAxis1.Label = "Flux";
            plotMod2.Refresh();
            plotMod2.CopyToClipboard();
        }

        private void btnSaveLightCurveMod2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            string file = saveFileDialog1.FileName;

            StreamWriter sw = new StreamWriter(file);
            for (int i = 0; i < this.lcMod2.Phases.Length; i++)
            {
                sw.WriteLine(string.Format("{0:0.0000} {1:0.0000E000}", this.lcMod2.Phases[i], this.lcMod2.Fluxes[i]));
            }
            sw.Flush();
            sw.Close();
        }

        private void btnShowMap_Click(object sender, EventArgs e)
        {
            if (this.star == null) return;
            
            MapView mv = new MapView();

            Star star1 = this.star.Clone();

            if (this.spotInitBoxMod2 != null)
            {
                for (int s = 0; s < this.spotInitBoxMod2.SpotsNumber; s++)
                {
                    star1.AddUniformCircularSpot(
                        this.spotInitBoxMod2.spots[s].longitude * Math.PI / 180,
                        this.spotInitBoxMod2.spots[s].colatutude * Math.PI / 180,
                        this.spotInitBoxMod2.spots[s].radius * Math.PI / 180,
                        this.spotInitBoxMod2.spots[s].teff,
                        this.spotInitBoxMod2.spots[s].beltsCount,
                        this.spotInitBoxMod2.spots[s].nearEquatorialPatchesCount);
                }
            }

            mv.SetStar(star1);
            mv.Show();
        }

        private void btnLCM_ShowSpot_Click(object sender, EventArgs e)
        {
            int spotNumber = lbSpotsStackMod2.SelectedIndex;
            if (lbSpotsStackMod2.SelectedIndex == -1) return;
            CircSpot spot = new CircSpot(
                    this.spotInitBoxMod2.spots[spotNumber].longitude * Math.PI / 180,
                    this.spotInitBoxMod2.spots[spotNumber].colatutude * Math.PI / 180,
                    this.spotInitBoxMod2.spots[spotNumber].radius * Math.PI / 180,
                    this.spotInitBoxMod2.spots[spotNumber].beltsCount,
                    this.spotInitBoxMod2.spots[spotNumber].nearEquatorialPatchesCount);
            DiskViewer dw = new DiskViewer();
            dw.Init(spot.Coords());
            dw.Show();
        }

        private void btnLCM_ClearGraph_Click(object sender, EventArgs e)
        {
            plotMod2.Clear();
            plotMod2.Refresh();
        }

        #endregion

        /*********************************************************************************/
        /**************************Spectrum Generation Code*******************************/
        /*********************************************************************************/

        #region SpectrumGenerationCode

        SpectrGrid sg;

        private void btnLoadSpectrumGrid_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string path = openFileDialog1.FileName;
            this.spectrumGrid = new SpectrGrid(path);
        }


        /********************************************************************/


        #endregion

        /*********************************************************************************/
        /********************************Map Analysing Code*******************************/
        /*********************************************************************************/

        #region MapAnalysing

        TSurface tsrfForAnalyss;
        Spline32D smoothDistr;

        private void btnInterpolate_Click(object sender, EventArgs e)
        {
            double polarValue = 0;
            for (int i = 0; i < this.tsrfForAnalyss.patch[0].Length; i++)
            {
                polarValue = polarValue + this.tsrfForAnalyss.teff[0].Length;
            }

            double dphi = 5 * Math.PI / 180.0;

            Spline31D sp31d;
            PolynomInterpolator polyInt;
            double[] x0;
            double[] y0;

            for (int i = 1; i < tsrfForAnalyss.GetNumberOfVisibleBelts(); i++)
            {
                x0=new double[tsrfForAnalyss.patch[i].Length];
                y0 = new double[x0.Length];
                for (int j = 0; j < x0.Length; j++)
                {
                    x0[i] = tsrfForAnalyss.patch[i][j].FiCenterOnStart();
                    y0[i] = tsrfForAnalyss.teff[i][j];
                }

                sp31d = new Spline31D(x0, y0);
            }
        }

        private void btnViewer3D_Click(object sender, EventArgs e)
        {
            if (tsrfForAnalyss == null) return;
            float[][][] mas = tsrfForAnalyss.GetPatchCoordMas1();
            Viewer3D viewer3d = new Viewer3D(mas[0], mas[1][0]);
            //viewer3d.Setup();
            //viewer3d.Initialize();

            CsGL.Basecode.App.Run(viewer3d);
        }

        private void btnLoadSrfForAnalizis_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string path = openFileDialog1.FileName;
            this.tsrfForAnalyss = new TSurface(path);
        }

        private void btnUTLS_LoadLightCurve_Click(object sender, EventArgs e)
        {

        }

        private void btnUTLS_ShowSurface_Click(object sender, EventArgs e)
        {
            if (this.tsrfForAnalyss == null)
            {
                MessageBox.Show("The surface was not initialized...", "Error...");
                return;
            }

            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.Blue;
            svf.color1 = Color.Red;
            svf.Init(this.tsrfForAnalyss.GetPatchCoordMas(), this.tsrfForAnalyss.GetTeffMas());
            svf.Show();
        }

        private void btnUTLS_IntMapToTeffMap_Click(object sender, EventArgs e)
        {

            //Spline31D spline_int = new Spline31D(
            //    new double[] {3500, 3750, 4000, 4250, 4500, 4750, 5000, 5250, 5500, 5750, 6000},
            //    new double[] {7.81908e007, 1.82271e008, 3.30485e008, 
            //                  5.29609e008, 7.87241e008, 1.10112e009,	
            //                  1.46731e009, 1.88604e009,	2.36006e009, 2.89795e009, 3.50708e009 });

            //vTSurface tsrf = new TSurface(this.tsrfForAnalyss.GetN(), this.tsrfForAnalyss.GetM(),
            //    this.tsrfForAnalyss.GetInc(), 5000);

            //for (int i = 0; i < tsrf.teff.Length; i++)
            //{
            //    for (int j = 0; j < tsrf.teff[i].Length; j++)
            //    {
            //        tsrf.teff[i][j] = spline_int.Interp(this.tsrfForAnalyss.teff[i][j]);
            //    }
            //}

            //SurfaceViewerForm svf = new SurfaceViewerForm();
            //svf.color0 = Color.Blue;
            //svf.color1 = Color.Red;
            //svf.Init(tsrf.GetPatchCoordMas(), tsrf.GetTeffMas());
            //svf.Show();

            //// saving

            //StreamWriter sw = new StreamWriter(Application.StartupPath + "\\teff_map.dat");
            //sw.Write(tsrf.ToText());
            //sw.Flush();
            //sw.Close();
        }

        #endregion

        /*********************************************************************************/
        /********************************DOPPLER IMAGING**********************************/
        /*********************************************************************************/

        #region DopplerImaging

        double[][] obsSpectrums;
        double[][] modSpectrums;
        double[] obsPhase;
        double[][] obsLambda;
        SpectrGrid spectrumGrid;
        TSurface srfDopp;

        private void ReadSpectrumFile_FDI(string path)
        {
            StreamReader sr = new StreamReader(path);
            int n, m;
            double phase;
            string str;
            string[] strMas;
            string[] stringSeparators = new string[] { " ", "\t" };
            str = sr.ReadLine();
            n = int.Parse(str);
            this.obsSpectrums = new double[n][];
            this.obsPhase = new double[n];
            this.obsLambda = new double[n][];
            for (int i = 0; i < n; i++)
            {
                str = sr.ReadLine();
                strMas = str.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                phase = double.Parse(strMas[0]);
                m = int.Parse(strMas[1]);
                this.obsSpectrums[i] = new double[m];
                this.obsPhase[i] = phase;
                this.obsLambda[i] = new double[m];
                for (int j = 0; j < m; j++)
                {
                    str = sr.ReadLine();
                    strMas = str.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    this.obsLambda[i][j] = double.Parse(strMas[0]);
                    this.obsSpectrums[i][j] = double.Parse(strMas[1]);
                }
            }
        }

        private void btnAddSpectrums_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string path = openFileDialog1.FileName;
            try
            {
                this.ReadSpectrumFile_FDI(path);
                txtOutputDI.AppendText("Spectrums were read successfully...\r\n");
            }
            catch
            {
                txtOutputDI.AppendText("Some error in file reading...\r\n");
                return;
            }
        }

        private void btnLoadIntensityGrid_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string path = openFileDialog1.FileName;
            try
            {
                this.spectrumGrid = new SpectrGrid(path);
                txtOutputDI.AppendText("Intensity grid was read successfully...\r\n");
            }
            catch
            {
                txtOutputDI.AppendText("Some error in file reading...\r\n");
                return;
            }
        }

        private void SetupPlotSpectrum()
        {
            FontFamily ff = new FontFamily("Times New Roman");
            plotSpectrum.YAxis1.LabelFont = new Font(ff, 12);
            plotSpectrum.XAxis1.LabelFont = new Font(ff, 12);
            plotSpectrum.XAxis1.TickTextFont = new Font(ff, 10);
            plotSpectrum.YAxis1.TickTextFont = new Font(ff, 10);
            plotSpectrum.TitleFont = new Font(ff, 15);
            plotSpectrum.XAxis1.Label = "Wavelength";
            plotSpectrum.YAxis1.Label = "Intensity";
            plotSpectrum.Title = "Spectrum";
            plotSpectrum.Refresh();
        }

        private void btnDoppMappingGo_Click(object sender, EventArgs e)
        {
            int n = int.Parse(txtN_FDI.Text);
            int m = int.Parse(txtM_FDI.Text);
            double inc = double.Parse(txtInc_FDI.Text.Replace(".", ","));
            inc = inc * Math.PI / 180.0;
            double veq = double.Parse(txtVeq_FDI.Text.Replace(".", ","));
            double regpar = double.Parse(txtRegPar_FDI.Text.Replace(".", ","));

            Surface srf = new Surface(n, m, inc);

            TSurface tsrf;
            double[][] phIntLineGrid = this.spectrumGrid.IntenLineGrid[0];
            double[][] phIntContGrid = this.spectrumGrid.IntenContGrid[0];
            double[][] spIntLineGrid = this.spectrumGrid.IntenLineGrid[1];
            double[][] spIntContGrid = this.spectrumGrid.IntenContGrid[1];

            DoppImager di = new DoppImager(srf, phIntLineGrid, spIntLineGrid, phIntContGrid, spIntContGrid,
                this.spectrumGrid.Mu,
                this.spectrumGrid.Lambda,
                this.obsSpectrums,
                this.obsPhase,
                this.obsLambda);

            di.DoppImGo(veq * Math.Sin(inc), regpar);

            this.srfDopp = di.RestoredSurface;
            this.modSpectrums = di.ModelSpectrumGrid;
            // Visualization of reconstructed surface;
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.Init(this.srfDopp.GetPatchCoordMas(), this.srfDopp.GetTeffMas());
            svf.color1 = Color.Black; svf.color0 = Color.White;
            svf.Show();
        }

        private void btnShowSpectrum_Click(object sender, EventArgs e)
        {
            int spNum = int.Parse(txtSpectrumNumber.Text);
            if (spNum >= this.obsPhase.Length)
            {
                MessageBox.Show("The spectrum's number is more than upper limit.", "Error...");
                return;
            }
            PointPlot pp = new PointPlot();
            pp.AbscissaData = obsLambda[spNum];
            pp.OrdinateData = obsSpectrums[spNum];
            pp.Marker = new Marker(Marker.MarkerType.FilledCircle);
            plotSpectrum.Add(pp);
            this.SetupPlotSpectrum();
        }

        private void btnShowModelSpectrum_Click(object sender, EventArgs e)
        {
            int spNum = int.Parse(txtSpectrumNumber.Text);
            if (spNum >= this.obsPhase.Length)
            {
                MessageBox.Show("The spectrum's number is more than upper limit.", "Error...");
                return;
            }
            LinePlot lp = new LinePlot();
            lp.Color = Color.Red;
            lp.AbscissaData = obsLambda[spNum];
            lp.OrdinateData = modSpectrums[spNum];
            plotSpectrum.Add(lp);
            this.SetupPlotSpectrum();
        }

        private void btnReadInputDI_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            txtOutputDI.AppendText("Reading the input file...");
            InputDI idi = new InputDI(openFileDialog1.FileName);
            txtOutputDI.AppendText("\r\nReading error string. \r\n" + idi.ErrorString);
            if (idi.Errors) return;

            txtOutputDI.AppendText("\r\nReading the observed spectrums file...");
            InputObsSpectra ios = new InputObsSpectra(idi.PathObs);
            txtOutputDI.AppendText("\r\nReading error string. \r\n" + ios.ErrorString);
            if (ios.Errors) return;
        }

        private void btnSaveDopplerImage_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            StreamWriter sw = new StreamWriter(saveFileDialog1.FileName);
            sw.Write(this.srfDopp.ToText());
            sw.Flush();
            sw.Close();
        }

        private void btnClearGraph_FDI_Click(object sender, EventArgs e)
        {
            plotSpectrum.Clear();
            plotSpectrum.Refresh();
        }

        #endregion

        /************************************************************************************/
        /******************************CIRCULAR SPOTS MAPPING********************************/
        /************************************************************************************/


        private LCM.LCContainer lcboxLCM = new Maper.LCM.LCContainer();

        private LCM.SpotParsInitBox spotsParsInitBox = new Maper.LCM.SpotParsInitBox();

        private LCM.MultiSpotMapper msm;

        private LCM.MultiSpotMapper1 msm1;

        private void refreshDGV()
        {
            for ( ; dgvLightCurves.Rows.Count!=0; )
            {
                dgvLightCurves.Rows.RemoveAt(0);
            }

            string[] str = new string[dgvLightCurves.Columns.Count];

            for (int i = 0; i < this.lcboxLCM.LightCurvesNumber; i++)
            {
                str[0] = (i + 1).ToString();
                str[1] = this.lcboxLCM.LightCurves[i].Band;
                str[2] = this.lcboxLCM.LightCurves[i].Phases.Length.ToString();
                str[3] = this.lcboxLCM.LightCurves[i].Sigma.ToString();
                str[4] = this.lcboxLCM.LightCurves[i].FluxMax.ToString();
                dgvLightCurves.Rows.Add(str);
            }
        }

        private void btnAddLightCurveLCM_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string path = openFileDialog1.FileName;
            try
            {
                this.lcboxLCM.AddLightCurve(path);
            }
            catch
            {
                MessageBox.Show("No such file or an error in file structure...", "Error");
                return;
            }
            this.refreshDGV();
            this.dgvLightCurves.CellValueChanged += new DataGridViewCellEventHandler(dgvLightCurves_CellValueChanged);
        }

        private void btnDeleteLightCurveLCM_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvLightCurves.Rows.Count; i++)
            {
                if (dgvLightCurves.Rows[i].Selected) this.lcboxLCM.DelLightCurve(i);
            }
            this.refreshDGV();
        }

        private void btnShowLightCurveLCM_Click(object sender, EventArgs e)
        {
            int lcNumber = 0;
            try
            {
                lcNumber = int.Parse(txtILCM_LightCurveNumber.Text);
            }
            catch
            {
                MessageBox.Show("Check the input light curve number...", "Error...");
            }

            LinePlot lp = new LinePlot();
            PointPlot pp = new PointPlot();

            try
            {
                lp.AbscissaData = this.msm1.GetModelLightCurves[lcNumber].Phases;
                lp.OrdinateData = this.msm1.GetModelLightCurves[lcNumber].Fluxes;

                pp.AbscissaData = this.lcboxLCM.LightCurves[lcNumber].Phases;
                pp.OrdinateData = this.lcboxLCM.LightCurves[lcNumber].Fluxes;
            }
            catch
            {
                return;
            }
            
            lp.Color = Color.Red;
            pp.Marker.Type = Marker.MarkerType.FilledCircle;
            

            plotILCM.Add(lp);
            plotILCM.Add(pp);

            plotILCM.Title = "Light Curve";
            plotILCM.XAxis1.Label = "Phase";
            plotILCM.YAxis1.Label = "Flux";

            plotILCM.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotILCM.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotILCM.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotILCM.Refresh();
        }

        private void dgvLightCurves_CellValueChanged(object sender, EventArgs e)
        {
            for (int q = 0; q < this.lcboxLCM.LightCurvesNumber; q++)
            {
                this.lcboxLCM.LightCurves[q].Sigma = double.Parse(dgvLightCurves[3, q].Value.ToString().Replace(".", ","));
                this.lcboxLCM.LightCurves[q].FluxMax = double.Parse(dgvLightCurves[4, q].Value.ToString().Replace(".", ","));
            }
        }

        private void btnSolveLCM_Click(object sender, EventArgs e)
        {
            double inc = double.Parse(txtILCM_Inc.Text.Replace(".", ",")) * Math.PI / 180.0;
            double tPh = double.Parse(txtILCM_TPh.Text.Replace(".", ","));
            double logg = double.Parse(txtILCM_Logg.Text.Replace(".", ","));

            Spline32D spliner2d;
            Spline31D[] splineInt = new Spline31D[lcboxLCM.LightCurvesNumber];
            Table3D tabInt = new Table3D(Application.StartupPath + @"\Data\NormSpInt\UBVRI.dat");
            for (int q = 0; q < lcboxLCM.LightCurvesNumber; q++)
            {
                int filterID = 0;
                switch (lcboxLCM.LightCurves[q].Band)
                {
                    case "U": filterID = 0; break;
                    case "B": filterID = 1; break;
                    case "V": filterID = 2; break;
                    case "R": filterID = 3; break;
                    case "I": filterID = 4; break;
                }
                spliner2d = new Spline32D(tabInt.YMas, tabInt.XMas, tabInt.FMas[filterID]);
                double[] teffInt, intInt;
                teffInt = new double[tabInt.YMas.Length];
                intInt = new double[tabInt.YMas.Length];
                for (int i = 0; i < tabInt.YMas.Length; i++)
                {
                    intInt[i] = spliner2d.Interp(tabInt.YMas[i], logg);
                    teffInt[i] = tabInt.YMas[i];
                }
                splineInt[q] = new Spline31D(teffInt, intInt);
            }

            Star star = new Star(inc, tPh, logg);

            LightCurve[] lcs = this.lcboxLCM.LightCurves;


            msm = new Maper.LCM.MultiSpotMapper(star, splineInt, lcs);

            double[] x;

            x = msm.StartMapping(this.spotsParsInitBox);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < x.Length / 4; i++)
            {
                sb.AppendFormat("\r\nSpot {0} parameters:\r\n Long={1:F2}, Lat={2:F2}, Rad={3:F2}, Teff={4:F0}",
                    i + 1, x[0 + i * 4] * 180 / Math.PI, x[1 + i * 4] * 180 / Math.PI,
                    x[2 + i * 4] * 180 / Math.PI, x[3 + i * 4]);
            }
            txtILCM_Results.AppendText(sb.ToString());
        }

        private void btnSolve2LCM_Click(object sender, EventArgs e)
        {
            double inc;
            double tPh;
            double logg;
            try
            {
                inc = double.Parse(txtILCM_Inc.Text.Replace(".", ",")) * Math.PI / 180.0;
                tPh = double.Parse(txtILCM_TPh.Text.Replace(".", ","));
                logg = double.Parse(txtILCM_Logg.Text.Replace(".", ","));
            }
            catch
            {
                MessageBox.Show("Some error in input data format...", "Error...");
                return;
            }
            if (this.spotsParsInitBox == null)
            {
                MessageBox.Show("No one spot has been set...", "Error...");
                return;
            }
            if (this.spotsParsInitBox.SpotsNumber == 0)
            {
                MessageBox.Show("No one spot has been set...", "Error...");
                return;
            }

            txtILCM_Results.AppendText("\r\nPreparing for stellar surface mapping...");

            IntensityProvider1D[] ip1d = new IntensityProvider1D[lcboxLCM.LightCurvesNumber];
            IntensityProvider1D[] ldcp1d = new IntensityProvider1D[lcboxLCM.LightCurvesNumber];
            LinearLimbDarkeningLow[] lldl = new LinearLimbDarkeningLow[lcboxLCM.LightCurvesNumber];

            double[] unspottedBrightness = new double[lcboxLCM.LightCurvesNumber];

            for (int q = 0; q < lcboxLCM.LightCurvesNumber; q++)
            {
                ip1d[q] = new IntensityProvider1D(Application.StartupPath + @"\Data\NormSpInt\UBVRIRcIcJK.dat",
                    new string[9] { "U", "B", "V", "R", "I", "Rc", "Ic", "J", "K" }, logg);
                ldcp1d[q] = new IntensityProvider1D(Application.StartupPath + @"\Data\LDC\UBVRI.dat",
                    new string[7] { "U", "B", "V", "R", "I", "Rc", "Ic" }, logg);
                ldcp1d[q].FixFilter = lcboxLCM.LightCurves[q].Band;

                double[] teffSet = new double[this.spotsParsInitBox.spots.Length + 1];
                teffSet[0] = tPh;
                for (int i = 1; i < spotsParsInitBox.spots.Length+1; i++)
                {
                    teffSet[i] = this.spotsParsInitBox.spots[i - 1].teff;
                }

                ldcp1d[q].TeffSet = teffSet;

                lldl[q] = new LinearLimbDarkeningLow(ldcp1d[q]);

                ip1d[q].FixFilter = lcboxLCM.LightCurves[q].Band;

                lldl[q].FixedFilter = lcboxLCM.LightCurves[q].Band;


                txtILCM_Results.AppendText("\r\nNormal model intensity for " + ip1d[q].FixFilter +
                    " filter: " + string.Format("{0:0.0000E000}", ip1d[q].GetIntensityForFixedFilter(tPh)));
                txtILCM_Results.AppendText("\r\nLimb darkening parameter for " + ip1d[q].FixFilter +
                    " filter: " + string.Format("{0:0.000}", ldcp1d[q].GetIntensityForFixedFilter(tPh)));

                unspottedBrightness[q] = Math.PI * ip1d[q].GetIntensityForFixedFilter(tPh) *
                    (1.0 - ldcp1d[q].GetIntensityForFixedFilter(tPh) / 3.0);

                txtILCM_Results.AppendText("\r\nUnspotted model brightness for " + ldcp1d[q].FixFilter +
                    " filter: " + string.Format("{0:0.0000E000}", unspottedBrightness[q]));
            }

            txtILCM_Results.AppendText("\r\nInitialization of star model ( Inc[deg] = " +
                (inc * 180 / Math.PI).ToString() + ", Tph[K] = " + tPh.ToString()
                + ", logg[dex] = " + logg.ToString() + ".)");
            
            Star star = new Star(inc, tPh, logg);

            //for (int i = 0; i < spotsParsInitBox.spots.Length; i++)
            //{
            //    star.AddUniformCircularSpot(0.0, 0.0, 1.0, 4000, this.spotsParsInitBox.spots[i].beltsCount,
            //        this.spotsParsInitBox.spots[i].nearEquatorialPatchesCount);
            //}

            LCM.LCModeller1[] modellers = new Maper.LCM.LCModeller1[lcboxLCM.LightCurvesNumber];

            for (int q = 0; q < modellers.Length; q++)
            {
                modellers[q] = new Maper.LCM.LCModeller1(star, lldl[q].GetLinerLimbDarkeningCoefficient /*lldl[q].GetLinerLimbDarkeningCoefficient*/,
                    ip1d[q].GetIntensityForFixedFilter, unspottedBrightness[q]);
            }

            msm1 = new Maper.LCM.MultiSpotMapper1(modellers, lcboxLCM.LightCurves);

            double[] x;

            double tau = 0.0;
            string minimizator = ""; ;

            if (rbILCM_MinimizationWithNM.Checked)
            {
                minimizator = "Simplex";
            }
            if (rbILCM_MinimizationWithTN.Checked)
            {
                minimizator = "TN";
            }
            if (rbILCM_MinimizationWithLM.Checked)
            {
                minimizator = "LM";
                try
                {
                    tau = double.Parse(txtILCM_Tau.Text.Replace(".", ","));
                }
                catch
                {
                    MessageBox.Show("Check the format of the input tau-parameter...", "Error...");
                    return;
                }
            }

            txtILCM_Results.AppendText("\r\nStart mapping using " + minimizator + " algorithm...");

            Counter counter = new Counter();
            counter.Start();
            x = msm1.StartMapping(this.spotsParsInitBox, minimizator, tau);
            counter.Stop();

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < x.Length / 4; i++)
            {
                sb.AppendFormat("\r\nSpot {0} parameters:\r\n Long[deg] = {1:F2}, CoLat[deg] = {2:F2}, Rad[deg] = {3:F2}, Teff[K] = {4:F0}",
                    i + 1, x[0 + i * 4] * 180 / Math.PI, x[1 + i * 4] * 180 / Math.PI,
                    x[2 + i * 4] * 180 / Math.PI, x[3 + i * 4]);
            }
            txtILCM_Results.AppendText(sb.ToString());
            txtILCM_Results.AppendText(string.Format("\r\nKhi2 value for found solution: {0:0.000E000}", msm1.Khi2Value));
            txtILCM_Results.AppendText(string.Format("\r\nTime of computations: {0:0.00} sec", counter.TotalSeconds));
        }

        private void ShowSpotsInitData(object sender, EventArgs e)
        {
            int num = lbILCM_SpotsStack.SelectedIndex;
            dgvILCM_SpotPars.Rows.Clear();
            this.dgvILCM_SpotPars.CellValueChanged -= new DataGridViewCellEventHandler(dgvSpots_CellValueChanged);
            if (num != -1)
            {
                dgvILCM_SpotPars.Rows.Clear();
                dgvILCM_SpotPars.Rows.Add(4);

                dgvILCM_SpotPars[0, 0].Value = "Longitude [deg]";
                dgvILCM_SpotPars[0, 1].Value = "CoLatitude [deg]";
                dgvILCM_SpotPars[0, 2].Value = "Radius [deg]";
                dgvILCM_SpotPars[0, 3].Value = "Teff [K]";

                dgvILCM_SpotPars[1, 0].Value = spotsParsInitBox.spots[num].longitude;
                dgvILCM_SpotPars[1, 1].Value = spotsParsInitBox.spots[num].colatutude;
                dgvILCM_SpotPars[1, 2].Value = spotsParsInitBox.spots[num].radius;
                dgvILCM_SpotPars[1, 3].Value = spotsParsInitBox.spots[num].teff;

                dgvILCM_SpotPars[2, 0].Value = spotsParsInitBox.spots[num].longitudeUpperLimit;
                dgvILCM_SpotPars[2, 1].Value = spotsParsInitBox.spots[num].colatitudeUpperLimit;
                dgvILCM_SpotPars[2, 2].Value = spotsParsInitBox.spots[num].radiusUpperLimit;
                dgvILCM_SpotPars[2, 3].Value = spotsParsInitBox.spots[num].teffUpperLimit;

                dgvILCM_SpotPars[3, 0].Value = spotsParsInitBox.spots[num].longitudeLowerLimit;
                dgvILCM_SpotPars[3, 1].Value = spotsParsInitBox.spots[num].colatitudeLowerLimit;
                dgvILCM_SpotPars[3, 2].Value = spotsParsInitBox.spots[num].radiusLowerLimit;
                dgvILCM_SpotPars[3, 3].Value = spotsParsInitBox.spots[num].teffLowerLimit;

                dgvILCM_SpotPars[4, 0].Value = spotsParsInitBox.spots[num].longitudeFixed;
                dgvILCM_SpotPars[4, 1].Value = spotsParsInitBox.spots[num].colatitudeFixed;
                dgvILCM_SpotPars[4, 2].Value = spotsParsInitBox.spots[num].radiusFixed;
                dgvILCM_SpotPars[4, 3].Value = spotsParsInitBox.spots[num].teffFixed;

                this.txtILCM_N.Text = spotsParsInitBox.spots[num].beltsCount.ToString();
                this.txtILCM_M.Text = spotsParsInitBox.spots[num].nearEquatorialPatchesCount.ToString();
            }
            
            this.dgvILCM_SpotPars.CellValueChanged += new DataGridViewCellEventHandler(dgvSpots_CellValueChanged);
        }

        private void btnAddSpotLCM_Click(object sender, EventArgs e)
        {
            spotsParsInitBox.AddSpot();

            lbILCM_SpotsStack.Items.Clear();


            for (int i = 0; i < this.spotsParsInitBox.SpotsNumber; i++)
            {
                lbILCM_SpotsStack.Items.Add("Spot " + (i + 1).ToString());
            }

            this.lbILCM_SpotsStack.SelectedIndexChanged += new EventHandler(ShowSpotsInitData);
            this.txtILCM_N.TextChanged += new EventHandler(txtILCM_N_TextChanged);
            this.txtILCM_M.TextChanged += new EventHandler(txtILCM_M_TextChanged);
        }

        private void txtILCM_N_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = lbILCM_SpotsStack.SelectedIndex;
                this.spotsParsInitBox.spots[num].beltsCount = int.Parse(txtILCM_N.Text);
            }
            catch
            {
                MessageBox.Show("Wrong format...", "Error...");
                return;
            }
        }

        private void txtILCM_M_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = lbILCM_SpotsStack.SelectedIndex;
                this.spotsParsInitBox.spots[num].nearEquatorialPatchesCount = int.Parse(txtILCM_M.Text);
            }
            catch
            {
                MessageBox.Show("Wrong format...", "Error...");
                return;
            }
        }

        private void dgvSpots_CellValueChanged(object sender, EventArgs e)
        {
            int num = lbILCM_SpotsStack.SelectedIndex;

            this.spotsParsInitBox.spots[num].longitude = double.Parse(dgvILCM_SpotPars[1, 0].Value.ToString().Replace(".", ","));
            this.spotsParsInitBox.spots[num].colatutude = double.Parse(dgvILCM_SpotPars[1, 1].Value.ToString().Replace(".", ","));
            this.spotsParsInitBox.spots[num].radius = double.Parse(dgvILCM_SpotPars[1, 2].Value.ToString().Replace(".", ","));
            this.spotsParsInitBox.spots[num].teff = double.Parse(dgvILCM_SpotPars[1, 3].Value.ToString().Replace(".", ","));

            this.spotsParsInitBox.spots[num].longitudeUpperLimit = double.Parse(dgvILCM_SpotPars[2, 0].Value.ToString().Replace(".", ","));
            this.spotsParsInitBox.spots[num].colatitudeUpperLimit = double.Parse(dgvILCM_SpotPars[2, 1].Value.ToString().Replace(".", ","));
            this.spotsParsInitBox.spots[num].radiusUpperLimit = double.Parse(dgvILCM_SpotPars[2, 2].Value.ToString().Replace(".", ","));
            this.spotsParsInitBox.spots[num].teffUpperLimit = double.Parse(dgvILCM_SpotPars[2, 3].Value.ToString().Replace(".", ","));

            this.spotsParsInitBox.spots[num].longitudeLowerLimit = double.Parse(dgvILCM_SpotPars[3, 0].Value.ToString().Replace(".", ","));
            this.spotsParsInitBox.spots[num].colatitudeLowerLimit = double.Parse(dgvILCM_SpotPars[3, 1].Value.ToString().Replace(".", ","));
            this.spotsParsInitBox.spots[num].radiusLowerLimit = double.Parse(dgvILCM_SpotPars[3, 2].Value.ToString().Replace(".", ","));
            this.spotsParsInitBox.spots[num].teffLowerLimit = double.Parse(dgvILCM_SpotPars[3, 3].Value.ToString().Replace(".", ","));

            this.spotsParsInitBox.spots[num].longitudeFixed = (bool)dgvILCM_SpotPars[4, 0].Value;
            this.spotsParsInitBox.spots[num].colatitudeFixed = (bool)dgvILCM_SpotPars[4, 1].Value;
            this.spotsParsInitBox.spots[num].radiusFixed = (bool)dgvILCM_SpotPars[4, 2].Value;
            this.spotsParsInitBox.spots[num].teffFixed = (bool)dgvILCM_SpotPars[4, 3].Value;
        }

        private void btnDeleteSpotLCM_Click(object sender, EventArgs e)
        {
            int num = lbILCM_SpotsStack.SelectedIndex;
            if (num != -1)
            {
                this.spotsParsInitBox.DelSpot(num);
            }

            lbILCM_SpotsStack.Items.Clear();

            for (int i = 0; i < this.spotsParsInitBox.SpotsNumber; i++)
            {
                lbILCM_SpotsStack.Items.Add("Spot " + (i + 1).ToString());
            }
            
            this.ShowSpotsInitData(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IntensityProvider2D ip2d = new IntensityProvider2D(Application.StartupPath + @"\Data\NormSpInt\UBVRI.dat",
                new string[5] { "U", "B", "V", "R", "I" });
            string output = "";
            for (int i = 0; i < 51; i++)
            {
                for (int j = 0; j < 51; j++)
                {
                    output += string.Format("{0:0.0000e000} ", ip2d.GetIntensity(0.0 + i * 0.1, 3500 + j * 50, "V"));
                }
                output += "\r\n";
            }
            StreamWriter sw = new StreamWriter(Application.StartupPath + "table.txt");
            sw.Write(output);
            sw.Flush();
            sw.Close();
        }

        private void btnShowObsLightCurveLCM_Click(object sender, EventArgs e)
        {
            int num = -1;
            for (int i = 0; i < dgvLightCurves.Rows.Count; i++)
            {
                if (dgvLightCurves.Rows[i].Selected)
                {
                    num = i;
                    break;
                }
            }
            if (num == -1)
            {
                MessageBox.Show("Choose the light curve first...", "Error...");
                return;
            }
            Marker marker = new Marker(Marker.MarkerType.FilledCircle, 4, Color.Black);
            PointPlot pp = new PointPlot(marker);
            pp.AbscissaData = lcboxLCM.LightCurves[num].Phases;
            pp.OrdinateData = lcboxLCM.LightCurves[num].Fluxes;
            plotILCM.Add(pp);

            plotILCM.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotILCM.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotILCM.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotILCM.Refresh();
        }

        private void btnRammstein_Click(object sender, EventArgs e)
        {
            Media.Player playerRammstein = new Media.Player();
            playerRammstein.Open("benzin.mp3");
            playerRammstein.Play(true);
        }

        private void btnClearGraphLCM_Click(object sender, EventArgs e)
        {
            plotILCM.Clear();
            plotILCM.Refresh();
        }

        private void AddTextToMod2TextBox(string str)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AddTextToMod2TextBox), new object[] { str });
                return;
            }
            txtResultMod2.Text += str;
        }

        private void StartTest()
        {
            double[] inc_vals = new double[4] { 0, 30, 60, 90 };
            double[] theta1_vals = new double[7] { 0, 30, 60, 90, 120, 150, 180 };
            double[] theta2_vals = new double[7] { 0, 30, 60, 90, 120, 150, 180 };
            double[] phi1_vals = new double[8] { 0, 45, 90, 135, 180, 225, 270, 315 };
            double[] phi2_vals = new double[8] { 0, 45, 90, 135, 180, 225, 270, 315 };
            double[] r1_vals = new double[3] { 10, 30, 90 };
            double[] r2_vals = new double[3] { 10, 30, 90 };

            for (int i = 0; i < inc_vals.Length; i++) inc_vals[i] = inc_vals[i] * Math.PI / 180;
            for (int i = 0; i < theta1_vals.Length; i++) theta1_vals[i] = theta1_vals[i] * Math.PI / 180;
            for (int i = 0; i < theta2_vals.Length; i++) theta2_vals[i] = theta2_vals[i] * Math.PI / 180;
            for (int i = 0; i < phi1_vals.Length; i++) phi1_vals[i] = phi1_vals[i] * Math.PI / 180;
            for (int i = 0; i < phi2_vals.Length; i++) phi2_vals[i] = phi2_vals[i] * Math.PI / 180;
            for (int i = 0; i < r1_vals.Length; i++) r1_vals[i] = r1_vals[i] * Math.PI / 180;
            for (int i = 0; i < r2_vals.Length; i++) r2_vals[i] = r2_vals[i] * Math.PI / 180;

            double tPh = 5000;
            double logg = 4.5;

            IntensityProvider1D ip1d= new IntensityProvider1D(Application.StartupPath + @"\Data\NormSpInt\UBVRI.dat",
                    new string[5] { "U", "B", "V", "R", "I" }, logg);
            ip1d.FixFilter = "V";
            IntensityProvider1D ldcp1d=new IntensityProvider1D(Application.StartupPath + @"\Data\LDC\UBVRI.dat",
                    new string[5] { "U", "B", "V", "R", "I" }, logg);
            ldcp1d.FixFilter = "V";
            LinearLimbDarkeningLow lldl= new LinearLimbDarkeningLow(ldcp1d);
            lldl.FixedFilter = "V";

            double unspottedBrightness = Math.PI * ip1d.GetIntensityForFixedFilter(tPh) *
                    (1.0 - ldcp1d.GetIntensityForFixedFilter(tPh) / 3.0);

            double[] phases = new double[100];
            for (int i = 0; i < phases.Length; i++) phases[i] = 0.01 * i;
            double[] fluxes1 = new double[100];
            double[] fluxes2=new double[100];

            StreamWriter sw;

            for (int i = 0; i < inc_vals.Length; i++)
            {
                for (int t1 = 0; t1 < theta1_vals.Length; t1++)
                {
                    for (int t2 = 0; t2 < theta2_vals.Length; t2++)
                    {
                        for (int p1 = 0; p1 < phi1_vals.Length; p1++)
                        {
                            for (int p2 = 0; p2 < phi2_vals.Length; p2++)
                            {
                                for (int r1 = 0; r1 < r1_vals.Length; r1++)
                                {
                                    for (int r2 = 0; r2 < r2_vals.Length; r2++)
                                    {
                                        this.AddTextToMod2TextBox(string.Format(
                                            "\r\nInc={0:00.0} Th1={1:000.0} Phi1={2:000.0} R1={3:000.0} Th2={4:000.0} Phi2={5:000.0} R2={6:000.0}",
                                            inc_vals[i] * 180 / Math.PI, theta1_vals[t1] * 180 / Math.PI, phi1_vals[p1] * 180 / Math.PI, r1_vals[r1] * 180 / Math.PI,
                                            theta2_vals[t2] * 180 / Math.PI, phi2_vals[p2] * 180 / Math.PI, r2_vals[r2] * 180 / Math.PI));
                                        
                                        Star model = new Star(inc_vals[i], tPh, logg);
                                        TSurface tsrf = new TSurface(100, 150, inc_vals[i], tPh);

                                        model.AddUniformCircularSpot(phi1_vals[p1], theta1_vals[t1],
                                            r1_vals[r1], 4500, 50, 150);
                                        model.AddUniformCircularSpot(phi2_vals[p2], theta2_vals[t2],
                                            r2_vals[r2], 3500, 50, 150);

                                        tsrf.AddCircularSpot(phi2_vals[p2], theta2_vals[t2], r2_vals[r2], 3500);
                                        tsrf.AddCircularSpot(phi1_vals[p1], theta1_vals[t1], r1_vals[r1], 4500);

                                        LCM.LCModeller1 modeller = 
                                            new Maper.LCM.LCModeller1(model, 
                                                lldl.GetLinerLimbDarkeningCoefficient, 
                                                ip1d.GetIntensityForFixedFilter, unspottedBrightness);
                                        fluxes1 = modeller.GetFluxes(phases, 1.0);

                                        LCGenerator1 lcg = new LCGenerator1(tsrf,
                                            lldl.GetLinerLimbDarkeningCoefficient,
                                            ip1d.GetIntensityForFixedFilter);

                                        fluxes2 = lcg.GetFluxes(phases, 1.0);

                                        string filename = Application.StartupPath + @"\TEST\" +
                                            string.Format(
                                            "Inc={0:00.0}_Th1={1:000.0}_Phi1={2:000.0}_R1={3:000.0}_Th2={4:000.0}_Phi2={5:000.0}_R2={6:000.0}",
                                            inc_vals[i] * 180 / Math.PI, theta1_vals[t1] * 180 / Math.PI, phi1_vals[p1] * 180 / Math.PI, r1_vals[r1] * 180 / Math.PI,
                                            theta2_vals[t2] * 180 / Math.PI, phi2_vals[p2] * 180 / Math.PI, r2_vals[r2] * 180 / Math.PI) +
                                            ".dat";
                                        filename = filename.Replace(",", ".");
                                        sw = new StreamWriter(filename);
                                        for (int k = 0; k < phases.Length; k++)
                                        {
                                            sw.WriteLine(string.Format("{0:0.00}\t{1:0.000000E000}\t{2:0.000000E000}", phases[k], fluxes1[k], fluxes2[k]));
                                        }
                                        sw.Flush();
                                        sw.Close();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnTestLCM_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(this.StartTest);
            thread.Start();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string[] filters = new string[4]{"B", "V", "Rc", "Ic"};
            double[] maxFlxs = new double[4]{0.9814, 1.1966, 1.4294, 1.6962};
            double logg = 4.5;
            int pointsCount = 1;
            Star star1 = new Star(65 * Math.PI / 180.0, 4660, 4.5);

            IntensityProvider1D ip1d = new IntensityProvider1D(Application.StartupPath + @"\Data\NormSpInt\UBVRIRcIcJK.dat",
                new string[9] { "U", "B", "V", "R", "I", "Rc", "Ic", "J", "K" }, logg);
            IntensityProvider1D ldcp1d = new IntensityProvider1D(Application.StartupPath + @"\Data\LDC\UBVRI.dat",
                new string[7] { "U", "B", "V", "R", "I", "Rc", "Ic" }, logg);
            LinearLimbDarkeningLow lldl = new LinearLimbDarkeningLow(ldcp1d);

            double teff0 = 3560;
            double deltaTeff = 100;
            double rad0 = 5;
            double deltaRad = 10;

            double teff, rad;
            StreamWriter sw = new StreamWriter("PolarSpottedStarFluxes.dat");
            for (int r = 0; r < 6; r++)
            {
                rad = rad0 + r * deltaRad;
                for (int t = 0; t < 10; t++)
                {
                    teff = teff0 + t * deltaTeff;
                    sw.WriteLine("Rad = {0:00.00} Teff = {1:0000.0}", rad, teff);
                    star1.RemoveAllSpots();
                    star1.AddUniformCircularSpot(0, 0, rad*Math.PI/180, teff, 40, 120);
                    for (int f = 0; f < filters.Length; f++)
                    {
                        ip1d.FixFilter = filters[f];
                        ldcp1d.FixFilter = filters[f];
                        lldl.FixedFilter = filters[f];
                        double unspottedBrightness = Math.PI * ip1d.GetIntensityForFixedFilter(star1.TeffPhot) *
                (1.0 - ldcp1d.GetIntensityForFixedFilter(star1.TeffPhot) / 3.0);
                        LCM.LCModeller1 lcm = new Maper.LCM.LCModeller1(star1, lldl.GetLinerLimbDarkeningCoefficient,
                ip1d.GetIntensityForFixedFilter, unspottedBrightness);
                        double[] lc = lcm.GetFluxes(new double[1] { 0.0 }, 1.0);
                        sw.WriteLine("{0}-band;\tUnspotted brigtness = {1:0.0000E000}; Spotted brightness = {2:0.0000E000}; NewMaxFlx = {3:0.0000}",
                            filters[f], unspottedBrightness, lc[0], maxFlxs[f] * unspottedBrightness / lc[0]);
                    }
                }
            }
            sw.Flush();
            sw.Close();
        }

        private void btnILCM_SaveModelLightCurve_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();

            string path = saveFileDialog1.FileName;

            StreamWriter sw;

            int lcNum = 0;

            try
            {
                sw = new StreamWriter(path);
                lcNum = int.Parse(txtILCM_LightCurveNumber.Text);
            }
            catch
            {
                return;
            }

            for (int i = 0; i < msm1.GetModelLightCurves[lcNum].Phases.Length; i++)
            {
                sw.WriteLine("{0:0.000000} {1:0.00000E000}", msm1.GetModelLightCurves[lcNum].Phases[i],
                    msm1.GetModelLightCurves[lcNum].Fluxes[i]);
            }
            sw.Flush();
            sw.Close();
        }

        /********************************************************************************************/
        /****************************************STOKES IMAGING**************************************/
        /********************************************************************************************/

        StokesImaging.MagnetizedSurface magSrf = null;

        StokesImaging.ArcParsInitBox arcInitBox = null;

        StokesImaging.StokesModeller stokesModeller = null;

        StokesImaging.StokesParsProvider stokesProvider = null;

        private void btnSI_CreateSurface_Click(object sender, EventArgs e)
        {
            double magStr, betaOffset, lambdaOffset, inc;
            int n, m;
            try
            {
                magStr = double.Parse(txtSI_PolesMagFieldStr.Text.Replace(".", ",")) * 1e6;
                betaOffset = double.Parse(txtSI_BetaOffset.Text.Replace(".", ",")) * Math.PI / 180;
                lambdaOffset = double.Parse(txtSI_LambdaOffset.Text.Replace(".", ",")) * Math.PI / 180;
                inc = double.Parse(txtSI_Inc.Text.Replace(".", ",")) * Math.PI / 180;
                n = int.Parse(txtSI_N.Text);
                m = int.Parse(txtSI_M.Text);
            }
            catch
            {
                return;
            }

            this.magSrf = new Maper.StokesImaging.MagnetizedSurface(inc, betaOffset, lambdaOffset, magStr, n, m);
        }

        private void ShowArcPars(object sender, EventArgs e)
        {
            int spotNumber = lbSM_SpotStack.SelectedIndex;
            if (spotNumber == -1) return;

            this.txtSM_Phi1.LostFocus -= new EventHandler(ArcParsChanged);
            this.txtSM_Phi2.LostFocus -= new EventHandler(ArcParsChanged);
            this.txtSM_Theta1.LostFocus -= new EventHandler(ArcParsChanged);
            this.txtSM_Theta2.LostFocus -= new EventHandler(ArcParsChanged);
            this.txtSM_Brightness.LostFocus -= new EventHandler(ArcParsChanged);

            txtSM_Phi1.Text = this.arcInitBox.spots[spotNumber].longitude1.ToString();
            txtSM_Phi2.Text = this.arcInitBox.spots[spotNumber].longitude2.ToString();
            txtSM_Theta1.Text = this.arcInitBox.spots[spotNumber].colatitude1.ToString();
            txtSM_Theta2.Text = this.arcInitBox.spots[spotNumber].colatitude2.ToString();
            txtSM_Brightness.Text = this.arcInitBox.spots[spotNumber].brightness.ToString();

            this.txtSM_Phi1.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Phi2.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Theta1.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Theta2.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Brightness.LostFocus += new EventHandler(ArcParsChanged);
        }

        private void ArcParsChanged(object sender, EventArgs e)
        {
            try
            {
                int spotNumber = lbSM_SpotStack.SelectedIndex;
                if (lbSM_SpotStack.SelectedIndex == -1) return;
                this.arcInitBox.spots[spotNumber].longitude1 = double.Parse(txtSM_Phi1.Text.Replace(".", ","));
                this.arcInitBox.spots[spotNumber].longitude2 = double.Parse(txtSM_Phi2.Text.Replace(".", ","));
                this.arcInitBox.spots[spotNumber].colatitude1 = double.Parse(txtSM_Theta1.Text.Replace(".", ","));
                this.arcInitBox.spots[spotNumber].colatitude2 = double.Parse(txtSM_Theta2.Text.Replace(".", ","));
                this.arcInitBox.spots[spotNumber].brightness = double.Parse(txtSM_Brightness.Text.Replace(".", ","));
            }
            catch
            {
                MessageBox.Show("Wrong format...", "Error...");
                return;
            }
        }

        private void btnSM_AddSpot_Click(object sender, EventArgs e)
        {
            if (this.magSrf == null)
            {
                MessageBox.Show("You should build the surface first...", "Error...");
                return;
            }

            if (this.arcInitBox == null)
            {
                this.arcInitBox = new StokesImaging.ArcParsInitBox();
            }

            this.arcInitBox.AddSpot();

            lbSM_SpotStack.Items.Clear();

            for (int i = 0; i < this.arcInitBox.SpotsNumber; i++)
            {
                lbSM_SpotStack.Items.Add("Spot " + (i + 1).ToString());
            }

            this.lbSM_SpotStack.SelectedIndexChanged += new EventHandler(ShowArcPars);
            this.txtSM_Phi1.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Phi2.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Theta1.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Theta2.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Brightness.LostFocus += new EventHandler(ArcParsChanged);
        }

        private void btnSM_DelSpot_Click(object sender, EventArgs e)
        {
            if (this.arcInitBox == null) return;
            int num = lbSM_SpotStack.SelectedIndex;
            if (num != -1)
            {
                this.arcInitBox.DelSpot(num);
            }

            lbSM_SpotStack.Items.Clear();

            for (int i = 0; i < this.arcInitBox.SpotsNumber; i++)
            {
                lbSM_SpotStack.Items.Add("Spot " + (i + 1).ToString());
            }

            this.ShowArcPars(sender, e);
        }

        private void btnSM_GenerateStokesCurves_Click(object sender, EventArgs e)
        {
            int pointsCount;
            double poleOptDepth;

            if (this.magSrf == null) return;

            try
            {
                pointsCount = int.Parse(txtSM_PointsNumber.Text);
                poleOptDepth = double.Parse(txtSI_PoleOptDepth.Text);
            }
            catch
            {
                MessageBox.Show("Wrong format...", "Error...");
                return;
            }

            txtSM_Results.Text += "\r\nModel parameters:";
            txtSM_Results.Text += string.Format("\r\nInc[deg]: {0:00.000}",
                this.magSrf.InclinationOfRotationAxis * 180 / Math.PI);
            txtSM_Results.Text += string.Format("\r\nLat. Dipol Offset [deg]: {0:00.000}",
                this.magSrf.LatitudeDipolOffset * 180 / Math.PI);
            txtSM_Results.Text += string.Format("\r\nLong. Dipol Offset [deg]: {0:00.000}",
                this.magSrf.LongitudeDipolOffset * 180 / Math.PI);

            try
            {
                this.stokesProvider = new Maper.StokesImaging.StokesParsProvider(
                    @txtPathToStockesIGrid.Text,
                    @txtPathToStockesQGrid.Text,
                    @txtPathToStockesVGrid.Text
                    );
            }
            catch
            {
                MessageBox.Show("Cannot find some Stockes Grid file...", "Error...");
                txtSM_Results.Text += "\r\nOops... Some error was occured...";
                return;
            }

            StokesImaging.WM85Interpolator wm85 = new Maper.StokesImaging.WM85Interpolator();

            double[] phases = new double[pointsCount];
            for (int i = 0; i < phases.Length; i++) phases[i] = i /(double)pointsCount;

            
            //this.stokesModeller = new Maper.StokesImaging.StokesModeller(
            //    /*stokesProvider.StokesI*/ wm85.GetStokesI,
            //    /*stokesProvider.StokesV*/ wm85.GetStokesV,
            //    /*stokesProvider.StokesQ*/ wm85.GetStokesQ,
            //    stokesProvider.StokesU,
            //    this.magSrf);

            this.stokesModeller = new Maper.StokesImaging.StokesModeller(
                stokesProvider.StokesI,
                stokesProvider.StokesV,
                stokesProvider.StokesQ,
                stokesProvider.StokesU,
                this.magSrf);


            this.stokesModeller.StartStokesCurvesModelling(phases, 1.0, poleOptDepth);

            
            LinePlot lpI = new LinePlot(this.stokesModeller.StokesI, phases);
            LinePlot lpV = new LinePlot(this.stokesModeller.StokesV, phases);
            LinePlot lpQ = new LinePlot(this.stokesModeller.StokesQ, phases);
            LinePlot lpU = new LinePlot(this.stokesModeller.StokesU, phases);

            plotSM_StokesI.Add(lpI);
            plotSM_StokesV.Add(lpV);
            plotSM_StokesQ.Add(lpQ);
            plotSM_StokesU.Add(lpU);

            plotSM_StokesI.Title = "Stokes I Curve";
            plotSM_StokesQ.Title = "Stokes Q Curve";
            plotSM_StokesU.Title = "Stokes U Curve";
            plotSM_StokesV.Title = "Stokes V Curve";

            plotSM_StokesI.XAxis1.Label = "Phase";
            plotSM_StokesQ.XAxis1.Label = "Phase";
            plotSM_StokesU.XAxis1.Label = "Phase";
            plotSM_StokesV.XAxis1.Label = "Phase";

            plotSM_StokesI.Refresh();
            plotSM_StokesV.Refresh();
            plotSM_StokesQ.Refresh();
            plotSM_StokesU.Refresh();

            

            txtSM_Results.Text += "\r\nError String: " + this.stokesModeller.ErrorString;
        }

        private void btnSM_AddSpotsToSurface_Click(object sender, EventArgs e)
        {
            if (this.magSrf == null) return;

            for (int i = 0; i < this.arcInitBox.SpotsNumber; i++)
            {
                this.magSrf.AddRectSpot(this.arcInitBox.spots[i].colatitude1 * Math.PI / 180,
                    this.arcInitBox.spots[i].colatitude2 * Math.PI / 180,
                    this.arcInitBox.spots[i].longitude1 * Math.PI / 180,
                    this.arcInitBox.spots[i].longitude2 * Math.PI / 180,
                    this.arcInitBox.spots[i].brightness);
            }
        }

        private void btnSM_ClearUseMask_Click(object sender, EventArgs e)
        {
            if (this.magSrf == null) return;

            this.magSrf.ClearBrightnessDensityArray();
        }

        private void btnSM_ShowMap_Click(object sender, EventArgs e)
        {
            if (this.magSrf == null)
            {
                MessageBox.Show("Surface was not created...", "Error...");
                return;
            }

            StokesImaging.MagnetizedSurface boolSrf = new Maper.StokesImaging.MagnetizedSurface(0.0, 0.0, 0.0, 0.0,
                this.magSrf.Patches.Length, this.magSrf.Patches[this.magSrf.Patches.Length - 1].Length);

            if (this.arcInitBox == null) return;

            for (int i = 0; i < this.arcInitBox.SpotsNumber; i++)
            {
                boolSrf.AddRectSpot(this.arcInitBox.spots[i].colatitude1 * Math.PI / 180,
                    this.arcInitBox.spots[i].colatitude2 * Math.PI / 180,
                    this.arcInitBox.spots[i].longitude1 * Math.PI / 180,
                    this.arcInitBox.spots[i].longitude2 * Math.PI / 180,
                    this.arcInitBox.spots[i].brightness);
            }

            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.White;
            svf.color1 = Color.Black;
            svf.Init(boolSrf.GetPatchCoordMas(), boolSrf.GetBrightnessDensityMas());
            svf.ShowDialog();
        }

        private void btnSI_ShowSurface_Click(object sender, EventArgs e)
        {
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.Blue;
            svf.color1 = Color.Red;
            svf.Init(magSrf.GetPatchCoordMas(), magSrf.GetBMas());
            svf.ShowDialog();
        }

        private void btnSM_ShowThetaSurface_Click(object sender, EventArgs e)
        {
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.Blue;
            svf.color1 = Color.Red;
            svf.Init(magSrf.GetPatchCoordMas(), magSrf.GetThetaMas());
            svf.ShowDialog();
        }

        private void btnSM_ClearGraph_Click(object sender, EventArgs e)
        {
            plotSM_StokesI.Clear();
            plotSM_StokesQ.Clear();
            plotSM_StokesU.Clear();
            plotSM_StokesV.Clear();
            plotSM_PrArea.Clear();

            plotSM_StokesI.Refresh();
            plotSM_StokesQ.Refresh();
            plotSM_StokesU.Refresh();
            plotSM_StokesV.Refresh();
            plotSM_PrArea.Refresh();
        }

        private void btnSM_ShowStokesCurves_Click(object sender, EventArgs e)
        {
            if (rbSM_StokesView.Checked)
            {
                LinePlot lpV = new LinePlot(this.stokesModeller.StokesV, this.stokesModeller.PhasesV);
                LinePlot lpQ = new LinePlot(this.stokesModeller.StokesQ, this.stokesModeller.PhasesQ);
                LinePlot lpU = new LinePlot(this.stokesModeller.StokesU, this.stokesModeller.PhasesU);

                plotSM_StokesQ.Clear();
                plotSM_StokesU.Clear();
                plotSM_StokesV.Clear();

                plotSM_StokesV.Add(lpV);
                plotSM_StokesQ.Add(lpQ);
                plotSM_StokesU.Add(lpU);

                plotSM_StokesQ.Title = "Stokes Q Curve";
                plotSM_StokesU.Title = "Stokes U Curve";
                plotSM_StokesV.Title = "Stokes V Curve";
            }

            if (rbSM_SimpleView.Checked)
            {
                double[] circPol = new double[this.stokesModeller.StokesV.Length];
                
                for (int i = 0; i < circPol.Length; i++)
                {
                    if (this.stokesModeller.StokesI[i] == 0)
                    {
                        circPol[i] = 0;
                    }
                    else
                    {
                        circPol[i] = 100 * Math.Abs(this.stokesModeller.StokesV[i]) /
                            this.stokesModeller.StokesI[i];
                    }
                }

                double[] linPol = new double[this.stokesModeller.StokesV.Length];
                for (int i = 0; i < linPol.Length; i++)
                {
                    if (this.stokesModeller.StokesI[i] == 0)
                    {
                        linPol[i] = 0;
                    }
                    else
                    {
                        linPol[i] = 100 * Math.Sqrt(Math.Pow(this.stokesModeller.StokesQ[i], 2) +
                            Math.Pow(this.stokesModeller.StokesU[i], 2)) /
                            this.stokesModeller.StokesI[i];
                    }
                }

                double[] posAng = new double[this.stokesModeller.PhasesV.Length];
                for (int i = 0; i < posAng.Length; i++)
                {
                    if (this.stokesModeller.StokesQ[i] == 0)
                    {
                        posAng[i] = 90;
                    }
                    posAng[i] = 0.5 * Math.Atan(this.stokesModeller.StokesU[i] /
                        this.stokesModeller.StokesQ[i]) * 180 / Math.PI;

                    if (this.stokesModeller.StokesU[i] < 0 && this.stokesModeller.StokesQ[i] < 0)
                    {
                        posAng[i] = posAng[i]-90;
                    }
                    if (this.stokesModeller.StokesU[i] > 0 && this.stokesModeller.StokesQ[i] < 0)
                    {
                        posAng[i] = posAng[i] + 90;
                    }
                }


                LinePlot lpC = new LinePlot(circPol, this.stokesModeller.PhasesI);
                LinePlot lpP = new LinePlot(linPol, this.stokesModeller.PhasesV);
                LinePlot lpPos = new LinePlot(posAng, this.stokesModeller.PhasesV);

                plotSM_StokesQ.Clear();
                plotSM_StokesU.Clear();
                plotSM_StokesV.Clear();

                plotSM_StokesQ.Title = "Linear Polarization [%]";
                plotSM_StokesU.Title = "Position Angle [deg]";
                plotSM_StokesV.Title = "Circular Polarization [%]";

                plotSM_StokesQ.Add(lpP);
                plotSM_StokesU.Add(lpPos);
                plotSM_StokesV.Add(lpC);
            }

            LinePlot lpT = new LinePlot(this.stokesModeller.StokesI, this.stokesModeller.PhasesI);

            plotSM_StokesI.Clear();
            plotSM_StokesI.Title = "Total Flux";
            plotSM_StokesI.Add(lpT);

            plotSM_StokesI.XAxis1.Label = "Phase";
            plotSM_StokesQ.XAxis1.Label = "Phase";
            plotSM_StokesU.XAxis1.Label = "Phase";
            plotSM_StokesV.XAxis1.Label = "Phase";

            plotSM_StokesI.Refresh();
            plotSM_StokesV.Refresh();
            plotSM_StokesQ.Refresh();
            plotSM_StokesU.Refresh();
        }

        private void btnSM_InterpWM85_Click(object sender, EventArgs e)
        {
            double nuratio;
            double optdepth;
            double theta;

            try
            {
                nuratio = double.Parse(txtSM_NuRatioTest.Text.Replace(".", ","));
                optdepth = double.Parse(txtSM_LambdaTest.Text.Replace(".", ","));
                theta = double.Parse(txtSM_ThetaTest.Text.Replace(".", ",")) * Math.PI / 180;
            }
            catch
            {
                MessageBox.Show("Wrong format of input data...", "Error...");
                return;
            }
            
            StokesImaging.WM85Interpolator wm85 = new Maper.StokesImaging.WM85Interpolator();

            double[] xGrid = null, fGrid = null, xInterp = null, yInterp = null;
            PointPlot pp = new PointPlot();
            LinePlot lp = new LinePlot();
            xInterp = new double[100];
            yInterp = new double[100];

            if (rbSM_GridTest_Total.Checked && rbSM_GridTest_NuRatio.Checked)
            {
                xGrid = new double[wm85.GetThetaSet.Length];
                fGrid = new double[wm85.GetThetaSet.Length];
                for (int i = 0; i < xInterp.Length; i++)
                {
                    xInterp[i] = wm85.GetThetaSet[0] + i * (wm85.GetThetaSet[wm85.GetThetaSet.Length - 1] - wm85.GetThetaSet[0]) / (double)xInterp.Length;
                    xInterp[i] = xInterp[i] * 180 / Math.PI;
                    yInterp[i] = wm85.InterpTotal(nuratio, xInterp[i] * Math.PI / 180, optdepth);
                }
                for (int i = 0; i < xGrid.Length; i++)
                {
                    xGrid[i] = wm85.GetThetaSet[i];
                    xGrid[i] = xGrid[i] * 180 / Math.PI;
                    fGrid[i] = wm85.InterpTotal(nuratio, xGrid[i] * Math.PI / 180, optdepth);
                }
            }
            if (rbSM_GridTest_Total.Checked && rbSM_GridTest_FixTheta.Checked)
            {
                xGrid = new double[wm85.GetNuRatSet.Length];
                fGrid = new double[wm85.GetNuRatSet.Length];
                for (int i = 0; i < xInterp.Length; i++)
                {
                    xInterp[i] = wm85.GetNuRatSet[0] + i * (wm85.GetNuRatSet[wm85.GetNuRatSet.Length - 1] - wm85.GetNuRatSet[0]) / (double)xInterp.Length;
                    yInterp[i] = wm85.InterpTotal(xInterp[i], theta, optdepth);
                }
                for (int i = 0; i < xGrid.Length; i++)
                {
                    xGrid[i] = wm85.GetNuRatSet[i];
                    fGrid[i] = wm85.InterpTotal(xGrid[i], theta, optdepth);
                }
            }
            if (rbSM_GridTest_Linear.Checked && rbSM_GridTest_NuRatio.Checked)
            {
                xGrid = new double[wm85.GetThetaSet.Length];
                fGrid = new double[wm85.GetThetaSet.Length];
                for (int i = 0; i < xInterp.Length; i++)
                {
                    xInterp[i] = wm85.GetThetaSet[0] + i * (wm85.GetThetaSet[wm85.GetThetaSet.Length - 1] - wm85.GetThetaSet[0]) / (double)xInterp.Length;
                    xInterp[i] = xInterp[i] * 180 / Math.PI;
                    yInterp[i] = wm85.InterpLinearPart(nuratio, xInterp[i] * Math.PI / 180, optdepth);
                }
                for (int i = 0; i < xGrid.Length; i++)
                {
                    xGrid[i] = wm85.GetThetaSet[i];
                    xGrid[i] = xGrid[i] * 180 / Math.PI;
                    fGrid[i] = wm85.InterpLinearPart(nuratio, xGrid[i] * Math.PI / 180, optdepth);
                }
            }
            if (rbSM_GridTest_Linear.Checked && rbSM_GridTest_FixTheta.Checked)
            {
                xGrid = new double[wm85.GetNuRatSet.Length];
                fGrid = new double[wm85.GetNuRatSet.Length];
                for (int i = 0; i < xInterp.Length; i++)
                {
                    xInterp[i] = wm85.GetNuRatSet[0] + i * (wm85.GetNuRatSet[wm85.GetNuRatSet.Length - 1] - wm85.GetNuRatSet[0]) / (double)xInterp.Length;
                    yInterp[i] = wm85.InterpLinearPart(xInterp[i], theta, optdepth);
                }
                for (int i = 0; i < xGrid.Length; i++)
                {
                    xGrid[i] = wm85.GetNuRatSet[i];
                    fGrid[i] = wm85.InterpLinearPart(xGrid[i], theta, optdepth);
                }
            }
            if (rbSM_GridTest_Circular.Checked && rbSM_GridTest_NuRatio.Checked)
            {
                xGrid = new double[wm85.GetThetaSet.Length];
                fGrid = new double[wm85.GetThetaSet.Length];
                for (int i = 0; i < xInterp.Length; i++)
                {
                    xInterp[i] = wm85.GetThetaSet[0] + i * (wm85.GetThetaSet[wm85.GetThetaSet.Length - 1] - wm85.GetThetaSet[0]) / (double)xInterp.Length;
                    xInterp[i] = xInterp[i] * 180 / Math.PI;
                    yInterp[i] = wm85.GetStokesV(14.0e6, xInterp[i] * Math.PI / 180, 1e7);
                    //yInterp[i] = wm85.InterpCircularPart(nuratio, xInterp[i] * Math.PI / 180, optdepth);
                }
                for (int i = 0; i < xGrid.Length; i++)
                {
                    xGrid[i] = wm85.GetThetaSet[i];
                    xGrid[i] = xGrid[i] * 180 / Math.PI;
                    fGrid[i] = wm85.InterpCircularPart(nuratio, xGrid[i] * Math.PI / 180, optdepth);
                }
            }
            if (rbSM_GridTest_Circular.Checked && rbSM_GridTest_FixTheta.Checked)
            {
                xGrid = new double[wm85.GetNuRatSet.Length];
                fGrid = new double[wm85.GetNuRatSet.Length];
                for (int i = 0; i < xInterp.Length; i++)
                {
                    xInterp[i] = wm85.GetNuRatSet[0] + i * (wm85.GetNuRatSet[wm85.GetNuRatSet.Length - 1] - wm85.GetNuRatSet[0]) / (double)xInterp.Length;
                    yInterp[i] = wm85.InterpCircularPart(xInterp[i], theta, optdepth);
                }
                for (int i = 0; i < xGrid.Length; i++)
                {
                    xGrid[i] = wm85.GetNuRatSet[i];
                    fGrid[i] = wm85.InterpCircularPart(xGrid[i], theta, optdepth);
                }
            }

            pp = new PointPlot(new Marker(Marker.MarkerType.FilledCircle, 3, Color.Black));
            pp.OrdinateData = fGrid;
            pp.AbscissaData = xGrid;
            lp = new LinePlot(yInterp, xInterp);

            plotSM_TestGrid.Add(pp);
            plotSM_TestGrid.Add(lp);

            plotSM_TestGrid.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotSM_TestGrid.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotSM_TestGrid.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotSM_TestGrid.Refresh();
        }

        private void btnSM_ClearGraphWM85_Click(object sender, EventArgs e)
        {
            plotSM_TestGrid.Clear();
            plotSM_TestGrid.Refresh();
        }

        private void btnSM_SaveCurve_Click(object sender, EventArgs e)
        {
            if (this.stokesModeller == null) return;

            saveFileDialog1.ShowDialog();

            string path = saveFileDialog1.FileName;
            
            StreamWriter sw;
            try
            {
                sw = new StreamWriter(path);
            }
            catch
            {
                return;
            }

            for (int i = 0; i < this.stokesModeller.PhasesI.Length; i++)
            {
                sw.WriteLine("{0:000.000}\t{1:0.00000E000}\t{2:0.00000E000}\t{3:0.00000E000}\t{4:0.00000E000}",
                    this.stokesModeller.PhasesI[i],
                    this.stokesModeller.StokesI[i],
                    this.stokesModeller.StokesV[i],
                    this.stokesModeller.StokesQ[i],
                    this.stokesModeller.StokesU[i]);
            }
            sw.Flush();
            sw.Close();


            //if (rbSM_SaveI.Checked)
            //{
            //    if (this.stokesModeller.StokesI == null) return;
            //    sw.WriteLine("I {0} {1}", "V", "I");
            //    sw.WriteLine("S {0}", this.stokesModeller.StokesI.Length);
            //    for (int i = 0; i < this.stokesModeller.PhasesI.Length; i++)
            //    {
            //        sw.WriteLine("{0:0.00000} {1:0.00000E000}",
            //            this.stokesModeller.PhasesI[i], this.stokesModeller.StokesI[i]);
            //    }
            //}
            //if (rbSM_SaveQ.Checked)
            //{
            //    if (this.stokesModeller.StokesQ == null) return;
            //    sw.WriteLine("I {0} {1}", "V", "Q");
            //    sw.WriteLine("S {0}", this.stokesModeller.StokesQ.Length);
            //    for (int i = 0; i < this.stokesModeller.PhasesQ.Length; i++)
            //    {
            //        sw.WriteLine("{0:0.00000} {1:0.00000E000}",
            //            this.stokesModeller.PhasesQ[i], this.stokesModeller.StokesQ[i]);
            //    }
            //}
            //if (rbSM_SaveU.Checked)
            //{
            //    if (this.stokesModeller.StokesU == null) return;
            //    sw.WriteLine("I {0} {1}", "V", "U");
            //    sw.WriteLine("S {0}", this.stokesModeller.StokesU.Length);
            //    for (int i = 0; i < this.stokesModeller.PhasesU.Length; i++)
            //    {
            //        sw.WriteLine("{0:0.00000} {1:0.00000E000}",
            //            this.stokesModeller.PhasesU[i], this.stokesModeller.StokesU[i]);
            //    }
            //}
            //if (rbSM_SaveV.Checked)
            //{
            //    if (this.stokesModeller.StokesV == null) return;
            //    sw.WriteLine("I {0} {1}", "V", "V");
            //    sw.WriteLine("S {0}", this.stokesModeller.StokesV.Length);
            //    for (int i = 0; i < this.stokesModeller.PhasesV.Length; i++)
            //    {
            //        sw.WriteLine("{0:0.00000} {1:0.00000E000}",
            //            this.stokesModeller.PhasesV[i], this.stokesModeller.StokesV[i]);
            //    }
            //}
            //sw.Flush();
            //sw.Close();
        }

        private void btnSMTest_Plot0_Click(object sender, EventArgs e)
        {
            this.stokesProvider = new Maper.StokesImaging.StokesParsProvider(
                    @txtPathToStockesIGrid.Text,
                    @txtPathToStockesQGrid.Text,
                    @txtPathToStockesVGrid.Text
                    );
            
        }

        /*************************************************************************************************/
        /*************************************************************************************************/

        StokesImaging.StokesCurvesBox stokesCurvesBox = null;
        StokesImaging.MagnetizedSurface magSrfRes = null;
        StokesImaging.StokesParsProvider spp_ism = null;

        private void ShowStokesCurvePars(object sender, EventArgs e)
        {
            int curveNumber = lbISM_StokesCurves.SelectedIndex;
            if (curveNumber == -1) return;

            this.txtISM_Sigma.LostFocus -= new EventHandler(StokesCurveParsChanged);

            txtISM_Filter.Text = this.stokesCurvesBox.StokesCurves[curveNumber].filter;
            txtISM_Type.Text = this.stokesCurvesBox.StokesCurves[curveNumber].type;
            txtISM_Sigma.Text = this.stokesCurvesBox.StokesCurves[curveNumber].sigma.ToString();

            this.txtISM_Sigma.LostFocus += new EventHandler(StokesCurveParsChanged);
        }

        void StokesCurveParsChanged(object sender, EventArgs e)
        {
            try
            {
                int curveNumber = lbISM_StokesCurves.SelectedIndex;
                if (curveNumber == -1) return;
                this.stokesCurvesBox.StokesCurves[curveNumber].sigma = 
                    double.Parse(txtISM_Sigma.Text.Replace(".", ","));
            }
            catch
            {
                MessageBox.Show("Wrong format...", "Error...");
                return;
            }      
        }

        private void btnISM_AddStokesCurve_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (this.stokesCurvesBox == null)
            {
                this.stokesCurvesBox = new Maper.StokesImaging.StokesCurvesBox();
            }
            string path = openFileDialog1.FileName;
            if (path == null) return;
            //try
            {
                this.stokesCurvesBox.AddStokesCurve(path);
            }
            //catch
            {
            //    MessageBox.Show("No such file or error in file structure...", "Error");
            }

            lbISM_StokesCurves.Items.Clear();

            for (int i = 0; i < this.stokesCurvesBox.StokesCurvesNumber; i++)
            {
                lbISM_StokesCurves.Items.Add("Stokes Curve # " + (i + 1).ToString());
            }

            this.lbISM_StokesCurves.SelectedIndexChanged += new EventHandler(ShowStokesCurvePars);
            this.txtISM_Sigma.LostFocus += new EventHandler(StokesCurveParsChanged);
        }

        private void btnISM_DeleteStokesCurve_Click(object sender, EventArgs e)
        {
            if (this.stokesCurvesBox == null) return;
            if (this.stokesCurvesBox.StokesCurvesNumber == 0) return;

            int num = lbISM_StokesCurves.SelectedIndex;
            if (num != -1)
            {
                this.stokesCurvesBox.DeleteStokesCurve(num);
            }

            lbISM_StokesCurves.Items.Clear();

            for (int i = 0; i < this.stokesCurvesBox.StokesCurvesNumber; i++)
            {
                lbISM_StokesCurves.Items.Add("Stokes Curve # " + (i + 1).ToString());
            }

            this.ShowStokesCurvePars(sender, e);
        }

        private void btnISM_StartMapping_Click(object sender, EventArgs e)
        {
            double polesMagStr;
            double polesLong;
            double polesCoLat;
            double inc;
            double regPar;
            int n, m;
            
            try
            {
                polesMagStr = double.Parse(txtISM_PolesMagStr.Text.Replace(".", ",")) * 1e6;
                polesLong = double.Parse(txtISM_PolesLongitude.Text.Replace(".", ",")) * Math.PI / 180;
                polesCoLat = double.Parse(txtISM_PolesColatitude.Text.Replace(".", ",")) * Math.PI / 180;
                inc = double.Parse(txtISM_Inclination.Text.Replace(".", ",")) * Math.PI / 180;
                regPar = double.Parse(txtISM_RegulParameter.Text.Replace(".", ","));
                n = int.Parse(txtISM_LatBeltsNumber.Text);
                m = int.Parse(txtISM_LongBeltsNumber.Text);
            }
            catch
            {
                MessageBox.Show("Wrong format of input data...", "Error...");
                return;
            }

            //StokesImaging.WM85Interpolator wm85 = new Maper.StokesImaging.WM85Interpolator();


            spp_ism = new Maper.StokesImaging.StokesParsProvider(
                txtISM_IPath.Text,
                txtISM_VPath.Text,
                txtISM_QPath.Text
                );
            
            StokesImaging.MagnetizedSurface srf = new Maper.StokesImaging.MagnetizedSurface(inc, polesCoLat, polesLong,
                polesMagStr, n, m);

            StokesImaging.StokesMapper sm = new Maper.StokesImaging.StokesMapper(srf,
                spp_ism.GiveStokesPar, this.stokesCurvesBox.StokesCurves);

            txtIS_Results.AppendText("\r\nStart imaging procedure...");
            MathLib.Counter counter = new MathLib.Counter();
            counter.Start();
            sm.StartMapping(regPar);
            counter.Stop();
            txtIS_Results.AppendText(
                string.Format("\r\nDone!\r\nComputings duration is: {0} sec", 
                counter.TotalSeconds.ToString()));

            this.magSrfRes = sm.ResultSurface;
        }

        private void btnISM_ShowStokesCurve_Click(object sender, EventArgs e)
        {
            int curveNumber = lbISM_StokesCurves.SelectedIndex;
            if (curveNumber == -1) return;

            PointPlot pp = new PointPlot(new Marker(Marker.MarkerType.FilledCircle, 3, Color.Black));
            if (this.stokesCurvesBox.StokesCurves[curveNumber].type == "I")
            {
                pp.AbscissaData = this.stokesCurvesBox.StokesCurves[curveNumber].phases;
                pp.OrdinateData = this.stokesCurvesBox.StokesCurves[curveNumber].value;
                plotISM_StokesI.Add(pp);
                plotISM_StokesI.Refresh();
            }
            if (this.stokesCurvesBox.StokesCurves[curveNumber].type == "V")
            {
                pp.AbscissaData = this.stokesCurvesBox.StokesCurves[curveNumber].phases;
                pp.OrdinateData = this.stokesCurvesBox.StokesCurves[curveNumber].value;
                plotISM_StokesV.Add(pp);
                plotISM_StokesV.Refresh();
            }
            if (this.stokesCurvesBox.StokesCurves[curveNumber].type == "Q")
            {
                pp.AbscissaData = this.stokesCurvesBox.StokesCurves[curveNumber].phases;
                pp.OrdinateData = this.stokesCurvesBox.StokesCurves[curveNumber].value;
                plotISM_StokesQ.Add(pp);
                plotISM_StokesQ.Refresh();
            }
            if (this.stokesCurvesBox.StokesCurves[curveNumber].type == "U")
            {
                pp.AbscissaData = this.stokesCurvesBox.StokesCurves[curveNumber].phases;
                pp.OrdinateData = this.stokesCurvesBox.StokesCurves[curveNumber].value;
                plotISM_StokesU.Add(pp);
                plotISM_StokesU.Refresh();
            }
        }

        private void btnISM_ModelCurves_Click(object sender, EventArgs e)
        {
            //StokesImaging.WM85Interpolator wm85 = new Maper.StokesImaging.WM85Interpolator();
            StokesImaging.StokesModeller modeller = new Maper.StokesImaging.StokesModeller(
                spp_ism.StokesI /*wm85.GetStokesI*/,
                spp_ism.StokesV /*wm85.GetStokesV*/,
                spp_ism.StokesQ /*wm85.GetStokesQ*/,
                spp_ism.StokesU,
                magSrfRes);

            int pointsCount = 100;
            double[] phases = new double[pointsCount];
            for (int i = 0; i < phases.Length; i++) phases[i] = i / (double)pointsCount;

            txtIS_Results.AppendText("\r\nStart stokes curve modelling...");
            modeller.StartStokesCurvesModelling(phases, 1.0, 1e7);
            txtIS_Results.AppendText("\r\nDone!");

            StreamWriter sw_i = new StreamWriter(Application.StartupPath + "\\stokes_i_reconstructed.dat");
            StreamWriter sw_q = new StreamWriter(Application.StartupPath + "\\stokes_q_reconstructed.dat");
            StreamWriter sw_v = new StreamWriter(Application.StartupPath + "\\stokes_v_reconstructed.dat");
            StreamWriter sw_u = new StreamWriter(Application.StartupPath + "\\stokes_u_reconstructed.dat");

            for (int i = 0; i < modeller.StokesI.Length; i++)
            {
                sw_i.WriteLine("{0}\t{1}", phases[i], modeller.StokesI[i]);
                sw_q.WriteLine("{0}\t{1}", phases[i], modeller.StokesQ[i]);
                sw_v.WriteLine("{0}\t{1}", phases[i], modeller.StokesV[i]);
                sw_u.WriteLine("{0}\t{1}", phases[i], modeller.StokesU[i]);
            }

            sw_i.Flush();
            sw_q.Flush();
            sw_v.Flush();
            sw_u.Flush();

            sw_i.Close();
            sw_q.Close();
            sw_v.Close();
            sw_u.Close();


            LinePlot lpI = new LinePlot(modeller.StokesI, phases);
            LinePlot lpV = new LinePlot(modeller.StokesV, phases);
            LinePlot lpQ = new LinePlot(modeller.StokesQ, phases);
            LinePlot lpU = new LinePlot(modeller.StokesU, phases);

            plotISM_StokesI.Add(lpI);
            plotISM_StokesV.Add(lpV);
            plotISM_StokesQ.Add(lpQ);
            plotISM_StokesU.Add(lpU);


            plotISM_StokesI.Title = "Stokes I Curve";
            plotISM_StokesQ.Title = "Stokes Q Curve";
            plotISM_StokesU.Title = "Stokes U Curve";
            plotISM_StokesV.Title = "Stokes V Curve";


            plotISM_StokesI.XAxis1.Label = "Phase";
            plotISM_StokesQ.XAxis1.Label = "Phase";
            plotISM_StokesU.XAxis1.Label = "Phase";
            plotISM_StokesV.XAxis1.Label = "Phase";
        }

        private void btmISM_ShowMap_Click(object sender, EventArgs e)
        {
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.White;
            svf.color1 = Color.Black;
            svf.Init(this.magSrfRes.GetPatchCoordMas(), this.magSrfRes.GetBrightnessDensityMas());
            svf.ShowDialog();
        }


        private void btnISM_StartFit_Click(object sender, EventArgs e)
        {
            //double polesMagStr;
            //double polesLong;
            //double polesCoLat;
            //double inc;
            //int n, m;
            //try
            //{
            //    polesMagStr = double.Parse(txtISM_PolesMagStr.Text.Replace(".", ",")) * 1e6;
            //    polesLong = double.Parse(txtISM_PolesLongitude.Text.Replace(".", ",")) * Math.PI / 180;
            //    polesCoLat = double.Parse(txtISM_PolesColatitude.Text.Replace(".", ",")) * Math.PI / 180;
            //    inc = double.Parse(txtISM_Inclination.Text.Replace(".", ",")) * Math.PI / 180;
            //    n = int.Parse(txtISM_LatBeltsNumber.Text);
            //    m = int.Parse(txtISM_LongBeltsNumber.Text);
            //}
            //catch
            //{
            //    MessageBox.Show("Wrong format of input data...", "Error...");
            //    return;
            //}

            //double longit1 = double.Parse(txtISM_Long1.Text.Replace(".", ","));
            //double longit2 = double.Parse(txtISM_Long2.Text.Replace(".", ","));
            //double longNum = int.Parse(txtISM_LongNumber.Text);
            //double colat1 = double.Parse(txtISM_Colat1.Text.Replace(".", ","));
            //double colat2 = double.Parse(txtISM_Colat2.Text.Replace(".", ","));
            //double colatNum = double.Parse(txtISM_ColatNumber.Text);

            //double[] longMas = new double[longNum];
            //double[] colatMas = new double[colatNum];
            //for (int i = 0; i < longMas.Length; i++)
            //{
            //    longMas[i] = longit1 + i * (longit2 - longit1) / longNum;
            //}
            //for (int i = 0; i < colatMas.Length; i++)
            //{
            //    colatMas[i] = colat1 + i * (colat2 - colat1) / colatNum;
            //}

            //StokesImaging.MagnetizedSurface msrf = new Maper.StokesImaging.MagnetizedSurface(inc, polesCoLat,
            //    polesLong, polesMagStr, n, m);

            //StokesImaging.WM85Interpolator wm85 = new Maper.StokesImaging.WM85Interpolator();

            //StokesImaging.StokesModeller modeller = new Maper.StokesImaging.StokesModeller(
            //    /*stokesProvider.StokesI*/ wm85.GetStokesI,
            //    /*stokesProvider.StokesV*/ wm85.GetStokesV,
            //    /*stokesProvider.StokesQ*/ wm85.GetStokesQ,
            //    wm85.GetStokesU,
            //    magSrfRes);
            //double[] modfluxesI;
            //double[] modfluxesV;

            //for (int l1 = 0; l1 < longMas.Length; l1++)
            //{
            //    for (int l2 = l1; l2 < longMas.Length; l2++)
            //    {
            //        for (int c1 = 0; c1 < colatMas.Length; c1++)
            //        {
            //            for (int c2 = c1; c2 < colatMas.Length; c2++)
            //            {
            //                msrf.AddRectSpot(colat1, colat2, longit1, longit2, 10);
            //                modeller.StartStokesCurvesModelling(this.stokesCurvesBox.StokesCurves[0].phases, 1.0);
            //                modfluxesI=modeller.StokesI;
            //                double sumKhi = 0;
            //                for (int i = 0; i < modfluxesI.Length; i++)
            //                {
            //                }

            //                msrf.ClearBrightnessDensityArray();
            //            }
            //        }
            //    }
            //}

        }

        /*********************************************************************************************************/
        /*********************************************************************************************************/

    }
}
