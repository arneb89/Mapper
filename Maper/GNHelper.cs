using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLib;

namespace Maper
{
    class GNHelper
    {
        // Jacoby matrix;
        private Matrix D;
        // Vector-function;
        private Matrix F;
        // Requared parameters for bulding J and F;
        private double jsp, jph, sqrtLambda;
        private Matrix I;
        private int hRowCount, hColCount;

        public GNHelper(Matrix H, Matrix I, double jph, double jsp, double sqrtlambda)
        {
            this.D = new Matrix(H.RowCount + H.ColumnCount, H.ColumnCount);
            this.F = new Matrix(H.RowCount + H.ColumnCount, 1);
            this.I = I;
            this.jph = jph;
            this.jsp = jsp;
            this.sqrtLambda = sqrtlambda;
            this.hRowCount = H.RowCount;
            this.hColCount = H.ColumnCount;

            for (int i = 0; i < H.RowCount; i++)
            {
                for (int j = 0; j < H.ColumnCount; j++)
                {
                    this.D[i,j] = H[i,j];
                }
            }
        }

        public Matrix GetJacobyMatrix(Matrix J)
        {
            for (int i = 0; i < this.hColCount; i++)
            {
                this.D[i + this.hRowCount, i] = this.sqrtLambda * ((J[i] - this.jph) + (J[i] - this.jsp));
            }
            return this.D;
        }

        public Matrix GetVectorFunction(Matrix J)
        {
            double sum = 0;
            for (int i = 0; i < this.hRowCount; i++)
            {
                sum = 0;
                for (int k = 0; k < this.hColCount; k++)
                {
                    sum = sum + this.D[i, k] * J[k];
                }
                this.F[i] = sum - this.I[i];
            }
            for (int i = 0; i < this.hColCount; i++)
            {
                this.F[i + this.hRowCount] = this.sqrtLambda * (J[i] - this.jph) * (J[i] - this.jsp);
            }
            return this.F;
        }
    }
}
