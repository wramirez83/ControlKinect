using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using ControlKinect.procesamiento;
using System.Speech.Synthesis;
using System.IO;


namespace ControlKinect
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor miKinect;
        int angulo; //Angulo del sensor -27 a 27
        #region "Variables"
        private const double GrosorCuerpo = 10;//Espesor de la elipse del centro del cuerpo
        private const double EspesorClip = 10;//Espesor de los rectángulos del borde del clip
        private readonly Brush CentrodePincel = Brushes.Blue; //Pincel usado para dibujar el punto central del esqueleto
        private readonly Brush SeguimientoJunturaPincel = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68)); //Pincel utilizado para dibujar juntas que actualmente se siguen
        private readonly Brush NoJunturaPincel = Brushes.Yellow; //Pincel utilizado para dibujar juntas que actualmente se infieren
        private readonly Pen SeguimientoHuesoLapicero = new Pen(Brushes.Green, 6);//Bolígrafo utilizado para dibujar huesos que actualmente se rastrean
        private readonly Pen NoHuesoLapicero = new Pen(Brushes.Gray, 1);//Bolígrafo utilizado para dibujar huesos que actualmente se infieren
        private DrawingImage FuenteImagen;//Imagen de dibujo que mostraremos
        private const double LineasUnion = 3;//Espesor de las líneas de unión dibujadas.
        private DrawingGroup drawingGroup;//Grupo de dibujo para salida de representación de esqueleto
        private const float RenderHeight = 480.0f;//Altura de nuestro dibujo de salida
        private const float RenderWidth = 640.0f;
        #endregion
        private double manoDerechaVZ, manoDerechaVX, manoDerechaVY, manoIzquiedaVZ, manoIzquiedaVX, manoIzquiedaVY, cabezaZ, cabezaXV, cabezaYV, codoDerechoZ, codoDerechoX, codoDerechoY, hombroDerechoZ, hombroDerechoX, hombroDerechoY;
        private double[,] matriz = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        Procesamiento proceso = new Procesamiento();
        private string estado = "Pausa";
        SintetizarVoz sintetizar = new SintetizarVoz();
        Reconocimiento rec = new Reconocimiento();
        string palabraTemporal = "";
        

        private void bestado(object sender, RoutedEventArgs e)
        {
            palabra.Text = "Pausa";
        }

        public MainWindow()
        {
            InitializeComponent();
            sintetizar.Config();
            
            Loaded += iniciarApp;
            Unloaded += CerrarApp;
           
        }
        private void LeerHablar(string a)
        {
            if(palabraTemporal != a)
            {
               // sintetizar.hablar(a);
                //palabraTemporal = a;
            }
            
        }


            private void iniciarApp(object sender, RoutedEventArgs e)
        {
           
            //*************DECLARAMOS PROCESAMIENtO*************
           
            
            //*********************************************
            //Cree un grupo de dibujo que se usará para dibujar
            this.drawingGroup = new DrawingGroup();
            //Crear una fuente de imagen que muestre nuestro esqueleto
            this.FuenteImagen = new DrawingImage(this.drawingGroup);
            //Mostrar la imagen en nuestro control de imagen
            Image.Source = FuenteImagen;
            //***********************************************
            anguloActual.Text = "0";
            angulo = 0;
            
            miKinect = KinectSensor.KinectSensors.FirstOrDefault();
            try
            {

                if(miKinect.Status == KinectStatus.Connected)//Confirmamos si el sensor esta conectado
                {
                    miKinect.Start();//Inicializar sensor
                    miKinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;//Indique a Kinect Sensor que use el modo predeterminado (Esqueleto humano de pie) || Sentado (esqueleto humano sentado)
                    miKinect.SkeletonFrameReady += sensor_SkeletonFrameReady;//Suscríbase al evento SkeletonFrameready de te Sensor para rastrear las uniones y crear las uniones para mostrar en nuestro control de imagen
                    Message.Text = "Kinect Listo";//bonito mensaje con colores para avisarle si su sensor funciona o no
                    Message.Background = new SolidColorBrush(Colors.Green);
                    Message.Foreground = new SolidColorBrush(Colors.White);
                    this.miKinect.SkeletonStream.Enable();//Encienda la secuencia de esqueleto para recibir marcos de esqueleto
                    id.Content = miKinect.DeviceConnectionId;//Mostramos el id de conexion de dispositivo
                    miKinect.ColorStream.Enable();
                    miKinect.ColorFrameReady += MiVideo;
                    CambiarAngulo();
                }
                else if (miKinect.Status == KinectStatus.Disconnected)
                {
                    Message.Text = "Kinect No Está Listo";
                    Message.Background = new SolidColorBrush(Colors.Green);
                    Message.Foreground = new SolidColorBrush(Colors.White);
                }
                else if(miKinect.Status == KinectStatus.NotPowered)
                {
                    Message.Text = "Kinect No Está Energizado";
                    Message.Background = new SolidColorBrush(Colors.Green);
                    Message.Foreground = new SolidColorBrush(Colors.White);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[1]; //Declaramos el array del esqueleto
            //Abre un objeto SkeletonFrame, que contiene un marco de datos de esqueleto.
            using (SkeletonFrame skeletonframe = e.OpenSkeletonFrame())
            {
                if(skeletonframe != null)//Compruebe si el marco está abierto
                {
                    skeletons = new Skeleton[skeletonframe.SkeletonArrayLength];
                    //Copia los datos del esqueleto a una serie de esqueletos, donde cada esqueleto contiene una colección de las articulaciones.
                    skeletonframe.CopySkeletonDataTo(skeletons);
                    //dibujar el esqueleto basado en el modo predeterminado (de pie), "sentado"
                    if(miKinect.SkeletonStream.TrackingMode == SkeletonTrackingMode.Default)
                    {
                        DrawStandingSkeletons(skeletons);
                    }
                    else if (miKinect.SkeletonStream.TrackingMode == SkeletonTrackingMode.Seated)
                    {
                        //Dibuja un esqueleto sentado con 10 articulaciones
                        DrawStandingSkeletons(skeletons);
                    }
                }
            }
        }
        private void DrawStandingSkeletons(Skeleton[] skeletons)
        {
            using (DrawingContext dc = this.drawingGroup.Open())
            {
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                if(skeletons.Length != 0)//Si la matriz de esqueleto tiene elementos
                {
                    foreach(Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);
                        if(skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if(skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(this.CentrodePincel, null, this.SkeletonPointToScreen(skel.Position), GrosorCuerpo, GrosorCuerpo);
                        }
                    }
                }
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        //------------------------------------
        private void DrawSeatedSkeletons(Skeleton[] skeletons)
        {
            using (DrawingContext dc = this.drawingGroup.Open())
            {
                //Dibuje un fondo transparente para establecer el tamaño de renderizado
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(this.CentrodePincel, null, this.SkeletonPointToScreen(skel.Position), GrosorCuerpo, GrosorCuerpo);
                        }

                    }
                }
                //Evitar dibujar fuera del lienzo
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }


        //------------------------------------
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - GrosorCuerpo, RenderWidth, EspesorClip));
            }
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, EspesorClip));
            }
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, EspesorClip, RenderHeight));
            }
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - EspesorClip, 0, EspesorClip, RenderHeight));
            }
        }
        /// <summary>
        /// Dibuja los huesos y las articulaciones de un esqueleto.
        /// </summary>
        /// <param name="skeleton">esqueleto para dibujar</param>
        /// <param name="drawingContext">contexto de dibujo para dibujar</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);
            // Brazo izquierdo
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);
            // Brazo Derecho
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);
            // Pierna izquierda
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);
            // Pierna Derecha
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);
            //****************************************************
            manoDerechaVZ = skeleton.Joints[JointType.HandRight].Position.Z;
            manoDerechaVX = skeleton.Joints[JointType.HandRight].Position.X;
            manoDerechaVY = skeleton.Joints[JointType.HandRight].Position.Y;
            manoDerechaZ.Text = manoDerechaVZ.ToString();
            manoDerechaX.Text = manoDerechaVX.ToString();
            manoDerechaY.Text = manoDerechaVY.ToString();
            manoIzquiedaVZ = skeleton.Joints[JointType.HandLeft].Position.Z;
            manoIzquiedaVX = skeleton.Joints[JointType.HandLeft].Position.X;
            manoIzquiedaVY = skeleton.Joints[JointType.HandLeft].Position.Y;
            manoIzquierdaZ.Text = manoIzquiedaVZ.ToString();
            manoIzquierdaX.Text = manoIzquiedaVX.ToString();
            manoIzquierdaY.Text = manoIzquiedaVY.ToString();
            cabezaZ = (skeleton.Joints[JointType.Head].Position.Z);
            cabezaXV = (skeleton.Joints[JointType.Head].Position.X);
            cabezaYV = (skeleton.Joints[JointType.Head].Position.Y);
            cabeza.Text = cabezaZ.ToString();
            cabezaX.Text = cabezaXV.ToString();
            cabezaY.Text = cabezaYV.ToString();
            codoDerechoZ = skeleton.Joints[JointType.ElbowRight].Position.Z;
            codoDerechoX = skeleton.Joints[JointType.ElbowRight].Position.X;
            codoDerechoY = skeleton.Joints[JointType.ElbowRight].Position.Y;
            hombroDerechoZ = skeleton.Joints[JointType.ShoulderRight].Position.Z;
            hombroDerechoX = skeleton.Joints[JointType.ShoulderRight].Position.X;
            hombroDerechoY = skeleton.Joints[JointType.ShoulderRight].Position.Y;
            //****************estado = proceso.analizar2(Ske);//proceso.analizar(matriz);
            estado = proceso.analizar2(skeleton);
            if(estado != "Pausa")
            {
                palabra.Text += " " + estado;
                // sintetizar.hablar(estado);
                LeerHablar(estado);
            }
            //****************************************************
            
            // Render Articulaciones
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;
                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.SeguimientoJunturaPincel;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.NoJunturaPincel;
                }
                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), LineasUnion, LineasUnion);
                }
            }
        }
        /// <summary>
        /// Dibuja una línea osea entre dos articulaciones.
        /// </summary>
        /// <param name="skeleton">esqueleto para extraer huesos</param>
        /// <param name="drawingContext">contexto de dibujo para dibujar</param>
        /// <param name="jointType0">conjunta para comenzar a dibujar desde</param>
        /// <param name="jointType1"> articulación para terminar el dibujo en</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];
            // Si no podemos encontrar ninguna de estas articulaciones, salga
            if (joint0.TrackingState == JointTrackingState.NotTracked || joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }
            // No dibuje si se infieren ambos puntos
            if (joint0.TrackingState == JointTrackingState.Inferred && joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }
            // Suponemos que todos los huesos extraídos se infieren a menos que se rastreen AMBAS articulaciones
            Pen drawPen = this.NoHuesoLapicero;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {

                drawPen = this.SeguimientoHuesoLapicero;
            }
            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }
        /// <summary>
        /// Mapea un SkeletonPoint para ubicarlo dentro de nuestro espacio de renderizado y lo convierte en Point
        /// </summary>
        /// <param name="skelpoint">Punto de Mapa</param>
        /// <returns>Punto Mapeado</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convertir punto en espacio de profundidad. 
            // No estamos utilizando la profundidad directamente, pero queremos los puntos en nuestra resolución de salida de 640x480.
            DepthImagePoint depthPoint = this.miKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }
    
    //------------------------------------
    private void BtnBajar(object sender, RoutedEventArgs e)
        {
            if (angulo > -27)
            {
                angulo = Convert.ToInt32(anguloActual.Text) - 2;
                anguloActual.Text = angulo.ToString();
                CambiarAngulo();
            }
        }

        private void BtnSubir(object sender, RoutedEventArgs e)
        {
            if(angulo < 27)
            {
                angulo = Convert.ToInt32(anguloActual.Text) + 2;
                anguloActual.Text = angulo.ToString();
                CambiarAngulo();
            }
        }
        private void CambiarAngulo()
        {
            try
            {
                miKinect.ElevationAngle = angulo;
            }
            catch(ArgumentOutOfRangeException e )
            {
                MessageBox.Show(e.Message);
            }
           
        }
        //Video RGB
        void MiVideo(object sender, ColorImageFrameReadyEventArgs e)
        {
            using(ColorImageFrame frameImagen = e.OpenColorImageFrame())
            {
                if (frameImagen == null)
                    return;
                byte[] datosColor = new byte[frameImagen.PixelDataLength];
                frameImagen.CopyPixelDataTo(datosColor);
                mostrarVideo.Source = BitmapSource.Create(
                    frameImagen.Width, frameImagen.Height,
                    96,
                    96,
                    PixelFormats.Bgr32,
                    null,
                    datosColor,
                    frameImagen.Width * frameImagen.BytesPerPixel
                    );
            }
        }
        void rutaVideo(string ruta)
        {
            //var ruta = @"c:\videos\1.mp4";
           
                try
                {
                    respuesta.Source = new Uri(ruta);
                }
                catch (Exception)
                {
                    //throw;
                }
                    
            
            
        }

        private void CerrarApp(object sender, RoutedEventArgs e)
        {
            miKinect.Stop();
        }
    }
}