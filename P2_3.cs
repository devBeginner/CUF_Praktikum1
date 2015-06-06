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
using GLab.Chaos.Datastructure;
#endregion

namespace Frame.Chaos
{

    /// <summary>
    ///   Your plugin description.
    /// </summary>
    internal class P2_3 : IPluginClient, IGuiExtension
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

        private double[] arrProbs = new double[10];

        private int iKlicks = 0;

        public P2_3()
        {
            Name = "P2 Aufgabe 3";
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

            // Register this plugin as a GUI extension
            GLabController.Instance.RegisterExtension(this);

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
                if (i % 500 == 0)
                {
                    colorPaint = GenerateRandomColor();
                    //colorPaint = Color.Green;
                    _frame.Repaint();
                }

                // Zufälliges Matrix anwenden
                Vector3 vecNeu = Vector3.Transform(vecAlt, getRandTransfMatrix());

                vecAlt = vecNeu;

                i++;

            } while (i < 100000);

            _frame.Refresh();

        }

        private Microsoft.Xna.Framework.Matrix getRandTransfMatrix()
        {
            double fProb = _rand.NextDouble();
            double dSum = arrProbs[1];
            int iIndex = 0;

            // Index mithilfe der Wahrscheinlichkeiten ermitteln

            while (fProb > dSum)
            {
                iIndex++;
                dSum += arrProbs[iIndex + 1];
            }

            return _ifs[iIndex];
        }

        private void readIfsProbabilities()
        {
            double fDeter = 0;

            // Array zurücksetzen
            for (int i = 0; i < arrProbs.Length; i++)
            {
                arrProbs[i] = 0;
            }

            // Alle Determinanten ermitteln und summe berechnen
            for (int i = 0; i < _ifs.Count; i++)
            {
                fDeter = calcDeterminant(_ifs[i]);
                arrProbs[0] += fDeter;
                arrProbs[i + 1] = fDeter;
            }

            // Prozentwerte berechnen
            for (int i = 1; i < arrProbs.Length; i++)
            {
                // Determinante geteilt durch die Summe der Determinanten
                arrProbs[i] /= arrProbs[0];
            }

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
                    strIFS = "Farn_1.IFS";
                    break;
                case 1:
                    strIFS = "Bigbang.IFS";
                    break;
                case 2:
                    strIFS = "Filmstreifen.IFS";
                    break;
                case 3:
                    strIFS = "Strauch.IFS";
                    break;
                case 4:
                    strIFS = "Swirl.IFS";
                    break;
                case 5:
                    strIFS = "Wirbel_blatt.IFS";
                    break;
                case 6:
                    Logger.Instance.LogInfo("\n" + "!!! Jetzt kommt Aufgabe 4 !!!");
                    strIFS = "IFS_TEST.IFS";
                    break;
                default:
                    strIFS = "Farn_1.IFS";
                    iKlicks = 0;
                    break;
            }
            iKlicks++;
            _ifs = GLabReader.ReadIfsFromFile("..\\..\\IFS2\\" + strIFS);
            Logger.Instance.LogInfo("IFS:\t" + strIFS);
            readIfsProbabilities();

            // Create a new raster image for drawing
            _image = new Image<Rgb, byte>(MAX_COORDINATE, MAX_COORDINATE, new Rgb(Color.Black));

            // Painter initialisieren mit neuem Koordinatensystem
            _coord = new CoordinateSystem(_ifs.CoordinateSystem.MinX, _ifs.CoordinateSystem.MaxX, _ifs.CoordinateSystem.MinY, _ifs.CoordinateSystem.MaxY);
            _painter = new Painter(ref _image, _coord);
            _frame.SetImage(_image);
            _painter.PaintCoordinateSystem(16, Color.Red);

            // Punkt zeichnen
            Aufgabe(getFixPoint());

        }

        private Vector3 getFixPoint()
        {

            float dX = 0;
            float dY = 0;
            float a = _ifs[0].M11;
            float b = _ifs[0].M21;
            float c = _ifs[0].M12;
            float d = _ifs[0].M22;
            float e = _ifs[0].M31;
            float f = _ifs[0].M32;

            dX = (-e * (d - 1) + b * f) / ((a - 1) * (d - 1) - b * c);
            dY = (-f * (a - 1) + c * e) / ((a - 1) * (d - 1) - b * c);

            return new Vector3(dX, dY, 1);

        }

        private double calcDeterminant(Microsoft.Xna.Framework.Matrix ifs_transf)
        {
            double dPerc = ifs_transf.M11 * ifs_transf.M22 - ifs_transf.M21 * ifs_transf.M12;

            if (dPerc == 0)
            {
                dPerc = 0.01d;
            }

            return Math.Abs(dPerc);
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