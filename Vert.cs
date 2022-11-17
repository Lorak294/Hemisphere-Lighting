using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GKProj2
{
    public class Vert
    {
        private static int _idProvider = 1;


        public int Id { get; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Point3D RealPosition { get { return RenderParameters.GetRealPoint(X, Y, Z); } }
        public int DispX { get { return (int)Math.Round(RealPosition.x); } }
        public int DispY { get { return (int)Math.Round(RealPosition.y); } }
        public int DispZ { get { return (int)Math.Round(RealPosition.z); } }

        private Vector _normVector;
        public Vector NormVector { get { return _normVector; } set { _normVector = value; } }
        public NormalizedColor? Color { get; set; }

        public Vert(double x, double y, double z, Vector normVector)
        {
            Id = _idProvider++;
            X = x;
            Y = y;
            Z = z;
            _normVector = normVector;
            Color = null;
        }

        public Vert(Vert otherVert)
        {
            Id = _idProvider++;
            X = otherVert.X;
            Y = otherVert.Y;
            Z = otherVert.Z;
            _normVector = otherVert.NormVector;
            Color = null;
        }
        public Vert(Vert otherVert, Vector newNormVector)
        {
            Id = _idProvider++;
            X = otherVert.X;
            Y = otherVert.Y;
            Z = otherVert.Z;
            _normVector = newNormVector;
            Color = null;
        }

        public Vert(string vertStr)
        {
            string[] args = vertStr.Split(' ');
            
            if (args[0] != "v")
                throw new ArgumentException("Wrong string psssed to vert constructor.");


            if (!double.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double _x))
                throw new ArgumentException("Wrong string psssed to vert constructor.");
            if (!double.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double _y))
                throw new ArgumentException("Wrong string psssed to vert constructor.");
            if (!double.TryParse(args[3], NumberStyles.Any, CultureInfo.InvariantCulture, out double _z))
                throw new ArgumentException("Wrong string psssed to vert constructor.");

            X = _x;
            Y = _y;
            Z = _z;
            _normVector = new Vector(0, 0, 0);
            Id = _idProvider++;
            Color = null;
        }

        public Point3D ToPoint3D()
        {
            return new Point3D(X, Y, Z);
        }

        public Point3D ToDispPoint3D()
        {
            return new Point3D(DispX,DispY,DispZ);
        }
    }
}
