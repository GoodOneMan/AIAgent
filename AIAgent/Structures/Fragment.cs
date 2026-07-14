using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace AIAgent.Structures
{
    public class Fragment
    {
        public double[] Matrix;
        public Tuple<Point[], uint[]> Triangles;
        public Point[] Vertices;
        public Line[] Lines;
        public Point[] Points;
        public Point[] SnapPoints;
    }
}
