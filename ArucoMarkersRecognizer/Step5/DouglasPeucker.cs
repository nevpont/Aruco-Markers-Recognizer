using System;
using System.Collections.Generic;
using System.Drawing;

namespace ArucoMarkersRecognizer.Step5
{
	public class Douglas_Peucker
	{
        private const double EPS = 5;

        private static double CalcDistFromPointToSegment(Point P, Point A, Point B)
        {
            var AB = new Point(B.X - A.X, B.Y - A.Y);
            var AP = new Point(P.X - A.X, P.Y - A.Y);
            var BP = new Point(P.X - B.X, P.Y - B.Y);

            if (AB.X * AP.X + AB.Y * AP.Y <= 0)
                return Math.Sqrt(AP.X * AP.X + AP.Y * AP.Y);
            if (AB.X * BP.X + AB.Y * BP.Y >= 0)
                return Math.Sqrt(BP.X * BP.X + BP.Y * BP.Y);

            return Math.Abs(AB.X * AP.Y - AB.Y * AP.X) / Math.Sqrt(AB.X * AB.X + AB.Y * AB.Y);
        }

        public static double CalcDistFromPointToPolygon(Point P, Point[] poly)
        {
            var dist = CalcDistFromPointToSegment(P, poly[0], poly[poly.Length - 1]);
            for (int i = 0; i + 1 < poly.Length; i++)
                dist = Math.Min(dist, CalcDistFromPointToSegment(P, poly[i], poly[i + 1]));
            return dist;
        }

        public static void DouglasPeucker(List<Point> points, int firstInd, int lastInd, ref List<Point> result, bool isFirst)
        {
            double maxDist = double.MinValue;
            int bestInd = firstInd;
            for(int i = firstInd; i < lastInd; i++)
            {
                var dist = CalcDistFromPointToSegment(points[i], points[firstInd], points[lastInd]);

                if (dist > maxDist)
                {
                    maxDist = dist;
                    bestInd = i;
                }
            }

            if (maxDist > EPS)
            {
                if (isFirst)
                {
                    result.Add(points[bestInd]);
                    var new_points = new List<Point>();
                    for (int i = bestInd; i < points.Count; i++)
                        new_points.Add(points[i]);
                    for (int i = 0; i < bestInd; i++)
                        new_points.Add(points[i]);
                    DouglasPeucker(new_points, 0, new_points.Count - 1, ref result, false);
                }
                else
                {
                    DouglasPeucker(points, firstInd, bestInd, ref result, false);
                    result.Add(points[bestInd]);
                    DouglasPeucker(points, bestInd, lastInd, ref result, false);
                }
            }
        }
	}
}