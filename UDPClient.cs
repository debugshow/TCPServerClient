using System;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace TCPClientServer
{
    class UDPClient
    {
        //创建UDP Socket
        public static IPEndPoint clientEndPoint;
       public static byte[] buffer = new byte[1024];
        public static ClientServer csData;
        public static UdpClient udpClient = new UdpClient();
        public void UDP_Client(ClientServer clientServer) {
            //clientSocket = localSocket;
            //udpClient = _udpClient;
            csData = clientServer;
            //获取目标UDP IP&Port
            clientEndPoint = clientServer.udpIPPort;
            //获取客户端IP和Port
            //udpclient.Connect(clientEndPoint);
           // Console.WriteLine("Connetct {0} Sucess ", clientEndPoint.ToString());
            while (true)
            {
                //接收消息
                Console.WriteLine("0 Enable UDP Server Listening and Client");
                Console.WriteLine("1 Send Message To UDP");
                Console.WriteLine("2 Close Connect");
                int Num =Convert.ToInt32( Console.ReadLine());
                switch (Num)
                {
                    case 0:
                        UdpClient udpSever = new UdpClient(csData.udpIPPort);
                        //开启UDP服务 加入线程池
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ListenUDPCLient), udpSever);
                        break;
                    case 1:
                        ClientSendClient();
                        break;
                    case 2:
                        ClientClose();
                        break;
                }
            }
        }
        private static void ListenUDPCLient(object _udpServer)
        {
            UdpClient udpServer = _udpServer as UdpClient;
            while (true)
            {
                //Console.WriteLine("等待接收数据");
                IPEndPoint clientEndPoint = null;
                buffer = udpServer.Receive(ref clientEndPoint);
                //对buffer数据进行认证和
                MemoryStream mStream = new MemoryStream();
                BinaryFormatter bFormatter = new BinaryFormatter();
                //bFormatter.Serialize(mStream, csData);
                mStream.Write(buffer, 0, buffer.Length);
                mStream.Flush();
                mStream.Position = 0;
                ClientServer cs = bFormatter.Deserialize(mStream) as ClientServer;
                //Array.Copy(buffer, buffer, buffer.Length);
                //解密
                byte[] plainData = new UDPClient().UDP_DES_Decrypt(cs.dasEncryptData, cs.KeyBytes, cs.IVBytes);
                string receiveMsg = Encoding.UTF8.GetString(plainData);
                //验证签名
                SHA1CryptoServiceProvider hashProvider = new SHA1CryptoServiceProvider();
                bool isRight = RSA_C_S.RsaVerifyData(plainData, cs.clientRSAPublicPar, hashProvider, cs.rsaSignData);
                string VerifyResult = isRight ? "YES" : "NO";
                Console.WriteLine("Server Message****Verify Result {0}", VerifyResult);
                Console.WriteLine("Server Message****Receive From {0} Message: {1} ", clientEndPoint.ToString(), receiveMsg);
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
            //对数据进行RSA签名
            //1.准备要签名的原文消息
            string sendMsg = "China";
            //对消息进行二进制编码
            byte[] plainData = Encoding.UTF8.GetBytes(sendMsg);
            //打印消息
            Console.WriteLine("原文：" + Environment.NewLine + sendMsg + Environment.NewLine);
            //对数据进行签名
            csData.rsaSignData=RSA_Sign(plainData, RSAUserPrivateparam.getRSAPrivatePara());
            //DES加密
            //设置加密密钥
            Console.WriteLine("Please Input Key");
            //string KEY_64 = Console.ReadLine();
            string KEY_64 = "A4G-8=Jk";
            //设置初始化向量
            Console.WriteLine("PLease INput IV");
            //string IV_64 = Console.ReadLine();
            string IV_64 = "JKbN=5[?";
            byte[] KeyBytes = Encoding.UTF8.GetBytes(KEY_64);
            byte[] IVBytes = Encoding.UTF8.GetBytes(IV_64);
            Console.WriteLine("KEY" + KEY_64);
            Console.WriteLine("IV" + IV_64);
            Console.WriteLine();
            //保存密钥和初始化向量
            csData.KeyBytes=KeyBytes;
            csData.IVBytes=IVBytes;
            //对消息进行加密
            csData.dasEncryptData=UDP_DES_Encrypt(plainData, KeyBytes, IVBytes);
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
            }catch(Exception ex)
            {
                Console.WriteLine("Send Error!!! | Remote Client isn't Open Listening");
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("{0} to {1} Sucess", udpClient.Client.LocalEndPoint.ToString(), clientEndPoint.ToString());
        }
        //RSA 签名
        public byte[] RSA_Sign(byte[] plainData,RSAParameters rsaPrivateParams)
        {
            //获取用户私钥进行签名
            //使用私钥对消息进行签名
            SHA1CryptoServiceProvider hashProvider = new SHA1CryptoServiceProvider();
            byte[] signatureData = RSA_C_S.RsaSignData(plainData, rsaPrivateParams, hashProvider);
            hashProvider.Clear();
            return signatureData;
        }
        //RSA认证
        public string RSA_Verify(byte[] plainData,RSAParameters rsaPublicParams,byte[] signatureData)
        {
            SHA1CryptoServiceProvider hashProvider = new SHA1CryptoServiceProvider();
            bool isVerifyOK = RSA_C_S.RsaVerifyData(plainData, rsaPublicParams, hashProvider, signatureData);
            string verifyResult = isVerifyOK ? "YES" : "NO";
            return verifyResult;
        }
        //DES加密
        public byte[] UDP_DES_Encrypt(byte[] plainBuffer,byte[] KeyBytes,byte[] IVBytes)
        {
            

            //byte[] plainBuffer = Encoding.UTF8.GetBytes(plainText);
            byte[] cypherBuffer = DES_C_C.TransformBuffer(plainBuffer, "Encrypt", KeyBytes, IVBytes);
            return cypherBuffer;
        }
        //DES解密
        public byte[] UDP_DES_Decrypt(byte[] cypherBuffer,byte[] KeyBytes,byte[] IVBytes)
        {
            //输入解密密钥
            //string KEY_64 = "A4G-8=Jk";//刀必须是8个字符(64Bit)
            //string IV_64 = "JKbN=5[?";//必须8个字符(64Bit)
            //Console.WriteLine("Please Input Key");
            //string KEY_64 = Console.ReadLine();
            //输入初始化向量
            // Console.WriteLine("PLease INput IV");
            // string IV_64 = Console.ReadLine();
            //byte[] KeyBytes = Encoding.UTF8.GetBytes(KEY_64);
           // byte[] IVBytes = Encoding.UTF8.GetBytes(IV_64);
            byte[] plainBuffer2 = DES_C_C.TransformBuffer(cypherBuffer, "Decrypt", KeyBytes, IVBytes);
            return plainBuffer2;
        }
    }
}
