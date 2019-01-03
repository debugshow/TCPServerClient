using System;
using System.Text;
using System.Threading;

namespace TCPClientServer
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Is Create Server (yes|no):");
            string isCreateServer=Console.ReadLine();
            if (isCreateServer == "yes") {
                //Create Server Thread
                TCPServer server = new TCPServer();
                Thread threadServer = new Thread(new ThreadStart(server.TCP_Server));
                threadServer.IsBackground = true;
                threadServer.Start();
            }
            //Create Client Thread isn't BackGround Thread
            TCPClient client = new TCPClient();
            Thread threadClient = new Thread(new ThreadStart(client.TCP_Client));
            //threadClient.IsBackground = true;
            threadClient.Start();
        }
    }
}
