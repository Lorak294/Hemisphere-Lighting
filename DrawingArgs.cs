using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GKProj2
{
    public static class DrawingArgs
    {
        public static int m;
        public static double ks, kd;
        public static Vert? lightPosition;
        public static Color sphereColor;
        public static Color lightColor;
        public static bool vecInterpolation;
        public static bool r3;
        public static bool textureDraw;
        public static LockBitmap? textureLockBitmap;

        public static Color GetPixelColor(int x, int y)
        {
            if (textureDraw && textureLockBitmap != null)
            {
                Point texturePixel = RenderParameters.GetSphereRealtivePixel(x, y);
                return textureLockBitmap.GetPixel(texturePixel.X % textureLockBitmap.Width, texturePixel.Y % textureLockBitmap.Height);
            }
            else
            {
                return sphereColor;
            }
        }
    }

    public static class RenderParameters
    {
        private const int MARGIN = 20;

        public static int width = 0;
        public static int height = 0;
        public static int XMove { get { return width / 2; } }
        public static int YMove { get { return height / 2; } }
        public static double SphereRadius { get { return Math.Min(XMove, YMove) - MARGIN; } }

        public static Point3D NormalizeRealPoint(double px, double py, double pz = 0)
        {
            return new Point3D((double)(px - XMove) / SphereRadius, (double)(py - YMove) / SphereRadius, pz / SphereRadius);
        }
        public static Point3D GetRealPoint(double px, double py, double pz = 0)
        {
            return new Point3D(px * SphereRadius + XMove, py * SphereRadius + YMove, pz * SphereRadius);
        }

        public static Point GetSphereRealtivePixel(int x, int y)
        {
            return new Point(x-(XMove-(int)SphereRadius), y- (YMove - (int)SphereRadius));
        }

    }
    public struct Point3D
    {
        public double x, y, z;

        public Point3D(double x, double y, double z = 0)
        {
            this.x = x; this.y = y; this.z = z;
        }
    }

    public struct NormalizedColor
    {
        public double r, g, b;

        public NormalizedColor(double r, double g, double b)
        {
            this.r = r; this.g = g; this.b = b;
        }

        public NormalizedColor(Color c)
        {
            this = ColorConverter.RGBToStandarized(c);
        }
    }
}
