using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GKProj2
{
    public class Matrix
    {
        private double[,] cells;
        public Matrix()
        {
            cells = new double[3, 3];

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    cells[i, j] = 0;
        }

        public Matrix(Vector col1, Vector col2, Vector col3)
        {
            cells = new double[3, 3];
            cells[0, 0] = col1.x; cells[1, 0] = col1.y; cells[2, 0] = col1.z;
            cells[0, 1] = col2.x; cells[1, 1] = col2.y; cells[2, 1] = col2.z;
            cells[0, 2] = col3.x; cells[1, 2] = col3.y; cells[2, 2] = col3.z;
        }

        public double GetValue(int x, int y)
        {
            return cells[x, y];
        }

        public Vector MultiplyByVector(Vector v)
        {
            return new Vector(
                v.x * cells[0, 0] + v.y * cells[0, 1] + v.z * cells[0, 2],
                v.x * cells[1, 0] + v.y * cells[1, 1] + v.z * cells[1, 2],
                v.x * cells[2, 0] + v.y * cells[2, 1] + v.z * cells[2, 2]
                );
        }
    }
}
