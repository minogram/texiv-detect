using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Texiv.Detect.Services;
using static System.Net.Mime.MediaTypeNames;

namespace Texiv.Detect.ViewModels;

public partial class MainViewModel : ReactiveObject, IDisposable
{
    private readonly YoloDetectionService _detectionService;
    private readonly VideoProcessingService _videoProcessingService;

    [Reactive] public partial string? VideoFilePath { get; set; }
    [Reactive] public partial bool IsPlaying { get; set; }
    [Reactive] public partial BitmapSource? CurrentFrame { get; set; }
    [Reactive] public partial string? StatusMessage { get; set; }
    [Reactive] public partial bool IsModelLoaded { get; set; }

    public MainViewModel()
    {
        _detectionService = new YoloDetectionService();
        _videoProcessingService = new VideoProcessingService(_detectionService);

        _videoProcessingService.FrameProcessed += OnFrameProcessed;
        _videoProcessingService.ErrorOccurred += OnErrorOccurred;

        StatusMessage = "Ready. Loading YOLO model...";
        
        // 시작 시 모델 로드 시도
        _ = TryLoadDefaultModelAsync();
    }

    private async Task TryLoadDefaultModelAsync()
    {
        try
        {
            var possiblePaths = new[]
            {
                @"d:\dev\texiv-detect\model\yolo11n.onnx",  // 기본 경로 (최우선)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "yolo11n.onnx"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "yolov8n.onnx"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yolo11n.onnx"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yolov8n.onnx")
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    StatusMessage = $"Loading model: {Path.GetFileName(path)}...";
                    await _detectionService.InitializeAsync(path);
                    IsModelLoaded = true;
                    StatusMessage = $"Model loaded: {Path.GetFileName(path)} from {path}";
                    return;
                }
            }

            StatusMessage = "No model found. Please use 'Load Model' to select a YOLO model file.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Model loading failed: {ex.Message}";
        }
    }

    [ReactiveCommand]
    private async Task LoadModelAsync()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "ONNX Model Files|*.onnx|All Files|*.*",
            Title = "Select YOLO Model File",
            InitialDirectory = @"d:\dev\texiv.detect\model"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                StatusMessage = "Loading model...";
                await _detectionService.InitializeAsync(openFileDialog.FileName);
                IsModelLoaded = true;
                StatusMessage = $"Model loaded successfully: {Path.GetFileName(openFileDialog.FileName)}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load model: {ex.Message}";
                IsModelLoaded = false;
            }
        }
    }

    [ReactiveCommand]
    private async Task OpenVideoAsync()
    {
        if (!IsModelLoaded)
        {
            StatusMessage = "Please load a YOLO model first!";
            return;
        }

        var openFileDialog = new OpenFileDialog
        {
            Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.webm|All Files|*.*",
            Title = "Select Video File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            VideoFilePath = openFileDialog.FileName;
            IsPlaying = false;
            
            StatusMessage = "Opening video...";
            var success = await _videoProcessingService.OpenVideoAsync(VideoFilePath);
            
            if (success)
            {
                StatusMessage = $"Video loaded: {Path.GetFileName(VideoFilePath)}";
            }
        }
    }

    [ReactiveCommand]
    private void Play()
    {
        if (!IsModelLoaded)
        {
            StatusMessage = "Please load a YOLO model first!";
            return;
        }

        if (!string.IsNullOrEmpty(VideoFilePath))
        {
            IsPlaying = true;
            _videoProcessingService.StartProcessing();
            StatusMessage = "Processing video with YOLO detection...";
        }
    }

    [ReactiveCommand]
    private void Pause()
    {
        if (IsPlaying)
        {
            IsPlaying = false;
            _videoProcessingService.Stop();
            StatusMessage = "Paused";
        }
    }

    private void OnFrameProcessed(object? sender, BitmapSource frame)
    {
        CurrentFrame = frame;
    }

    private void OnErrorOccurred(object? sender, string error)
    {
        StatusMessage = $"Error: {error}";
    }

    public void Dispose()
    {
        _videoProcessingService?.Dispose();
        _detectionService?.Dispose();
    }
}
