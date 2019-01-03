using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Security.Cryptography;

namespace TCPClientServer
{
    class RSA_C_S
    {
        //RSA签名
        public static byte[] RsaSignData(byte[] plainData, RSAParameters rsaPrivatePrams, Object hashProvider)
        {
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.ImportParameters(rsaPrivatePrams);
            byte[] signatureData = rsaProvider.SignData(plainData, hashProvider as SHA1CryptoServiceProvider);
            rsaProvider.Clear();
            return signatureData;
        }
        //RSA认证
        public static bool RsaVerifyData(byte[] plainData, RSAParameters rsaPublicParams, Object hashProvider, byte[] signatureData)
        {
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.ImportParameters(rsaPublicParams);
            bool isVerifyOK = rsaProvider.VerifyData(plainData, hashProvider, signatureData);
            rsaProvider.Clear();
            return isVerifyOK;
        }
        //RSA加密
        static public byte[] RsaEncrypt(byte[] plainData, RSAParameters rsaPublicParams)
        {
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            ////Parameters(RSAParameters)	 导入指定的 RSAParameters。
            rsaProvider.ImportParameters(rsaPublicParams);
            //使用 RSA 算法加密数据。
            byte[] encrypteData = rsaProvider.Encrypt(plainData, false);
            //释放 AsymmetricAlgorithm 类使用的所有资源。
            rsaProvider.Clear();
            return encrypteData;
        }
        //脱密
        static public byte[] RsaDecrypt(byte[] decryptedData, RSAParameters rSAParameters)
        {
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            //Parameters(RSAParameters)	 导入指定的 RSAParameters。
            rsaProvider.ImportParameters(rSAParameters);
            //使用指定填充对以前通过 RSA 算法加密的数据进行解密。
            byte[] plainData = rsaProvider.Decrypt(decryptedData, false);
            //释放 AsymmetricAlgorithm 类使用的所有资源。
            rsaProvider.Clear();
            return plainData;
        }
    }
}
