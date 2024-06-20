using OpenCvSharp;
using System.Runtime.CompilerServices;

namespace OpenCvRecognition;

internal class Analyzer(AnalyzerOption option) : IDisposable
{
    [ThreadStatic]
    private static Mat? nextImage;
    private CancellationTokenSource _cts = new();
    private bool used = false;
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Start(Func<Mat> source, Action<Mat> output, CancellationToken? ct = null)
    {
        if (used) return;
        used = true;
        if (ct != null)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct.Value);
        }
        var t = _cts.Token;
        Task.Run(() => HandleImageProcessing(source, output, t), t);
    }
    async void HandleImageProcessing(Func<Mat> source, Action<Mat> output, CancellationToken t)
    {
        try
        {
            while (true)
            {
                t.ThrowIfCancellationRequested();
                if (AnalyzeImage(source(), t) is Mat outputImage)
                {
                    output.Invoke(outputImage);
                };
                await Task.Delay(option.Rate, t);
            }
        } catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// 图片识别
    ///<summary/>
    public Mat? AnalyzeImage(Mat image, CancellationToken? ct = null) =>
        option.CascadeClassifiers is CascadeClassifier[] cascadeClassifier ?
             AnalyzeImage(image, cascadeClassifier, ct) : null;
    /// <summary>
    /// 图片识别
    ///<summary/>
    internal static Mat? AnalyzeImage(Mat frame, CascadeClassifier[] cascadeClassifiers, CancellationToken? ct)
    {
        if (!frame.Empty())
        {
            // 灰度化
            // Mat gray = new Mat();
            // Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
            bool success = false;
            foreach (var cascadeClassifier in cascadeClassifiers)
            {
                ct?.ThrowIfCancellationRequested();
                Rect[] rects = cascadeClassifier.DetectMultiScale(frame, 1.1, 3, HaarDetectionTypes.DoRoughSearch, new Size(30, 30));
                Scalar scalar = Scalar.Blue;
                foreach (var rect in rects)
                {
                    Cv2.Rectangle(frame, rect, Scalar.Red, 2);
                    success = true;
                }
            }
            if (success)
            {
                return frame;
            }
        }
        return null;
    }
    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            disposedValue = true;
            _cts.Cancel();
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}