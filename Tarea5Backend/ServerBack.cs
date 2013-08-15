using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Tarea5Backend
{
    public class ServerBack : NetworkEntity
    {
        public ServerBack()
        {

        }

        public override void Start()
        {
            IPEndPoint Ep = new IPEndPoint(IPAddress.Loopback, 8000);
            //escogemos un puerto

            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(Ep);
                listener.Listen(300);//

                requester = null;
                OnMessageReceived("??");
                requester = listener.Accept();

                OnMessageReceived("He aceptado");

                Thread thread = new Thread(new ThreadStart(ConnectionProc));
                thread.Start();


            }
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("Winsock error: " + e.ToString());
                Console.ReadLine();
            }
        }
    }
}
