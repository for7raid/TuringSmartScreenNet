using System.Windows;

namespace TuringSmartScreenNet
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), App.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
