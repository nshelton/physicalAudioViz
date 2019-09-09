using System;

namespace NAudio.Dsp
{

    public struct Complex
    {
        public float X;
        public float Y;
    }

    /// <summary>
    /// Summary description for FastFourierTransform.
    /// </summary>
    public static class FastFourierTransform
    {
        /// <summary>
        /// This computes an in-place complex-to-complex FFT 
        /// x and y are the real and imaginary arrays of 2^m points.
        /// </summary>
        public static void FFT(bool forward, int m, float[] data)
        {

        }

        public static void FFT(bool forward, int m, float[] reals, float[] complex)
        {
            int n, i, i1, j, k, i2, l, l1, l2;
            float c1, c2, tx, ty, t1, t2, u1, u2, z;

            // Calculate the number of points
            n = 1;
            for (i = 0; i < m; i++)
                n *= 2;

            // Do the bit reversal
            i2 = n >> 1;
            j = 0;
            for (i = 0; i < n - 1; i++)
            {
                if (i < j)
                {
                    tx = reals[i];
                    ty = complex[i];
                    reals[i] = reals[j];
                    complex[i] = complex[j];
                    reals[j] = tx;
                    complex[j] = ty;
                }
                k = i2;

                while (k <= j)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }

            // Compute the FFT 
            c1 = -1.0f;
            c2 = 0.0f;
            l2 = 1;
            for (l = 0; l < m; l++)
            {
                l1 = l2;
                l2 <<= 1;
                u1 = 1.0f;
                u2 = 0.0f;
                for (j = 0; j < l1; j++)
                {
                    for (i = j; i < n; i += l2)
                    {
                        i1 = i + l1;
                        t1 = u1 * reals[i1] - u2 * complex[i1];
                        t2 = u1 * complex[i1] + u2 * reals[i1];
                        reals[i1] = reals[i] - t1;
                        complex[i1] = complex[i] - t2;
                        reals[i] += t1;
                        complex[i] += t2;
                    }
                    z = u1 * c1 - u2 * c2;
                    u2 = u1 * c2 + u2 * c1;
                    u1 = z;
                }
                c2 = (float)Math.Sqrt((1.0f - c1) / 2.0f);
                if (forward)
                    c2 = -c2;
                c1 = (float)Math.Sqrt((1.0f + c1) / 2.0f);
            }

            // Scaling for forward transform 
            if (forward)
            {
                for (i = 0; i < n; i++)
                {
                    reals[i] /= n;
                    complex[i] /= n;
                }
            }
        }

        /// <summary>
        /// Applies a Hamming Window
        /// </summary>
        /// <param name="n">Index into frame</param>
        /// <param name="frameSize">Frame size (e.g. 1024)</param>
        /// <returns>Multiplier for Hamming window</returns>
        public static double HammingWindow(int n, int frameSize)
        {
            return 0.54 - 0.46 * Math.Cos((2 * Math.PI * n) / (frameSize - 1));
        }

        /// <summary>
        /// Applies a Hann Window
        /// </summary>
        /// <param name="n">Index into frame</param>
        /// <param name="frameSize">Frame size (e.g. 1024)</param>
        /// <returns>Multiplier for Hann window</returns>
        public static double HannWindow(int n, int frameSize)
        {
            return 0.5 * (1 - Math.Cos((2 * Math.PI * n) / (frameSize - 1)));
        }

        /// <summary>
        /// Applies a Blackman-Harris Window
        /// </summary>
        /// <param name="n">Index into frame</param>
        /// <param name="frameSize">Frame size (e.g. 1024)</param>
        /// <returns>Multiplier for Blackmann-Harris window</returns>
        public static double BlackmannHarrisWindow(int n, int frameSize)
        {
            return 0.35875 - (0.48829 * Math.Cos((2 * Math.PI * n) / (frameSize - 1))) + (0.14128 * Math.Cos((4 * Math.PI * n) / (frameSize - 1))) - (0.01168 * Math.Cos((6 * Math.PI * n) / (frameSize - 1)));
        }
    }
}