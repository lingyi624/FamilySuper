using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace FamilySuper.Infrastructure.Services;

public interface IOcrService
{
    bool IsAvailable { get; }
    Task<OcrResult> RecognizeAsync(byte[] imageData, CancellationToken cancellationToken = default);
    Task<string> RecognizeTextAsync(byte[] imageData, CancellationToken cancellationToken = default);
}

public record OcrResult(string Name, string IdNumber, string Address, string BirthDate, string Gender);

public class OcrService : IOcrService
{
    private readonly ILogger<OcrService> _logger;
    private readonly bool _enabled;
    private readonly string _modelPath;
    private readonly string _detModelFile;
    private readonly string _recModelFile;
    private readonly string _clsModelFile;
    private readonly string _dictFile;

    private InferenceSession? _detSession;
    private InferenceSession? _recSession;
    private List<string>? _recLabels;
    private readonly object _initLock = new();
    private bool _initialized;

    public OcrService(IConfiguration configuration, ILogger<OcrService> logger)
    {
        _logger = logger;
        _enabled = configuration.GetValue("Ocr:Enabled", false);
        _modelPath = configuration["Ocr:ModelPath"] ?? "./models/ocr";
        _detModelFile = Path.Combine(_modelPath, configuration["Ocr:DetModelName"] ?? "ch_PP-OCRv4_det_infer.onnx");
        _recModelFile = Path.Combine(_modelPath, configuration["Ocr:RecModelName"] ?? "ch_PP-OCRv4_rec_infer.onnx");
        _clsModelFile = Path.Combine(_modelPath, configuration["Ocr:ClsModelName"] ?? "ch_ppocr_mobile_v2.0_cls_infer.onnx");
        _dictFile = Path.Combine(_modelPath, "ppocr_keys_v1.txt");
    }

    public bool IsAvailable => _enabled && File.Exists(_recModelFile) && File.Exists(_dictFile);

    private void EnsureInitialized()
    {
        if (_initialized) return;
        lock (_initLock)
        {
            if (_initialized) return;

            if (!IsAvailable)
            {
                _logger.LogWarning("OCR 服务不可用: Enabled={Enabled}, RecModelExists={RecExists}, DictExists={DictExists}",
                    _enabled, File.Exists(_recModelFile), File.Exists(_dictFile));
                _initialized = true;
                return;
            }

            try
            {
                _logger.LogInformation("正在加载 OCR 识别模型: {Model}", _recModelFile);
                var options = new SessionOptions
                {
                    GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
                };
                _recSession = new InferenceSession(_recModelFile, options);

                _recLabels = new List<string> { "blank" };
                _recLabels.AddRange(File.ReadAllLines(_dictFile, Encoding.UTF8).Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)));

                if (File.Exists(_detModelFile))
                {
                    _logger.LogInformation("正在加载 OCR 检测模型: {Model}", _detModelFile);
                    _detSession = new InferenceSession(_detModelFile, options);
                }

                _logger.LogInformation("OCR 模型加载完成,字典 {Count} 条", _recLabels.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCR 模型加载失败");
                _recSession = null;
                _detSession = null;
                _recLabels = null;
            }
            finally
            {
                _initialized = true;
            }
        }
    }

    public async Task<OcrResult> RecognizeAsync(byte[] imageData, CancellationToken cancellationToken = default)
    {
        var fullText = await RecognizeTextAsync(imageData, cancellationToken);
        if (string.IsNullOrEmpty(fullText))
        {
            return new OcrResult(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }
        return ParseIdCard(fullText);
    }

    public async Task<string> RecognizeTextAsync(byte[] imageData, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        if (_recSession is null || _recLabels is null)
        {
            _logger.LogWarning("OCR 引擎未加载,返回空文本");
            return string.Empty;
        }

        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var ms = new MemoryStream(imageData);
                using var bitmap = new Bitmap(ms);

                List<string> textLines;
                if (_detSession is not null)
                {
                    var boxes = DetectTextBoxes(bitmap);
                    textLines = boxes.Select(b => RecognizeRegion(bitmap, b)).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                }
                else
                {
                    textLines = new List<string> { RecognizeRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height)) };
                }

                var fullText = string.Join("\n", textLines.Where(s => !string.IsNullOrWhiteSpace(s)));
                _logger.LogInformation("OCR 识别完成,提取文本 {Length} 字符", fullText.Length);
                return fullText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCR 识别失败");
                return string.Empty;
            }
        }, cancellationToken);
    }

    private List<Rectangle> DetectTextBoxes(Bitmap bitmap)
    {
        try
        {
            var (maxSide, ratio) = ComputeTargetSize(bitmap.Width, bitmap.Height, 960);
            var resized = new Bitmap(bitmap, (int)(bitmap.Width * ratio), (int)(bitmap.Height * ratio));

            var tensor = BitmapToTensor(resized, 0.485, 0.229, true);

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("x", tensor)
            };

            using var results = _detSession!.Run(inputs);
            var output = results.First().AsTensor<float>();

            return ExtractBoxes(output, resized.Width, resized.Height, ratio);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "检测模型推理失败,使用整图识别");
            return new List<Rectangle> { new(0, 0, bitmap.Width, bitmap.Height) };
        }
    }

    private static (int maxSide, float ratio) ComputeTargetSize(int width, int height, int targetSize)
    {
        var maxSide = Math.Max(width, height);
        if (maxSide > targetSize)
        {
            return (targetSize, (float)targetSize / maxSide);
        }
        return (maxSide, 1.0f);
    }

    private static DenseTensor<float> BitmapToTensor(Bitmap bitmap, double mean, double std, bool swapRB)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var tensor = new DenseTensor<float>(new[] { 1, 3, height, width });

        var bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        try
        {
            var bytes = new byte[bmpData.Stride * height];
            Marshal.Copy(bmpData.Scan0, bytes, 0, bytes.Length);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var idx = y * bmpData.Stride + x * 3;
                    float b = bytes[idx] / 255.0f;
                    float g = bytes[idx + 1] / 255.0f;
                    float r = bytes[idx + 2] / 255.0f;

                    if (swapRB)
                    {
                        tensor[0, 0, y, x] = (r - (float)mean) / (float)std;
                        tensor[0, 1, y, x] = (g - (float)mean) / (float)std;
                        tensor[0, 2, y, x] = (b - (float)mean) / (float)std;
                    }
                    else
                    {
                        tensor[0, 0, y, x] = (b - (float)mean) / (float)std;
                        tensor[0, 1, y, x] = (g - (float)mean) / (float)std;
                        tensor[0, 2, y, x] = (r - (float)mean) / (float)std;
                    }
                }
            }
        }
        finally
        {
            bitmap.UnlockBits(bmpData);
        }

        return tensor;
    }

    private static List<Rectangle> ExtractBoxes(Tensor<float> output, int width, int height, float ratio)
    {
        var boxes = new List<Rectangle>();
        var dims = output.Dimensions;
        var outH = dims[2];
        var outW = dims[3];
        var threshold = 0.3f;

        for (var y = 0; y < outH; y += 4)
        {
            int startX = -1;
            for (var x = 0; x < outW; x++)
            {
                var val = output[0, 0, y, x];
                if (val > threshold && startX < 0) startX = x;
                else if (val <= threshold && startX >= 0)
                {
                    AddBox(boxes, startX, y, x - startX, 4, ratio, width, height);
                    startX = -1;
                }
            }
            if (startX >= 0)
            {
                AddBox(boxes, startX, y, outW - startX, 4, ratio, width, height);
            }
        }

        return MergeOverlappingBoxes(boxes);
    }

    private static void AddBox(List<Rectangle> boxes, int x, int y, int w, int h, float ratio, int origW, int origH)
    {
        var rx = (int)(x / ratio);
        var ry = (int)(y / ratio);
        var rw = (int)(w / ratio);
        var rh = (int)(h / ratio);
        rx = Math.Clamp(rx, 0, origW - 1);
        ry = Math.Clamp(ry, 0, origH - 1);
        rw = Math.Clamp(rw, 1, origW - rx);
        rh = Math.Clamp(rh, 1, origH - ry);
        boxes.Add(new Rectangle(rx, ry, rw, rh));
    }

    private static List<Rectangle> MergeOverlappingBoxes(List<Rectangle> boxes)
    {
        if (boxes.Count == 0) return boxes;
        var merged = boxes.OrderBy(b => b.Y).ToList();
        var result = new List<Rectangle> { merged[0] };

        foreach (var box in merged.Skip(1))
        {
            var last = result[^1];
            if (Math.Abs(box.Y - last.Y) <= 8 && box.X <= last.Right + 20)
            {
                var x = Math.Min(last.X, box.X);
                var y = Math.Min(last.Y, box.Y);
                var right = Math.Max(last.Right, box.Right);
                var bottom = Math.Max(last.Bottom, box.Bottom);
                result[^1] = new Rectangle(x, y, right - x, bottom - y);
            }
            else
            {
                result.Add(box);
            }
        }

        return result.Where(b => b.Width > 10 && b.Height > 5).ToList();
    }

    private string RecognizeRegion(Bitmap bitmap, Rectangle region)
    {
        try
        {
            var targetHeight = 48;
            var aspectRatio = (float)region.Width / region.Height;
            var targetWidth = (int)(targetHeight * aspectRatio);
            targetWidth = Math.Max(8, ((targetWidth + 3) / 4) * 4);

            using var cropped = new Bitmap(targetWidth, targetHeight, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(cropped))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, new Rectangle(0, 0, targetWidth, targetHeight), region, GraphicsUnit.Pixel);
            }

            var tensor = new DenseTensor<float>(new[] { 1, 3, targetHeight, targetWidth });
            var bmpData = cropped.LockBits(new Rectangle(0, 0, targetWidth, targetHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                var bytes = new byte[bmpData.Stride * targetHeight];
                Marshal.Copy(bmpData.Scan0, bytes, 0, bytes.Length);

                for (var y = 0; y < targetHeight; y++)
                {
                    for (var x = 0; x < targetWidth; x++)
                    {
                        var idx = y * bmpData.Stride + x * 3;
                        tensor[0, 0, y, x] = (bytes[idx + 2] / 255.0f - 0.5f) / 0.5f;
                        tensor[0, 1, y, x] = (bytes[idx + 1] / 255.0f - 0.5f) / 0.5f;
                        tensor[0, 2, y, x] = (bytes[idx] / 255.0f - 0.5f) / 0.5f;
                    }
                }
            }
            finally
            {
                cropped.UnlockBits(bmpData);
            }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("x", tensor)
            };

            using var results = _recSession!.Run(inputs);
            var output = results.First().AsTensor<float>();
            return CtcDecode(output);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "区域识别失败: {Region}", region);
            return string.Empty;
        }
    }

    private string CtcDecode(Tensor<float> output)
    {
        var dims = output.Dimensions;
        var seqLen = dims[1];
        var numClasses = dims[2];
        var result = new StringBuilder();
        var lastIdx = -1;

        for (var t = 0; t < seqLen; t++)
        {
            var maxIdx = 0;
            var maxVal = float.MinValue;
            for (var c = 0; c < numClasses; c++)
            {
                var val = output[0, t, c];
                if (val > maxVal)
                {
                    maxVal = val;
                    maxIdx = c;
                }
            }

            if (maxIdx == 0) { lastIdx = 0; continue; }
            if (maxIdx == lastIdx) continue;

            if (maxIdx < _recLabels!.Count)
            {
                result.Append(_recLabels[maxIdx]);
            }
            lastIdx = maxIdx;
        }

        return result.ToString();
    }

    private static OcrResult ParseIdCard(string text)
    {
        var idMatch = Regex.Match(text, @"\d{17}[\dXx]");
        var idNumber = idMatch.Success ? idMatch.Value : string.Empty;

        var nameMatch = Regex.Match(text, @"姓\s*名\s*([\u4e00-\u9fa5]{2,4})");
        var name = nameMatch.Success ? nameMatch.Groups[1].Value : string.Empty;

        var genderMatch = Regex.Match(text, @"性\s*别\s*([男女])");
        var gender = genderMatch.Success ? genderMatch.Groups[1].Value : string.Empty;

        var birthMatch = Regex.Match(text, @"出\s*生\s*(\d{4}\d{2}\d{2})");
        var birthDate = birthMatch.Success ? birthMatch.Groups[1].Value : string.Empty;

        var addrMatch = Regex.Match(text, @"住\s*址\s*([\u4e00-\u9fa5]+[\u4e00-\u9fa5\d]+)");
        var address = addrMatch.Success ? addrMatch.Groups[1].Value : string.Empty;

        if (string.IsNullOrEmpty(birthDate) && idNumber.Length == 18)
        {
            birthDate = idNumber.Substring(6, 8);
            if (string.IsNullOrEmpty(gender))
            {
                gender = (int.Parse(idNumber.Substring(16, 1)) % 2 == 1) ? "男" : "女";
            }
        }

        return new OcrResult(name, idNumber, address, birthDate, gender);
    }
}
