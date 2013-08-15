using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Timers;
using System.Net;
using System.Net.Sockets;

using Tarea5Backend;

namespace Tarea5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Juego game;
        Boolean bApretado = false;

        Dictionary<Image, Nave> ImgsToNaves = new Dictionary<Image,Nave>();
        Dictionary<Nave, Image> NavesToImgs = new Dictionary<Nave, Image>();

        Dictionary<Disparo, Image> BalasToImgsAbajo = new Dictionary<Disparo, Image>();
        Dictionary<Image, Disparo> ImgsToBalasAbajo = new Dictionary<Image, Disparo>();
        Dictionary<Disparo, Image> BalasToImgsArriba = new Dictionary<Disparo, Image>();
        Dictionary<Image, Disparo> ImgsToBalasArriba = new Dictionary<Image, Disparo>();
        List<Disparo> disparosArriba = new List<Disparo>();
        List<Disparo> disparosAbajo = new List<Disparo>();

        int tipoDeDisparo;

        object o = new object();

        
        Dictionary<Camion, Image> CamionesToImgs = new Dictionary<Camion, Image>();
        Dictionary<Image, Camion> ImgsToCamiones = new Dictionary<Image, Camion>();
        List<Thread> threadsCamiones = new List<Thread>();

        //Agregado para t6
        Dictionary<Escudo, Image> EscudosToImgs = new Dictionary<Escudo, Image>();
        
        List<Escudo> escudos = new List<Escudo>();
        List<Escudo> escudosEnemigos = new List<Escudo>();

        DateTime tiempoInicio;

        Opciones2 ventanaOpciones;

        int nivelEscudos;

        Boolean fabricaViva, minaViva;
        int vidaFabrica, vidaMina, vidaFabrica2, vidaMina2;

        Cañon enemigo;
        Image imgEnemigo;

        Socket server;
        Socket cliente;
        Socket yo;
        NetworkStream stream;
        IPEndPoint ep;

        ClientBack clientback;
        ServerBack serverBack;

        Boolean escuchando;
        Boolean conectado;
      




        public MainWindow()
        {
            InitializeComponent();
            
            Comienzo(true);

        }






        void Comienzo(bool primero)
        {
            game = new Juego();
            nivelEscudos = 1;
            fabricaViva = true;
            minaViva = true;
            vidaFabrica = 10;
            vidaMina = 10;
            vidaFabrica2 = 10;
            vidaMina2 = 10;
            tipoDeDisparo = 1;
            escuchando = false;
            if (primero)
            {/*
                ventanaOpciones = new Opciones();
                ventanaOpciones.mandarOpciones += new Action(recibirOpciones);
                ventanaOpciones.cierraTodo += new Action(ventanaOpciones_cierraTodo);
                ventanaOpciones.Show();
              * */

                ventanaOpciones = new Opciones2();
                ventanaOpciones.mandarOpciones += new Action(recibirOpciones);
                ventanaOpciones.cierraTodo += new Action(ventanaOpciones_cierraTodo);
                ventanaOpciones.Show();

            }
        }

        void Juegue()
        {
            game.nuevoCamion += new Action<Camion>(agregarNuevoCamion);
            Cronometro aumentadorDeNivel = new Cronometro(70);
            aumentadorDeNivel.nuevoTick += new Action(aumentadorDeNivel_nuevoTick);

            Cronometro refrescador = new Cronometro(1);
            refrescador.nuevoTick += new Action(refrescador_nuevoTick);

            Cronometro avanzadorDeDisparos = new Cronometro(0, 50);
            avanzadorDeDisparos.nuevoTick += new Action(avanzadorDeDisparos_nuevoTick);

            

            /*
            Cronometro navesAttack = new Cronometro(1);
            navesAttack.nuevoTick += new Action(navesAttack_nuevoTick);
            */
            tiempoInicio = DateTime.Now;

            
        }


        void ventanaOpciones_cierraTodo()
        {
            ventanaOpciones.Close();
            this.Close();
        }

        //          ***********    SOCKETS      *********

        void recibiMensaje(string msg)
        {
            switch (msg)
            {

                case "nuevoJug":
                    enemigo = new Cañon();
                    enemigo.Hp = 7;
            
                    game.Cañon.Hp = 7;
            
                    game.Recursos = 1000;
                    imgEnemigo = new Image();
                    imgEnemigo.Source = new BitmapImage(new Uri(@"/Tarea5;component/Enemigo.png", UriKind.Relative));
                    Grid.SetColumn(imgEnemigo, 14);
                    Grid.SetRow(imgEnemigo, 3);
                    Grid.SetColumnSpan(imgEnemigo, 1);
                    Grid.SetRowSpan(imgEnemigo, 3);
                    imgEnemigo.Stretch = Stretch.Fill;
                    Jueguito.Children.Add(imgEnemigo);
                    return;
                case "el"://enemy left
                    enemigo.X--;
                    Grid.SetColumn(imgEnemigo, enemigo.X);
                    return;
                case "er":
                    enemigo.X++;
                    Grid.SetColumn(imgEnemigo, enemigo.X);
                    return;
                case "ndl": //nuevo disparo
                    disparoEnemigo("ndl");
                    return;
                case "ndm": //nuevo disparo
                    disparoEnemigo("ndm");
                    return;
                case "ndp": //nuevo disparo
                    disparoEnemigo("ndp");
                    return;
                case "nc": //nuevo camión
                    agregarNuevoCamionEnemigo();
                    return;
                case "ne1"://nuevo escudo
                    agregarNuevoEscudoEnemigo(1);
                    return;
                case "ne2":
                    agregarNuevoEscudoEnemigo(2);
                    return;
                case "ne3":
                    agregarNuevoEscudoEnemigo(3);
                    return;
                //FALTA ESCRIBIR TODO LO DEMAS

            }
            /*
            if (msg.Contains("hp"))
            {
                enemigo.Hp = 7;
                game.Cañon.Hp = 7;
            }
            int num=0;
            if (msg.Contains("re"))
                 num = msg.IndexOf("re");
                game.Recursos = 1000;
            */
        }




        void establecerServer()
        {
            try
            {
                escuchando = true;
                IPAddress localAddr = IPAddress.Loopback;
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Bind(new IPEndPoint(IPAddress.Any, 8000));
                server.Listen(3);
                

                while (escuchando)
                {

                    cliente = server.Accept();

                    escuchando = false;

                }

                
                Thread conexion = new Thread(new ThreadStart(escuchar));
                conexion.SetApartmentState(ApartmentState.STA);
                conexion.IsBackground=true;
                conexion.Start();
                 
                
                oponenteConectado();
            }

            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("Winsock error: " + e.ToString());
                Console.ReadLine();
            }

        }
        

        void escuchar()
        {
            byte[] data = new byte[256];
            int length;
            string message;


            try
            {
                while (true)
                {
                    data = new byte[256];
                    length = cliente.Receive(data);
                    message = Encoding.UTF8.GetString(data, 0, length);
                    //message = Encoding.ASCII.GetString(data, 0, length);
                    
                    this.Dispatcher.Invoke(new Action<string>(recibiMensaje),message);
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                cliente.Shutdown(SocketShutdown.Both);
                cliente.Close();
                
            }

        }

        void conectarAServer(string direccion)
        {

            IPAddress dirIpServer = IPAddress.Parse(direccion);
            ep = new IPEndPoint(dirIpServer, 8000);

            cliente = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {

                cliente.Connect(ep);
                Thread conexion = new Thread(new ThreadStart(escuchar));
            conexion.SetApartmentState(ApartmentState.STA);
            conexion.Start();
            conexion.IsBackground = true;
            conectado = true;
            Juegue();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("Winsock error: " + e.ToString());
                Console.ReadLine();
            }
            
            


        }

        public virtual void SendMessage(string msg)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                //byte[] data = Encoding.ASCII.GetBytes(message);
                cliente.Send(data);
                

            }
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("Winsock error: " + e.ToString());
                Console.ReadLine();
            }
        }

        void oponenteConectado()
        {
            conectado = true;
            SendMessage("nuevoJug");
            recibiMensaje("nuevoJug");
            espereTiempo(0.1);
            
            /*
            SendMessage("hp" + game.Cañon.Hp);    //le dice las vidas con que se juega
            espereTiempo(0.1);
            SendMessage("re" + game.Recursos);
          */
            Juegue();

        }
        
        /*
        void estableceServerBack(string direccion)
        {

            IPAddress dirLocal = IPAddress.Parse(direccion);
            serverBack = new ServerBack();
            serverBack.Start();
        }

        void estableceClientBack(string direccion)
        {
            clientback = new ClientBack();
            clientback.Start();
        }

        */




        /*
        void navesAttack_nuevoTick()
        {
            avanzarNaves();

            generarNavesConNivel();

            dispararNaves();
        }
        */

        //      ****    TICKS   ****

        void avanzadorDeDisparos_nuevoTick()
        {
            avanzarDisparos();
            verificarDisparos();
        }

        void refrescador_nuevoTick()
        {
            
            
            
            textTiempo.Text = textTiempo.Text.Substring(0, 27) + (int.Parse(textTiempo.Text.Substring(27)) + 1);
            textDinero.Text = "Dinero disponible   =   " + game.Dinero;
            textRecursos.Text = " Recursos disponibles  =   " + game.Recursos;
            textEscudo.Text = "Nivel de escudos =   " + nivelEscudos;
            textVidas.Text = "Vidas =" + game.Cañon.Hp;
            if (bApretado)
                textComprando.Text = "c0mPrAnDo...";
            else
                textComprando.Text = " ";






            numLaser.Text = ""+game.Laser;
            numMisil.Text = "" + game.Misil;
            numPlasma.Text = "" + game.Plasma;
            numNuke.Text = "" + nivelEscudos;
        }


        void aumentadorDeNivel_nuevoTick()
        {
              game.NivelJuego++;
            textLvlJuego.Text = "Nivel del juego    =   " + game.NivelJuego;
        }





        //          *********       TECLADO         **********
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            //OPCIONES DE COMPRA (si no tiene dinero suficiente no hace nada)
            if (conectado)
            {
                if (e.Key.Equals(Key.D1))
                {
                    if (bApretado)
                    {
                        game.Comprar(1);
                        bApretado = false;
                    }

                    else
                    {
                        bordeArma1.Visibility = Visibility.Visible;
                        bordeArma2.Visibility = Visibility.Hidden;
                        bordeArma3.Visibility = Visibility.Hidden;
                        bordeArma4.Visibility = Visibility.Hidden;
                        tipoDeDisparo = 1;
                        textArma.Text = "Arma que se está usando = Laser";
                    }
                }

                if (e.Key.Equals(Key.D2))
                {
                    if (bApretado)
                    {
                        game.Comprar(2);
                        bApretado = false;
                    }

                    else
                    {

                        tipoDeDisparo = 2;
                        bordeArma1.Visibility = Visibility.Hidden;
                        bordeArma2.Visibility = Visibility.Visible;
                        bordeArma3.Visibility = Visibility.Hidden;
                        bordeArma4.Visibility = Visibility.Hidden;
                        textArma.Text = "Arma que se está usando = Misil";

                    }
                }

                if (e.Key.Equals(Key.D3))
                {
                    if (bApretado)
                    {
                        game.Comprar(3);
                        bApretado = false;
                    }

                    else
                    {

                        tipoDeDisparo = 3;
                        bordeArma1.Visibility = Visibility.Hidden;
                        bordeArma2.Visibility = Visibility.Hidden;
                        bordeArma3.Visibility = Visibility.Visible;
                        bordeArma4.Visibility = Visibility.Hidden;
                        textArma.Text = "Arma que se está usando = Plasma";
                    }
                }

                if (e.Key.Equals(Key.D4))
                {
                    if (game.Dinero >= 1 && bApretado == false)
                    {
                        agregarNuevoEscudo();   //Pone un escudo
                        game.Dinero--;
                    }
                    if (game.Dinero >= 15 && bApretado && nivelEscudos < 3)
                    {
                        nivelEscudos++;         //Sube nivel de escudos
                        game.Dinero -= 50;
                        bApretado = false;
                    }
                }

                if (e.Key.Equals(Key.D0))
                {
                    if (bApretado)
                    {
                        if (fabricaViva) 
                        game.Comprar(0);
                        bApretado = false;
                    }

                }



                if (e.Key.Equals(Key.B))
                {
                    bApretado = true;
                    textComprando.Text = "c0mPrAnDo...";
                }




                //compra de escudo
                if (e.Key.Equals(Key.Up))
                {
                    if (game.Dinero >= 1 && bApretado == false)
                    {
                        agregarNuevoEscudo();   //Pone un escudo
                        game.Dinero--;
                    }
                    if (game.Dinero >= 15 && bApretado && nivelEscudos < 3)
                    {
                        nivelEscudos++;         //Sube nivel de escudos
                        game.Dinero -= 50;
                        bApretado = false;
                    }


                }



                //Movimientos

                if (e.Key.Equals(Key.Left))
                    moverIzquierda();

                if (e.Key.Equals(Key.Right))
                    moverDerecha();


                //Disparo
                if (e.Key.Equals(Key.Space))
                {
                    if (tipoDeDisparo == 1 && game.Laser > 0)
                        dispararCañon();
                    if (tipoDeDisparo == 2 && game.Misil > 0)
                        dispararCañon();
                    if (tipoDeDisparo == 3 && game.Plasma > 0)
                        dispararCañon();
                    if (tipoDeDisparo == 4 && game.Nuke > 0)
                        dispararCañon();


                }
            }
        }

       


        //Acciones de CAÑON
        private void moverDerecha()
        {
            if (game.Cañon.X < 27)
            {
                game.Cañon.X++;
                Grid.SetColumn(imgcañon, game.Cañon.X);
                SendMessage("er");
            }
            espereTiempo(0.1);
        }

        void moverIzquierda()
        {
            if (game.Cañon.X > 0)
            {
                game.Cañon.X--;
                Grid.SetColumn(imgcañon, game.Cañon.X);
                SendMessage("el");
            }
            espereTiempo(0.1);

        }


        
         


        //Otros métodos


        //OK
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
        




        //metodos naves
        /*
                //          ********        METODOS DE NAVES        ********
        //OK

        private void generarNavesConNivel()
        {
            
            
            
           
                switch (game.NivelJuego)
                {

                    case 1:
                        generarNave(1, 0.5);
                        return;

                    case 2:

                        switch (eligeAlAzar(3))
                        {
                            case 1:
                                generarNave(1, 0.3); return;
                            case 2:
                                generarNave(2, 0.3); return;
                        }
                        return;
                
                    case 3:
                        switch (eligeAlAzar(4))
                        {
                            case 1:
                                generarNave(1, 0.2); return;
                            case 2:
                                generarNave(2, 0.2); return;
                            case 3:
                                generarNave(3, 0.1); return;
                        }
                        return;

                    case 4:
                        switch (eligeAlAzar(5))
                        {
                            case 1:
                                generarNave(1, 0.1); return;
                            case 2:
                                generarNave(2, 0.2); return;
                            case 3:
                                generarNave(3, 0.2); return;
                            case 4:
                                generarNave(4, 0.1); return;
                        }
                        return;

                    case 5:
                        switch (eligeAlAzar(5))
                        {
                            case 1:
                                generarNave(2, 0.1); return;
                            case 2:
                                generarNave(3, 0.2); return;
                            case 3:
                                generarNave(4, 0.2); return;
                            case 4:
                                generarNave(5, 0.1); return;
                        }
                        return;

                    case 6:
                        switch (eligeAlAzar(6))
                        {
                            case 1:
                                generarNave(2, 0.1); return;
                            case 2:
                                generarNave(3, 0.1); return;
                            case 3:
                                generarNave(4, 0.2); return;
                            case 4:
                                generarNave(5, 0.2); return;
                            case 5:
                                generarNave(6, 0.1); return;
                        }
                        return;

                    case 7:
                        switch (eligeAlAzar(6))
                        {
                            case 1:
                                generarNave(3, 0.1); return;
                            case 2:
                                generarNave(4, 0.2); return;
                            case 3:
                                generarNave(5, 0.2); return;
                            case 4:
                                generarNave(6, 0.2); return;
                            case 5:
                                generarNave(7, 0.1); return;
                        }
                        return;

                    case 8:
                        switch (eligeAlAzar(7))
                        {
                            case 1:
                                generarNave(3, 0.1); return;
                            case 2:
                                generarNave(4, 0.1); return;
                            case 3:
                                generarNave(5, 0.1); return;
                            case 4:
                                generarNave(6, 0.2); return;
                            case 5:
                                generarNave(7, 0.2); return;
                            case 6:
                                generarNave(8, 0.1); return;
                        }
                        return;

                    case 9:
                        switch (eligeAlAzar(7))
                        {
                            case 1:
                                generarNave(4, 0.1); return;
                            case 2:
                                generarNave(5, 0.1); return;
                            case 3:
                                generarNave(6, 0.2); return;
                            case 4:
                                generarNave(7, 0.2); return;
                            case 5:
                                generarNave(8, 0.2); return;
                            case 6:
                                generarNave(9, 0.1); return;
                        }
                        return;

                    case 10:
                        switch (eligeAlAzar(7))
                        {
                            case 1:
                                generarNave(5, 0.1); return;
                            case 2:
                                generarNave(6, 0.1); return;
                            case 3:
                                generarNave(7, 0.2); return;
                            case 4:
                                generarNave(8, 0.2); return;
                            case 5:
                                generarNave(9, 0.2); return;
                            case 6:
                                generarNave(10, 0.1); return;
                        }
                        return;

                    default:

                        switch (eligeAlAzar(7))
                        {
                            case 1:
                                generarNave(5, 0.1); return;
                            case 2:
                                generarNave(6, 0.1); return;
                            case 3:
                                generarNave(7, 0.2); return;
                            case 4:
                                generarNave(8, 0.2); return;
                            case 5:
                                generarNave(9, 0.2); return;
                            case 6:
                                generarNave(10, 0.2); return;
                        }
                        return;
                }
            
            
        }

        //OK SOLO FALTA GENERAL CON DISTINTAS IMGS
        private void generarNave(int nivel, double probabilidad)
        {
           
            Random r = new Random();
            double azar=r.NextDouble();

            if (ImgsToNaves.Count == 0)
            {
                if (probabilidad >= azar)
                {
                    game.Naves.Add(new Nave(nivel));
                    Image nuevaImgNave = new Image();

                    ImgsToNaves.Add(nuevaImgNave, game.Naves[game.Naves.Count - 1]);
                    NavesToImgs[game.Naves.Last()] = nuevaImgNave;

                    nuevaImgNave.Source = new BitmapImage(new Uri(@"/Tarea5;component/nave2.png", UriKind.Relative));

                    Jueguito.Children.Add(nuevaImgNave);

                    Grid.SetColumn(nuevaImgNave, 0);
                    Grid.SetRow(nuevaImgNave, 0);
                    Grid.SetColumnSpan(nuevaImgNave, 2);
                    Grid.SetRowSpan(nuevaImgNave, 3);
                    nuevaImgNave.VerticalAlignment = VerticalAlignment.Bottom;
                    nuevaImgNave.HorizontalAlignment = HorizontalAlignment.Right;
                }
            }
            else
            {
                if (probabilidad >= azar && !(game.Naves.Last().X < 2 && game.Naves.Last().Y == 0)) 
                {
                    //Lanzamos nueva nave
                    game.Naves.Add(new Nave(nivel));
                    Image nuevaImgNave = new Image();

                    ImgsToNaves.Add(nuevaImgNave, game.Naves[game.Naves.Count - 1]);
                    NavesToImgs.Add(game.Naves[game.Naves.Count - 1], nuevaImgNave);

                    nuevaImgNave.Source = new BitmapImage(new Uri(@"/Tarea5;component/nave2.png", UriKind.Relative));

                    Jueguito.Children.Add(nuevaImgNave);

                    Grid.SetColumn(nuevaImgNave, 0);
                    Grid.SetRow(nuevaImgNave, 0);
                    Grid.SetColumnSpan(nuevaImgNave, 2);
                    Grid.SetRowSpan(nuevaImgNave, 3);
                    nuevaImgNave.VerticalAlignment = VerticalAlignment.Bottom;
                    nuevaImgNave.HorizontalAlignment = HorizontalAlignment.Right;


                }
            }
           
        }

        private int eligeAlAzar(int max)
        {
            int x;
            Random r = new Random();
            int respuesta = r.Next(1, max);
            if (respuesta != 1) 
                x=0;
            return respuesta;
        }


        //OK 
        private void avanzarNaves()
        {
           
            //MOVEMOS CADA NAVE
            for (int i = 0; i < game.Naves.Count; i++)
            {
                if (game.Naves[i].Y >= 21)
                {
                    MessageBox.Show("Usted Ha Perdido el JUEGOOO");
                    for (int j = 0; j < threadsCamiones.Count; j++)
                    {
                        threadsCamiones[j].Abort();
                    }
                    
                    
                    this.Close();
                }


                //Mover a la derecha si se puede
                if (game.Naves[i].Sentido == 0 && game.Naves[i].X != 26)
                {
                    Grid.SetColumn(NavesToImgs[game.Naves[i]], game.Naves[i].X + 1);
                    game.Naves[i].X++;
                }

                //Mover a la izquierda si se puede
                if (game.Naves[i].Sentido == 1 && game.Naves[i].X != 0)
                {
                    Grid.SetColumn(NavesToImgs[game.Naves[i]], game.Naves[i].X - 1);
                    game.Naves[i].X--;
                }

                //Mover abajo, cambiar sentido a izquierda
                if (game.Naves[i].X == 26)
                {
                    game.Naves[i].Sentido = 1;
                    Grid.SetRow(NavesToImgs[game.Naves[i]], game.Naves[i].Y + 3);
                    game.Naves[i].Y += 3;
                }

                //Mover abajo cambiar sentido a derecha
                if (game.Naves[i].X == 0)
                {
                    game.Naves[i].Sentido = 0;
                    Grid.SetRow(NavesToImgs[game.Naves[i]], game.Naves[i].Y + 3);
                    game.Naves[i].Y += 3;
                }
            }

        }




        */



        //METODOS   ****************        CAMIONEROS          *********************


        //OK
        public void avanzarUnCamion(object imgcamiono)
        {

            
                Image imgcamion = imgcamiono as Image;
                Camion c = ImgsToCamiones[imgcamion];
                while (c.sigue)
                {

                    Thread.Sleep(1000);
                    if (c.Sentido == 1) //1 significa izq, 0 der.
                    {
                        if (c.X == 3) //fin de la mina
                        {
                            
                            this.Dispatcher.Invoke(new Action<Image,string>(cambiarImagen),imgcamion, @"/Tarea5;component/CamioncitoDer.png");
                            c.Sentido = 0;
                            //Cambia a la derecha
                        }

                        else if (c.X == 4)
                        {
                            //FILA DE ESPERA PARA RECOGER EN LA MINA

                            Monitor.Enter(o); 
                            
                                  c.X--;
                            
                                this.Dispatcher.Invoke(new Action<UIElement, int>(setearColumna), imgcamion, 3);
                                Thread.Sleep(2000) ;
                            if(minaViva && fabricaViva)
                                game.Recursos -= 5;
                            
                            Monitor.Exit(o);
                        }

                        else
                        {
                            c.X--;
                            this.Dispatcher.Invoke(new Action<UIElement, int>(setearColumna), imgcamion, c.X - 1);
                        }
                    }

                    else        //sentido hacia DERECHA
                    {
                        if (c.X == 24)
                        {
                            this.Dispatcher.Invoke(new Action<Image, string>(cambiarImagen), imgcamion, @"/Tarea5;component/CamioncitoIzq.png");
                            if(minaViva && fabricaViva)
                            game.Dinero += 5;
                            c.Sentido = 1;
                        }

                        else
                        {
                            c.X++;
                            this.Dispatcher.Invoke(new Action<UIElement, int>(setearColumna), imgcamion, c.X + 1);
                        }

                    }
                    

                }//Fin de whiletrue

        }

        public void avanzarUnCamionEnemigo(object imgcamiono)
        {


            Image imgcamion = imgcamiono as Image;
            Camion c = ImgsToCamiones[imgcamion];
            while (c.sigue)
            {

                Thread.Sleep(1000);
                if (c.Sentido == 1) //1 significa izq, 0 der.
                {
                    if (c.X == 3) //fin de la mina
                    {

                        this.Dispatcher.Invoke(new Action<Image, string>(cambiarImagen), imgcamion, @"/Tarea5;component/CamioncitoDer.png");
                        c.Sentido = 0;
                        //Cambia a la derecha
                    }

                    else if (c.X == 4)
                    {
                        //FILA DE ESPERA PARA RECOGER EN LA MINA

                        Monitor.Enter(o);

                        c.X--;

                        this.Dispatcher.Invoke(new Action<UIElement, int>(setearColumna), imgcamion, 3);
                        Thread.Sleep(2000);

                        Monitor.Exit(o);
                    }

                    else
                    {
                        c.X--;
                        this.Dispatcher.Invoke(new Action<UIElement, int>(setearColumna), imgcamion, c.X - 1);
                    }
                }

                else        //sentido hacia DERECHA
                {
                    if (c.X == 24)
                    {
                        this.Dispatcher.Invoke(new Action<Image, string>(cambiarImagen), imgcamion, @"/Tarea5;component/CamioncitoIzq.png");
                       
                        c.Sentido = 1;
                    }

                    else
                    {
                        c.X++;
                        this.Dispatcher.Invoke(new Action<UIElement, int>(setearColumna), imgcamion, c.X + 1);
                    }

                }


            }//Fin de whiletrue

        }



        //OK
        private void agregarNuevoCamion(Camion c)
        {

            SendMessage("nc"); //nuevo camion
            Image nuevoCamion = new Image();
            nuevoCamion.Source = new BitmapImage(new Uri(@"/Tarea5;component/CamioncitoIzq.png", UriKind.Relative));
            
            Jueguito.Children.Add(nuevoCamion);

            Grid.SetColumn(nuevoCamion, 24);
            Grid.SetRow(nuevoCamion, 26);
            Grid.SetColumnSpan(nuevoCamion, 1);
            Grid.SetRowSpan(nuevoCamion, 2);
            nuevoCamion.VerticalAlignment = VerticalAlignment.Bottom;

            ImgsToCamiones.Add(nuevoCamion, c);
            CamionesToImgs.Add(c, nuevoCamion);
            threadsCamiones.Add(new Thread( new ParameterizedThreadStart(avanzarUnCamion) ) );

            game.Camiones.Add(c);

            
            threadsCamiones.Last().Start(nuevoCamion);
            
            
        }

        private void agregarNuevoCamionEnemigo()
        {

            Camion c = new Camion();
            Image nuevoCamion = new Image();
            nuevoCamion.Source = new BitmapImage(new Uri(@"/Tarea5;component/CamioncitoIzq.png", UriKind.Relative));

            Jueguito.Children.Add(nuevoCamion);

            Grid.SetColumn(nuevoCamion, 24);
            Grid.SetRow(nuevoCamion, 0);
            Grid.SetColumnSpan(nuevoCamion, 1);
            Grid.SetRowSpan(nuevoCamion, 2);
            nuevoCamion.VerticalAlignment = VerticalAlignment.Bottom;

            ImgsToCamiones.Add(nuevoCamion, c);
            CamionesToImgs.Add(c, nuevoCamion);
            threadsCamiones.Add(new Thread(new ParameterizedThreadStart(avanzarUnCamionEnemigo)));

            game.Camiones.Add(c);


            threadsCamiones.Last().Start(nuevoCamion);

        }






        //Dispatcher.Invoke
        void setearColumna(UIElement u, int i)
        {
            Grid.SetColumn(u, i);
        }

        void cambiarImagen(Image i, string s)
        {

            i.Source = new BitmapImage(new Uri(s, UriKind.Relative));
        }

        void agregarImagen(Image i)
        {
            Jueguito.Children.Add(i);
        }



        //METODOS SOBRE         ********            DISPAROS        ***************

        private void verificarDisparos()
        {
            
      
            //PUEDE QUE ESTO NO SEA NECESARIO

            //disparos arriba
            for (int i = 0; i < disparosArriba.Count; i++) 
            {
                bool volver = true; //esto está al reves debería ser no volver pero filo.. funciona...
                if (volver)
                {
                    if (disparosArriba[i].X == enemigo.X && disparosArriba[i].Y == 5)
                    {
                        enemigo.Hp -= disparosArriba[i].Daño;
                        if (enemigo.Hp < 1)
                            destruirEnemigo();

                        Jueguito.Children.Remove(BalasToImgsArriba[disparosArriba[i]]);
                        ImgsToBalasArriba.Remove(BalasToImgsArriba[disparosArriba[i]]);
                        BalasToImgsArriba.Remove(disparosArriba[i]);
                        disparosArriba.Remove(disparosArriba[i]);
                        i--;
                        volver = false;
                        break;
                    }
                }

                /*
                for (int j = 0; j < game.Naves.Count; j++)
                {
                    //Verificar navecitas
                    if (Math.Abs(game.Naves[j].X - disparosArriba[i].X) < 2 && game.Naves[j].Y + 1 == disparosArriba[i].Y
                        && disparosArriba[i].TipoDeDisparo!=Tarea5.Disparo.tipoDisparo.Nuke)
                    {
                        game.Naves[j].Hp -= disparosArriba[i].Daño;
                        if (game.Naves[j].Hp <= 0)
                             destruirNave(game.Naves[j]);
                        Jueguito.Children.Remove(BalasToImgsArriba[disparosArriba[i]]);
                        ImgsToBalasArriba.Remove(BalasToImgsArriba[disparosArriba[i]]);
                        BalasToImgsArriba.Remove(disparosArriba[i]);
                        disparosArriba.Remove(disparosArriba[i]);
                        i--;
                        break;
                    }
                    if (Math.Abs(game.Naves[j].X - disparosArriba[i].X) < 2 && game.Naves[j].Y + 1 == disparosArriba[i].Y && 
                        disparosArriba[i].TipoDeDisparo == Tarea5.Disparo.tipoDisparo.Nuke)
                    {
                        for (int k = 0; k < game.Naves.Count;)
                        {
                            destruirNave(game.Naves[game.Naves.Count - 1]);
                        }

                        Jueguito.Children.Remove(BalasToImgsArriba[disparosArriba[i]]);
                        ImgsToBalasArriba.Remove(BalasToImgsArriba[disparosArriba[i]]);
                        BalasToImgsArriba.Remove(disparosArriba[i]);
                        disparosArriba.Remove(disparosArriba[i]);
                        break;

                    }
                    

                }
                  */




                if (volver)
                {
                    for (int j = 0; j < game.Camiones.Count; j++)
                    {
                        //verificar camioncitos
                        if (1 == disparosArriba[i].Y && game.Camiones[j].X == disparosArriba[i].X)
                        {


                            destruirCamion(CamionesToImgs[game.Camiones[j]]);

                            Jueguito.Children.Remove(BalasToImgsArriba[disparosArriba[i]]);
                            ImgsToBalasArriba.Remove(BalasToImgsArriba[disparosArriba[i]]);
                            BalasToImgsArriba.Remove(disparosArriba[i]);
                            disparosArriba.Remove(disparosArriba[i]);
                            i--;
                            volver = false;
                            break;
                        }
                    }
                }

                if (volver)
                {

                    for (int j = 0; j < escudosEnemigos.Count; j++)
                    {//Verificar escudos

                        if (disparosArriba[i].Y == escudosEnemigos[j].Y && disparosArriba[i].X == escudosEnemigos[j].X)
                        {
                            escudosEnemigos[j].Hp-=disparosArriba[i].Daño;
                            if (escudosEnemigos[j].Hp < 1)
                                destruirEscudo(escudosEnemigos[j],false);

                            Jueguito.Children.Remove(BalasToImgsArriba[disparosArriba[i]]);
                            ImgsToBalasArriba.Remove(BalasToImgsArriba[disparosArriba[i]]);
                            BalasToImgsArriba.Remove(disparosArriba[i]);
                            disparosArriba.Remove(disparosArriba[i]);
                            i--;
                            volver = false;
                            break;
                        }
                    }

                }

                if (volver)
                {
                    if (disparosArriba[i].X >= 25 && disparosArriba[i].Y == 3)
                    {
                        vidaFabrica2 -= disparosArriba[i].Daño;
                        if (vidaFabrica2 < 1)
                        
                            
                            Jueguito.Children.Remove(fabric2);
                        
                        Jueguito.Children.Remove(BalasToImgsArriba[disparosArriba[i]]);
                        ImgsToBalasArriba.Remove(BalasToImgsArriba[disparosArriba[i]]);
                        BalasToImgsArriba.Remove(disparosArriba[i]);
                        disparosArriba.Remove(disparosArriba[i]);
                        i--;
                        volver = false;
                        break;
                    }
                }

                if (volver)
                {
                    if (disparosArriba[i].X <= 2 && disparosArriba[i].Y == 3)
                    {
                        vidaMina2 -= disparosArriba[i].Daño;
                        if (vidaMina2 < 1)
                        
                            Jueguito.Children.Remove(mine2);
                           
                        Jueguito.Children.Remove(BalasToImgsArriba[disparosArriba[i]]);
                        ImgsToBalasArriba.Remove(BalasToImgsArriba[disparosArriba[i]]);
                        BalasToImgsArriba.Remove(disparosArriba[i]);
                        disparosArriba.Remove(disparosArriba[i]);
                        i--;
                        volver = false;
                        break;
                    }
                }


            }

            //disparos abajo *****************************************************************************
            
            for (int i = 0; i < disparosAbajo.Count; i++)
            {
                bool volver = true;

                

                    for (int j = 0; j < game.Camiones.Count; j++)
                    {
                        //verificar camioncitos
                        if (27 == disparosAbajo[i].Y && game.Camiones[j].X == disparosAbajo[i].X)
                        {


                            destruirCamion(CamionesToImgs[game.Camiones[j]]);

                            Jueguito.Children.Remove(BalasToImgsAbajo[disparosAbajo[i]]);
                            ImgsToBalasAbajo.Remove(BalasToImgsAbajo[disparosAbajo[i]]);
                            BalasToImgsAbajo.Remove(disparosAbajo[i]);
                            disparosAbajo.Remove(disparosAbajo[i]);
                            i--;
                            volver = false;
                            break;
                        }
                    }

                    if (volver)
                    {

                        for (int j = 0; j < escudos.Count; j++)
                        {//Verificar escudos

                            if (disparosAbajo[i].Y == escudos[j].Y && disparosAbajo[i].X == escudos[j].X)
                            {
                                escudos[j].Hp -= disparosAbajo[i].Daño;
                                if(escudos[j].Hp < 1)
                                destruirEscudo(escudos[j],true);

                                Jueguito.Children.Remove(BalasToImgsAbajo[disparosAbajo[i]]);
                                ImgsToBalasAbajo.Remove(BalasToImgsAbajo[disparosAbajo[i]]);
                                BalasToImgsAbajo.Remove(disparosAbajo[i]);
                                disparosAbajo.Remove(disparosAbajo[i]);
                                i--;
                                volver = false;
                                break;
                            }
                        }

                    }

                    if (volver)
                    {
                        if (disparosAbajo[i].X >= 25 && disparosAbajo[i].Y == 25)
                        {
                            vidaFabrica -= disparosAbajo[i].Daño;
                            if (vidaFabrica < 1)
                                destruirFabrica();

                            Jueguito.Children.Remove(BalasToImgsAbajo[disparosAbajo[i]]);
                            ImgsToBalasAbajo.Remove(BalasToImgsAbajo[disparosAbajo[i]]);
                            BalasToImgsAbajo.Remove(disparosAbajo[i]);
                            disparosAbajo.Remove(disparosAbajo[i]);
                            i--;
                            volver = false;
                            break;
                        }
                    }

                    if (volver)
                    {
                        if (disparosAbajo[i].X <= 2 && disparosAbajo[i].Y == 25)
                        {
                            vidaMina -= disparosAbajo[i].Daño;
                            if (vidaMina < 1)
                                destruirMina();

                            Jueguito.Children.Remove(BalasToImgsAbajo[disparosAbajo[i]]);
                            ImgsToBalasAbajo.Remove(BalasToImgsAbajo[disparosAbajo[i]]);
                            BalasToImgsAbajo.Remove(disparosAbajo[i]);
                            disparosAbajo.Remove(disparosAbajo[i]);
                            i--;
                            volver = false;
                            break;
                        }
                    }

                    if (volver)
                    {
                        if (disparosAbajo[i].X == game.Cañon.X && disparosAbajo[i].Y == 22)
                        {
                            game.Cañon.Hp -= disparosAbajo[i].Daño;
                            if (game.Cañon.Hp < 1)
                                destruirCañon();

                            Jueguito.Children.Remove(BalasToImgsAbajo[disparosAbajo[i]]);
                            ImgsToBalasAbajo.Remove(BalasToImgsAbajo[disparosAbajo[i]]);
                            BalasToImgsAbajo.Remove(disparosAbajo[i]);
                            disparosAbajo.Remove(disparosAbajo[i]);
                            i--;
                            volver = false;
                            break;
                        }
                    }




            }

            
        }


        private void destruirCamion(Image img)
        {
            

            Jueguito.Children.Remove(img);

            Camion c = ImgsToCamiones[img];
            int indice = game.Camiones.IndexOf(c);
            threadsCamiones.Remove(threadsCamiones[indice]);
            c.sigue = false;

            CamionesToImgs.Remove(c);

            ImgsToCamiones.Remove(img);
            

            game.Camiones.Remove(c);
           
        }

        void destruirNave(Nave nave)
        {
            

            Jueguito.Children.Remove(NavesToImgs[nave]);
           
            ImgsToNaves.Remove(NavesToImgs[nave]);
            NavesToImgs.Remove(nave);
            
            game.Naves.Remove(nave);

        }

        void destruirEscudo(Escudo escudo, bool mio)
        {

            Jueguito.Children.Remove(EscudosToImgs[escudo]);

            
            EscudosToImgs.Remove(escudo);
            if (mio)
                escudos.Remove(escudo);
            else
                escudosEnemigos.Remove(escudo);
        }

        void destruirFabrica()
        {
            Jueguito.Children.Remove(fabric);
            fabricaViva = false;
        }

        void destruirMina()
        {
            Jueguito.Children.Remove(mine);
            minaViva = false;
        }

        void destruirEnemigo()
        {
            Jueguito.Children.Remove(imgEnemigo);
            MessageBox.Show("ERES UN WINEEEER");
            espereTiempo(0.5);
            this.Close();
        }
        
        void destruirCañon()
        {
            Jueguito.Children.Remove(imgcañon);
            MessageBox.Show("ERES UN LOSEEEER");
            espereTiempo(0.5);
            this.Close();
        }


        private void dispararCañon()
        {
            
            Disparo bala = new Disparo();
            bala.X = game.Cañon.X;
            bala.Y = 21;
            Image balita = new Image();
            Jueguito.Children.Add(balita);
            Grid.SetColumn(balita, game.Cañon.X);

            //Establecer el tipo de bala
            if (tipoDeDisparo == 1)
            {
                SendMessage("ndl");
                bala.TipoDeDisparo = Tarea5.Disparo.tipoDisparo.Laser;
                balita.Source = new BitmapImage(new Uri(@"/Tarea5;component/disparo.png", UriKind.Relative));

                Grid.SetRow(balita, 21);
                game.Laser--;

            }
            if (tipoDeDisparo == 2)
            {
                SendMessage("ndm");
                bala.TipoDeDisparo = Tarea5.Disparo.tipoDisparo.Misil;
                balita.Source = new BitmapImage(new Uri(@"/Tarea5;component/Misil.png", UriKind.Relative));
                balita.Stretch = Stretch.None;
                Grid.SetRow(balita, 20);
                Grid.SetColumnSpan(balita, 1);
                Grid.SetRowSpan(balita, 2);

                game.Misil--;
            }
            if (tipoDeDisparo == 3)
            {
                SendMessage("ndp");
                bala.TipoDeDisparo = Tarea5.Disparo.tipoDisparo.Plasma;
                balita.Source = new BitmapImage(new Uri(@"/Tarea5;component/plasma.png", UriKind.Relative));
                
                Grid.SetRow(balita, 20);
                Grid.SetColumnSpan(balita, 2);
                Grid.SetRowSpan(balita, 2);

                game.Plasma--;
            }

            //AQUI SOLO DESTRUIMOS TODO XD
            if (tipoDeDisparo == 4)
            {
                bala.TipoDeDisparo = Tarea5.Disparo.tipoDisparo.Nuke;
                balita.Source = new BitmapImage(new Uri(@"/Tarea5;component/Nuke.png", UriKind.Relative));
                
                Grid.SetRow(balita, 20);
                Grid.SetColumnSpan(balita, 2);
                Grid.SetRowSpan(balita, 2);
                
                game.Nuke--;
                //falta ponerlo
            }
            
            

            

            BalasToImgsArriba.Add(bala, balita);
            ImgsToBalasArriba.Add(balita, bala);
            disparosArriba.Add(bala);
            

        }

        private void disparoEnemigo(string tipo)
        {

            Disparo bala = new Disparo();
            bala.X = enemigo.X;
            bala.Y = 6;
            Image balita = new Image();
            Jueguito.Children.Add(balita);
            Grid.SetColumn(balita, bala.X);

            //Establecer el tipo de bala
            if (tipo=="ndl")
            {
                bala.TipoDeDisparo = Tarea5.Disparo.tipoDisparo.Laser;
                balita.Source = new BitmapImage(new Uri(@"/Tarea5;component/disparo.png", UriKind.Relative));

                Grid.SetRow(balita, 6);
                
            }
            if (tipo=="ndm")
            {
                bala.TipoDeDisparo = Tarea5.Disparo.tipoDisparo.Misil;
                balita.Source = new BitmapImage(new Uri(@"/Tarea5;component/Misil.png", UriKind.Relative));
                balita.Stretch = Stretch.None;
                Grid.SetRow(balita, 6);
                Grid.SetColumnSpan(balita, 1);
                Grid.SetRowSpan(balita, 2);

                
            }
            if (tipo=="ndp")
            {
                bala.TipoDeDisparo = Tarea5.Disparo.tipoDisparo.Plasma;
                balita.Source = new BitmapImage(new Uri(@"/Tarea5;component/plasma.png", UriKind.Relative));

                Grid.SetRow(balita, 6);
                Grid.SetColumnSpan(balita, 2);
                Grid.SetRowSpan(balita, 2);

                
            }

            BalasToImgsAbajo.Add(bala, balita);
            ImgsToBalasAbajo.Add(balita, bala);
            disparosAbajo.Add(bala);

        }

        private void dispararNaves()
        {

            for (int i = 0; i < game.Naves.Count; i++)
            {
                Nave nave = game.Naves[i];
                for (int j = 0; j < game.Camiones.Count; j++)
                {

                    if (nave.X == game.Camiones[j].X)
                        dispararNave(nave);

                }
                
                if (game.Cañon.X == nave.X)
                    dispararNave(nave);
            }

        }

        //OK
        private void dispararNave(Nave nave)
        {
            Disparo bala = new Disparo();
            bala.X = nave.X;
            bala.Y = nave.Y+2;
            Image balita = new Image();
            Jueguito.Children.Add(balita);
            Grid.SetColumn(balita, bala.X);
            Grid.SetRow(balita, bala.Y);

            balita.Source = new BitmapImage(new Uri(@"/Tarea5;component/disparo2.png", UriKind.Relative));


            BalasToImgsAbajo.Add(bala, balita);
            ImgsToBalasAbajo.Add(balita, bala);
            disparosAbajo.Add(bala);

        }



        void avanzarDisparos()
        {
            //Avanzar hacia arriba
            for (int i = 0; i < disparosArriba.Count; i++)
            {
                Disparo bala = disparosArriba[i];
                Image img = BalasToImgsArriba[bala];

                if (bala.Y == 0)
                {
                    disparosArriba.Remove(bala);
                    BalasToImgsArriba.Remove(bala);
                    ImgsToBalasArriba.Remove(img);
                    Jueguito.Children.Remove(img);
                }
                else
                {
                    Grid.SetRow(img, bala.Y - 1);
                    bala.Y--;
                }
                
            }

            //Hacia abajo
            for (int i = 0; i < disparosAbajo.Count; i++)
            {
                Disparo bala = disparosAbajo[i];
                Image img = BalasToImgsAbajo[bala];

                if (bala.Y == 27)
                {
                    disparosAbajo.Remove(bala);
                    BalasToImgsAbajo.Remove(bala);
                    ImgsToBalasAbajo.Remove(img);
                    Jueguito.Children.Remove(img);
                }
                else
                {
                    Grid.SetRow(img, bala.Y + 1);
                    bala.Y++;
                }


            }
        }

        void explosion(Image imagen, int i)
        {
            imagen.Source = new BitmapImage(new Uri(@"/Tarea5;component/explosion"+i+".png", UriKind.Relative));
            //no funciona

        }



        //      ********************            ESCUDOS         *****************

        private void agregarNuevoEscudo()
        {

            int y = eligeDondePonerEscudo();
            if (y > 18)
            {
                Escudo nuevoEscudo = new Escudo(game.Cañon.X, y);
                nuevoEscudo.Hp = nivelEscudos;
                escudos.Add(nuevoEscudo);

                Image img = new Image();
                img.Source = new BitmapImage(new Uri(@"/Tarea5;component/Shield.png", UriKind.Relative));
                if (nuevoEscudo.Hp == 1)
                    SendMessage("ne1");
                if (nuevoEscudo.Hp == 2)
                    SendMessage("ne2");
                if (nuevoEscudo.Hp == 3)
                    SendMessage("ne3");

                Jueguito.Children.Add(img);

                Grid.SetColumn(img, nuevoEscudo.X);
                Grid.SetRow(img, nuevoEscudo.Y);
                Grid.SetColumnSpan(img, 1);
                Grid.SetRowSpan(img, 1);
                img.Stretch = Stretch.Fill;


                EscudosToImgs.Add(nuevoEscudo, img);

            }

        }

        private void agregarNuevoEscudoEnemigo(int nivel)
        {
            int y = eligeDondePonerEscudoEnemigo();
            if (y < 10)
            {
                Escudo nuevoEscudo = new Escudo(enemigo.X, y);
                nuevoEscudo.Hp = nivel;
                escudosEnemigos.Add(nuevoEscudo);

                Image img = new Image();
                img.Source = new BitmapImage(new Uri(@"/Tarea5;component/Shield.png", UriKind.Relative));

                Jueguito.Children.Add(img);

                Grid.SetColumn(img, nuevoEscudo.X);
                Grid.SetRow(img, nuevoEscudo.Y);
                Grid.SetColumnSpan(img, 1);
                Grid.SetRowSpan(img, 1);
                img.Stretch = Stretch.Fill;


                EscudosToImgs.Add(nuevoEscudo, img);
            }


        }



        private int eligeDondePonerEscudo()
        {
            //Pone el escudo en la parte más alta de la torre de escudos que haya en esa posX (la del cañon en ese momento)
            int posY = 22;
            for (int i = 0; i < escudos.Count; i++) 
            {
                if (escudos[i].X == game.Cañon.X)
                {
                    if (escudos[i].Y < posY)
                        posY = escudos[i].Y;

                }
            
            }

            posY--;

            return posY;

        }

        private int eligeDondePonerEscudoEnemigo()
        {
            //Pone el escudo en la parte más alta de la torre de escudos que haya en esa posX (la del cañon en ese momento)
            int posY = 5;
            for (int i = 0; i < escudosEnemigos.Count; i++)
            {
                if (escudosEnemigos[i].X == enemigo.X)
                {
                    if (escudosEnemigos[i].Y > posY)
                        posY = escudosEnemigos[i].Y;

                }

            }

            posY++;

            return posY;

        }


        private void recibirOpciones()
        {
            /*
            game.Recursos = ventanaOpciones.Recursos;
            game.Cañon.Hp = ventanaOpciones.Vidas;
             * */
            if (ventanaOpciones.hoster)
            {
                
                /*Thread servidor = new Thread(new ThreadStart(establecerServer));
                servidor.SetApartmentState(ApartmentState.STA);
                servidor.IsBackground = true;
                servidor.Start();
                 */
           
                establecerServer();
            }
            else
            {
                conectarAServer(ventanaOpciones.ip);
            }
            ventanaOpciones.Close();
            
        }

        

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        

       
    }
}
