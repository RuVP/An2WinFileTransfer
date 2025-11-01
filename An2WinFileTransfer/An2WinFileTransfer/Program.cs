using System;
using System.Windows.Forms;
using An2WinFileTransfer.UI.Forms;
using Serilog;

namespace An2WinFileTransfer
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Initialize Serilog from App.config
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .CreateLogger();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Log.Information("Application started.");
                Application.Run(new FormMain());
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
