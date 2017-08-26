using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maper
{
    public class Noiser
    {
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        /// <summary>
        /// Be carefull!!!!!!!!!!!!!!!!
        /// </summary>
        /// <param name="map"></param>
        /// <param name="sigma"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int[,] AddNormNoise(int[,] map, double sigma, int count)
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

            Random rnd = new Random(count);

            int[,] mapN = new int[map.GetLength(0), map.GetLength(1)];

            double u1, u2, v1, v2, x1, x2, s;

            int xPN=map.GetLength(0), yPN=map.GetLength(1);

            int size = map.Length;
            
            for (int i = 0; i < size; i = i + 2)
            {
                do
                {
                    u1 = (double)rnd.NextDouble();
                    u2 = (double)rnd.NextDouble();
                    v1 = 2 * u1 - 1;
                    v2 = 2 * u2 - 1;
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1);

                x1 = v1 * Math.Sqrt(-Math.Log(s) / s);
                x2 = v2 * Math.Sqrt(-Math.Log(s) / s);

                mapN[i % xPN, i / xPN] =(int)(map[i % xPN, i / xPN]+
                    map[i % xPN, i / xPN]* x1*sigma);

                if (i + 1 < size)
                {
                    mapN[(i+1) % xPN, (i + 1) / xPN] = (int)(map[(i+1) % xPN, (i + 1) / xPN]+ 
                        map[(i+1) % xPN, (i + 1) / xPN]*x2*sigma);
                }
                else
                {
                     break;
                }
            }  
            return mapN;
        }

        public static double[] AddNormNoise(double[] mas, double sigma, int count)
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

            Random rnd = new Random((int)DateTime.Now.Ticks);

            int size = mas.Length;

            double[] masN = new double[size];

            double u1, u2, v1, v2, x1, x2, s;

            for (int i = 0; i < size; i = i + 2)
            {
                do
                {
                    u1 = (double)rnd.NextDouble();
                    u2 = (double)rnd.NextDouble();
                    v1 = 2 * u1 - 1;
                    v2 = 2 * u2 - 1;
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1);

                x1 = v1 * Math.Sqrt(-Math.Log(s) / s);
                x2 = v2 * Math.Sqrt(-Math.Log(s) / s);

                masN[i] = mas[i] +  x1 * sigma;

                if (i + 1 < size)
                {
                    masN[i+1] = mas[i + 1] + x2 * sigma;
                }
                else
                {
                    break;
                }
            }
            return masN;
        }
    }
}
