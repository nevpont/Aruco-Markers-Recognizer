using ArucoMarkersRecognizer.Step5;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArucoMarkersRecognizer.Step4
{
    class CornerDetector
    {
        private static readonly int whiteThreshold = 200;

        private static List<Point> SortPoints(List<Point> points)
        {
            int xSum = 0;
            int ySum = 0;
            for (int i = 0; i < points.Count; i++)
            {
                xSum += points[i].X;
                ySum += points[i].Y;
            }

            var center = new Point(xSum / points.Count, ySum / points.Count);

            return points
                .OrderBy(p =>
                {
                    int x = p.X - center.X;
                    int y = p.Y - center.Y;
                    double r = Math.Sqrt(x * x + y * y);
                    double phi = Math.Acos(x / r);
                    if (y < 0)
                        phi *= -1;
                    return phi;
                })
                .ToList();
        }

        private static double CalcArea(Point A, Point B, Point C)
        {
            return 0.5 * Math.Abs((B.X - A.X) * (C.Y - A.Y) - (B.Y - A.Y) * (C.X - A.X));
        }

        public static bool checkPointInPolygon(Point p, List<Point> poly)
        {
            const double eps = 1e-7;

            double s1 = 0, s2 = 0;
            for (int i = 0; i < poly.Count; i++)
            {
                s1 += CalcArea(p, poly[i], poly[(i + 1) % poly.Count]);

                if (i + 2 < poly.Count)
                    s2 += CalcArea(poly[0], poly[i + 1], poly[i + 2]);
            }

            return Math.Abs(s1 - s2) < eps;
        }

        public static List<Point[]> FindCorners(MyImage source)
        {
            var dx = new int[] { 1, -1, 0, 0 };
            var dy = new int[] { 0, 0, 1, -1 };

            var minMarkerPerimeterRate = 0.05;
            var minDimension = Math.Min(source.Height, source.Width);
            var minPerimeter = minDimension * minMarkerPerimeterRate;
            Console.WriteLine("MinPerimeter = " + minPerimeter);

            var candidateCorners = new List<List<Point>>();
            var visited = new bool[source.Height, source.Width];
            for (int x = 1; x < source.Height - 1; x++)
            {
                for (int y = 1; y < source.Width - 1; y++)
                {
                    if (!visited[x, y] && source.red[x, y] >= whiteThreshold)
                    {
                        var points = new List<Point>();

                        var queue = new Queue<Point>();
                        var startPixel = new Point(x, y);

                        queue.Enqueue(startPixel);
                        visited[x, y] = true;

                        while (queue.Count > 0)
                        {
                            var currentPixel = queue.Dequeue();

                            points.Add(currentPixel);

                            for (int direction = 0; direction < dx.Length; direction++)
                            {
                                var nextPoint = new Point(currentPixel.X + dx[direction], currentPixel.Y + dy[direction]);
                                if (!visited[nextPoint.X, nextPoint.Y] && source.red[nextPoint.X, nextPoint.Y] >= whiteThreshold)
                                {
                                    queue.Enqueue(nextPoint);
                                    visited[nextPoint.X, nextPoint.Y] = true;
                                }
                            }
                        }

                        points = SortPoints(points);
                        var corners = new List<Point>();
                        Douglas_Peucker.DouglasPeucker(points, 0, points.Count - 1, ref corners, true);

                        double perimeter = 0;
                        var minSide = double.MaxValue;
                        var maxSide = double.MinValue;
                        for (int i = 0; i < corners.Count; i++)
                        {
                            var side = Math.Sqrt(Math.Pow(corners[i].X - corners[(i + 1) % corners.Count].X, 2) +
                                Math.Pow(corners[i].Y - corners[(i + 1) % corners.Count].Y, 2));

                            perimeter += side;
                            minSide = Math.Min(minSide, side);
                            maxSide = Math.Max(maxSide, side);
                        }
                        
                        if (corners.Count == 4 &&
                            minSide / maxSide > 0.6 &&
                            perimeter >= minPerimeter)
                        {
                            Console.WriteLine("corner 1: " + corners[0]);
                            Console.WriteLine(minSide + " " + maxSide + " " + minSide / maxSide + " " + perimeter);

                            candidateCorners.Add(corners);
                        }
                    };
                }
            };
            
            return candidateCorners.Select(c => c.Select(p => new Point(p.Y, p.X)).ToArray()).ToList();
        }
    }
}
