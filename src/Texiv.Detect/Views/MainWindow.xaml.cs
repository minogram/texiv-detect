using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Texiv.Detect.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Disposables.Fluent;
using System;

namespace Texiv.Detect.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IViewFor<MainViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        
        ViewModel = new MainViewModel();
        DataContext = ViewModel;
    }

    protected override void OnClosed(EventArgs e)
    {
        ViewModel?.Dispose();
        base.OnClosed(e);
    }

    public MainViewModel? ViewModel
    {
        get => (MainViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(null));

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = value as MainViewModel;
    }
}