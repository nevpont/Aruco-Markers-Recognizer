using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Linq;

namespace ArucoMarkersRecognizer.Step3
{
	public class SobelFilter
	{
		public static MyImage Convert(MyImage source)
		{
			var result = new MyImage(source.Height, source.Width);
			Parallel.For(1, source.Height-1, x =>
			{
				Parallel.For(1, source.Width-1, y =>
				{
					var g = source.red;

                    //var gx = -g[x - 1, y - 1] - 2 * g[x, y - 1] - g[x + 1, y - 1] + g[x - 1, y + 1] + 2 * g[x, y + 1] + g[x + 1, y + 1];
                    var gx = g[x, y] - g[x - 1, y - 1];

                    //var gy = -g[x - 1, y - 1] - 2 * g[x - 1, y] - g[x - 1, y + 1] + g[x + 1, y - 1] + 2 * g[x + 1, y] + g[x + 1, y + 1];
                    var gy = g[x - 1, y] - g[x, y - 1];

					var val = Math.Sqrt(gx * gx + gy * gy);
                    result.SetValue(x, y, val);
				});
			});

            return result;
		}

		public static MyImage Convert2(MyImage source)
		{
			var result = new MyImage(source.Height, source.Width);
			Parallel.For(2, source.Height - 2, x =>
			{
				Parallel.For(2, source.Width - 2, y =>
				{
					var g = source.red;
					var gx = g[x - 2, y - 2] + 2 * g[x - 1, y - 2] - 2 * g[x + 1, y - 2] - g[x + 2, y - 2] + 4 * g[x - 2, y - 1] + 8 * g[x - 1, y - 1] - 8 * g[x + 1, y - 1]
					                                                                                                                                   - 4 * g[x + 2, y - 1] + 6 * g[x - 2, y] + 12 * g[x - 1, y] - 12 * g[x + 1, y] - 6 * g[x + 2, y] + 4 * g[x - 2, y + 1] + 8 * g[x - 1, y + 1]
					         - 8 * g[x + 1, y + 1] - 4 * g[x + 2, y + 1] + g[x - 2, y + 2] + 2 * g[x - 1, y + 2] - 2 * g[x + 1, y + 2] - g[x + 2, y + 2];
					var gy = -g[x - 2, y - 2] - 4 * g[x - 1, y - 2] - 6 * g[x, y - 2] - 4 * g[x + 1, y - 2] - g[x + 2, y - 2] - 2 * g[x - 2, y - 1] - 8 * g[x - 1, y - 1]
					         - 12 * g[x, y - 1] - 8 * g[x + 1, y - 1] - 2 * g[x + 2, y - 1] + 2 * g[x - 2, y + 1] + 8 * g[x - 1, y + 1] + 12 * g[x, y + 1] + 8 * g[x + 1, y + 1]
					         + 2 * g[x + 2, y + 1] + g[x - 2, y + 2] + 4 * g[x - 1, y + 2] + 6 * g[x, y + 2] + 4 * g[x + 1, y + 2] + g[x + 2, y + 2];

					var ret = Math.Sqrt(gx * gx + gy * gy);
                    result.SetValue(x, y, ret);
				});
			});
			return result;
		}
	}
}