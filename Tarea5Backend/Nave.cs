using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tarea5Backend
{
    public class Nave
    {

        int nivelNave, x, y, hp, sentido;

        public int Sentido
        {
            get { return sentido; }
            set { sentido = value; }
        }

        public int NivelNave
        {
            get { return nivelNave; }
            set { nivelNave = value; }
        }

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public int Hp
        {
            get { return hp; }
            set { hp = value; }
        }

        public Nave(int nivel)
        {
            nivelNave = nivel;
            hp = nivel;
            x = 0;
            y = 0;
            sentido = 0;

        }

       
    }
}
