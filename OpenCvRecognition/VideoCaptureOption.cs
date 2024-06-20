using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace OpenCvRecognition;

/// <summary>
/// VideoCapture设置
/// </summary>
public class VideoCaptureOption
{
    private VideoCapturePreload? videowCaptureCache;
    int _videoCaptureIndex = 0;
    public int VideoCaptureIndex
    {
        get => _videoCaptureIndex;
        set
        {
            _videoCaptureIndex = value;
            SetNewAsysnc();

            Thread thread = new(() => { });
            thread.Interrupt();
        }
    }
    public bool PreloadEnabled { get; set; } = true;
    private void SetNewAsysnc()
    {
        videowCaptureCache = GetNewProviderAsysnc();
    }
    private VideoCapturePreload GetNewProviderAsysnc() => new(() => new(VideoCaptureIndex));

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal VideoCapture GetNew()
    {
        try
        {
            return
                //获取预加载
                videowCaptureCache?.TryGet() ??
                //被用过就新创建一个
                GetNewProviderAsysnc().TryGet()!;
        }
        finally
        {
            SetNewAsysnc();
        }
    }
}
