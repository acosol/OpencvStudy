using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OpenCvRecognition;
using OpenCvRecognition.WPF;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using Prism.Mvvm;

namespace OpencvStudy.Components;

public class MainWindowViewModel : BindableBase
{
    Recognition_Wpf _recognition;
    public MainWindowViewModel(Recognition_Wpf recognition)
    {
        _recognition = recognition;
        RecognitionInit(recognition);
    }

    string _startButtonContent = "Start";
    public string StartButtonContent
    {
        get => _startButtonContent;
        set => SetProperty(ref _startButtonContent, value);
    }
    CancellationTokenSource? _cts;
    bool _startButtonStatus = true;
    public bool StartButtonStatus
    {
        get => _startButtonStatus;
        set
        {
            SetProperty(ref _startButtonStatus, value);
            if (!value)
            {
                _ = _recognition.RecognizeImageAsync((_cts = new()).Token);
                StartButtonContent = "Stop";
            }else
            {
                _cts?.Cancel();
                StartButtonContent = "Start";
            }
            Video = null;
        }
    }

    private void RecognitionInit(Recognition_Wpf recognition)
    {
        recognition.SetCapture(o => o.VideoCaptureIndex = 0);
        recognition.SetAnalyzer(o=>o.LoadDefaultCascadeClassifier());
        recognition.ImageUpdated += (mat) =>
        {
            try
            {
                Video = mat.ToWriteableBitmap();
            } catch (Exception)
            {
            }
        };
        recognition.RecognitionSuccess += (mat) =>
        {
            try
            {
                Image = mat.ToWriteableBitmap();
            } catch (Exception)
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
