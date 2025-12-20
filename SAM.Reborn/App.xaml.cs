using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
namespace SAM.Picker.Modern;
public partial class App : Application {
  protected override void OnStartup(StartupEventArgs e) {
    base.OnStartup(e);
    DispatcherUnhandledException += App_DispatcherUnhandledException;
    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
  }
  private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
    File.WriteAllText("modern_picker_crash.txt", e.Exception.ToString());
    MessageBox.Show("Crash: " + e.Exception.Message);
    e.Handled = true;
  }
  private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
    File.WriteAllText("modern_picker_domain_crash.txt", e.ExceptionObject.ToString());
    MessageBox.Show("Domain Crash: " + e.ExceptionObject);
  }
}
