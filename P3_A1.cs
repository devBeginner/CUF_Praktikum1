#region

using System;
using GLab.Core;
using GLab.Core.PluginInterfaces;
using GLab.Rendering;
using GLab.Rendering.Camera;
using GLab.Rendering.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GLab.Chaos;
using GLab.Chaos.Datastructure;
using GLab.Core.Forms.Input;
using GLab.Core.Forms.Control;

#endregion

namespace GLab.Example.Chaos
{
    /// <summary>
    ///   Creating a scene with different types of cameras and some basic primitives.
    /// </summary>
    class P3_A1 : IPluginClient
    {
        private RenderView _flyCamView;

        private ContentManager _contentManager;

        private FlyCamera _flyCamera;
        //private RenderView _polarCamView;
        //private PolarCamera _polarCamera;

        private Scene _scene;

        private MatrixStack _matrixStack;
        private PlatonicSolidFactory _psf;
        private LindenmayerSystem _lindenmayerSystem;
        private float _size;
        private float _width;

        private Random _rand;

        private Vector3 _vecStart;
        private Vector3 _pos;

        FrmFlowControl flow;
        string filename = "";
        string currentAxiom = "";

        /// <summary>
        ///   Init. the example.
        /// </summary>
        public P3_A1()
        {
            Name = "Praktikum 3";

        }

        /// <summary>
        ///   Setup the example.
        /// </summary>
        public override void Setup()
        {
            _scene = new Scene();
            _rand = new Random();
            _contentManager = new ContentManager(XnaRenderer.Instance.Services,
                                     GLabController.Instance.HomeDirectory + "\\Data\\FractalTerrain");

            _psf = new PlatonicSolidFactory(XnaRenderer.Instance);
            _matrixStack = new MatrixStack();
            filename = InputHelper.LoadFileDialog("Load LIN-File");
            if (filename == "")
                return;
            _lindenmayerSystem = GLabReader.ReadLindenmayerSystemFromFile(filename);

            if (filename.Contains("1_")) { _pos = new Vector3(0, 0, Math.Abs(_lindenmayerSystem.CoordinateSystem.MaxX) + Math.Abs(_lindenmayerSystem.CoordinateSystem.MinX)); }
            if (filename.Contains("2_")) { _pos = new Vector3(0, 0, 3); }
            if (filename.Contains("3_")) { _pos = new Vector3(3, 5, 3); }

            flow = new FrmFlowControl();

            // Breite und Länge initialisieren
            _vecStart = new Vector3(0, 0, 0);

            Vector3 pos = _pos; // new Vector3(0, 0, 5);
            Vector3 lookAt = Vector3.Zero;
            Vector3 up = Vector3.UnitY;


            //_polarCamera = new PolarCamera();
            //_polarCamera.SetLookAt(pos, lookAt, up);
            //_polarCamera.ActivateControl();
            //_polarCamView = new RenderView(512, 512, _scene, _polarCamera,
            //                               "Polar Camera");

            //Logger.Instance.LogInfo(
            //    "Controls (Polar Camera): <W>, <A>, <S>, <D>, <Q>, <E> and mouse movement (while left or right clicking).");

            _flyCamera = new FlyCamera();
            _flyCamera.ActivateControl();
            _flyCamera.SetLookAt(pos, lookAt, up);
            _flyCamera.Movement = 0.2f;
            _flyCamView = new RenderView(256, 256, _scene, _flyCamera, "Fly Camera");

            Logger.Instance.LogInfo(
                "Controls (Fly Camera): <W>, <A>, <S>, <D>, <Q>, <E> and mouse movement (while left clicking). Use <R> to reset the camera to the initial position.");

            //XnaRenderer.Instance.Views.Add(_polarCamView);
            XnaRenderer.Instance.Views.Add(_flyCamView);

            Start();
            _scene.RenderCoordinateAxes = false;
            flow.StepCount = 1;
            currentAxiom = _lindenmayerSystem.Axiom;
            int iStep = 11;
            while (true)
            {
                iStep++;
                nextGeneration(_scene, 0, iStep, currentAxiom);
                flow.Wait();
                Logger.Instance.LogInfo("" + iStep);
            }

        }


        private void nextGeneration(Scene _scene, int iCurRec, int iMaxRec, string strAxiom)
        {
            GeometricPrimitive gp = null;
            string _currentString = strAxiom;
            float changeAngle = (float)_lindenmayerSystem.ChangeOfAngle;
            _matrixStack = new MatrixStack();
            const float DEF_WIDTH = 0.05f;

            if (_lindenmayerSystem.GrSwitch == true)
            {
                // Matrixstack initialisieren
                _scene.Renderables.Clear();         // Szene zurücksetzen
            }

            // Identität laden und in die korrekte Ausgangsrichtung bringen
            _matrixStack.LoadIdentity();
            _matrixStack.Rotate(_lindenmayerSystem.InitialAngle, new Vector3(0, 0, 1));
            _matrixStack.PushMatrix();

            // breite und länge für die rekursionsstufe setzen
            _width = DEF_WIDTH;// DEF_WIDTH;// * _lindenmayerSystem.ScalePerIteration;// *(float)Math.Pow(_lindenmayerSystem.ScalePerIteration, iCurRec);
            _size = (float)Math.Pow(_lindenmayerSystem.ScalePerIteration, iCurRec);


            if (filename.Contains("_3d"))
                _width = _size;
            if (filename.Contains("Sierpinsk"))
                _width = 1f;
            if (filename.Contains("3_Hilbert"))
                _width = 0.05f;


            System.Drawing.Color coltmp = GenerateRandomColor();
            Color currentColor = new Color(coltmp.R, coltmp.G, coltmp.B);

            // Interpretiere den aktuellen String _currentString Zeichen für Zeichen
            for (int i = 0; i < _currentString.Length; i++)
            {

                switch (_currentString[i])
                {

                    case 'X':
                    case 'Y':
                    case 'G':
                    case 'F':

                        _matrixStack.PushMatrix();

                        _matrixStack.Translate(new Vector3(0.5f*_size, 0, 0)); 

                        _matrixStack.Scale(new Vector3(_size, _width , _width));                      


                        gp = _psf.CreateGeometricPrimitive(PlatonicSolid.Hexahedron, currentColor, _matrixStack.Transform);

                        _scene.Renderables.Add(gp);

                        _matrixStack.PopMatrix();

                        _matrixStack.Translate(new Vector3(_size, 0, 0));

                        //flow.Wait();

                        break;

                    case 'g':
                    case 'f':
                        _matrixStack.Translate(new Vector3(_size, 0, 0));
                        break;

                    case '-':
                        _matrixStack.Rotate(-changeAngle, 0, 0, 1); // rechts-Drehung in x-y-Ebene
                        break;

                    case '+':
                        _matrixStack.Rotate(changeAngle, 0, 0, 1); // Links-Drehung in x-y-Ebene
                        break;

                    case '&':
                        _matrixStack.Rotate(-changeAngle, 0, 1, 0); // rechts-Drehung in x-y-Ebene
                        break;

                    case '^':
                        _matrixStack.Rotate(changeAngle, 0, 1, 0); // Links-Drehung in x-y-Ebene
                        break;

                    case '*':
                        _matrixStack.Rotate(-changeAngle, 1, 0, 0); // rechts-Drehung in x-y-Ebene
                        break;

                    case '/':
                        _matrixStack.Rotate(changeAngle, 1, 0, 0); // Links-Drehung in x-y-Ebene
                        break;

                    case '|':
                        _matrixStack.Rotate(180, 0, 0, 1);
                        _matrixStack.Rotate(180, 0, 1, 0);
                        _matrixStack.Rotate(180, 1, 0, 0);
                        break;

                    case '[':
                        _matrixStack.PushMatrix();
                        if (filename.Contains("Baum_3") || filename.Contains("Busch_3"))
                            _width = _width * _lindenmayerSystem.ScalePerIteration;
                        //_size *= _lindenmayerSystem.ScalePerIteration;
                        break;

                    case ']':
                        _matrixStack.PopMatrix();
                        if (filename.Contains("Baum_3") || filename.Contains("Busch_3"))
                            _width = _width / _lindenmayerSystem.ScalePerIteration;
                        //_size /= _lindenmayerSystem.ScalePerIteration;
                        break;

                } // end of switch
                //Logger.Instance.LogInfo(""+_currentString[i]);
            }
                flow.Wait();
            //_scene.Renderables.Add(gp);
            //_matrixStack.PopMatrix();

            

            // Wenn max Iteration
            if (iCurRec == iMaxRec)
            {
                if (!filename.Contains("2_Koch"))
                {
                    currentAxiom = _lindenmayerSystem.Axiom;
                }
                return;
            }
            else
            {

                iCurRec++;

                if (filename.Contains("2_Koch"))
                {
                    currentAxiom = "";
                    for (int i = 0; i < _currentString.Length; i++)
                    {
                        if (_currentString[i] == 'F')
                        {
                            if (_rand.Next() % 2 == 0)
                            {

                                currentAxiom += "F-F++F-F";
                            }
                            else
                            {
                                currentAxiom += "F+F--F+F";

                            }

                        }
                        else
                        {
                            currentAxiom += _currentString[i];
                        }
                    }
                }
                else
                {
                    currentAxiom = _lindenmayerSystem.ComposeStringFromOtherString(currentAxiom, 1);
                }

                nextGeneration(_scene, iCurRec, iMaxRec, currentAxiom);
            }

        }

        private System.Drawing.Color GenerateRandomColor()
        {
            return System.Drawing.Color.FromArgb(_rand.Next(256), _rand.Next(256), _rand.Next(256));
        }

        /// <summary>
        ///   Executed once in each of the iterations of the render loop.
        /// </summary>
        public override void Run()
        {
            XnaRenderer.Instance.RenderFrame();
        }

        /// <summary>
        ///   Cleans up any created resources and stops the render loop.
        /// </summary>
        public override void Teardown()
        {
            Stop();
            //_polarCamView.Close();
            if (filename!="")
            _flyCamView.Close();
            XnaRenderer.Instance.Views.Clear();
        }
    }
}