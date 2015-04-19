#region

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GLab.Core;
using GLab.Core.Forms;
using GLab.Core.PluginInterfaces;
using GLab.Chaos;
using Microsoft.Xna.Framework;
using Color = System.Drawing.Color;

#endregion

namespace Frame.Chaos
{
    
    /// <summary>
    ///   Your plugin description.
    /// </summary>
    internal class Aufgabe4 : IPluginClient, IGuiExtension
    {
        /// <summary>
        ///   Initial size of the raster image.
        /// </summary>
        private const int ImageWidth = 512, ImageHeight = 512;

        private FrmImage _frame;
        private Image<Rgb, byte> _image;
        private Random _rand;
        private Painter _painter;
        
        public Aufgabe4()
        {
            Name = "Aufgabe 4";
        }

        #region IGuiExtension Members

        public void AddMenuStripItems(MenuStrip menuStrip)
        {
            // Add items to the menu here
        }

        public void AddToolStripItems(ToolStrip toolStrip)
        {
            // Add items to the toolbar here
        }

        public void RemoveMenuStripItems(MenuStrip menuStrip)
        {
            // Remove all items from the menu
        }

        public void RemoveToolStripItems(ToolStrip toolStrip)
        {
            // Remove all items from the toolbar
        }

        #endregion

        public override void Setup()
        {
            // Init. a new random number generator
            _rand = new Random();

            // Create a new raster image for drawing
            _image = new Image<Rgb, byte>(ImageWidth, ImageHeight, new Rgb(Color.Black));
            
            // Painter initialisieren
            _painter = new Painter(ref _image);

            // Create a new frame to display the raster image
            _frame = new FrmImage(Name, _image, DisplayMode.Zoomable)
                {
                    InterpolationMode = InterpolationMode.NearestNeighbor,
                    SmoothingMode = SmoothingMode.None
                };
            _frame.SetImage(_image);

            // Register the click event handler
            _frame.PictureBox.MouseClick += DrawDot;

            //Inform user about possibility to draw dots with the mouse
            Logger.Instance.LogInfo("");
            
            // Register this plugin as a GUI extension
            GLabController.Instance.RegisterExtension(this);

            // Additional setup of your plugin
            Aufgabe();
        }
        
        private void Aufgabe()
        {
            // Zweidimensionales Array erstellen
            int[,] iMatrix = new int[513, 512];
            int p1 = 0;
            int p2 = 0;
            String ausgabe = "";

            // Erste Zeile des Array belegen
            iMatrix[1, 0] = 1;

            // Array Zeilenweise durchlaufen
            for (int iY = 0; iY < ImageHeight; iY++)
            {
                // Array ab der zweiten Spalte durchlaufen
                for (int iX = 1; iX < ImageWidth; iX++)
                {
                    if ((int)(iY & iX) == (int)0)
                    {
                        drawDotRED(iX, iY);
                    }
                }
            }


            //for (int iY = 0; iY < iMatrix.GetLength(1); iY++)
            //{
            //    // Array ab der zweiten Spalte durchlaufen
            //    for (int iX = 1; iX < iMatrix.GetLength(0); iX++)
            //    {   
            //        // wenn die Zahl in der Matrix ungerade ist
            //        if (iMatrix[iX , iY] == 1) _painter.PaintPoint(new Vector2(iX - 1 , Math.Abs(iY-ImageHeight)), Color.Red);
            //    }
            //}

            _frame.Repaint();
        }

        private void drawDotRED(int x, int y)
        {

            _image[Math.Abs(y-(ImageHeight-1)), x] = new Rgb(255, 0, 0);

        }
        
        private void DrawDot(object sender, MouseEventArgs e)
        {
            float xFactor = e.X;
            float yFactor = e.Y;
            
            _image[ (int)yFactor,(int)xFactor ] = new Rgb(255,0,0);

            _frame.Repaint();

        }

        private Color GenerateRandomColor()
        {
            return Color.FromArgb(_rand.Next(256), _rand.Next(256), _rand.Next(256));
        }

        public override void Run()
        {
            // Executed in each iteration of the render loop
        }

        public override void Teardown()
        {
            GLabController.Instance.UnregisterExtension(this);
            _frame.Dispose();
            _image.Dispose();

            // Additional teardown of your plugin
            // ...
        }
    }
}