using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlKinect.procesamiento
{
    class palabras
    {
        public String Resultado(int codigo)
        {
            switch(codigo)
            {
                case 1:
                    return "Hola";
                    break;
                default:
                    return "Error";
                    break;
            }
        }
    }
}
