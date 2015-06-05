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

            GLabController.Instance.RegisterPlugin(new P2_6());
            GLabController.Instance.RegisterPlugin(new P2_5());
            GLabController.Instance.RegisterPlugin(new P2_3());
            GLabController.Instance.RegisterPlugin(new P2_2());
            GLabController.Instance.RegisterPlugin(new P2_1());

            //GLabController.Instance.RegisterPlugin(new Aufgabe5());
            //GLabController.Instance.RegisterPlugin(new Aufgabe4());
            //GLabController.Instance.RegisterPlugin(new Aufgabe3());
            //GLabController.Instance.RegisterPlugin(new Aufgabe2());
            //GLabController.Instance.RegisterPlugin(new Aufgabe1());
           
            Application.Run(GLabController.Instance.Workspace);
        }
    }
}