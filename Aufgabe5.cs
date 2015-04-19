﻿#region

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
            
            // Punkte benennen
            _initPoints[0].name = "P1";
            _initPoints[0].x = 0;
            _initPoints[0].y = 0;
            _initPoints[1].name = "P2";
            _initPoints[1].x = ImageWidth-1;
            _initPoints[1].y = ImageHeight-1;
            _initPoints[2].name = "Q1";
            _initPoints[3].name = "Q2";
            _initPoints[4].name = "Pneu";
            
            // MyMethod: draw some random dots to get started
            // MyMethod(_image);

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
            const int MAX_RANG = 5;
            int iRandRang;
            int iTeiler;
            int iQx;
            int iQy;
            int dX;
            int dY;
            int iKante;

            for (int iIter = 0; iIter < 10000; iIter++) {

                // Nach 100 Iterationen die Farbe ändern und neu zeichnen
                if (iIter % 100 == 0)
                {
                    //colPoints = GenerateRandomColor();
                    _frame.Repaint();
                }

                // Rang und teiler bestimmen
                iRandRang = _rand.Next(MAX_RANG)+1;
                iTeiler = iRandRang * 3;
                iKante = _initPoints[1].x / iTeiler;

                // Quadrat bestimmen und Offset zum 
                iQx = _rand.Next(iTeiler);
                iQy = _rand.Next(iTeiler);
                dX = iKante * iQx;
                dY = iKante * iQy;

                // Die beiden Punkte zum Zeichnen bestimmen
                _initPoints[3].x = _initPoints[0].x + dX;
                _initPoints[3].y = _initPoints[0].y + dY;

                _initPoints[4].x = _initPoints[3].x + iKante;
                _initPoints[4].y = _initPoints[3].y + iKante;

                // Quadrat zeichnen
                _painter.PaintRectangle(new Vector2(_initPoints[3].x + (1  * iKante/ 3), _initPoints[3].y + (1  * iKante/ 3)), new Vector2(_initPoints[3].x + (2* iKante / 3) , _initPoints[3].y + (2 * iKante / 3)), System.Drawing.Color.Red);

                //// Koordinaten des neuen Punktes berechnen
                //_initPoints[4].x = (_initPoints[3].x + _initPoints[iPoint].x) / 2;
                //_initPoints[4].y = (_initPoints[3].y + _initPoints[iPoint].y) / 2;


                //// Neuen Punkt imm alten Speichern
                //_initPoints[3].x = _initPoints[4].x;
                //_initPoints[3].y = _initPoints[4].y;

                //// Punkt zeichnen
                //_image[_initPoints[4].y,_initPoints[4].x] = new Rgb(colPoints);
                _frame.Repaint();
            }

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