using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Interop.ComApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent.Model
{
    class Extractor
    {
        protected bool compare_box(InwLBox3f box_com, BoundingBox3D box_net)
        {
            if (box_com.max_pos.data1 != box_net.Max.X || box_com.max_pos.data2 != box_net.Max.Y || box_com.max_pos.data3 != box_net.Max.Z ||
               box_com.min_pos.data1 != box_net.Min.X || box_com.min_pos.data2 != box_net.Min.Y || box_com.min_pos.data3 != box_net.Min.Z)
                return false;
            return true;
        }
        protected bool compare_box(double[] box1, double[] box2)
        {
            if (!box1.SequenceEqual(box2))
                return false;
            return true;
        }
        protected bool compare_point(double[] matrix, Point3D center)
        {
            if (matrix[12] != center.X || matrix[13] != center.Y || matrix[14] != center.Z)
                return false;
            else
                return true;
        }

        protected double[] present_matrix(Array matrix)
        {
            double[] _matrix = new double[16];

            for (int i = 0; i < 16; i++)
            {
                _matrix[i] = (double)matrix.GetValue(i + 1);
            }

            return _matrix;
        }
        protected double[] present_color(Color color)
        {
            return new double[] { color.R, color.G, color.B };
        }
        protected double[] present_box(BoundingBox3D box)
        {
            return new double[] { box.Min.X, box.Min.Y, box.Min.Z, box.Max.X, box.Max.Y, box.Max.Z };
        }
        protected double[] present_box(InwLBox3f box)
        {
            return new double[] { box.min_pos.data1, box.min_pos.data2, box.min_pos.data3, box.max_pos.data1, box.max_pos.data2, box.max_pos.data3 };
        }

    }
}
