using AIAgent.Structures;
using Autodesk.Navisworks.Api.Interop.ComApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent.Model
{
    class SimplePrimitivesImplementation : InwSimplePrimitivesCB
    {
        public List<Point> Vertices;
        public List<Line> Lines;

        public void Point(InwSimpleVertex v1)
        {

        }

        public void SnapPoint(InwSimpleVertex v1)
        {

        }

        public void Line(InwSimpleVertex v1, InwSimpleVertex v2)
        {
            set_lines((Array)(object)v1.coord, (Array)(object)v2.coord);
        }

        public void Triangle(InwSimpleVertex v1, InwSimpleVertex v2, InwSimpleVertex v3)
        {
            set_vertices((Array)(object)v1.coord, (Array)(object)v2.coord, (Array)(object)v3.coord);
        }

        private void set_lines(Array point1, Array point2)
        {
            Point[] points = new Point[2];
            points[0] = (new Point((float)(object)point1.GetValue(1), (float)(object)point1.GetValue(2), (float)(object)point1.GetValue(3)));
            points[1] = (new Point((float)(object)point2.GetValue(1), (float)(object)point2.GetValue(2), (float)(object)point2.GetValue(3)));
            Lines.Add(new Line() { Points = points });
        }

        private void set_vertices(Array point1, Array point2, Array point3)
        {
            Vertices.Add(new Point((float)(object)point1.GetValue(1), (float)(object)point1.GetValue(2), (float)(object)point1.GetValue(3)));
            Vertices.Add(new Point((float)(object)point2.GetValue(1), (float)(object)point2.GetValue(2), (float)(object)point2.GetValue(3)));
            Vertices.Add(new Point((float)(object)point3.GetValue(1), (float)(object)point3.GetValue(2), (float)(object)point3.GetValue(3)));
        }
    }
}
