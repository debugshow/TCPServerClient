using System.Security.Cryptography;

namespace TCPClientServer
{
    class RSA_C_S
    {
        //RSA签名
        public static  byte[] RsaSignData(byte[] plainData, RSAParameters rsaPrivatePrams)
        {
            SHA1CryptoServiceProvider hashProvider = new SHA1CryptoServiceProvider();
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.ImportParameters(rsaPrivatePrams);
            byte[] signatureData = rsaProvider.SignData(plainData, hashProvider);
            rsaProvider.Clear();
            return signatureData;
        }
        //RSA认证
        public static  bool RsaVerifyData(byte[] plainData, RSAParameters rsaPublicParams, byte[] signatureData)
        {
            SHA1CryptoServiceProvider hashProvider = new SHA1CryptoServiceProvider();
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.ImportParameters(rsaPublicParams);
            bool isVerifyOK = rsaProvider.VerifyData(plainData, hashProvider, signatureData);
            rsaProvider.Clear();
            return isVerifyOK;
        }
        //RSA加密
         public static byte[] RsaEncrypt(byte[] plainData, RSAParameters rsaPublicParams)
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
        //RSA脱密
         public static byte[] RsaDecrypt(byte[] decryptedData, RSAParameters rSAParameters)
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
