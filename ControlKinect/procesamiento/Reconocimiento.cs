using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.AudioFormat;
using System.Speech.Recognition;

namespace ControlKinect.procesamiento
{
    class Reconocimiento
    {
        SpeechRecognitionEngine recono = new SpeechRecognitionEngine();
        protected string rtPublica;
        public Reconocimiento()
        {
            
            //Configuramos el reconocimiento
            recono.SetInputToDefaultAudioDevice();
            recono.LoadGrammar(new DictationGrammar());
            recono.SpeechRecognized += Analizar;
            recono.RecognizeAsync(RecognizeMode.Multiple);
        }
        public void Analizar(object sender, SpeechRecognizedEventArgs e)
        {
            
            string coman = comandos(e.Result.Text);
            rtPublica = coman;
            Console.WriteLine(rtPublica);
        }
        public string GetRuta()
        {
            return rtPublica;
        }
        //Se genera diccionario de comandos
        string comandos(string palabra)
        {
            string ruta;
            switch (palabra)
            {
                default:
                    ruta = "";
                    break;
                case "uno":
                        ruta = "1.mp4";
                        break;
            }
            return ruta;
        }
        public string RutaPublica()
        {
            Console.WriteLine(rtPublica);
            return rtPublica;
        }
    }
}
