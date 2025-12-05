using System;
using System.Reactive;
using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Texiv.Detect.ViewModels;

public partial class MainViewModel : ReactiveObject
{
    [Reactive] public partial string? VideoFilePath { get; set; }
    [Reactive] public partial bool IsPlaying { get; set; }

    public MainViewModel()
    {
    }

    [ReactiveCommand]
    private void OpenVideo()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.webm|All Files|*.*",
            Title = "Select Video File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            VideoFilePath = openFileDialog.FileName;
            IsPlaying = false;
        }
    }

    [ReactiveCommand]
    private void Play()
    {
        if (!string.IsNullOrEmpty(VideoFilePath))
        {
            IsPlaying = true;
        }
    }
}
