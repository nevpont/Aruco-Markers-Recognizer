using ArucoMarkersRecognizer.Step4;
using ArucoMarkersRecognizer.Step5;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArucoMarkersRecognizer.Step6
{
    class MarkersRecognizer
    {
        private const double borderRate = 0.1;
        private const int blackThreshold = 120;

        public static List<Point[]> SelectMarkers(List<Point[]> candidateCorners, Bitmap undistoredImage)
        {
            return candidateCorners.Where(corners =>
            {
                int minX = int.MaxValue;
                int maxX = int.MinValue;
                int minY = int.MaxValue;
                int maxY = int.MinValue;
                double perimeter = 0;
                for (int i = 0; i < corners.Length; i++)
                {
                    perimeter += Math.Sqrt(Math.Pow(corners[i].X - corners[(i + 1) % corners.Length].X, 2) + 
                        Math.Pow(corners[i].Y - corners[(i + 1) % corners.Length].Y, 2));
                    minX = Math.Min(minX, corners[i].X);
                    maxX = Math.Max(maxX, corners[i].X);
                    minY = Math.Min(minY, corners[i].Y);
                    maxY = Math.Max(maxY, corners[i].Y);
                }

                var side = perimeter / 4;

                var total_cnt = 0;
                var border_cnt = 0;
                double border_sum = 0;
                for (int x = minX; x <= maxX; x++)
                    for (int y = minY; y <= maxY; y++)
                    {
                        if (CornerDetector.checkPointInPolygon(new Point(x, y), corners.ToList()))
                        {
                            total_cnt++;
                            var dist = Douglas_Peucker.CalcDistFromPointToPolygon(new Point(x, y), corners);
                            var pixel = undistoredImage.GetPixel(x, y);
                            var res = 0.21 * pixel.R + 0.72 * pixel.G + 0.07 * pixel.B;
                            if (dist < side * borderRate)
                            {
                                border_cnt++;
                                border_sum += res;
                            }
                        }
                    }

                var average = border_sum / border_cnt;

                return average <= blackThreshold;
            }).ToList();
        }
    }
}
