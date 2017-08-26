using System;
using CsGL.Basecode;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Maper
{
    public sealed class Viewer3D:Model
    {
        // --- Fields ---
        #region Public Properties
        /// <summary>
        /// Lesson title.
        /// </summary>
        public override string Title
        {
            get
            {
                return "Viewer3D";
            }
        }

        /// <summary>
        /// Lesson description.
        /// </summary>
        public override string Description
        {
            get
            {
                return "Viewer3D.";
            }
        }

        #endregion Public Properties

        private float[][][] coord;
        private float[][] colors;
        private Color color0 = Color.Black;
        private Color color1 = Color.White;

        public Viewer3D(float[][] rects, float[] vals)
            : base()
        {
            float[] dec = new float[3];

            this.colors = new float[vals.Length][];
            for (int i = 0; i < vals.Length; i++)
            {
                this.colors[i] = new float[3];
            }

            this.coord = new float[vals.Length][][];
            for (int i = 0; i < vals.Length; i++)
            {
                this.coord[i] = new float[4][];
                for (int j = 0; j < 4; j++)
                {
                    this.coord[i][j] = new float[3];
                }
            }

            float radius = 1.0f;

            for (int i = 0; i < this.coord.Length; i++)
            {
                dec = this.SphereToDec(new float[3] { radius, (float)rects[i][0], (float)rects[i][2] });
                this.coord[i][0] = dec;
                dec = this.SphereToDec(new float[3] { radius, (float)rects[i][0], (float)rects[i][3] });
                this.coord[i][1] = dec;
                dec = this.SphereToDec(new float[3] { radius, (float)rects[i][1], (float)rects[i][2] });
                this.coord[i][2] = dec;
                dec = this.SphereToDec(new float[3] { radius, (float)rects[i][1], (float)rects[i][3] });
                this.coord[i][3] = dec;
            }

            double iMax = vals.Max();
            double iMin = vals.Min();

            int R1, G1, B1, R0, G0, B0;

            R1 = color1.R; G1 = color1.G; B1 = color1.B;
            R0 = color0.R; G0 = color0.G; B0 = color0.B;

            double kR, kB, kG;
            if (iMin != iMax)
            {
                kR = (float)(R1 - R0) / (255 * (iMax - iMin));
                kG = (float)(G1 - G0) / (255 * (iMax - iMin));
                kB = (float)(B1 - B0) / (255 * (iMax - iMin));
            }
            else
            {
                kR = 0; kG = 0; kB = 0;
            }


            for (int i = 0; i < this.colors.Length; i++)
            {
                this.colors[i][0] = (float)(kR * (vals[i] - iMin) + R0);
                this.colors[i][1] = (float)(kG * (vals[i] - iMin) + G0);
                this.colors[i][2] = (float)(kB * (vals[i] - iMin) + B0);
            }
        }

        private float[] SphereToDec(float[] spc)
        {
            float[] dec = new float[3];
            dec[2] = spc[0] * (float)Math.Sin(spc[1]) * (float)Math.Sin(spc[2]);
            dec[0] = spc[0] * (float)Math.Sin(spc[1]) * (float)Math.Cos(spc[2]);
            dec[1] = spc[0] * (float)Math.Cos(spc[1]);
            return dec;
        }

        private static float rquad = 0;
        private static float xrot = 0;
        private static float yrot = 0;
        private static float zrot = 0;
        private static float zoom = -6.0f;

        // --- Basecode Methods ---
        #region Draw()
        /// <summary>
        /// Draws NeHe Lesson 01 scene.
        /// </summary>
        public override void Draw()
        {									                            // Here's Where We Do All The Drawing
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);			// Clear Screen And Depth Buffer
            glLoadIdentity();			
												                        // Reset The Current Modelview Matrix
            glTranslatef(-0.0f, 0.0f, zoom);

            glRotatef(xrot, 1.0f, 0.0f, 0.0f);
            glRotatef(yrot, 0.0f, 1.0f, 0.0f);
            glRotatef(zrot, 0.0f, 0.0f, 1.0f);

            glBegin(GL_QUADS);											// Draw A Quad
            for (int i = 0; i < this.coord.Length; i++)
            {
                glColor3f(this.colors[i][0], this.colors[i][1], this.colors[i][2]);
                glVertex3f(this.coord[i][0][0], this.coord[i][0][1], this.coord[i][0][2]);		// Top Left
                glColor3f(this.colors[i][0], this.colors[i][1], this.colors[i][2]);
                glVertex3f(this.coord[i][2][0], this.coord[i][2][1], this.coord[i][2][2]);		// Top Right
                glColor3f(this.colors[i][0], this.colors[i][1], this.colors[i][2]);
                glVertex3f(this.coord[i][3][0], this.coord[i][3][1], this.coord[i][3][2]);  	// Bottom Right
                glColor3f(this.colors[i][0], this.colors[i][1], this.colors[i][2]);
                glVertex3f(this.coord[i][1][0], this.coord[i][1][1], this.coord[i][1][2]);		// Bottom Left
            }
            glEnd();
        }
        #endregion Draw()

        public override void ProcessInput()
        {
            base.ProcessInput();					        // Handle The Default Basecode Keys

            if (KeyState[(int)Keys.Up])
            {												// Is Up Arrow Being Pressed?
                xrot -= 0.8f;								// If So, Decrease xspeed
            }

            if (KeyState[(int)Keys.Down])
            {												// Is Down Arrow Being Pressed?
                xrot += 0.8f;								// If So, Increase xspeed
            }

            if (KeyState[(int)Keys.Left])
            {												// Is Left Arrow Being Pressed?
                yrot -= 0.8f;								// If So, Decrease yspeed
            }

            if (KeyState[(int)Keys.Right])
            {											    // Is Right Arrow Being Pressed?
                yrot += 0.8f;								// If So, Increase yspeed
            }

            if (KeyState[(int)Keys.Home])
            {
                zoom += 0.1f;
            }

            if(KeyState[(int)Keys.End])
            {
                zoom -= 0.1f;
            }
        }
    }
}
