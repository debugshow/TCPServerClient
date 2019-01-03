using System;
using System.Security.Cryptography;

namespace TCPClientServer
{
    class RSAUserPrivateparam
    {
        //RSA私钥，用户自己拥有
        private static RSAParameters rsaPrivateParameter;

        public static void setRSAPrivatePara(RSAParameters rSAParameters)
        {
            rsaPrivateParameter = rSAParameters;
        }

        public static  RSAParameters getRSAPrivatePara()
        {
            return rsaPrivateParameter;
        }
    }
}
