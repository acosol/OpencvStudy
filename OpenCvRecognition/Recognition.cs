using OpenCvSharp;

namespace OpenCvRecognition;
public delegate void ImageUpdated(Mat mat);
/// <summary>
/// 观察者模式
/// </summary>
/// <param name="dispatcher"></param>
public class Recognition(SynchronizationContext dispatcher) : IRecognition, IDisposable
{
    CancellationTokenSource? totalCts;
    VideoCaptureOption _captureOption { get; set; } = new();
    AnalyzerOption _analyzerOption { get; set; } = new();

    public event ImageUpdated? ImageUpdated;
    public event ImageUpdated? RecognitionSuccess;

    /// <summary>
    /// 设置使用的摄像头
    /// </summary>
    /// <param name="capture"></param>
    public void SetCapture(Action<VideoCaptureOption> option)
    {
        option.Invoke(_captureOption);
    }
    /// <summary>
    /// 设置分析器
    /// </summary>
    /// <param name="option"></param>
    public void SetAnalyzer(Action<AnalyzerOption> option)
    {
        option.Invoke(_analyzerOption);
    }

    protected virtual void ImageUpdatedInternal(Mat mat)
    {
        if (ImageUpdated is not null)
            dispatcher.Post(o => ImageUpdated?.Invoke(mat), null);
        //ImageUpdated?.Invoke(mat);
    }
    protected virtual void RecognitionSuccessInternal(Mat mat)
    {
        if (RecognitionSuccess is not null)
            dispatcher.Post(o => RecognitionSuccess?.Invoke(mat), null);
        //RecognitionSuccess?.Invoke(mat);
    }

    /// <summary>
    /// 开启新线程
    /// </summary>
    /// <param name="ct"></param>
    public async Task RecognizeImageAsync(CancellationToken? ct = null)
    {
        var t = new Task(() => RecognizeImage(ct),
            TaskCreationOptions.AttachedToParent |
            TaskCreationOptions.LongRunning);

        try
        {
            t.Start();
            await t;
        } catch (Exception)
        {
        }
    }

    private void RecognizeImage(CancellationToken? ct = null)
    {
        using VideoCapture? _capture = _captureOption.GetNew();
        using Analyzer? analyzer = _analyzerOption.GetNew();

        Mat frame = new();
        Task analyzerTask = Task.CompletedTask;
        int sleepTime = (int)Math.Round(1000 / _capture!.Fps);
        try
        {
            while (true)
            {
                ct?.ThrowIfCancellationRequested();
                _capture.Read(frame);
                //镜像视图
                Cv2.Flip(frame, frame, FlipMode.Y);
                ImageUpdatedInternal(frame); //输出实时画面
                analyzer?.Start(frame.Clone,RecognitionSuccessInternal,ct);
                Cv2.WaitKey(sleepTime);
            }
        } catch (OperationCanceledException) { }
        finally
        {
            Cv2.DestroyAllWindows();
        }
    }

    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                totalCts?.Dispose();
            }

            disposedValue = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
