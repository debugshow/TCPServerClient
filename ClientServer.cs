using System;
using System.Security.Cryptography;
using System.Net;

namespace TCPClientServer
{
    [Serializable]
    public class ClientServer
    {
        //TCP IP Address
        public IPEndPoint tcpIPPort { get; set; }
        //UDP 
        public IPEndPoint udpIPPort { get; set; }
        //接收端口
        public int listenport { get; set; }
        //目标端口
        public int remoteport { get; set; }
        //RSA公钥 公开存放
        public RSAParameters clientRSAPublicPar { get; set; }
      //public SHA1CryptoServiceProvider hashProvider { get; set; }
        //RSA签名
        public byte[] rsaSignData { get; set; }
        //DES加密
        public byte[] dasEncryptData { get; set; }

        public byte[] KeyBytes { get; set; }
        public byte[] IVBytes { get; set; }
    }
}
