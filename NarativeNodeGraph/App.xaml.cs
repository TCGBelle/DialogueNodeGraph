using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
namespace NarativeNodeGraph
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // WPF UI thread exceptions
            DispatcherUnhandledException += (s, e) =>
            {
                Debug.WriteLine(e.Exception);
                MessageBox.Show(e.Exception.ToString(), "DispatcherUnhandledException");
                e.Handled = true; // prevents instant shutdown so you can read it
            };

            // Non-UI thread exceptions
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                Debug.WriteLine(ex?.ToString() ?? e.ExceptionObject.ToString());
                MessageBox.Show(ex?.ToString() ?? e.ExceptionObject.ToString(), "UnhandledException");
            };

            // Task exceptions
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Debug.WriteLine(e.Exception);
                MessageBox.Show(e.Exception.ToString(), "UnobservedTaskException");
                e.SetObserved();
            };
        }
    }

}
