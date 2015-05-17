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
using GLab.Chaos.Datastructure;
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
        private Palette _palette;
        
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

            // Reader initialisieren
            _palette = GLabReader.ReadPaletteFromFile("Multcol4.pal");

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
            int iVergleich;
            int iAnzFarben = _palette.Count;

            // Array Zeilenweise durchlaufen
            for (int iY = 0; iY < ImageHeight; iY++)
            {
                // Array ab der zweiten Spalte durchlaufen
                for (int iX = 0; iX < ImageWidth; iX++)
                {

                    int i1, i2;
                    i1 = iY;
                    i2 = iX;
                    i1 = (i1 >> 1) ^ iY;
                    i2 = (i2 >> 1) ^ iX;

                    // Bitweise verknüpfen und zuweisen
                    //iVergleich = (int)(iY & iX);
                    iVergleich = (int)(i1 & i2);
                    
                    //// Wenn der Vergleich 0 ergibt
                    //if (iVergleich == 0)
                    //{
                        //_palette[iVergleich % iAnzFarben];
                        // roten Punkt zeichnen
                        drawMyDot(iX, iY, Color.Red);
                    //} 
                    //else
                    //{
                        //drawMyDot(iX, iY, _palette[iVergleich%iAnzFarben]);
                    //}

                }
            }

            _frame.Repaint();
        }

        private void drawMyDot(int x, int y, Color color)
        {

            //_image[Math.Abs(y - (ImageHeight - 1)), x] = new Rgb(color);
            _painter.PaintPoint(new Vector2(x, y), color);

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