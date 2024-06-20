using OpenCvSharp;

namespace OpenCvRecognition;

/// <summary>
/// 预加载提高响应速度，享元模式
/// </summary>
/// <param name="_videoCapture"></param>
internal class VideoCapturePreload(Func<VideoCapture> func)
{
    private static readonly TaskFactory tf = new(new(), TaskCreationOptions.AttachedToParent, TaskContinuationOptions.None, new LimitedConcurrencyLevelTaskScheduler(1));
    readonly Task<VideoCapture> tInt = tf.StartNew(func);
    private bool Used { get; set; } = false;
    public async Task<VideoCapture?> TryGetAsync()
    {
        lock (this)
        {
            if (Used)
                return null;
            Used = true;
        }
        return await tInt;
    }
    public VideoCapture? TryGet()
    {
        return TryGetAsync().Result;
    }
}