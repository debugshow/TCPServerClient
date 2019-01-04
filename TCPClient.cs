using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
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
        private static Socket clientSocket= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //UDP 客户端
        private static UdpClient udpClient = new UdpClient();
        //客户端信息
        private static List<ClientServer> csList = new List<ClientServer>();
        //与UDP Client互传信息
        private static ClientServer c_s = new ClientServer();
        //私钥自己拥有
        private static RSAParameters rsaPrivateParameter = new RSAParameters();
        //private static RSAUserPrivateparam rsaPrivateParams=new RSAUserPrivateparam();
        public void TCP_Client()
        {
            c_s.commands = "SendCleint";
            byte[] plainData = Encoding.UTF8.GetBytes("China");
            //创建RSACryptoServiceProvider实例，以自动生成公钥对并保存之
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            //set Init Public Key
            c_s.clientRSAPublicPar=rsaProvider.ExportParameters(false);
            //导出参数信息，包含私钥
            rsaPrivateParameter = rsaProvider.ExportParameters(true);
            //
            rsaProvider.Clear();
            //使用私钥对消息进行签名   保存签名
            c_s.rsaSignData=RSA_C_S.RsaSignData(plainData, rsaPrivateParameter);
            while (true)
            {
                Console.WriteLine(" 0 Create TCP Client & Connect Server.");
                Console.WriteLine(" 1 Put Key To Server");
                Console.WriteLine(" 2 Get Client List.");
                Console.WriteLine(" 3 Talk To Another Client.");
                Console.WriteLine(" 4 Exit!");
                int number = Convert.ToInt32(Console.ReadLine());
                switch (number)
                {
                    case 0:
                        ConnectServer();
                        break; 
                    case 1:
                        SendPublicKey();
                        break;
                    case 2:
                        GetClientList();
                        break;
                    case 3:
                        ClientToClient();
                        break;
                    case 4:
                        ClientClose();
                        return;
                }
            }
        }
        //Connect Server
        private static void ConnectServer()
        {
            Console.WriteLine("Please Input Server IP:");
            string serverip = Console.ReadLine();
            Console.WriteLine("PLease Input Server Port:");
            int serverPort = Convert.ToInt32(Console.ReadLine());
            //Console.WriteLine("Server Set Above!");
            IPAddress serverIP = IPAddress.Parse(serverip);
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
            IPAddress localIP = IPAddress.Parse("127.0.0.1");
            //设置UDP 端口
            IPEndPoint localEndPoint = new IPEndPoint(localIP, 60000);
            //设置Udp IP & Port
            c_s.udpIPPort = localEndPoint;
            if (IsPortUsed(c_s.udpIPPort.Port) == true)
            {
                c_s.udpIPPort.Port += 1;
            }
            //UdpClient udpServer
            udpClient = new UdpClient(c_s.udpIPPort);
            //创建UDP监听
            ThreadPool.QueueUserWorkItem(new WaitCallback(UDPReceiveServer), udpClient);
            //启动监听TCP消息
            ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveServer), clientSocket);
        }

        internal static bool IsPortUsed(int port)
        {
            bool inUse = false;
            IPGlobalProperties iPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] iPEndPoints = iPGlobalProperties.GetActiveUdpListeners();
            foreach (IPEndPoint ip in iPEndPoints)
            {
                if (ip.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }

        private static void UDPReceiveServer(object client)
        {
            UdpClient udpServer = client as UdpClient;
            Console.WriteLine("UDP Server Enable........");
            while (true)
            {
                //Console.WriteLine("等待接收数据");
                IPEndPoint clientEndPoint = null;
                try
                {
                    buffer = udpServer.Receive(ref clientEndPoint);
                }catch(Exception ex)
                {
                    Console.WriteLine("Client Closeing");
                }
                //对buffer数据进行认证和
                MemoryStream mStream = new MemoryStream();
                BinaryFormatter bFormatter = new BinaryFormatter();
                mStream.Write(buffer, 0, buffer.Length);
                mStream.Flush();
                mStream.Position = 0;
                ClientServer cs = bFormatter.Deserialize(mStream) as ClientServer;
                //解密
                //设置DES脱密密钥
                Console.WriteLine("Please Input Key");
                //string KEY_64 = Console.ReadLine();
                string KEY_64 = "A4G-8=Jk";
                Console.WriteLine(KEY_64);
                //设置初始化向量
                Console.WriteLine("PLease INput IV");
                //string IV_64 = Console.ReadLine();
                string IV_64 = "JKbN=5[?";
                Console.WriteLine(IV_64);
                byte[] KeyBytes = Encoding.UTF8.GetBytes(KEY_64);
                byte[] IVBytes = Encoding.UTF8.GetBytes(IV_64);
                //先DES解密
                byte[] plainData = DES_C_C.TransformBuffer(cs.dasEncryptData, "Decrypt", KeyBytes, IVBytes); 
                string receiveMsg = Encoding.UTF8.GetString(plainData);
                //验证签名
                //Console.WriteLine("UDP Server DES Data*****: {0}", BitConverter.ToString(plainData));
                //Console.WriteLine("UDP Server Sign Data*****: {0}", BitConverter.ToString(cs.rsaSignData));
                //Console.WriteLine("UDP Server DES Data*****: {0}", BitConverter.ToString(cs.dasEncryptData));

                //Console.WriteLine("UDP Client DES Data*****: {0}", BitConverter.ToString(cs.clientRSAPublicPar.Exponent));
                //Console.WriteLine("UDP Client DES Data*****: {0}", BitConverter.ToString(cs.clientRSAPublicPar.Modulus));
                //对数据进行验证签名
                bool isRight = RSA_C_S.RsaVerifyData(plainData, cs.clientRSAPublicPar, cs.rsaSignData);
                string VerifyResult = isRight ? "YES" : "NO";
                Console.WriteLine("Server Message****Verify Result :{0}", VerifyResult);
                Console.WriteLine("Server Message****Receive From {0} Message:{1} ", clientEndPoint.ToString(), receiveMsg);
            }
        }
        //Servered Client List
        private static void SendPublicKey()
        {
            
            //设置TCPip地址和端口
            c_s.tcpIPPort=clientSocket.LocalEndPoint as IPEndPoint;
            //
            MemoryStream mStream = new MemoryStream();
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(mStream, c_s);
            mStream.Flush();
            //mStream.Position = 0;
            clientSocket.Send(mStream.GetBuffer(), (int)mStream.Length, SocketFlags.None);
            Console.WriteLine("Send Sucess");
            mStream.Position = 0;
        }

        private void GetClientList()
        {
            c_s.commands = "GetList";
            MemoryStream mStream = new MemoryStream();
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(mStream, c_s);
            mStream.Flush();
            //mStream.Position = 0;
            clientSocket.Send(mStream.GetBuffer(), (int)mStream.Length, SocketFlags.None);
            Console.WriteLine("Get Client List Send Sucess");
            c_s.commands = "SendCleint";
        }
        //接收TCP服务端信息
        private static  void ReceiveServer(object client_Socket)
        {
            byte[] bufferTCP = new Byte[1024 * 1024];
            Socket clientSocket_ = client_Socket as Socket;
            //Receive Client List
            //接收第二步
            int bufferLength = clientSocket_.Receive(bufferTCP, 1024 * 1024, 0);
            Console.WriteLine("Receive Sucess");
            MemoryStream mStream_ = new MemoryStream();
            BinaryFormatter bFormatter_ = new BinaryFormatter();
            mStream_.Write(bufferTCP, 0, bufferLength);
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
            Console.WriteLine("Please Select Client Number:");
            int number = Convert.ToInt32(Console.ReadLine());
            //set number check
            c_s = csList[number];
            Console.WriteLine(c_s.tcpIPPort.ToString());
            //创建UDP Client
            new UDPClient().UDP_Client(udpClient, c_s, rsaPrivateParameter);
        }
        //exit
        private static void ClientClose()
        {
            try
            {
                clientSocket.Shutdown(SocketShutdown.Both);
            }catch(Exception ex)
            {
                //
            }
            clientSocket.Close();
            Console.WriteLine("Client Connetc Close!");
            Console.ReadLine();
        }
    }
}
