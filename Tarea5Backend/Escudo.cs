using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Tarea5Backend
{
    public class Escudo
    {

        int x, y;
        int hp;

        public int Hp
        {
            get { return hp; }
            set { hp = value; }
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

        public Escudo(int x, int y)
        {

            this.x = x;
            this.y = y;

        }


    }

    
}
