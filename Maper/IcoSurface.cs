using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper
{
    public class IcoSurface
    {
        const double x = 0.525731112119133606;
        const double z = 0.850650808352039932;

        double[][] vdata = new double[12][]{    
                     new double[3]{-x, 0.0, z},  new double[3]{x, 0.0, z}, 
                     new double[3]{-x, 0.0, -z}, new double[3]{x, 0.0, -z},    
                     new double[3]{0.0, z, x},   new double[3]{0.0, z, -x}, 
                     new double[3]{0.0, -z, x},  new double[3]{0.0,-z, -x},    
                     new double[3]{z, x, 0.0},   new double[3]{-z, x, 0.0}, 
                     new double[3]{z, -x, 0.0},  new double[3]{-z, -x, 0.0} };

        int[][] tindices = new int[20][]{ 
                     new int[3]{0,4,1},  new int[3]{0,9,4},  
                     new int[3]{9,5,4},  new int[3]{4,5,8}, 
                     new int[3]{4,8,1},  new int[3]{8,10,1}, 
                     new int[3]{8,3,10}, new int[3]{5,3,8}, 
                     new int[3]{5,2,3},  new int[3]{2,7,3},    
                     new int[3]{7,10,3}, new int[3]{7,6,10}, 
                     new int[3]{7,11,6}, new int[3]{11,0,6}, 
                     new int[3]{0,1,6},  new int[3]{6,1,10}, 
                     new int[3]{9,0,11}, new int[3]{9,11,2}, 
                     new int[3]{9,2,5},  new int[3]{7,2,11} };
        //http://fly.cc.fer.hr/~unreal/theredbook/chapter02.html

        double[][] verts;

        private double[] Normalize(ref double[] v)
        {
            double norm = Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
            v[0] /= norm;
            v[1] /= norm;
            v[2] /= norm;
            return v;
        }

        private void Subdivide(double[] v1, double[] v2, double[] v3, int depth)
        {
            double[] v12 = new double[3];
            double[] v23 = new double[3];
            double[] v31 = new double[3];

            if (depth == 0)
            {
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                v12[0] = 0.5 * (v1[0] + v2[0]);
                v12[1] = 0.5 * (v1[1] + v2[1]);
                v12[2] = 0.5 * (v1[2] + v2[2]);
                v23[0] = 0.5 * (v2[0] + v3[0]);
                v23[1] = 0.5 * (v2[1] + v3[1]);
                v23[2] = 0.5 * (v2[2] + v3[2]);
                v31[0] = 0.5 * (v3[0] + v1[0]);
                v31[1] = 0.5 * (v3[1] + v1[1]);
                v31[2] = 0.5 * (v3[2] + v1[2]);
            }

            this.Normalize(ref v12);
            this.Normalize(ref v23);
            this.Normalize(ref v31);

            this.Subdivide(v1, v12, v31, depth - 1);
            this.Subdivide(v2, v23, v12, depth - 1);
            this.Subdivide(v3, v31, v23, depth - 1);
            this.Subdivide(v12, v23, v31, depth - 1);
        }

        public void GetCoordArray(int depth)
        {

            int vertsInitNumber = 12 + 3 * depth;

            verts = new double[vertsInitNumber][];
            for (int i = 0; i < vertsInitNumber; i++) verts[i] = new double[3];


        }
    }
}
