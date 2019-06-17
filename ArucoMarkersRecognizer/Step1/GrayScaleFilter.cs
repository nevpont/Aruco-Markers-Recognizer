using System.Threading.Tasks;

namespace ArucoMarkersRecognizer
{
    internal class GrayScaleFilter
    {
        public static MyImage Convert(MyImage source)
        {
            Parallel.For(0, source.Height, i =>
            {
                Parallel.For(0, source.Width, j =>
                {
                    var res = 0.21 * source.red[i, j] + 0.72 * source.green[i, j] + 0.07 * source.blue[i, j];
                    source.SetValue(i, j, res);
                });
            });
            return source;
        }
    }
}