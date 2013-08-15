using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Tarea5
{
    public class Cronometro
    {

        DispatcherTimer dt = new DispatcherTimer(DispatcherPriority.Render);
        public event Action nuevoTick;
        public int intervalo;

        public Cronometro(int segundos)
        {
            intervalo = segundos;
            dt.Interval = new TimeSpan(0, 0, segundos);
            dt.Tick +=new EventHandler(dt_Tick);
            dt.Start();


        }

        public Cronometro(int segundos, int milisegundos)
        {
            intervalo = segundos;
            dt.Interval = new TimeSpan(0, 0, 0, segundos, milisegundos);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Start();


        }


        public void dt_Tick(object sender, EventArgs e)
        {
            if (nuevoTick != null)
                nuevoTick();
        }


        
    }
}
