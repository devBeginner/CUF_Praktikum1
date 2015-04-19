#region
using System;
using System.Windows.Forms;
using GLab.Core;
#endregion

namespace Frame.Chaos
{
    internal static class Program
    {
        /// <summary>
        ///   The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            GLabController.Instance.RegisterPlugin(new Aufgabe3());
            GLabController.Instance.RegisterPlugin(new Aufgabe2());
            GLabController.Instance.RegisterPlugin(new Aufgabe1());
           
            Application.Run(GLabController.Instance.Workspace);
        }
    }
}