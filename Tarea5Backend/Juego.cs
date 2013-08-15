using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tarea5Backend
{
    public class Juego
    {

        int recursos = 1000;



        public int Recursos
        {
            get { return recursos; }
            set { recursos = value; }
        }

        int laser, misil, plasma, nuke, dinero;

        public int Laser
        {
            get { return laser; }
            set { laser = value; }
        }

        public int Misil
        {
            get { return misil; }
            set { misil = value; }
        }

        public int Plasma
        {
            get { return plasma; }
            set { plasma = value; }
        }

        public int Nuke
        {
            get { return nuke; }
            set { nuke = value; }
        }

        

        public int Dinero
        {
            get { return dinero; }
            set { dinero = value; }
        }

        List<Nave> naves = new List<Nave>();

        public List<Nave> Naves
        {
            get { return naves; }
            set { naves = value; }
        }
        List<Camion> camiones = new List<Camion>();

        public List<Camion> Camiones
        {
            get { return camiones; }
            set { camiones = value; }
        }
        Cañon cañon = new Cañon();

        public Cañon Cañon
        {
            get { return cañon; }
            set { cañon = value; }
        }


        int nivelJuego = 1;

        public int NivelJuego
        {
            get { return nivelJuego; }
            set { nivelJuego = value; }
        }


        public event Action<Camion> nuevoCamion;

        



        public Juego()
        {

            laser = 50;
            misil = 0;
            plasma = 0;
            nuke = 0;
            dinero = 250;

            /*
            DateTime inicioJuego = DateTime.Now;
            
             TimeSpan juegoTranscurrido;
             while (true)
             {
                 juegoTranscurrido = DateTime.Now - inicioJuego;
                 if (juegoTranscurrido.TotalSeconds >= 70 + 70 * (nivelJuego - 1))
                     nivelJuego++;

                 espereTiempo(0.1);
             }
             * */
        }



        public void Comprar(int opcion)
        {
            
           //Opciones de compra 0-> Camioncito, 1-laser, 2-misil, 3-plasma, 4-nuke

            switch (opcion)
            {
                case 0:
                    if (dinero >= 10)
                    {
                        Camion c = new Camion();
                        nuevoCamion(c);
                        
                        dinero -= 10;

                    }
                    return;

                case 1:
                    if (dinero >= 10)
                    {
                        laser += 10;
                        dinero -= 10;
                    }
                    return;

                case 2:
                    if (dinero >= 10)
                    {
                        misil += 6;
                        dinero -= 10;
                    }
                    return;

                case 3:
                    if (dinero >= 10)
                    {
                        plasma += 10;
                        dinero -= 10;
                    }
                    return;

                case 4:
                    if (dinero >= 40)
                    {
                        nuke += 1;
                        dinero -= 40;
                    }
                    return;
            }

        }



        void espereTiempo(double segundos)
        {
            // ME ESPERA x SEGUNDOS =D

            DateTime inicio = DateTime.Now;
            TimeSpan diferencia;
            while (true)
            {

                diferencia = DateTime.Now - inicio;
                if (diferencia.TotalSeconds >= segundos)
                {
                    break;
                }
            }


        }

    }
}
