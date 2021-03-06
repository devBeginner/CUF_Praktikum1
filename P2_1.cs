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
using GLab.Core.Forms.Input;
using GLab.Chaos.Datastructure;
#endregion

namespace Frame.Chaos
{

    /// <summary>
    ///   Your plugin description.
    /// </summary>
    internal class P2_1 : IPluginClient, IGuiExtension
    {
        /// <summary>
        ///   Initial size of the raster image.
        /// </summary>
        private const int ImageWidth = 512, ImageHeight = 512;

        private FrmImage _frame;
        private Image<Rgb, byte> _image;
        private Random _rand;

        private Point[] _initPoints = new Point[5];
        private Painter _painter;
        private IteratedFunctionSystem _ifs;

        private const int MAX_COORDINATE = 1024;
        private CoordinateSystem _coord;

        private int iKlicks = 0;

        public P2_1()
        {
            Name = "P2 Aufgabe 1";
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
            _coord = new CoordinateSystem(-1, 1, -1, 1);

            // Init. a new random number generator
            _rand = new Random();

            // Create a new raster image for drawing
            _image = new Image<Rgb, byte>(MAX_COORDINATE, MAX_COORDINATE, new Rgb(Color.Black));

            // Painter initialisieren mit neuem Koordinatensystem
            _painter = new Painter(ref _image, _coord);

            // Create a new frame to display the raster image
            _frame = new FrmImage(Name, MAX_COORDINATE, MAX_COORDINATE, DisplayMode.Zoomable)
                {
                    InterpolationMode = InterpolationMode.NearestNeighbor,
                    SmoothingMode = SmoothingMode.None
                };
            _frame.SetImage(_image);

            // Register the click event handler
            _frame.PictureBox.MouseClick += drawAufgabeFromDot;

            ////Inform user about possibility to draw dots with the mouse
            //Logger.Instance.LogInfo("Select 3 Dots by left clicking!");

            // Register this plugin as a GUI extension
            GLabController.Instance.RegisterExtension(this);

            //Aufgabe(new Vector3(0.5f, 0.5f, 1));

        }

        // 
        private void Aufgabe(Vector3 vecAlt)
        {

            // Startpunkt in der Mitte des Koordinatensystem
            Color colorPaint = GenerateRandomColor();
            int i = 0;

            do
            {
                // Punkt zeichnen
                _painter.PaintPoint(vecAlt, colorPaint);

                // neue Zufallsfarbe
                if (i % 100 == 0)
                {
                    colorPaint = GenerateRandomColor();
                    _frame.Repaint();
                }

                // Zufälliges Matrix anwenden
                Vector3 vecNeu = Vector3.Transform(vecAlt, _ifs[_rand.Next(_ifs.Count)]);

                vecAlt = vecNeu;

                i++;

            } while (i < 15000);

            _frame.Refresh();

        }

        private void drawAufgabeFromDot(object sender, MouseEventArgs e)
        {
            float iX, iY;
            string strIFS = "";

            iX = (e.X * (_coord.MaxX - _coord.MinX)) / (float)ImageWidth + _coord.MinX;
            iY = (e.Y * (_coord.MaxY - _coord.MinY)) / (float)ImageHeight + _coord.MinY;

            // IFS laden
            switch (iKlicks)
            {
                case 0:
                    strIFS = "Sierpinski.IFS";
                    break;
                case 1:
                    strIFS = "Drachenflaeche_4.IFS";
                    break;
                case 2:
                    strIFS = "Duerer_5_eck.IFS";
                    break;
                case 3:
                    strIFS = "Farn_1.IFS";
                    break;
                case 4:
                    strIFS = "Kristall_1.IFS";
                    break;
                case 5:
                    strIFS = "Menger_teppich.IFS";
                    break;
                case 6:
                    strIFS = "Sierp_verwandter_1.IFS";
                    break;
                default:
                    strIFS = "Sierpinski.IFS";
                    iKlicks = 0;
                    break;
            }
            iKlicks++;
            _ifs = GLabReader.ReadIfsFromFile("..\\..\\IFS\\" + strIFS);
            Logger.Instance.LogInfo("IFS:\t" + strIFS);
            

            // Create a new raster image for drawing
            _image = new Image<Rgb, byte>(MAX_COORDINATE, MAX_COORDINATE, new Rgb(Color.Black));

            // Painter initialisieren mit neuem Koordinatensystem
            _coord = new CoordinateSystem(_ifs.CoordinateSystem.MinX, _ifs.CoordinateSystem.MaxX, _ifs.CoordinateSystem.MinY, _ifs.CoordinateSystem.MaxY);
            _painter = new Painter(ref _image, _coord);
            _frame.SetImage(_image);
            _painter.PaintCoordinateSystem(16, Color.Red);

            // Punkt zeichnen
            Aufgabe(new Vector3(iX, iY, 1));



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