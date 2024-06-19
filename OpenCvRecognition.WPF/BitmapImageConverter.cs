using System.IO;
using System.Windows.Media.Imaging;

namespace OpenCvRecognition.WPF;

public static class BitmapImageConverter
{
    public static BitmapImage ToBitmapImage(this MemoryStream memory)
    {
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        return bitmapImage;
    }
}
