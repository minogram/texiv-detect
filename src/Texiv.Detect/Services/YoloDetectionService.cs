using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Texiv.Detect.Services;

public class YoloDetectionService : IDisposable
{
    private InferenceSession? _session;
    private bool _isInitialized;
    private string[]? _labels;

    public async Task InitializeAsync(string modelPath)
    {
        if (_isInitialized)
            return;

        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"YOLO model not found at: {modelPath}");
        }

        await Task.Run(() =>
        {
            var options = new SessionOptions();
            options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
            
            _session = new InferenceSession(modelPath, options);
            _labels = GetCocoLabels();
        });

        _isInitialized = true;
    }

    public List<Detection> Detect(Mat frame, float confidenceThreshold = 0.25f, float iouThreshold = 0.45f)
    {
        if (!_isInitialized || _session == null)
        {
            throw new InvalidOperationException("YOLO detector is not initialized");
        }

        // 입력 크기 (YOLO 기본값)
        const int inputWidth = 640;
        const int inputHeight = 640;

        // 프레임 전처리
        var inputTensor = PreprocessImage(frame, inputWidth, inputHeight);

        // 추론 실행
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_session.InputNames[0], inputTensor)
        };

        using var results = _session.Run(inputs);
        var output = results.First().AsTensor<float>();

        // 후처리
        var detections = PostprocessOutput(output, frame.Width, frame.Height, confidenceThreshold, iouThreshold);

        return detections;
    }

    private DenseTensor<float> PreprocessImage(Mat frame, int inputWidth, int inputHeight)
    {
        // 이미지 리사이즈
        using var resized = new Mat();
        Cv2.Resize(frame, resized, new Size(inputWidth, inputHeight));

        // BGR to RGB 변환
        using var rgb = new Mat();
        Cv2.CvtColor(resized, rgb, ColorConversionCodes.BGR2RGB);

        // 정규화 및 텐서 생성 [1, 3, 640, 640]
        var tensor = new DenseTensor<float>(new[] { 1, 3, inputHeight, inputWidth });

        for (int y = 0; y < inputHeight; y++)
        {
            for (int x = 0; x < inputWidth; x++)
            {
                var pixel = rgb.At<Vec3b>(y, x);
                tensor[0, 0, y, x] = pixel[0] / 255f; // R
                tensor[0, 1, y, x] = pixel[1] / 255f; // G
                tensor[0, 2, y, x] = pixel[2] / 255f; // B
            }
        }

        return tensor;
    }

    private List<Detection> PostprocessOutput(Tensor<float> output, int imageWidth, int imageHeight, 
        float confidenceThreshold, float iouThreshold)
    {
        var detections = new List<Detection>();
        
        // YOLO v8/v11 출력 형식: [1, 84, 8400] 또는 [1, 4+classes, anchors]
        var dimensions = output.Dimensions.ToArray();
        int numDetections = dimensions[2]; // 8400
        int numClasses = dimensions[1] - 4; // 80 (COCO)

        for (int i = 0; i < numDetections; i++)
        {
            // 클래스별 최대 신뢰도 찾기
            float maxConfidence = 0;
            int maxClassId = 0;

            for (int j = 0; j < numClasses; j++)
            {
                float confidence = output[0, 4 + j, i];
                if (confidence > maxConfidence)
                {
                    maxConfidence = confidence;
                    maxClassId = j;
                }
            }

            if (maxConfidence < confidenceThreshold)
                continue;

            // 바운딩 박스 좌표 (중심 x, 중심 y, 너비, 높이)
            float cx = output[0, 0, i];
            float cy = output[0, 1, i];
            float w = output[0, 2, i];
            float h = output[0, 3, i];

            // 원본 이미지 크기로 스케일링
            float x = (cx - w / 2) * imageWidth / 640f;
            float y = (cy - h / 2) * imageHeight / 640f;
            float width = w * imageWidth / 640f;
            float height = h * imageHeight / 640f;

            detections.Add(new Detection
            {
                ClassId = maxClassId,
                ClassName = _labels?[maxClassId] ?? $"Class {maxClassId}",
                Confidence = maxConfidence,
                X = (int)Math.Max(0, x),
                Y = (int)Math.Max(0, y),
                Width = (int)Math.Min(width, imageWidth - x),
                Height = (int)Math.Min(height, imageHeight - y)
            });
        }

        // NMS (Non-Maximum Suppression) 적용
        return ApplyNMS(detections, iouThreshold);
    }

    private List<Detection> ApplyNMS(List<Detection> detections, float iouThreshold)
    {
        var result = new List<Detection>();
        var sorted = detections.OrderByDescending(d => d.Confidence).ToList();

        while (sorted.Any())
        {
            var best = sorted.First();
            result.Add(best);
            sorted.RemoveAt(0);

            sorted.RemoveAll(d => CalculateIoU(best, d) > iouThreshold && best.ClassId == d.ClassId);
        }

        return result;
    }

    private float CalculateIoU(Detection a, Detection b)
    {
        var x1 = Math.Max(a.X, b.X);
        var y1 = Math.Max(a.Y, b.Y);
        var x2 = Math.Min(a.X + a.Width, b.X + b.Width);
        var y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

        var intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
        var areaA = a.Width * a.Height;
        var areaB = b.Width * b.Height;
        var union = areaA + areaB - intersection;

        return union > 0 ? (float)intersection / union : 0;
    }

    public Mat DrawDetections(Mat frame, List<Detection> detections, HashSet<string> clothingClasses)
    {
        var output = frame.Clone();

        foreach (var detection in detections)
        {
            // 의류 관련 클래스만 표시
            if (!clothingClasses.Contains(detection.ClassName.ToLower()))
                continue;

            var rect = new Rect(detection.X, detection.Y, detection.Width, detection.Height);
            
            // 바운딩 박스 그리기
            Cv2.Rectangle(output, rect, GetColorForClass(detection.ClassName), 2);
            
            // 라벨 텍스트
            var label = $"{detection.ClassName}: {detection.Confidence:P0}";
            var textSize = Cv2.GetTextSize(label, HersheyFonts.HersheySimplex, 0.6, 1, out var baseline);
            
            // 라벨 배경
            var labelRect = new Rect(rect.X, rect.Y - textSize.Height - 10, 
                textSize.Width + 10, textSize.Height + 10);
            Cv2.Rectangle(output, labelRect, GetColorForClass(detection.ClassName), -1);
            
            // 라벨 텍스트
            Cv2.PutText(output, label, new Point(rect.X + 5, rect.Y - 5),
                HersheyFonts.HersheySimplex, 0.6, Scalar.White, 1);
        }

        return output;
    }

    private Scalar GetColorForClass(string className)
    {
        return className.ToLower() switch
        {
            "person" => new Scalar(255, 128, 0),                 // 주황색
            "shirt" or "t-shirt" => new Scalar(255, 0, 0),      // 파란색
            "pants" or "jeans" => new Scalar(0, 255, 0),         // 초록색
            "dress" => new Scalar(255, 0, 255),                  // 마젠타
            "jacket" or "coat" => new Scalar(0, 255, 255),       // 노란색
            "shoes" => new Scalar(255, 255, 0),                  // 청록색
            "bag" or "handbag" or "backpack" => new Scalar(128, 0, 128), // 보라색
            _ => new Scalar(0, 0, 255)                           // 빨간색
        };
    }

    private string[] GetCocoLabels()
    {
        return new[]
        {
            "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat",
            "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat",
            "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack",
            "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball",
            "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket",
            "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple",
            "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair",
            "couch", "potted plant", "bed", "dining table", "toilet", "tv", "laptop", "mouse",
            "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator",
            "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush"
        };
    }

    public void Dispose()
    {
        _session?.Dispose();
        _isInitialized = false;
    }
}

public class Detection
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
