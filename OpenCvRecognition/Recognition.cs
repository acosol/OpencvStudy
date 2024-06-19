using OpenCvSharp;
using System.Runtime.CompilerServices;

namespace OpenCvRecognition;
public delegate void ImageUpdated(Mat mat);
public class Recognition(SynchronizationContext dispatcher) : IDisposable
{
    CancellationTokenSource? cts;
    VideoCapture? _capture;
    CascadeClassifier[]? cascadeClassifiers;

    public event ImageUpdated? ImageUpdated;
    public event ImageUpdated? RecognitionSuccess;

    /// <summary>
    /// 设置级联分类器
    /// </summary>
    /// <param name="cascadeClassifiers"></param>
    public void SetCascadeClassifier(params string[] cascadeClassifiers)
    {
        this.cascadeClassifiers = cascadeClassifiers
                        .Select(c => new CascadeClassifier(c))
                        .ToArray();
    }
    /// <summary>
    /// 设置默认的级联分类器
    /// </summary>
    public void LoadDefaultCascadeClassifier()
    {
        cascadeClassifiers =
            [
                new(@"haarcascades\haarcascade_frontalface_alt2.xml"),
                new(@"haarcascades\haarcascade_eye_tree_eyeglasses.xml")
            ];
    }

    /// <summary>
    /// 设置使用的摄像头
    /// </summary>
    /// <param name="capture"></param>
    protected void SetVideoCapture(VideoCapture capture)
    {
        _capture = capture;
    }
    /// <summary>
    /// 设置使用的摄像头
    /// </summary>
    /// <param name="index"></param>
    public void SetVideoCapture(int index)
    {
        SetVideoCapture(new VideoCapture(index));
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
    /// 在新线程中识别图片
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
        } catch (OperationCanceledException)
        {
            // do nothing
        } catch (Exception)
        {
        }
    }

    public void RecognizeImage(CancellationToken? ct = null)
    {
        ThrowIfNotSetCapture();
        int sleepTime = (int)Math.Round(1000 / _capture!.Fps);
        Mat frame = new();

        var h = frame.Channels();

        try
        {
            while (true)
            {
                ct?.ThrowIfCancellationRequested();
                _capture.Read(frame);
                ImageUpdatedInternal(frame);
                if (cascadeClassifiers != null)
                {
                    Task.CompletedTask.ContinueWith(t =>
                    {
                        if (AnalyzeImage(frame.Clone(), cascadeClassifiers, ct) is Mat f)
                            RecognitionSuccessInternal(f);
                    });
                }
                Cv2.WaitKey(sleepTime);
            }
        }
        finally
        {
            Cv2.DestroyAllWindows();
        }

        /// <summary>
        /// 图片识别
        ///<summary/>
        Mat? AnalyzeImage(Mat frame, CascadeClassifier[] cascadeClassifiers, CancellationToken? ct)
        {
            if (!frame.Empty())
            {
                // 灰度化
                // Mat gray = new Mat();
                // Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                bool success = false;
                foreach (var cascadeClassifier in cascadeClassifiers)
                {
                    Rect[] rects = cascadeClassifier.DetectMultiScale(frame, 1.1, 3, HaarDetectionTypes.DoRoughSearch, new Size(30, 30));
                    Scalar scalar = Scalar.Blue;
                    foreach (var rect in rects)
                    {
                        Cv2.Rectangle(frame, rect, Scalar.Red, 2);
                    }
                    success = true;
                }
                if (success)
                {
                    return frame;
                }
            }
            return null;
        }
    }

    protected void ThrowIfNotSetCapture()
    {
        if (_capture is null || _capture.IsDisposed)
        {
            throw new UninitializedCaptureException();
        }
    }

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                cts?.Dispose();
                _capture?.Dispose();
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

public class UninitializedCaptureException : Exception { }