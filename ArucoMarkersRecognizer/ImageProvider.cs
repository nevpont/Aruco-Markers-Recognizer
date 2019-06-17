using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ArucoMarkersRecognizer
{
    public class ImageProvider
    {
        public Bitmap GetImage(string filename)
        {
            return new Bitmap(filename);
        }

        public void SaveImage(Bitmap bmp, string filename)
        {
            bmp.Save(filename, ImageFormat.Jpeg);
        }
    }
}