using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tarea5
{
    public class Disparo
    {

        int x, y, daño;

        public int Daño
        {
            get { return daño; }
            set { daño = value; }
        }

        tipoDisparo tipoDeDisparo;

        public tipoDisparo TipoDeDisparo
        {
            get { return tipoDeDisparo; }
            set { tipoDeDisparo = value;
            if (value == tipoDisparo.Laser)
                daño = 1;
            if (value == tipoDisparo.Misil)
                daño = 2;
            if (value == tipoDisparo.Plasma)
                daño = 3;
            }
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

        public Disparo()
        {
            
        }

        public enum tipoDisparo
        {
            Laser, Misil, Plasma, Nuke
        }

    }
}
