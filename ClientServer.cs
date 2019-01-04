using System;
using System.Security.Cryptography;
using System.Net;

namespace TCPClientServer
{
    [Serializable]
    public class ClientServer
    {
        public string commands { get; set; }
        //TCP IP Address
        public IPEndPoint tcpIPPort { get; set; }
        //UDP 
        public IPEndPoint udpIPPort { get; set; }
        //RSA公钥 公开存放
        public RSAParameters clientRSAPublicPar { get; set; }
        //RSA签名
        public byte[] rsaSignData { get; set; }
        //DES加密
        public byte[] dasEncryptData { get; set; }
    }
}
