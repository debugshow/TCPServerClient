using System;
using System.Security.Cryptography;
using System.IO;

namespace TCPClientServer
{
    class DES_C_C
    {
        public static byte[] TransformBuffer(byte[] buffer, string transformMode, byte[] keyBytes, byte[] ivBytes)
        {
            //第1步一创建加密变换器
            DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
            ICryptoTransform transform = null;
            if (transformMode == "Encrypt")
                transform = desProvider.CreateEncryptor(keyBytes, ivBytes); //要为加密器指定密钥和IV
            if (transformMode == "Decrypt")
                transform = desProvider.CreateDecryptor(keyBytes, ivBytes);
            //第2步一使用加密变换器，创建加密变换流
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            //用于输出变换结果的
            //第3步一执行加密变换:将数据写入加密变换流即可
            cryptoStream.Write(buffer, 0, buffer.Length);
            cryptoStream.FlushFinalBlock(); //一定要有这一句!
           //第4步一获取加密变换的结果数据
            int length = Convert.ToInt32(memoryStream.Length);
            byte[] outBuffer = new byte[length];
            memoryStream.Seek(0, 0);//从头开始读
            memoryStream.Read(outBuffer, 0, length);
            cryptoStream.Close();
            memoryStream.Close();
            return outBuffer;

        }
    }
}
