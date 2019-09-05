using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

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
        
        public string analizar2(Skeleton skeleton)
        {
            //****************HOLA*********************
            if (skeleton.Joints[JointType.HandRight].Position.Z < skeleton.Joints[JointType.Head].Position.Z)
            {
                if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ShoulderCenter].Position.Y)
                {
                    if (skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ShoulderRight].Position.X)
                    {
                        return "Hola";
                    }
                }
            }
            //****************FIN HOLA *****************
            //****************ADIOS*********************
            if(skeleton.Joints[JointType.ShoulderRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y && skeleton.Joints[JointType.ElbowRight].Position.Y  < skeleton.Joints[JointType.HandRight].Position.Y )
            {
                if(skeleton.Joints[JointType.ElbowRight].Position.X < skeleton.Joints[JointType.HandRight].Position.X)
                {
                    return "Adios";
                }
            }
            //***************FIN ADIOS******************
                        return "Pausa";

        }
        

    }
}