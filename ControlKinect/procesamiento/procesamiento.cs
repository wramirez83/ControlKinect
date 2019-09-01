using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlKinect.procesamiento
{
    class Procesamiento
    {

        public Procesamiento()
        {

        }
        public void escribir(string[,] matriz)
        {
            Console.WriteLine(matriz[0, 0]);
        }
        public string analizar(double[,] matriz)
        {
            //hola = 1
            //adios = 2
            /*
            matriz[0, 0] = manoDerechaVZ;
            matriz[0, 1] = manoDerechaVX;
            matriz[0, 2] = manoDerechaVY;
            matriz[1, 0] = manoIzquiedaVZ;
            matriz[1, 1] = manoIzquiedaVX;
            matriz[1, 2] = manoIzquiedaVY;
            matriz[2, 0] = cabezaZ;
            matriz[2, 1] = cabezaXV;
            matriz[2, 2] = cabezaYV;
            matriz[3, 0] = codoDerechoZ;
            matriz[3, 1] = codoDerechoX;
            matriz[3, 2] = codoDerechoY;
            matriz[4, 0] = hombroDerechoZ;
            matriz[4, 1] = hombroDerechoX;
            matriz[4, 2] = hombroDerechoY;
            */
            //Hola
            if (matriz[0, 2] > matriz[3, 2] && matriz[0, 2] < matriz[4, 2])//ManoDerechaY > codoDerecho && ManoDerechaY<HombroDerechoY
            {
                if (matriz[0, 1] > matriz[4, 1])//ManoDerecha X > hombroDerecho X
                {
                    if (matriz[0, 1] > matriz[3, 1])//ManoDerecha X > codoDerecho X
                    {
                        
                        return "Hola";
                    }
                }

            }
            return "Pausa";

        }

    }
}