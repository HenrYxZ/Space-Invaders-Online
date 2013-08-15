using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Tarea5Backend
{
    public abstract class NetworkEntity
    {

        protected Socket listener;
        protected Socket requester;

        protected string message;
        protected byte[] data;
        protected int length;

        public event Action<string> MessageReceived;
        protected virtual void OnMessageReceived(string msg)
        {
            if (MessageReceived != null)
                MessageReceived(msg);
        }
        public abstract void Start();
        public virtual void SendMessage(string msg)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                //byte[] data = Encoding.ASCII.GetBytes(message);
                requester.Send(data);
                OnMessageReceived(msg);

            }
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("Winsock error: " + e.ToString());
                Console.ReadLine();
            }
        }

        protected virtual void ConnectionProc()
        {
            try
            {
                while (true)
                {
                    data = new byte[256];
                    length = requester.Receive(data);
                    message = Encoding.UTF8.GetString(data, 0, length);
                    //message = Encoding.ASCII.GetString(data, 0, length);
                    OnMessageReceived(message);
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                requester.Shutdown(SocketShutdown.Both);
                requester.Close();
                OnMessageReceived("cerrando");
            }
        }

    }
}
