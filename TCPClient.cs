using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;

namespace TCPClientServer
{
    public class TCPClient
    {
        private static byte[] buffer = new Byte[1024 * 1024];
        //TCP 客户端
        private static Socket clientSocket;
        //UDP 客户端
        //private static UdpClient udpClient = new UdpClient();
        //客户端信息
        private static List<ClientServer> csList = new List<ClientServer>();
        //与UDP Client互传信息
        private static ClientServer cs = new ClientServer();
        public void TCP_Client()
        {
            byte[] plainData = Encoding.UTF8.GetBytes("China");
            //创建RSACryptoServiceProvider实例，以自动生成公钥对并保存之
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            //set Init Public Key
            cs.clientRSAPublicPar=rsaProvider.ExportParameters(false);
            //导出参数信息，包含私钥
            RSAParameters rsaPrivateParams = rsaProvider.ExportParameters(true);
            rsaProvider.Clear();
            //使用私钥对消息进行签名
            SHA1CryptoServiceProvider hashProvider = new SHA1CryptoServiceProvider();
            //保存
            //cs.hashProvider = hashProvider;
            //保存签名
            cs.rsaSignData=RSA_C_S.RsaSignData(plainData, rsaPrivateParams, hashProvider);
            while (true)
            {
                Console.WriteLine("0 Create TCP Client & Connect Server.");
                Console.WriteLine("1 ExChange Key & Put Key To Server Then Get List.");
                Console.WriteLine("2 Talk To Another Client.");
                Console.WriteLine("3 Exit!");
                int number = Convert.ToInt32(Console.ReadLine());
                switch (number)
                {
                    case 0:
                        ConnectServer();
                        break; ;
                    case 1:
                        ReceiveServerList();
                        break;
                    case 2:
                        ClientToClient();
                        break;
                    case 3:
                        ClientClose();
                        break;
                }
            }
        }
        //Connect Server
        private static void ConnectServer()
        {
            ///Open anther terminal
            Console.WriteLine("Please Input Server IP:");
            string serverip = Console.ReadLine();
            Console.WriteLine("PLease Input Server Port:");
            int serverPort = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Server Set Above!");
            IPAddress serverIP = IPAddress.Parse(serverip);
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(new IPEndPoint(serverIP, serverPort));
                Console.WriteLine("Connect Server Sucess!");
            }
            catch
            {
                Console.WriteLine("Error,Please Check IP or Port!");
                return;
            }
        }
        //Servered Client List
        private static void ReceiveServerList()
        {
            IPAddress localIP = IPAddress.Parse("127.0.0.1");
            //设置UDP 端口
            cs.listenport = (int)60000;
           IPEndPoint localEndPoint = new IPEndPoint(localIP, cs.listenport);
            //设置Udp IP & Port
            cs.udpIPPort = localEndPoint;
            //设置TCPip地址和端口
            cs.tcpIPPort=clientSocket.LocalEndPoint as IPEndPoint;
            //添加公钥
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            //导出参数信息，包含公钥
            RSAParameters rsaPublicParams = rsaProvider.ExportParameters(false);
            RSAParameters rsaPrivateParams = rsaProvider.ExportParameters(true);
            cs.clientRSAPublicPar=rsaPublicParams;
            rsaProvider.Clear();
            //保存私钥
            RSAUserPrivateparam.setRSAPrivatePara(rsaPrivateParams);
            MemoryStream mStream = new MemoryStream();
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(mStream, cs);
            mStream.Flush();
            //mStream.Position = 0;
            clientSocket.Send(mStream.GetBuffer(), (int)mStream.Length, SocketFlags.None);
            Console.WriteLine("Send Sucess");
            //Receive Client List
            mStream.Position = 0;
            //接收第二步
            int bufferLength = clientSocket.Receive(buffer, 1024 * 1024, 0);
            Console.WriteLine("Receive Sucess");
            MemoryStream mStream_ = new MemoryStream();
            BinaryFormatter bFormatter_ = new BinaryFormatter();
            mStream_.Write(buffer, 0, bufferLength);
            mStream_.Flush();
            mStream_.Position = 0;
            csList = (List<ClientServer>)bFormatter_.Deserialize(mStream_);
        }
       
        private static void ClientToClient()
        {
            foreach (ClientServer _cs in csList)
            {
                if (_cs.tcpIPPort.ToString() == clientSocket.LocalEndPoint.ToString())
                {
                    Console.WriteLine("{0} is Client {1} LocalHost!!", csList.IndexOf(_cs), _cs.tcpIPPort.ToString());
                    continue;
                }
                Console.WriteLine("{0} Client {1}", csList.IndexOf(_cs), _cs.tcpIPPort.ToString());
            }
            Console.WriteLine(clientSocket.LocalEndPoint.ToString());
            Console.WriteLine("Please Select Client Number:");
            int number = Convert.ToInt32(Console.ReadLine());
            //set number check
            cs = csList[number];
            Console.WriteLine(cs.tcpIPPort.ToString());
            //创建UDP Client
            new UDPClient().UDP_Client(cs);
        }
        //exit
        private static void ClientClose()
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            Console.WriteLine("Client Connetc Close!");
            Console.ReadLine();
        }
    }
}
