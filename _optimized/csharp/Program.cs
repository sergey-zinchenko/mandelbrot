using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public static class Program
    {
        private static readonly int Height = 1024;
        private static readonly int Width = 1024;
        private static readonly int NumCpu = Environment.ProcessorCount;

        private static readonly uint[] Result = new uint[Height * Width];
        private static unsafe volatile uint* _pResult;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MandelbrotSimd()
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 4
            };
            Parallel.For(0, Height / 2, options, Mandelbrot_0_simd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static void Mandelbrot_0_simd(int h)
        {
            const int vectorSize = 8;
            const int vectorCount = 2;
            const float four = 4.0f;
            const float minX = -2.0f;
            const float minY = -1.0f;
            float scaleX = 3.0f / Width;
            float scaleY = 2.0f / Height;
            const int maxIters = 256;

            var minYVec = Vector256.Create(minY);
            var scaleXVec = Vector256.Create(scaleX);
            var scaleYVec = Vector256.Create(scaleY);
            var fourVec = Vector256.Create(four);
            var displacementVector = Vector256.Create(0f, 1, 2, 3, 4, 5, 6, 7);
            var identVector = Vector256.Create(1u);

            int offset = h * Width;
            int mirrorOffset = (Height - h - 1) * Width;

            for (int w = 0; w < Width; w += vectorSize * vectorCount)
            {
                var cxVec1Set = Vector256.Create(minX + w * scaleX);
                var cxVec2Set = Vector256.Create(minX + (w + vectorSize) * scaleX);
                var cyVecSet = Vector256.Create((float)h);
                var cxVec1 = Fma.MultiplyAdd(displacementVector, scaleXVec, cxVec1Set);
                var cxVec2 = Fma.MultiplyAdd(displacementVector, scaleXVec, cxVec2Set);
                var cyVec = Fma.MultiplyAdd(cyVecSet, scaleYVec, minYVec);
                var zReVec1 = Vector256<float>.Zero;
                var zImVec1 = Vector256<float>.Zero;
                var zReVec2 = Vector256<float>.Zero;
                var zImVec2 = Vector256<float>.Zero;
                var nvVec1 = Vector256<uint>.Zero;
                var nvVec2 = Vector256<uint>.Zero;
                var breakVec1 = Vector256<uint>.Zero;
                var breakVec2 = Vector256<uint>.Zero;

                int i = 0;
                do
                {
                    var zImVec1Pow2 = Avx.Multiply(zImVec1, zImVec1);
                    var zImVec2Pow2 = Avx.Multiply(zImVec2, zImVec2);

                    var zReNewVec1 = Fma.MultiplySubtract(zReVec1, zReVec1, zImVec1Pow2);
                    var zReNewVec2 = Fma.MultiplySubtract(zReVec2, zReVec2, zImVec2Pow2);

                    var zImNewVec1 = Avx.Multiply(zImVec1, zReVec1);
                    var zImNewVec2 = Avx.Multiply(zImVec2, zReVec2);

                    zReNewVec1 = Avx.Add(zReNewVec1, cxVec1);
                    zReNewVec2 = Avx.Add(zReNewVec2, cxVec2);
                    zImNewVec1 = Avx.Add(zImNewVec1, zImNewVec1);
                    zImNewVec2 = Avx.Add(zImNewVec2, zImNewVec2);
                    zImNewVec1 = Avx.Add(zImNewVec1, cyVec);
                    zImNewVec2 = Avx.Add(zImNewVec2, cyVec);

                    var zReNewVec1Pow2 = Avx.Multiply(zReNewVec1, zReNewVec1);
                    var zImNewVec1Pow2 = Avx.Multiply(zImNewVec1, zImNewVec1);
                    var zReNewVec2Pow2 = Avx.Multiply(zReNewVec2, zReNewVec2);
                    var zImNewVec2Pow2 = Avx.Multiply(zImNewVec2, zImNewVec2);

                    var mag2Vec1 = Avx.Add(zReNewVec1Pow2, zImNewVec1Pow2);
                    var mag2Vec2 = Avx.Add(zReNewVec2Pow2, zImNewVec2Pow2);

                    var cmpVec1 = Avx.Compare(mag2Vec1, fourVec, FloatComparisonMode.OrderedLessThanNonSignaling);
                    var cmpVec2 = Avx.Compare(mag2Vec2, fourVec, FloatComparisonMode.OrderedLessThanNonSignaling);

                    var breakVec1Epi32 = cmpVec1.AsUInt32();
                    var breakVec2Epi32 = cmpVec2.AsUInt32();
                    breakVec1Epi32 = Avx2.Add(breakVec1Epi32, identVector);
                    breakVec2Epi32 = Avx2.Add(breakVec2Epi32, identVector);
                    breakVec1 = Avx2.Or(breakVec1Epi32, breakVec1);
                    breakVec2 = Avx2.Or(breakVec2Epi32, breakVec2);
                    var nvVec1Epi32 = Avx2.AndNot(breakVec1, identVector);
                    var nvVec2Epi32 = Avx2.AndNot(breakVec2, identVector);
                    nvVec1 = Avx2.Add(nvVec1Epi32, nvVec1);
                    nvVec2 = Avx2.Add(nvVec2Epi32, nvVec2);

                    zImNewVec1Pow2 = Avx.Multiply(zImNewVec1, zImNewVec1);
                    zImNewVec2Pow2 = Avx.Multiply(zImNewVec2, zImNewVec2);

                    zReVec1 = Fma.MultiplySubtract(zReNewVec1, zReNewVec1, zImNewVec1Pow2);
                    zReVec2 = Fma.MultiplySubtract(zReNewVec2, zReNewVec2, zImNewVec2Pow2);

                    zImVec1 = Avx.Multiply(zImNewVec1, zReNewVec1);
                    zImVec2 = Avx.Multiply(zImNewVec2, zReNewVec2);

                    zReVec1 = Avx.Add(zReVec1, cxVec1);
                    zReVec2 = Avx.Add(zReVec2, cxVec2);
                    zImVec1 = Avx.Add(zImVec1, zImVec1);
                    zImVec2 = Avx.Add(zImVec2, zImVec2);
                    zImVec1 = Avx.Add(zImVec1, cyVec);
                    zImVec2 = Avx.Add(zImVec2, cyVec);

                    var zReVec1Pow2 = Avx.Multiply(zReVec1, zReVec1);
                    zImVec1Pow2 = Avx.Multiply(zImVec1, zImVec1);
                    var zReVec2Pow2 = Avx.Multiply(zReVec2, zReVec2);
                    zImVec2Pow2 = Avx.Multiply(zImVec2, zImVec2);

                    mag2Vec1 = Avx.Add(zReVec1Pow2, zImVec1Pow2);
                    mag2Vec2 = Avx.Add(zReVec2Pow2, zImVec2Pow2);

                    cmpVec1 = Avx.Compare(mag2Vec1, fourVec, FloatComparisonMode.OrderedLessThanNonSignaling);
                    cmpVec2 = Avx.Compare(mag2Vec2, fourVec, FloatComparisonMode.OrderedLessThanNonSignaling);

                    breakVec1Epi32 = cmpVec1.AsUInt32();
                    breakVec2Epi32 = cmpVec2.AsUInt32();
                    breakVec1Epi32 = Avx2.Add(breakVec1Epi32, identVector);
                    breakVec2Epi32 = Avx2.Add(breakVec2Epi32, identVector);
                    breakVec1 = Avx2.Or(breakVec1, breakVec1Epi32);
                    breakVec2 = Avx2.Or(breakVec2, breakVec2Epi32);

                    nvVec1Epi32 = Avx2.AndNot(breakVec1, identVector);
                    nvVec2Epi32 = Avx2.AndNot(breakVec2, identVector);
                    nvVec1 = Avx2.Add(nvVec1Epi32, nvVec1);
                    nvVec2 = Avx2.Add(nvVec2Epi32, nvVec2);

                    i += vectorCount;
                } while ((!Avx.TestZ(Avx2.AndNot(breakVec1, identVector), identVector) ||
                          !Avx.TestZ(Avx2.AndNot(breakVec2, identVector), identVector)) && i < maxIters);

                Avx.Store((_pResult + offset + w), nvVec1);
                Avx.Store((_pResult + mirrorOffset + w), nvVec1);
                Avx.Store((_pResult + offset + w + vectorSize), nvVec2);
                Avx.Store((_pResult + mirrorOffset + w + vectorSize), nvVec2);
            }
        }

        public static unsafe void Main()
        {
            Console.WriteLine("NumCpu : {0}", NumCpu);
            Console.WriteLine("Avx2.IsSupported : {0}", Avx2.IsSupported);
            Console.WriteLine("Avx2.IsSupported : {0}", Avx2.IsSupported);
            Console.WriteLine("Vector256.IsHardwareAccelerated : {0}", Vector256.IsHardwareAccelerated);
            fixed (uint* pr = Result)
            {
                _pResult = pr;

                var measurements = new List<double>();
                for (int i = -1; i < 100; i++)
                {
                    Console.Write(i + 1 + "\t ");
                    Console.Out.Flush();
                    Array.Clear(Result);
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    MandelbrotSimd();
                    stopWatch.Stop();
                    var executionTime = stopWatch.Elapsed;
                    if (i >= 0)
                    {
                        measurements.Add(executionTime.TotalMilliseconds);
                    }

                    var sum = Result.Aggregate(0u, (s, u) => s + u);
                    Console.WriteLine("Execution Time:      {0:F2}ms\t  {1}", executionTime.TotalMilliseconds, sum);
                }

                var average = measurements.Average();
                var sumOfSquares = measurements.Select(x => Math.Pow(x - average, 2)).Sum();
                var standardDeviation = Math.Sqrt(sumOfSquares / (measurements.Count - 1)) / average * 100;
                Console.WriteLine("Avg: {0:F2}ms, StdDev: {1:F2}%", average, standardDeviation);
            }

            var resultStrings = Result.Select(x => x.ToString()).ToArray();
            var resultFile = string.Join(",", resultStrings);
            var filePath = "output.txt";
            File.WriteAllText(filePath, resultFile);
        }
    }
}