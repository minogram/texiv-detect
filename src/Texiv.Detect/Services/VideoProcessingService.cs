using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;

namespace Texiv.Detect.Services;

public class VideoProcessingService : IDisposable
{
    private VideoCapture? _capture;
    private Thread? _processingThread;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isProcessing;
    private readonly YoloDetectionService _detectionService;

    public event EventHandler<BitmapSource>? FrameProcessed;
    public event EventHandler<string>? ErrorOccurred;

    public bool IsProcessing => _isProcessing;

    public VideoProcessingService(YoloDetectionService detectionService)
    {
        _detectionService = detectionService;
    }

    public async Task<bool> OpenVideoAsync(string videoPath)
    {
        try
        {
            Stop();

            _capture = new VideoCapture(videoPath);
            if (!_capture.IsOpened())
            {
                ErrorOccurred?.Invoke(this, "Failed to open video file");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error opening video: {ex.Message}");
            return false;
        }
    }

    public void StartProcessing()
    {
        if (_isProcessing || _capture == null || !_capture.IsOpened())
            return;

        _isProcessing = true;
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        _processingThread = new Thread(() => ProcessVideoLoop(token))
        {
            IsBackground = true
        };
        _processingThread.Start();
    }

    private void ProcessVideoLoop(CancellationToken cancellationToken)
    {
        if (_capture == null)
            return;

        var clothingClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "person", "backpack", "umbrella", "handbag", "tie", "suitcase"
        };

        var fps = _capture.Fps;
        var frameDelay = fps > 0 ? (int)(1000 / fps) : 33;

        using var frame = new Mat();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!_capture.Read(frame) || frame.Empty())
                {
                    _capture.Set(VideoCaptureProperties.PosFrames, 0);
                    continue;
                }

                // YOLO 객체 탐지 수행
                var detections = _detectionService.Detect(frame);

                // 탐지 결과를 프레임에 그리기
                using var annotatedFrame = _detectionService.DrawDetections(frame, detections, clothingClasses);

                // Mat을 BitmapSource로 변환
                var bitmapSource = ConvertMatToBitmapSource(annotatedFrame);

                // UI 스레드에서 이벤트 발생
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    FrameProcessed?.Invoke(this, bitmapSource);
                });

                Thread.Sleep(frameDelay);
            }
            catch (Exception ex)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    ErrorOccurred?.Invoke(this, $"Processing error: {ex.Message}");
                });
            }
        }
    }

    private BitmapSource ConvertMatToBitmapSource(Mat mat)
    {
        var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
        
        var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            bitmap.GetHbitmap(),
            IntPtr.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

        bitmapSource.Freeze();
        bitmap.Dispose();

        return bitmapSource;
    }

    public void Stop()
    {
        if (_isProcessing)
        {
            _cancellationTokenSource?.Cancel();
            _processingThread?.Join(1000);
            _isProcessing = false;
        }
    }

    public void Dispose()
    {
        Stop();
        _capture?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
