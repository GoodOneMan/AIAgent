using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent.Structures
{
    public struct Point
    {
        public const int SizeInBytes = sizeof(float) * 3;
        public const int Stride = SizeInBytes;
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public Point(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
    }
}
