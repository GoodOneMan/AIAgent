using AIAgent.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent.Model
{
    public static class MeshOptimizer
    {
        public static Tuple<T[], uint[]> SimplifyMesh<T>(T[] Vertices, uint[] Indices, uint VertexSize, float Threshold)
        {
            uint[] destination = new uint[Indices.Length];
            //uint[] Indices = Indices;
            UIntPtr IndexCount = (UIntPtr)Indices.Length;
            Pointer VertexPositions = Pointer.Create(Vertices);
            UIntPtr VertexCount = (UIntPtr)Vertices.Length;
            UIntPtr VertexPositionsStride = (UIntPtr)VertexSize;
            UIntPtr TargetIndexCount = (UIntPtr)(Indices.Length * Threshold);
            float TargetError = 0f;
            UIntPtr Options = (UIntPtr)0;
            IntPtr ResultError = (IntPtr)0;

            UInt32 result = 0;

            TargetError = 1e-2f;
            result = MeshOptimizerNative.Simplify(destination, Indices, IndexCount, VertexPositions.Address, VertexCount, VertexPositionsStride, TargetIndexCount, TargetError, Options, ResultError);

            VertexPositions.Free();

            Array.Resize(ref destination, (int)result);

            return Optimize(Vertices, destination, Point.SizeInBytes, 1.5f);

        }


        public static Tuple<T[], uint[]> Optimize<T>(T[] Vertices, uint[] Indices, uint VertexSize, float Threshold)
        {
            var results = Reindex(Vertices, Indices, VertexSize);
            var vertices = results.Item1;
            var indices = results.Item2;

            OptimizeCache(indices, vertices.Length);
            OptimizeOverdraw(indices, vertices, VertexSize, Threshold);
            OptimizeVertexFetch(indices, vertices, VertexSize);
            return Tuple.Create(vertices, indices);
        }


        public static Tuple<T[], uint[]> Reindex<T>(T[] Vertices, uint[] Indices, uint VertexSize)
        {
            var remap = new uint[Vertices.Length];
            var vertexPointer = Pointer.Create(Vertices);
            var indexCount = (Indices?.Length ?? Vertices.Length);
            var totalVertices = MeshOptimizerNative.GenerateVertexRemap(
                remap,
                Indices,
                (UIntPtr)indexCount,
                vertexPointer.Address,
                (UIntPtr)Vertices.Length,
                (UIntPtr)VertexSize
            );

            var indices = new uint[indexCount];
            MeshOptimizerNative.RemapIndexBuffer(indices, Indices, (UIntPtr)indexCount, remap);

            var vertices = new T[totalVertices];
            var targetVerticesPointer = Pointer.Create(vertices);
            MeshOptimizerNative.RemapVertexBuffer(targetVerticesPointer.Address, vertexPointer.Address, (UIntPtr)Vertices.Length, (UIntPtr)VertexSize, remap);

            vertexPointer.Free();
            targetVerticesPointer.Free();
            return Tuple.Create(vertices, indices);
        }


        public static void OptimizeCache(uint[] Indices, int VertexCount)
        {
            MeshOptimizerNative.OptimizeVertexCache(Indices, Indices, (UIntPtr)Indices.Length, (UIntPtr)VertexCount);
        }


        public static void OptimizeOverdraw<T>(uint[] Indices, T[] Vertices, uint Stride, float Threshold)
        {
            var pointer = Pointer.Create(Vertices);
            MeshOptimizerNative.OptimizeOverdraw(Indices, Indices, (UIntPtr)Indices.Length, pointer.Address, (UIntPtr)Vertices.Length, (UIntPtr)Stride, Threshold);
            pointer.Free();
        }


        public static void OptimizeVertexFetch<T>(uint[] Indices, T[] Vertices, uint VertexSize)
        {
            var pointer = Pointer.Create(Vertices);
            MeshOptimizerNative.OptimizeVertexFetch(pointer.Address, Indices, (UIntPtr)Indices.Length, pointer.Address, (UIntPtr)Vertices.Length, (UIntPtr)VertexSize);
            pointer.Free();
        }
    }

    internal static class MeshOptimizerNative
    {
        //private const string MeshOptimizerDLL = @"C:\Program Files\Autodesk\Navisworks Manage 2023\Plugins\ImportDataOPM\Xbim\meshoptimizer.dll";

        private const string MeshOptimizerDLL = "opt.dll";

        [DllImport(MeshOptimizerDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint meshopt_generateVertexRemap(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexSize);

        [DllImport(MeshOptimizerDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern void meshopt_remapIndexBuffer(uint[] Destination, uint[] Indices, UIntPtr IndexCount, uint[] Remap);

        [DllImport(MeshOptimizerDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern void meshopt_remapVertexBuffer(IntPtr Destination, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexSize, uint[] Remap);

        [DllImport(MeshOptimizerDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern void meshopt_optimizeVertexCache(uint[] Destination, uint[] Indices, UIntPtr IndexCount, UIntPtr VertexCount);

        [DllImport(MeshOptimizerDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern void meshopt_optimizeOverdraw(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr VertexPositions, UIntPtr VertexCount, UIntPtr Stride, float Threshold);

        [DllImport(MeshOptimizerDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint meshopt_optimizeVertexFetch(IntPtr Destination, uint[] Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexSize);

        [DllImport(MeshOptimizerDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint meshopt_simplify(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr VertexPositions, UIntPtr VertexCount, UIntPtr VertexPositionsStride, UIntPtr TargetIndexCount, float TargetError, UIntPtr Options, IntPtr ResultError);

        [DllImport(MeshOptimizerDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint meshopt_simplifySloppy(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr VertexPositions, UIntPtr VertexCount, UIntPtr VertexPositionsStride, UIntPtr TargetIndexCount, float TargetError, UIntPtr Options, IntPtr ResultError);

        public static uint GenerateVertexRemap(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexSize)
        {
            return meshopt_generateVertexRemap(Destination, Indices, IndexCount, Vertices, VertexCount, VertexSize);
        }

        public static void RemapIndexBuffer(uint[] Destination, uint[] Indices, UIntPtr IndexCount, uint[] Remap)
        {
            meshopt_remapIndexBuffer(Destination, Indices, IndexCount, Remap);
        }

        public static void RemapVertexBuffer(IntPtr Destination, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexSize, uint[] Remap)
        {
            meshopt_remapVertexBuffer(Destination, Vertices, VertexCount, VertexSize, Remap);
        }

        public static void OptimizeVertexCache(uint[] Destination, uint[] Indices, UIntPtr IndexCount, UIntPtr VertexCount)
        {
            meshopt_optimizeVertexCache(Destination, Indices, IndexCount, VertexCount);
        }

        public static void OptimizeOverdraw(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr VertexPositions, UIntPtr VertexCount, UIntPtr Stride, float Threshold)
        {
            meshopt_optimizeOverdraw(Destination, Indices, IndexCount, VertexPositions, VertexCount, Stride, Threshold);
        }

        public static uint OptimizeVertexFetch(IntPtr Destination, uint[] Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexSize)
        {
            return meshopt_optimizeVertexFetch(Destination, Indices, IndexCount, Vertices, VertexCount, VertexSize);
        }

        public static uint Simplify(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr VertexPositions, UIntPtr VertexCount, UIntPtr VertexPositionsStride, UIntPtr TargetIndexCount, float TargetError, UIntPtr Options, IntPtr ResultError)
        {
            return meshopt_simplify(Destination, Indices, IndexCount, VertexPositions, VertexCount, VertexPositionsStride, TargetIndexCount, TargetError, Options, ResultError);
        }

        public static uint SimplifySloppy(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr VertexPositions, UIntPtr VertexCount, UIntPtr VertexPositionsStride, UIntPtr TargetIndexCount, float TargetError, UIntPtr Options, IntPtr ResultError)
        {
            return meshopt_simplifySloppy(Destination, Indices, IndexCount, VertexPositions, VertexCount, VertexPositionsStride, TargetIndexCount, TargetError, Options, ResultError);
        }
    }

    internal class Pointer
    {
        private GCHandle _handle;
        public IntPtr Address { get; private set; }

        private Pointer()
        {
        }

        public void Free()
        {
            _handle.Free();
        }

        public static Pointer Create<T>(T Object)
        {
            var pointer = new Pointer
            {
                _handle = GCHandle.Alloc(Object, GCHandleType.Pinned)
            };
            pointer.Address = pointer._handle.AddrOfPinnedObject();
            return pointer;
        }
    }
}
