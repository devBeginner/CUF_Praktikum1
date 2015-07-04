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
using GLab.Core.Forms.Control;
#endregion

namespace Frame.Chaos
{


    /// <summary>
    ///   Your plugin description.
    /// </summary>
    internal class P2_6 : IPluginClient, IGuiExtension
    {
        private struct Ant
        {
            public int iZustand;
            public int iCurrX;
            public int iCurrY;
            public int iDirec;

        }
        /// <summary>
        ///   Initial size of the raster image.
        /// </summary>
        private const int ImageWidth = 501, ImageHeight = 501;

        private FrmImage _frame;
        private Image<Rgb, byte> _image;
        private Random _rand;

        private Point[] _initPoints = new Point[5];
        private Painter _painter;

        private const int MAX_COORDINATE = 101;
        private CoordinateSystem _coord;

        private double[] arrProbs = new double[10];
        
        FrmFlowControl flow = new FrmFlowControl();

        private String strAnt;
        private Palette palAnt;

        private Vector2[] vecDirec = { new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0) };
        private int[] arrRot = { -1, 1 };
        private int[,] arrZustand = new int[ImageWidth, ImageHeight];
        private int AnzahlZustaende;

        Ant ant1 = new Ant();
        Ant ant2 = new Ant();

        private string[] arrAnts = {
            "..\\..\\Ants\\Ant_0.ant",
            "..\\..\\Ants\\Ant_1.ant",
            "..\\..\\Ants\\Ant_2.ant",
            "..\\..\\Ants\\Ant_3.ant",
            "..\\..\\Ants\\Ant_4.ant",
            "..\\..\\Ants\\Ant_5.ant",
            "..\\..\\Ants\\Ant_6.ant",
            "..\\..\\Ants\\Ant_7.ant",
            "..\\..\\Ants\\Ant_8.ant",
            "..\\..\\Ants\\Ant_9.ant",
            "..\\..\\Ants\\Ant_10.ant",
            "..\\..\\Ants\\Ant_11.ant",
            "..\\..\\Ants\\Ant_12.ant",
            "..\\..\\Ants\\Ant_13.ant",
            "..\\..\\Ants\\Ant_14.ant"};

        public P2_6()
        {
            Name = "P2 Aufgabe 6";
            ant1.iZustand = -1;
            ant2.iZustand = -1;
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
            _coord = new CoordinateSystem(-0, ImageWidth, 0, ImageHeight);

            // Init. a new random number generator
            _rand = new Random();

            // Create a new raster image for drawing
            _image = new Image<Rgb, byte>(ImageWidth, ImageHeight, new Rgb(Color.White));

            // Painter initialisieren mit neuem Koordinatensystem
            _painter = new Painter(ref _image);

            // Create a new frame to display the raster image
            _frame = new FrmImage(Name, ImageWidth + 1, ImageHeight + 1, DisplayMode.Zoomable)
                {
                    InterpolationMode = InterpolationMode.NearestNeighbor,
                    SmoothingMode = SmoothingMode.None
                };

            _frame.SetImage(_image);

            // Register the click event handler
            _frame.PictureBox.MouseClick += addAnt2;

            // Register this plugin as a GUI extension
            GLabController.Instance.RegisterExtension(this);

            palAnt = GLabReader.ReadPaletteFromFile("..\\..\\Ants\\Chaos_ant.pal");

            Aufgabe();

        }

        private void addAnt2(object sender, MouseEventArgs e)
        {
            if (ant2.iZustand < 0)
            {
                ant2.iZustand = 0;
                ant2.iCurrX = e.X;
                ant2.iCurrY = e.Y;
            }
        }
        
        private Ant doStep(Ant ant)
        {
            if (ant.iZustand < 0) return ant;

            // Alten und neuen Zustand an der aktuellen stelle ermitteln
            int zustAlt = arrZustand[ant.iCurrX, ant.iCurrY];
            int zustNeu = (zustAlt + 1) % AnzahlZustaende;
            Color colNew = palAnt[zustNeu];

            // neue Farbe an der stelle setzen und Zustand anpassen
            _image[ant.iCurrY, ant.iCurrX] = new Rgb(colNew.R, colNew.G, colNew.B);
            arrZustand[ant.iCurrX, ant.iCurrY] = zustNeu;

            // Prüfen, welche rotation an dem entsprechenden Zustand durchgeführt werden muss
            // und dann die entspechende Richtung auswählen
            int iRot = Convert.ToInt32(strAnt.Substring((AnzahlZustaende - 1) - zustNeu, 1));
            ant.iDirec += vecDirec.Length + arrRot[iRot];
            ant.iDirec %= vecDirec.Length;

            // Einen Schritt in die neue Richtung machen
            ant.iCurrX = (ant.iCurrX + (int)vecDirec[ant.iDirec].X + ImageWidth) % ImageWidth;
            ant.iCurrY = (ant.iCurrY + (int)vecDirec[ant.iDirec].Y + ImageHeight) % ImageHeight;

            return ant;
        }

        // wenn aktuelles feld weiß ist, dann rot färben - nach links drehen und eins vorwärts
        // wenn aktuelles feld rot, dann weiß färben - nach rechts drehen und eins vorwärts
        private void Aufgabe()
        {            
          
            flow.StepCount = 1000;

            for (int iAnt = 0; iAnt < arrAnts.Length; iAnt++)
            {
                strAnt = GLabReader.ReadAntFromFile(arrAnts[iAnt]);
                AnzahlZustaende = strAnt.Length;
                arrZustand = new int[ImageWidth, ImageHeight];
                _image = new Image<Rgb, byte>(ImageWidth, ImageHeight, new Rgb(Color.White));
                _frame.SetImage(_image);
                ant1.iZustand = 0;
                ant1.iCurrX = (ImageWidth - 1) / 2;
                ant1.iCurrY = (ImageHeight - 1) / 2;

                for (int i = 0; i < 10000000; i++)
                {
                    if (i % 1000 == 0)
                    {
                        _frame.Repaint();
                        Logger.Instance.LogInfo(""+i);
                    }

                    //for (Int64 ix = 0; ix < 10000; ix++) { }
                    flow.Wait();
                    ant1 = doStep(ant1);
                    ant2 = doStep(ant2);

                }
            }

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