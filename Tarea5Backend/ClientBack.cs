using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Tarea5Backend
{
    public class ClientBack : NetworkEntity
    {

        public ClientBack()
        {

        }

        public override void Start()
        {
            IPAddress direc = IPAddress.Loopback;
            IPEndPoint Ep = new IPEndPoint(direc, 8000);
            //escogemos un puerto

            requester = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //AddressFamily.InterNetwork -> IPV4


            try
            {
                requester.Connect(Ep);

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
