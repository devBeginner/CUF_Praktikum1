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
        private const int MAX_DEPTH = 5;
        
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

            // rekursive Funkiton
            paintRectBlack(new Vector2(0, 0), new Vector2(ImageWidth, ImageHeight), 1);
            _frame.Repaint();
            
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
            float xFactor = e.X;
            float yFactor = e.Y;
            
            _image[ (int)yFactor,(int)xFactor ] = new Rgb(255,0,0);

            _frame.Repaint();

            
        }

        private void  drawDotRED(int x , int y){
            
            _image[y ,x ] = new Rgb(255,0,0);

        }

        private void paintRectBlack (Vector2 vec1, Vector2 vec2, int depth)
        {
            // Wenn die maximale Tiefe erreicht ist, Funktion beenden
            if (depth > MAX_DEPTH) return;

            // Schwarzes Rechteck im oberen Rechten Quadranten zeichnen
            _painter.PaintRectangle(new Vector2((vec1.X + vec2.X)/ 2, (vec1.Y+vec2.Y) / 2), new Vector2(vec2.X, vec2.Y), System.Drawing.Color.Black);
            
            // Rekutsiver Aufruf für die anderen drei Rechtecke
            // links oben
            paintRectBlack(new Vector2(vec1.X,(vec1.Y+vec2.Y) / 2), new Vector2((vec1.X + vec2.X)/ 2,vec2.Y), depth + 1);
            //links unten
            paintRectBlack(new Vector2(vec1.X, vec1.Y), new Vector2((vec1.X + vec2.X)/ 2, (vec1.Y+vec2.Y) / 2), depth + 1);
            // rechts unten
            paintRectBlack(new Vector2((vec1.X + vec2.X) / 2, vec1.Y), new Vector2(vec2.X , (vec1.Y + vec2.Y) / 2), depth + 1 );

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