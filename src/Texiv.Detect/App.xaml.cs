using System.Configuration;
using System.Data;
using System.Windows;
using Texiv.Detect.ViewModels;
using Texiv.Detect.Views;

namespace Texiv.Detect
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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
