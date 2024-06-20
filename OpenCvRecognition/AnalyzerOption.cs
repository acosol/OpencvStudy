using OpenCvSharp;

namespace OpenCvRecognition;

public class AnalyzerOption
{
    internal CascadeClassifier[]? CascadeClassifiers { get; set; }
    public TimeSpan Rate { get; set; } = TimeSpan.FromSeconds(1);
    /// <summary>
    /// 设置级联分类器
    /// </summary>
    /// <param name="cascadeClassifiers"></param>
    public void SetCascadeClassifier(params string[] cascadeClassifiers)
    {
        this.CascadeClassifiers = cascadeClassifiers
                        .Select(c => new CascadeClassifier(c))
                        .ToArray();
    }
    /// <summary>
    /// 设置默认的级联分类器
    /// </summary>
    public void LoadDefaultCascadeClassifier()
    {
        CascadeClassifiers =
            [
                new(@"haarcascades\haarcascade_frontalface_alt2.xml"),
                new(@"haarcascades\haarcascade_eye_tree_eyeglasses.xml")
            ];
    }
    internal Analyzer? GetNew() => CascadeClassifiers != null ? new(this) : null;
}
