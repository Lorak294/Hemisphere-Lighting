using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GKProj2
{
    public static class MathFunctions
    {
        public static NormalizedColor CalculateFinalColor(
            Vector normVector,
            Vector lightVector,
            Vector visionVector,
            double m, double kd, double ks,
            NormalizedColor textureColor,
            NormalizedColor lightColor)
        {
            double cosNL = Math.Max(0.0, DotProduct(normVector, lightVector));
            Vector rVector = new Vector(2 * cosNL * normVector.x - lightVector.x, 2 * cosNL * normVector.y - lightVector.y, 2 * cosNL * normVector.z - lightVector.z);
            double cosVRm = Math.Pow(Math.Max(0, DotProduct(visionVector, rVector)), m);

            double finalR = kd * lightColor.r * textureColor.r * cosNL + ks * lightColor.r * textureColor.r * cosVRm;
            double finalG = kd * lightColor.g * textureColor.g * cosNL + ks * lightColor.g * textureColor.g * cosVRm;
            double finalB = kd * lightColor.b * textureColor.b * cosNL + ks * lightColor.b * textureColor.b * cosVRm;

            return new NormalizedColor(finalR, finalG, finalB);
        }

        public static double DotProduct(Vector v1, Vector v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        public static double TriangleArea(Point3D a, Point3D b, Point3D c, bool r3 = true)
        {
            if (r3)
            {
                double
                x_ac = c.x - a.x,
                x_ab = b.x - a.x,
                y_ab = b.y - a.y,
                y_ac = c.y - a.y,
                z_ab = b.z - a.z,
                z_ac = c.z - a.z;

                return 0.5 * Math.Sqrt(
                    (y_ab * z_ac - z_ab * y_ac) * (y_ab * z_ac - z_ab * y_ac) +
                    (z_ab * x_ac - x_ab * z_ac) * (z_ab * x_ac - x_ab * z_ac) +
                    (x_ab * y_ac - y_ab * x_ac) * (x_ab * y_ac - y_ab * x_ac));
            }
            else
            {
                return 0.5 * Math.Abs(
                    a.x*(b.y-c.y) +
                    b.x*(c.y-a.y) +
                    c.x*(a.y-b.y));
            }
            
        }

        public static double Distance(Point3D p1, Point3D p2, bool r3 = true)
        {
            if (r3)
                return Math.Sqrt((p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y) + (p2.z - p1.z) * (p2.z - p1.z));
            else
                return Math.Sqrt((p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y));
        }

        public static double Sign(Point3D p1, Point3D p2, Point3D p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        public static bool PointInTriangle(Point3D pt, Point3D v1, Point3D v2, Point3D v3)
        {
            double d1, d2, d3;
            bool has_neg, has_pos;

            d1 = Sign(pt, v1, v2);
            d2 = Sign(pt, v2, v3);
            d3 = Sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }
    }
}
