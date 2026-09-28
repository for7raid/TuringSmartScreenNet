using System.Diagnostics;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            KillOtherInstances();
            base.OnStartup(e);
        }

        private void KillOtherInstances()
        {
            var current = Process.GetCurrentProcess();
            var name = current.ProcessName;

            foreach (var p in Process.GetProcessesByName(name))
            {
                if (p.Id == current.Id) continue;

                try
                {
                    p.Kill();
                    p.WaitForExit(3000); // подождать до 3 сек
                }
                catch (Exception ex)
                {
                    // нет прав / процесс уже завершён
                    Debug.WriteLine($"Не удалось закрыть PID {p.Id}: {ex.Message}");
                }
            }
        }
    }

}
