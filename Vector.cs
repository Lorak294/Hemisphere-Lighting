using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GKProj2
{
    public class Vector
    {
        public double x, y, z;

        public double Length { get { return Math.Sqrt(x*x + y*y + z*z); } }

        public Vector(double x, double y, double z)
        {
            this.x = x; this.y = y; this.z = z;
        }

        public Vector(Point3D p1, Point3D p2)
        {
            x = p2.x - p1.x;
            y = p2.y - p1.y;
            z = p2.z - p1.z;
        }

        public void Normalize()
        {
            double l = Length;
            x /= l;
            y /= l;
            z /= l;
        }
    }
}
