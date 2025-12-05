using System.Configuration;
using System.Data;
using System.Reactive.Concurrency;
using System.Windows;
using ReactiveUI;
using Splat.ModeDetection;
using Texiv.Detect.ViewModels;
using Texiv.Detect.Views;

namespace Texiv.Detect
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // ReactiveUI 초기화 (v22 필수)
            Splat.ModeDetector.OverrideModeDetector(Mode.Run);
            RxApp.MainThreadScheduler = DispatcherScheduler.Current;
            RxApp.TaskpoolScheduler = TaskPoolScheduler.Default;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var window = new MainWindow();
            var viewModel = new MainViewModel();
            window.DataContext = viewModel;
            window.ViewModel = viewModel;
            window.Show();
        }
    }

}
