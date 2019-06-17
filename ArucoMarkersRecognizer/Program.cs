using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using ArucoMarkersRecognizer.Step3;
using ArucoMarkersRecognizer.Step5;
using Emgu.CV;
using System.Windows.Forms;
using ArucoMarkersRecognizer.Step4;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Drawing.Imaging;
using ArucoMarkersRecognizer.Step6;

namespace ArucoMarkersRecognizer
{
    class MyForm : Form
    {
        private VideoCapture videoCapture;
        private Graphics g;
        private int sizer = 2;
        
        private ImageProvider imageProvider = new ImageProvider();
        private Pen pen = new Pen(Color.Yellow, 2);

        private static readonly HttpClient HttpClient = new HttpClient();

        private static Bitmap UndistorImage(Bitmap image)
        {
            MemoryStream ms;
            using (ms = new MemoryStream())
                image.Save(ms, ImageFormat.Jpeg);

            var response = HttpClient.PostAsync(
                    "http://127.0.0.1:8080/undistorte",
                    new ByteArrayContent(ms.ToArray()))
                .Result;

            var responseStream = response.Content.ReadAsStreamAsync().Result;
            return new Bitmap(Image.FromStream(responseStream));
        }

        public MyForm()
        {
            videoCapture = new VideoCapture();
            videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1280);
            videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 720);

            var image = videoCapture.QueryFrame().Bitmap;
            Height = image.Height / sizer;
            Width = image.Width / sizer;

            g = CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            var timer = new Timer();
            timer.Interval = 300;
            timer.Tick += new EventHandler(timerTick);
            timer.Start();
        }

        private void timerTick(object sender, EventArgs e)
        {
            Console.WriteLine("TICK!");
            
            var image = new Bitmap(videoCapture.QueryFrame().Bitmap);
           
            var undistoredImage = UndistorImage(image);
            
            var resultAfterStep1 = (MyImage)undistoredImage;
            resultAfterStep1 = GrayScaleFilter.Convert(resultAfterStep1);
            
            var resultAfterStep2 = BinarizationConverter.Convert(resultAfterStep1);
            
            var resultAfterStep3 = SobelFilter.Convert(resultAfterStep2);
            
            var candidateCorners = CornerDetector.FindCorners(resultAfterStep3);

            var markerCorners = MarkersRecognizer.SelectMarkers(candidateCorners, undistoredImage);
            
            var miniImage = new Bitmap(undistoredImage, new Size(undistoredImage.Width / sizer, undistoredImage.Height / sizer));
            g.DrawImage(miniImage, 0, 0, miniImage.Width, miniImage.Height);

            foreach (var corners in markerCorners)
            {
                g.DrawPolygon(pen, corners.Select(p => new Point(p.X / sizer, p.Y / sizer)).ToArray());
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Application.Run(new MyForm());
        }
	}
}
