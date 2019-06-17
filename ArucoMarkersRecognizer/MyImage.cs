using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ArucoMarkersRecognizer
{
    public class MyImage
    {
        public readonly int Height;
        public readonly int Width;
        public readonly double[,] green;
        public readonly double[,] blue;
        public readonly double[,] red;
        public unsafe byte* Pointer;
        public int Stride;

        public MyImage(int height, int width)
        {
            Height = height;
            Width = width;
            green = new double[height, width];
            blue = new double[height, width];
            red = new double[height, width];
        }

        public void SetValue(int x, int y, double value)
        {
            green[x, y] = value;
            blue[x, y] = value;
            red[x, y] = value;
        }

	    public static MyImage FromArray(double[] points, int height, int width, List<int> pointIndexsToKeep)
	    {
			var a = new HashSet<int>(pointIndexsToKeep);
			var res = new MyImage(height, width);
		    var c = 0;
		    for(var i = 0; i < height; i++)
		    {
			    for(int j = 0; j < width; j++)
			    {
					if(a.Contains(c))
						res.blue[i, j] = points[c];
				    c += 1;
			    }
		    }

		    return res;
	    }

	    public double[] ToArray()
	    {
		    var res = new List<double>();
		    for(var i = 0; i < Height; i++)
		    {
			    for(int j = 0; j < Width; j++)
			    {
				    res.Add(blue[i, j]);
			    }
		    }

		    return res.ToArray();
	    }

        public static explicit operator MyImage(Bitmap processedBitmap)
        {
            var height = processedBitmap.Height - processedBitmap.Height % 8;
            var width = processedBitmap.Width - processedBitmap.Width % 8;
            var image = new MyImage(height, width);

            unsafe
            {
                var bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadWrite, processedBitmap.PixelFormat);
                image.Stride = bitmapData.Stride;
                var bytesPerPixel = Image.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                var heightInPixels = bitmapData.Height - processedBitmap.Height % 8;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
                image.Pointer = (byte*)(void*)bitmapData.Scan0.ToPointer();
                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    Parallel.For(0, width, x =>
                    {
                        var xPor3 = x * bytesPerPixel;

                        image.blue[y, x] = currentLine[xPor3++];
                        image.green[y, x] = currentLine[xPor3++];
                        image.red[y, x] = currentLine[xPor3];
                    })
                    ;
                });
                processedBitmap.UnlockBits(bitmapData);
            }

            return image;
        }

        public static explicit operator Bitmap(MyImage image)
        {
            var width = image.Width - image.Width % 8;
            var height = image.Height - image.Height % 8;
            var bmp = new Bitmap(width, height);
            unsafe
            {
                var bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                var bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                var heightInPixels = height;
                byte* ptrFirstPixel = (byte*)bmpdata.Scan0;
                Parallel.For(0, heightInPixels, ly =>
                {
                    var y = ly;
                    byte* currentLine = ptrFirstPixel + (y * bmpdata.Stride);
                    Parallel.For(0, width, _x =>
                    {
                        var x = _x;
                        var xPor3 = x * bytesPerPixel;
                        currentLine[xPor3] = ToByte(image.red[y, x]);
                        currentLine[xPor3 + 1] = ToByte(image.blue[y, x]);
                        currentLine[xPor3 + 2] = ToByte(image.green[y, x]);
                        currentLine[xPor3 + 3] = 255;
                    });
                });
                bmp.UnlockBits(bmpdata);
            }

            return bmp;
        }

        public static byte ToByte(double d)
        {
            var val = (int)d;
            if (val > byte.MaxValue)
                return byte.MaxValue;
            if (val < byte.MinValue)
                return byte.MinValue;
            return (byte)val;
        }
    }
}