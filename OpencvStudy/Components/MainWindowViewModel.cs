using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OpenCvRecognition.WPF;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using Prism.Mvvm;

namespace OpencvStudy.Components;

public class MainWindowViewModel : BindableBase
{
    public MainWindowViewModel(Recognition_Wpf recognition)
    {
        RecognitionInit(recognition);

        StartCommand = new DelegateCommand(() =>
        {
            StartButtonStatus = false;
            _ = recognition.RecognizeImageAsync();
            StartButtonStatus = true;
        });
    }

    bool _startButtonStatus = true;
    public bool StartButtonStatus
    {
        get => _startButtonStatus;
        private set => SetProperty(ref _startButtonStatus, value);
    }

    private void RecognitionInit(Recognition_Wpf recognition)
    {
        recognition.SetVideoCapture(0);
        recognition.LoadDefaultCascadeClassifier();
        recognition.ImageUpdated += (mat) =>
        {
            try
            {
                Video = mat.ToWriteableBitmap();
            } catch (Exception ex)
            {
            }
        };
        recognition.RecognitionSuccess += (mat) =>
        {
            return;
            try
            {
                //Image?.WritePixelsFromMat(mat);
            } catch (Exception ex)
            {
            }
        };
    }

    public ICommand? StartCommand { get; }

    private BitmapSource? _video;
    public BitmapSource? Video
    {
        get => _video;
        set
        {
            SetProperty(ref _video, value);
        }
    }
    private BitmapSource? _image;
    public BitmapSource? Image
    {
        get => _image;
        set
        {
            SetProperty(ref _image, value);
        }
    }
}
