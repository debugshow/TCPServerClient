using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TCPClientServer
{
    public class TCPServer
    {
        private  byte[] buffer = new byte[1024 * 1024];
        //Server Port
        private int serverPort = 8012;
        private IPAddress serverIP = IPAddress.Parse("127.0.0.1");
        //TCP Socket
        private  Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Save Server Client List
        private List<ClientServer> csList;
        public void TCP_Server()
        {
            //绑定地址和端口
            serverSocket.Bind(new IPEndPoint(serverIP, serverPort));
            serverSocket.Listen(10);
            Console.WriteLine("Enable Listening {0}", serverSocket.LocalEndPoint.ToString());
            csList = new List<ClientServer>();
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                //加入线程池 TCP服务器持续接收数据
                ThreadPool.QueueUserWorkItem(new WaitCallback(DataComm), clientSocket);
            }
        }

        public void DataComm(object obj_clientSocket)
        {
            Socket clientSocket = obj_clientSocket as Socket;
            EndPoint remoteEndPoint = clientSocket.RemoteEndPoint;
            //接收客户端信息
            int bufferLength=0;//= clientSocket.Receive(buffer);
            while (true)
            {
                try
                {
                    bufferLength = clientSocket.Receive(buffer);
                }
                catch (Exception ex)
                {
                    //关闭连接时出错，不做任何反应
                    Console.WriteLine("Client Closed...");
                }
                //创建内存流
                MemoryStream mStream = new MemoryStream();
                BinaryFormatter bFormatter = new BinaryFormatter();
                mStream.Write(buffer, 0, bufferLength);
                mStream.Flush();
                mStream.Position = 0;
                //数据反序列化
                ClientServer cs = bFormatter.Deserialize(mStream) as ClientServer;
                /*bool isClient = false;
                foreach (ClientServer c in csList)
                {
                    if (c.tcpIPPort == cs.tcpIPPort)
                    {
                        isClient = true;
                    }
                }*/
                if (cs.commands== "SendCleint")
                {
                    csList.Add(cs);
                    Console.WriteLine("Server receive {0} Sucess", csList.Count);
                }
                if (cs.commands == "GetList")
                {
                    //发送客户端列表
                    MemoryStream mStream_ = new MemoryStream();
                    BinaryFormatter bFormatter_ = new BinaryFormatter();
                    //对象序列化
                    bFormatter_.Serialize(mStream_, csList);
                    mStream_.Flush();
                    mStream_.Position = 0;
                    //Send
                    clientSocket.Send(mStream_.GetBuffer(), (int)mStream_.Length, SocketFlags.None);

                }
                Console.WriteLine("List {0}", csList.Count);
            }
        }
    }
}
