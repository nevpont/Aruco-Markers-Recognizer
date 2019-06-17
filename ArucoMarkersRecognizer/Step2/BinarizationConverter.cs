using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ArucoMarkersRecognizer
{
    public class BinarizationConverter
    {
        public static MyImage Convert(MyImage source)
        {
            var th = GetOtsuThreshold(source);
	        var width = source.Width;
	        var height = source.Height;
			Parallel.For(0, source.Height, i =>
			{
			    Parallel.For(0, source.Width, j =>
			    {
			        var value = source.red[i, j] > th ? 255 : 0;
                    source.SetValue(i, j, value);
			    });
			});
			return source;
        }


        public static int GetOtsuThreshold(MyImage img)
        {
            float[] d = new float[256];
            int[] hist = new int[256];
            d.Initialize();
            
            int threshold;
            long c1, c2, c12, m1, m2, diff;
            
            GetHistogram(img, hist);

            for (threshold = 1; threshold != 255; threshold++)
            {
                c1 = CalcCnt(1, threshold, hist);
                c2 = CalcCnt(threshold + 1, 255, hist);
                c12 = c1 * c2;
                if (c12 == 0)
                    continue;
                m1 = CalcMean(1, threshold, hist);
                m2 = CalcMean(threshold + 1, 255, hist);
                diff = m1 * c2 - m2 * c1;
                d[threshold] = (float)diff * diff / c12;
            }

            threshold = FindMax(d);

            return threshold;
        }

        private static int CalcCnt(int l, int r, int[] hist)
        {
            int sum = 0;
            for (int i = l; i <= r; i++)
                sum += hist[i];

            return sum;
        }

        private static int CalcMean(int l, int r, int[] hist)
        {
            int sum = 0;
            for (int i = l; i <= r; i++)
                sum += i * hist[i];

            return sum;
        }

        private static int FindMax(float[] a)
        {
            float maxVal = a[0];
            int index = 0;

            for (int i = 1; i < a.Length; i++)
            {
                if (a[i] > maxVal)
                {
                    maxVal = a[i];
                    index = i;
                }
            }

            return index;
        }

        private static void GetHistogram(MyImage image, int[] hist)
        {
            hist.Initialize();
            Parallel.For(0, image.Height, i =>
            {
                Parallel.For(0, image.Width, j =>
                {
                    hist[(int)image.red[i, j]]++;
                });
            });
        }
    }
}