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
using GLab.Core.Forms.Input;
#endregion

namespace Frame.Chaos
{
    
    /// <summary>
    ///   Your plugin description.
    /// </summary>
    internal class Aufgabe5 : IPluginClient, IGuiExtension
    {
        /// <summary>
        ///   Initial size of the raster image.
        /// </summary>
        private const int ImageWidth = 512, ImageHeight = 512;

        private FrmImage _frame;
        private Image<Rgb, byte> _image;
        private Random _rand;
        private int _countInitDots;
        private Point[] _initPoints = new Point[5];
        private Painter _painter;
        
        public Aufgabe5()
        {
            Name = "Aufgabe 5";
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
            _frame = new FrmImage(Name, ImageWidth, ImageHeight, DisplayMode.Zoomable)
                {
                    InterpolationMode = InterpolationMode.NearestNeighbor,
                    SmoothingMode = SmoothingMode.None
                };
            _frame.SetImage(_image);

            // Register the click event handler
            _frame.PictureBox.MouseClick += DrawDot;

            //Inform user about possibility to draw dots with the mouse
            Logger.Instance.LogInfo("Select 3 Dots by left clicking!");
                        
            // Register this plugin as a GUI extension
            GLabController.Instance.RegisterExtension(this);

            Aufgabe();

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
            // Wenn noch keine 4 Punkte gezeichnet wurden,
            if ( _countInitDots < 3 ) {

                _countInitDots++;
                _image[e.Y, e.X] = new Rgb(255,0,0);

                // Koordinaten in das Punktearray einfügen
                _initPoints[_countInitDots-1].x = e.X;
                _initPoints[_countInitDots-1].y = e.Y;

                _frame.Repaint();
            }

            // wenn der dritte Punkt gesetzt wurde, dann die Aufgabe ausführen
            if (_countInitDots == 3) Aufgabe();

        }

        // Zeichnet die geforderten Sachen
        // _initPoints[3] ist "Palt"
        // _initPoints[4] ist "Pneu"
        private void Aufgabe()
        {

            Vector2[] vecs = new Vector2[10];
            vecs[0].X = 0; vecs[0].Y = 0;
            vecs[1].X = 0; vecs[1].Y = 511;
            vecs[2].X = 511; vecs[2].Y = 511;
            vecs[3].X = 511; vecs[3].Y = 0;

            vecs[4].X = 511 / 3; vecs[4].Y = 511 / 3;
            vecs[5].X = 511 / 3; vecs[5].Y = 511 / 3 * 2;
            vecs[6].X = 511 / 3 * 2; vecs[6].Y = 511 / 3;
            vecs[7].X = 511 / 3 * 2; vecs[7].Y = 511 / 3 * 2;

            vecs[8].X = 0; vecs[8].Y = 0;
            
            int iRand;

            for (int i = 0; i < 10000; i++){

                iRand = _rand.Next() % 8;

                vecs[9].X = (vecs[8].X + vecs[iRand].X)/2 / 3;
                vecs[9].Y = (vecs[8].Y + vecs[iRand].Y)/2 / 3;

                vecs[8].X = vecs[9].X;
                vecs[8].Y = vecs[9].Y;
                
                // Punkt zeichnen
                _image[Convert.ToInt32( vecs[8].Y ), Convert.ToInt32( vecs[8].X )] = new Rgb(255, 0, 0);

            }


            //// Punkt zeichnen
            _frame.Repaint();
            
        }

        private System.Drawing.Color GenerateRandomColor()
        {
            return System.Drawing.Color.FromArgb(_rand.Next(256), _rand.Next(256), _rand.Next(256));
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