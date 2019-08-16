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

namespace ControlKinect
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor miKinect;
        int angulo;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void iniciarApp(object sender, RoutedEventArgs e)
        {
            anguloActual.Text = "0";
            angulo = 0;
            miKinect = KinectSensor.KinectSensors.FirstOrDefault();
            miKinect.Start();
            miKinect.ColorStream.Enable();
            miKinect.ColorFrameReady += MiVideo;
        }

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
    }
}
