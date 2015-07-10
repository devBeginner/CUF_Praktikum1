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
    internal class P3_4 : IPluginClient, IGuiExtension
    {
        // zum speichern der Stati
        public enum eState { empty, wood, termite, termitandwood }

        private FrmImage _frame;
        private Image<Rgb, byte> _image;
        private Random _rand = new Random();


        private const int MAX_TERMITE = 3000;
        private const int MAX_WOOD = 5000;
        private const int ImageWidth = 256, ImageHeight = 256;

        // Zum speichern der Zustände der einzelnen Pixel (leer, termite oder holz)
        private eState[,] state = new eState[ImageWidth, ImageHeight];

        // Array mit den Termiten, um sie in jeder Runde einfach weiterlaufen zu lassen
        private Termite[] termites = new Termite[MAX_TERMITE];

        // Array mit den verschiedenen Richtungen, der jeweils nächste Eintrag 
        // entspricht einer Rotation um 45° nach recht.
        private static Vector2[] arrDirection = { 
            new Vector2(0, 1), 
            new Vector2(1, 1), 
            new Vector2(1, 0), 
            new Vector2(1, -1), 
            new Vector2(0, -1), 
            new Vector2(-1, -1), 
            new Vector2(-1, 0),
            new Vector2(-1, 1) 
        };

        // Array zum ändern der Richtung links vor, gerade aus, oder rechts vor
        // das gerade ausgehen ist wahrscheinlicher (3x0)
        private static int[] arrChDir = { -1, 0, 0, 0, 1 };

        public P3_4()
        {
            Name = "Termiten";
        }

        // Die Termite 
        internal class Termite
        {
            // Aktuelle Position únd Richtung
            public int X;
            public int Y;
            public int Dir;

            // Die Position für die nächste zufällige Laufrichtung
            private int Xnext;
            private int Ynext;
            private int DirNext;

            private eState status;

            // Zufallszahl initialisieren
            private Random _rand = new Random();

            // hat die Termite einen Chip
            public bool hasChip = false;

            // kann sie sich nicht bewegen, weil alle Richtungen blockiert sind
            public bool stand = false;

            // Anzahl der Runden, bevor sie wieder einen Holzchip aufnehmen darf
            public int countDown = 0;

            public Termite(eState[,] state)
            {
                getRandPosition(state);
                Dir = _rand.Next() % arrDirection.Length;
                state[X, Y] = eState.termite;
                hasChip = false;
                countDown = 0;
                status = eState.termite;
            }

            // Zufällige nächste Position ermitteln
            private void loadNextPosition(eState[,] state)
            {
                int count = -1;

                // Wenn die Termite erst einen Chip abgelegt hat, dann in Zufällige Richtung laufen, an der eine Freie stelle ist
                if (countDown > 0)
                {
                    // Solange versuchen eine zufälltige Richtung zu ermitteln, bis alle probiert wurden oder ein freies 
                    // Feld gefunden wurde
                    do
                    {
                        // Zufällig Richtung ermitteln und 
                        DirNext = _rand.Next() % arrDirection.Length;

                        // Neue Position der Termite laden
                        Xnext = (X + (int)arrDirection[DirNext].X + ImageWidth) % ImageWidth;
                        Ynext = (Y + (int)arrDirection[DirNext].Y + ImageHeight) % ImageHeight;

                        count++;

                        // Wenn keine freie Stelle gefunden wurde und noch nicht alle Richtungen durhclaufen wurden
                    } while (count < arrDirection.Length && state[Xnext, Ynext] != eState.empty);

                    // ein Schritt weniger, bis die Termite wieder einen Holzchip aufnehmen kann
                    countDown--;

                }
                // zufällige Richtung nach vorne (rechts/links)
                else
                {
                    // Solange versuchen, eine neue Position zu ermitteln, 
                    // bis eine gefunden wurde, an der keine andere Termite sitzt
                    // oder aber alle Richtungen durchprobiert wurden und keine gefunden wurde 
                    do
                    {
                        // Zufällig nach vorne, links oder rechts drehen
                        DirNext = Dir + arrDirection.Length + arrChDir[_rand.Next() % arrChDir.Length];

                        // Index ermitteln
                        DirNext %= arrDirection.Length;

                        // Neue Position der Termite laden
                        Xnext = (X + (int)arrDirection[DirNext].X + ImageWidth) % ImageWidth;
                        Ynext = (Y + (int)arrDirection[DirNext].Y + ImageHeight) % ImageHeight;

                        count++;

                    } while (count < arrDirection.Length && state[Xnext, Ynext] == eState.termite && state[Xnext, Ynext] == eState.termitandwood);


                }

                // wenn keine Stelle zum hinlaufen gefunden wurde, stehen bleiben und umdrehen
                // sodass sie in der nächsten Runde in die entgegengesetzte Richtung versucht
                // zu laufen
                if (count == arrDirection.Length)
                {
                    Dir += arrDirection.Length / 2;
                    Dir %= arrDirection.Length;

                    stand = true;

                }
                else
                {
                    stand = false;
                }

            }

            //setz den chip an der Stelle und gibt zurück ob es erfolgreich war
            private bool setWood(eState[,] state)
            {

                bool set = false;
                int randDir = 0;
                int x;
                int y;

                if (state[X, Y] == eState.wood)
                {
                    return false;
                }

                // Holz an die aktuelle Stelle, oder eins rechts oder links setzen
                do
                {
                    randDir = arrChDir[_rand.Next() % arrChDir.Length];

                    if (randDir != 0)
                    {
                        randDir += DirNext + arrDirection.Length;
                        randDir %= arrDirection.Length;

                        // Neue Position zum hinlegen laden
                        x = (X + (int)arrDirection[randDir].X + ImageWidth) % ImageWidth;
                        y = (Y + (int)arrDirection[randDir].Y + ImageHeight) % ImageHeight;
                    }
                    else
                    {
                        x = X;
                        y = Y;
                    }

                    // wenn an der Stelle nichts ist
                    if (state[x, y] == eState.empty || (x == X && y == Y))
                    {
                        // Holz setzen an die aktuelle Stelle
                        state[X, Y] = eState.wood;

                        // merken, dass sie den Chip weggelegt hat
                        hasChip = false;

                        status = eState.termite;

                        set = true;

                    }

                } while (set == false);

                // Zurückgeben, dass es gesetzt wurde
                return true;

            }

            // gibt zurück, ob dort ein Chip lag und setzt die stelle auf false
            private bool pickWood(eState[,] state)
            {
                if (state[Xnext, Ynext] != eState.wood)
                {
                    return false;
                }
                else
                {
                    // Status an der aktuellen Stelle entfernen und 
                    // Termite auf die neue Stelle setzen
                    state[X, Y] = eState.empty;
                    state[Xnext, Ynext] = eState.termite;

                    // merken, dass die Termite einen Chip hat
                    this.hasChip = true;
                    status = eState.termitandwood;

                    countDown = 20;

                    return true;
                }
            }

            // setzt die Termite irgendwohin, wo kein Holz ist
            private void getRandPosition(eState[,] state)
            {
                do
                {
                    X = _rand.Next() % ImageWidth;
                    Y = _rand.Next() % ImageHeight;

                } while (state[X, Y] != eState.empty);
            }

            // An die neue berechnete Stelle gehen
            private void move(eState[,] state)
            {
                X = Xnext;
                Y = Ynext;
                Dir = DirNext;
            }

            // einen Schritt machen
            internal void doStep(eState[,] state)
            {
                eState nextState;

                // neue Position laden
                loadNextPosition(state);

                // Status an der neuen Position laden
                nextState = state[Xnext, Ynext];

                if (stand)
                    return;

                // Wenn an der neuen Position ein Chip ist
                switch (nextState)
                {
                    case eState.wood:

                        if (hasChip)
                        {
                            this.setWood(state);    // Chip an aktueller Position ablegen
                            countDown = 20;         // 20 Schritte machen bevor wieder was aufgenommen werden kann
                            doStep(state);          // einen Schritt machen
                        }
                        else
                        {
                            pickWood(state);    // Chip aufnehmen und Status anpassen
                            move(state);             // Position verändern

                        }
                        break;

                    case eState.empty:
                        if (state[X, Y] != eState.wood)
                            state[X, Y] = eState.empty;     // Aktuelle Stelle auf leer setzen
                        move(state);                         // in die Richtung gehen
                        state[X, Y] = status;   // Neue Position setzen

                        break;

                    case eState.termite:
                        break;

                    case eState.termitandwood:
                        break;
                }
            }

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
            // Create a new raster image for drawing
            _image = new Image<Rgb, byte>(ImageWidth, ImageHeight, new Rgb(Color.White));

            // Create a new frame to display the raster image
            _frame = new FrmImage(Name, ImageWidth + 1, ImageHeight + 1, DisplayMode.Zoomable)
                {
                    InterpolationMode = InterpolationMode.NearestNeighbor,
                    SmoothingMode = SmoothingMode.None
                };
            _frame.SetImage(_image);

            // Register this plugin as a GUI extension
            GLabController.Instance.RegisterExtension(this);

            // Stati initialisieren
            init();

            // Aufgabe starten
            Aufgabe();
        }

        // wenn aktuelles feld weiß ist, dann rot färben - nach links drehen und eins vorwärts
        // wenn aktuelles feld rot, dann weiß färben - nach rechts drehen und eins vorwärts
        private void Aufgabe()
        {
            FrmFlowControl flow = new FrmFlowControl();
            flow.StepCount = 1;
            int i = 0;
            int i2 = 0;
            int iOut = 1;

            while (true)
            {
                i++;

                //for (int ix = 0; ix < 100000; ix++) { }

                // Alle Termiten eins weiter setzen
                moveAllTermites();

                if (i == iOut)
                {
                    // Statearray durchlaufen und Farben im Bild aktualisieren
                    Logger.Instance.LogInfo("" + i2 + " * " + iOut);
                    updateColors();
                    i2++;
                    i = 0;
                    flow.Wait();    
                }
            }

        }

        // Alle Termiten einen Schritt ausführen lassen
        private void moveAllTermites()
        {
            for (int i = 0; i < MAX_TERMITE; i++)
            {
                termites[i].doStep(state);
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

        // aktualisiert das bild
        // durchläuft das Statusarray und zeichnet die ensprechenden Pixel
        // je nach Zustand
        private void updateColors()
        {
            int x = 0;
            int y = 0;

            // Gesamtes Array auf empty setzen
            for (x = 0; x < ImageWidth; x++)
            {
                for (y = 0; y < ImageHeight; y++)
                {
                    switch (state[x, y])
                    {
                        case eState.empty:
                            _image[y, x] = new Rgb(Color.White);
                            break;

                        case eState.termite:
                            _image[y, x] = new Rgb(Color.Black);
                            break;

                        case eState.wood:
                            _image[y, x] = new Rgb(Color.Brown);
                            break;

                        case eState.termitandwood:
                            _image[y, x] = new Rgb(Color.Red);
                            break;
                    }
                }
            }

            _frame.Repaint();
        }

        // Imitalisiert das Array und setzt die Chips und die 
        // Termiten an eine zufällige Stelle
        private void init()
        {
            int x = 0;
            int y = 0;

            // Gesamtes Array auf empty setzen
            for (x = 0; x < ImageWidth; x++)
            {
                for (y = 0; y < ImageHeight; y++)
                {
                    state[x, y] = eState.empty;
                }
            }

            // Zufällig die Chips verteilen
            for (int i = 0; i < MAX_WOOD; i++)
            {
                bool bSet = false;
                do
                {
                    // Zufällige Position ermitteln
                    x = _rand.Next() % ImageWidth;
                    y = _rand.Next() % ImageHeight;

                    // Wenn an der Position kein Chip liegt
                    if (state[x, y] == eState.empty)
                    {
                        state[x, y] = eState.wood;
                        bSet = true;
                    }
                } while (bSet == false);
            }

            // Termiten erzeugen
            // Termiten erstellen
            for (int i = 0; i < MAX_TERMITE; i++)
            {
                termites[i] = new Termite(state);
            }
        }
    }
}