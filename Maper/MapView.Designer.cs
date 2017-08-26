namespace Maper
{
    partial class MapView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.panelGridColor = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbSphereTexture = new System.Windows.Forms.RadioButton();
            this.rbMercatorProjection = new System.Windows.Forms.RadioButton();
            this.btnSetGridsColor = new System.Windows.Forms.Button();
            this.btnGridParsRefresh = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtLatLinesWidth = new System.Windows.Forms.TextBox();
            this.txtLongLinesWidth = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtLatitudeLinesNumber = new System.Windows.Forms.TextBox();
            this.txtLongitudeLinesNumber = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.panelColorMin = new System.Windows.Forms.Panel();
            this.panelColorMax = new System.Windows.Forms.Panel();
            this.btnSetMinColor = new System.Windows.Forms.Button();
            this.btnSetMaxColor = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnSavePicture = new System.Windows.Forms.Button();
            this.txtOutputFile = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnPathToFile = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtYLabel = new System.Windows.Forms.TextBox();
            this.txtXLabel = new System.Windows.Forms.TextBox();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label12 = new System.Windows.Forms.Label();
            this.tabPage4.SuspendLayout();
            this.panelGridColor.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Height [pix]";
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(72, 43);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(56, 20);
            this.txtHeight.TabIndex = 4;
            this.txtHeight.Text = "400";
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(72, 69);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(56, 20);
            this.txtWidth.TabIndex = 5;
            this.txtWidth.Text = "800";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Width [pix]";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.panelGridColor);
            this.tabPage4.Controls.Add(this.groupBox1);
            this.tabPage4.Controls.Add(this.btnSetGridsColor);
            this.tabPage4.Controls.Add(this.btnGridParsRefresh);
            this.tabPage4.Controls.Add(this.label7);
            this.tabPage4.Controls.Add(this.label6);
            this.tabPage4.Controls.Add(this.txtLatLinesWidth);
            this.tabPage4.Controls.Add(this.txtLongLinesWidth);
            this.tabPage4.Controls.Add(this.label4);
            this.tabPage4.Controls.Add(this.txtLatitudeLinesNumber);
            this.tabPage4.Controls.Add(this.txtLongitudeLinesNumber);
            this.tabPage4.Controls.Add(this.label5);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(458, 111);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Grid";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // panelGridColor
            // 
            this.panelGridColor.Controls.Add(this.label8);
            this.panelGridColor.Location = new System.Drawing.Point(284, 10);
            this.panelGridColor.Name = "panelGridColor";
            this.panelGridColor.Size = new System.Drawing.Size(89, 56);
            this.panelGridColor.TabIndex = 19;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(15, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(58, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Grids Color";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbSphereTexture);
            this.groupBox1.Controls.Add(this.rbMercatorProjection);
            this.groupBox1.Location = new System.Drawing.Point(9, 62);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(269, 41);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mode";
            // 
            // rbSphereTexture
            // 
            this.rbSphereTexture.AutoSize = true;
            this.rbSphereTexture.Checked = true;
            this.rbSphereTexture.Location = new System.Drawing.Point(138, 18);
            this.rbSphereTexture.Name = "rbSphereTexture";
            this.rbSphereTexture.Size = new System.Drawing.Size(116, 17);
            this.rbSphereTexture.TabIndex = 1;
            this.rbSphereTexture.TabStop = true;
            this.rbSphereTexture.Text = "Texture For Sphere";
            this.rbSphereTexture.UseVisualStyleBackColor = true;
            // 
            // rbMercatorProjection
            // 
            this.rbMercatorProjection.AutoSize = true;
            this.rbMercatorProjection.Location = new System.Drawing.Point(6, 18);
            this.rbMercatorProjection.Name = "rbMercatorProjection";
            this.rbMercatorProjection.Size = new System.Drawing.Size(117, 17);
            this.rbMercatorProjection.TabIndex = 0;
            this.rbMercatorProjection.Text = "Mercator Projection";
            this.rbMercatorProjection.UseVisualStyleBackColor = true;
            // 
            // btnSetGridsColor
            // 
            this.btnSetGridsColor.Location = new System.Drawing.Point(284, 72);
            this.btnSetGridsColor.Name = "btnSetGridsColor";
            this.btnSetGridsColor.Size = new System.Drawing.Size(89, 31);
            this.btnSetGridsColor.TabIndex = 0;
            this.btnSetGridsColor.Text = "Set Grids Color";
            this.btnSetGridsColor.UseVisualStyleBackColor = true;
            this.btnSetGridsColor.Click += new System.EventHandler(this.btnSetGridsColor_Click);
            // 
            // btnGridParsRefresh
            // 
            this.btnGridParsRefresh.Location = new System.Drawing.Point(379, 10);
            this.btnGridParsRefresh.Name = "btnGridParsRefresh";
            this.btnGridParsRefresh.Size = new System.Drawing.Size(72, 93);
            this.btnGridParsRefresh.TabIndex = 18;
            this.btnGridParsRefresh.Text = "Refresh";
            this.btnGridParsRefresh.UseVisualStyleBackColor = true;
            this.btnGridParsRefresh.Click += new System.EventHandler(this.btnGridParsRefresh_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(144, 39);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Lat. Lines Width";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(135, 13);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(93, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Long. Lines Width";
            // 
            // txtLatLinesWidth
            // 
            this.txtLatLinesWidth.Location = new System.Drawing.Point(234, 36);
            this.txtLatLinesWidth.Name = "txtLatLinesWidth";
            this.txtLatLinesWidth.Size = new System.Drawing.Size(44, 20);
            this.txtLatLinesWidth.TabIndex = 15;
            this.txtLatLinesWidth.Text = "1";
            // 
            // txtLongLinesWidth
            // 
            this.txtLongLinesWidth.Location = new System.Drawing.Point(234, 10);
            this.txtLongLinesWidth.Name = "txtLongLinesWidth";
            this.txtLongLinesWidth.Size = new System.Drawing.Size(44, 20);
            this.txtLongLinesWidth.TabIndex = 14;
            this.txtLongLinesWidth.Text = "1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Long. Lines";
            // 
            // txtLatitudeLinesNumber
            // 
            this.txtLatitudeLinesNumber.Location = new System.Drawing.Point(74, 36);
            this.txtLatitudeLinesNumber.Name = "txtLatitudeLinesNumber";
            this.txtLatitudeLinesNumber.Size = new System.Drawing.Size(44, 20);
            this.txtLatitudeLinesNumber.TabIndex = 13;
            this.txtLatitudeLinesNumber.Text = "5";
            // 
            // txtLongitudeLinesNumber
            // 
            this.txtLongitudeLinesNumber.Location = new System.Drawing.Point(74, 10);
            this.txtLongitudeLinesNumber.Name = "txtLongitudeLinesNumber";
            this.txtLongitudeLinesNumber.Size = new System.Drawing.Size(44, 20);
            this.txtLongitudeLinesNumber.TabIndex = 11;
            this.txtLongitudeLinesNumber.Text = "4";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 39);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Lat. Lines";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.panelColorMin);
            this.tabPage3.Controls.Add(this.panelColorMax);
            this.tabPage3.Controls.Add(this.btnSetMinColor);
            this.tabPage3.Controls.Add(this.btnSetMaxColor);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(458, 111);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Colors";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // panelColorMin
            // 
            this.panelColorMin.Location = new System.Drawing.Point(150, 43);
            this.panelColorMin.Name = "panelColorMin";
            this.panelColorMin.Size = new System.Drawing.Size(99, 31);
            this.panelColorMin.TabIndex = 4;
            // 
            // panelColorMax
            // 
            this.panelColorMax.Location = new System.Drawing.Point(150, 6);
            this.panelColorMax.Name = "panelColorMax";
            this.panelColorMax.Size = new System.Drawing.Size(99, 31);
            this.panelColorMax.TabIndex = 3;
            // 
            // btnSetMinColor
            // 
            this.btnSetMinColor.Location = new System.Drawing.Point(6, 43);
            this.btnSetMinColor.Name = "btnSetMinColor";
            this.btnSetMinColor.Size = new System.Drawing.Size(138, 31);
            this.btnSetMinColor.TabIndex = 2;
            this.btnSetMinColor.Text = "Set Bitmap Min Color";
            this.btnSetMinColor.UseVisualStyleBackColor = true;
            this.btnSetMinColor.Click += new System.EventHandler(this.btnSetMinColor_Click);
            // 
            // btnSetMaxColor
            // 
            this.btnSetMaxColor.Location = new System.Drawing.Point(6, 6);
            this.btnSetMaxColor.Name = "btnSetMaxColor";
            this.btnSetMaxColor.Size = new System.Drawing.Size(138, 31);
            this.btnSetMaxColor.TabIndex = 1;
            this.btnSetMaxColor.Text = "Set Bitmap Max Color";
            this.btnSetMaxColor.UseVisualStyleBackColor = true;
            this.btnSetMaxColor.Click += new System.EventHandler(this.btnSetMaxColor_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txtHeight);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.btnSavePicture);
            this.tabPage1.Controls.Add(this.txtWidth);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.txtOutputFile);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.btnPathToFile);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(458, 111);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Export";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnSavePicture
            // 
            this.btnSavePicture.BackColor = System.Drawing.Color.Gray;
            this.btnSavePicture.ForeColor = System.Drawing.Color.Black;
            this.btnSavePicture.Location = new System.Drawing.Point(7, 7);
            this.btnSavePicture.Name = "btnSavePicture";
            this.btnSavePicture.Size = new System.Drawing.Size(97, 28);
            this.btnSavePicture.TabIndex = 2;
            this.btnSavePicture.Text = "Save Picture";
            this.btnSavePicture.UseVisualStyleBackColor = false;
            this.btnSavePicture.Click += new System.EventHandler(this.btnSavePicture_Click);
            // 
            // txtOutputFile
            // 
            this.txtOutputFile.Location = new System.Drawing.Point(139, 11);
            this.txtOutputFile.Name = "txtOutputFile";
            this.txtOutputFile.Size = new System.Drawing.Size(260, 20);
            this.txtOutputFile.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(110, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "File";
            // 
            // btnPathToFile
            // 
            this.btnPathToFile.BackColor = System.Drawing.Color.Gray;
            this.btnPathToFile.ForeColor = System.Drawing.Color.Black;
            this.btnPathToFile.Location = new System.Drawing.Point(405, 11);
            this.btnPathToFile.Name = "btnPathToFile";
            this.btnPathToFile.Size = new System.Drawing.Size(47, 20);
            this.btnPathToFile.TabIndex = 9;
            this.btnPathToFile.Text = "...";
            this.btnPathToFile.UseVisualStyleBackColor = false;
            this.btnPathToFile.Click += new System.EventHandler(this.btnPathToFile_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(466, 137);
            this.tabControl1.TabIndex = 14;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label11);
            this.tabPage2.Controls.Add(this.label10);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.txtYLabel);
            this.tabPage2.Controls.Add(this.txtXLabel);
            this.tabPage2.Controls.Add(this.txtTitle);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(458, 111);
            this.tabPage2.TabIndex = 4;
            this.tabPage2.Text = "Labels";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 66);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(60, 13);
            this.label11.TabIndex = 17;
            this.label11.Text = "Y-axis label";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(9, 40);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(60, 13);
            this.label10.TabIndex = 16;
            this.label10.Text = "X-axis label";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(42, 14);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(27, 13);
            this.label9.TabIndex = 15;
            this.label9.Text = "Title";
            // 
            // txtYLabel
            // 
            this.txtYLabel.Location = new System.Drawing.Point(75, 63);
            this.txtYLabel.Name = "txtYLabel";
            this.txtYLabel.Size = new System.Drawing.Size(187, 20);
            this.txtYLabel.TabIndex = 14;
            this.txtYLabel.Text = "Latitude";
            // 
            // txtXLabel
            // 
            this.txtXLabel.Location = new System.Drawing.Point(75, 37);
            this.txtXLabel.Name = "txtXLabel";
            this.txtXLabel.Size = new System.Drawing.Size(187, 20);
            this.txtXLabel.TabIndex = 13;
            this.txtXLabel.Text = "Longitude";
            // 
            // txtTitle
            // 
            this.txtTitle.Location = new System.Drawing.Point(75, 11);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(187, 20);
            this.txtTitle.TabIndex = 12;
            this.txtTitle.Text = "Surface model";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Gray;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.Location = new System.Drawing.Point(475, 25);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(107, 112);
            this.button1.TabIndex = 19;
            this.button1.Text = "Refresh";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Silver;
            this.panel1.Controls.Add(this.label12);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.tabControl1);
            this.panel1.Location = new System.Drawing.Point(12, 473);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(729, 144);
            this.panel1.TabIndex = 20;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Monotype Corsiva", 24F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.Red;
            this.label12.Location = new System.Drawing.Point(588, 36);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(132, 78);
            this.label12.TabIndex = 20;
            this.label12.Text = "MapView\r\nv. 1.0";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MapView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(753, 629);
            this.Controls.Add(this.panel1);
            this.ForeColor = System.Drawing.Color.Black;
            this.MaximizeBox = false;
            this.Name = "MapView";
            this.Text = "MapView";
            this.Load += new System.EventHandler(this.MapView_Load);
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.panelGridColor.ResumeLayout(false);
            this.panelGridColor.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtLatitudeLinesNumber;
        private System.Windows.Forms.TextBox txtLongitudeLinesNumber;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btnSavePicture;
        private System.Windows.Forms.TextBox txtOutputFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnPathToFile;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtLatLinesWidth;
        private System.Windows.Forms.TextBox txtLongLinesWidth;
        private System.Windows.Forms.Button btnSetGridsColor;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbSphereTexture;
        private System.Windows.Forms.RadioButton rbMercatorProjection;
        private System.Windows.Forms.Button btnGridParsRefresh;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panelGridColor;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnSetMinColor;
        private System.Windows.Forms.Button btnSetMaxColor;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtYLabel;
        private System.Windows.Forms.TextBox txtXLabel;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Panel panelColorMin;
        private System.Windows.Forms.Panel panelColorMax;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label12;
    }
}