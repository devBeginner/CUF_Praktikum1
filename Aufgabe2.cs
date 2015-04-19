#region

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Emgu.CV;
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
    internal class Aufgabe2 : IPluginClient, IGuiExtension
    {
        /// <summary>
        ///   Initial size of the raster image.
        /// </summary>
        private const int ImageWidth = 512, ImageHeight = 512;

        private FrmImage _frame;
        private Image<Rgb, byte> _image;
        private Random _rand;
        private Painter _painter;
        
        public Aufgabe2()
        {
            Name = "Aufgabe 2";
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
            _image = new Image<Rgb, byte>(ImageWidth, ImageHeight, new Rgb(Color.Red));
            
            // Painter initialisieren
            _painter = new Painter(ref _image);

            // Create a new frame to display the raster image
            _frame = new FrmImage(Name, ImageWidth, ImageHeight, DisplayMode.Zoomable)
                {
                    InterpolationMode = InterpolationMode.NearestNeighbor,
                    SmoothingMode = SmoothingMode.None
                };
            _frame.SetImage(_image);

            // Register the click event handler
            _frame.PictureBox.MouseClick += DrawDot;

            //Inform user about possibility to draw dots with the mouse
            Logger.Instance.LogInfo("");

            aufgabe();
            
            // MyMethod: draw some random dots to get started
            // MyMethod(_image);

            // Register this plugin as a GUI extension
            GLabController.Instance.RegisterExtension(this);

            // Additional setup of your plugin
            // ...
        }

        private void MyMethod(Image<Rgb, byte> image)
            // Insert your code here instead of drawing 500 random dots
        {
            for (int k = 0; k < 500; ++k)
                image[_rand.Next(image.Height), _rand.Next(image.Width)] = new Rgb(GenerateRandomColor());

            // Repaint the frame so the changes to the raster image can be seen
            _frame.Repaint();
        }

        private void DrawDot(object sender, MouseEventArgs e)
        {

            _image[e.Y, e.X] = new Rgb(255,0,0);

            _frame.Repaint();

            
        }

        private void  drawDotRED(int x , int y){
            
            _image[y ,x ] = new Rgb(255,0,0);

        }
        // Zeichnet die geforderten Sachen
        // _initPoints[3] ist "Palt"
        // _initPoints[4] ist "Pneu"
        private void aufgabe()
        {
            //// Alles rot färben
            //for (int h = 0; h < ImageHeight; h++)
            //{

            //    for (int w = 0; w < ImageWidth; w++)
            //    {
            //        drawDotRED(w, h);
            //    }

            //}
            //_frame.Repaint();


            // Schwarzes rechteck zeichnen
            Point p1 = new Point();
            Point p2 = new Point();

            p1.x = 0;
            p1.y = 0;
            p2.x = ImageWidth;
            p2.y = ImageHeight;

            paintRectBlack(p1, p2);
               
        }

        private void paintRectBlack (Point p1, Point p2)
        {
            Vector2 vec1;
            Microsoft.Xna.Framework.Vector2 vec2;

            vec1.X = p1.x;
            vec1.Y = p1.y;

            _painter.PaintRectangle(new Vector2((float)0, (float)0),new Vector2((float)10, (float)10), System.Drawing.Color.Black);
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