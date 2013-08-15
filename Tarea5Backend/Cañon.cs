using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tarea5Backend
{
    public class Cañon
    {
        int x, hp;

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

        public Cañon()
        {
            x = 14;
        }
        

    }
}
