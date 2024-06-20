
namespace OpenCvRecognition
{
    public interface IRecognition
    {
        event ImageUpdated? ImageUpdated;
        event ImageUpdated? RecognitionSuccess;

        Task RecognizeImageAsync(CancellationToken? ct = null);
        void SetCapture(Action<VideoCaptureOption> option);
    }
}