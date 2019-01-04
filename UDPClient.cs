using System;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace TCPClientServer
{
    class UDPClient
    {
        //创建UDP Socket
        public static IPEndPoint clientEndPoint;
        public static ClientServer csData;
        public static UdpClient udpClient ;
        private RSAParameters rsaPrivatePara ;
        public void UDP_Client(UdpClient _udpClient,ClientServer clientServer, RSAParameters rsaPrivateParams)
        {
            udpClient = _udpClient;
            rsaPrivatePara = rsaPrivateParams;
            csData = clientServer;
            //获取目标UDP IP&Port
            clientEndPoint = clientServer.udpIPPort;
            //获取客户端IP和Port
            while (true)
            {
                Console.WriteLine(" 1 Send Message To UDP");
                Console.WriteLine(" 2 Close Connect");
                int Num =Convert.ToInt32( Console.ReadLine());
                switch (Num)
                {
                    case 1:
                        ClientSendClient();
                        break;
                    case 2:
                        ClientClose();
                        break;
                }
            }
        }

        public void ClientClose()
        {
            udpClient.Close();
        }
        public void ClientSendClient()
        {
            //输入发送的消息
            Console.WriteLine("Send {0} Message:", clientEndPoint.ToString());
            //string sendMsg = Console.ReadLine();
            //1.准备要签名的原文消息
            string sendMsg = "China";
            //对消息进行二进制编码
            byte[] plainData = Encoding.UTF8.GetBytes(sendMsg);
            //打印消息
            Console.WriteLine("PlainText："  +sendMsg );
            //DES加密
            //设置加密密钥
            Console.WriteLine("Please Input Key");
            //string KEY_64 = Console.ReadLine();
            string KEY_64 = "A4G-8=Jk";
            Console.WriteLine( KEY_64);
            //设置初始化向量
            Console.WriteLine("PLease INput IV");
            //string IV_64 = Console.ReadLine();
            string IV_64 = "JKbN=5[?";
            Console.WriteLine( IV_64);
            byte[] KeyBytes = Encoding.UTF8.GetBytes(KEY_64);
            byte[] IVBytes = Encoding.UTF8.GetBytes(IV_64);
            
            
            //Console.WriteLine();
            //用客户端私钥数据进行签名
            csData.rsaSignData = RSA_C_S.RsaSignData(plainData, rsaPrivatePara);
            //对消息进行加密
            csData.dasEncryptData= DES_C_C.TransformBuffer(plainData, "Encrypt", KeyBytes, IVBytes);
            //Console.WriteLine("UDP Server DES Data*****: {0}", BitConverter.ToString(plainData));
            //Console.WriteLine("UDP Client Sign Data*****: {0}",BitConverter.ToString(csData.rsaSignData));
            //Console.WriteLine("UDP Server DES Data*****: {0}", BitConverter.ToString(csData.dasEncryptData));
            //Console.WriteLine("UDP Client DES Data*****: {0}", BitConverter.ToString(csData.clientRSAPublicPar.Exponent));
            // Console.WriteLine("UDP Client DES Data*****: {0}", BitConverter.ToString(csData.clientRSAPublicPar.Modulus));
            //csData序列化
            MemoryStream mStream = new MemoryStream();
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(mStream, csData);
            mStream.Flush();
            mStream.Position = 0;
            //发送对象
            try
            {
                udpClient.Send(mStream.GetBuffer(), (int)mStream.Length, clientEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send Error!!! | Remote Client isn't Open Listening");
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("{0} to {1} Sucess", udpClient.Client.LocalEndPoint.ToString(), clientEndPoint.ToString());
        }
    }
}
