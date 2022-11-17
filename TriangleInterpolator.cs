using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GKProj2
{
    public class TriangleInterpolator
    {
        private double alfa;
        private double beta;
        private double gamma;

        public TriangleInterpolator(Point3D v1, Point3D v2, Point3D v3, Point3D p, bool r3)
        {
            if (r3)
            {
                double area = MathFunctions.TriangleArea(v1, v2, v3, r3);
                alfa = MathFunctions.TriangleArea(p, v2, v3, r3) / area;
                beta = MathFunctions.TriangleArea(p, v1, v3, r3) / area;

            }
            else
            {
                alfa = ((v2.y - v3.y) * (p.x - v3.x) + (v3.x - v2.x) * (p.y - v3.y)) /
                    ((v2.y - v3.y) * (v1.x - v3.x) + (v3.x - v2.x) * (v1.y - v3.y));

                beta = ((v3.y - v1.y) * (p.x - v3.x) + (v1.x - v3.x) * (p.y - v3.y)) /
                    ((v2.y - v3.y) * (v1.x - v3.x) + (v3.x - v2.x) * (v1.y - v3.y));
            }

            gamma = 1 - alfa - beta;

            //if (gamma < 0 || alfa < 0 || beta < 0)
            //{
            //    Debug.WriteLine("WTF");
            //}
        }

        public double Interpolate(double value1, double value2, double value3)
        {
            return alfa * value1 + beta * value2 + gamma * value3;
        }
    }
}
