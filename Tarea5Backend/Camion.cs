using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tarea5Backend
{
    public class Camion
    {

        int x, sentido; //0 izquierda, 1 derecha
        public bool sigue = true;

        public int Sentido
        {
            get { return sentido; }
            set { sentido = value; }
        }

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public Camion()
        {
            x = 24;
            sentido = 1;

        }

    }
}
