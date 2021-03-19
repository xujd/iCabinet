using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace iCabinet.Core
{
    public class AESUtil
    {
        private static byte[] IV = new byte[] { 125,125,125,125,125,125,125,125,
                                    125,125,125,125,125,125,125,125};
        private static byte[] Key = new byte[] {125,125,125,125,125,125,125,125,
                                   125,125,125,125,125,125,125,125 };
        //加密
        public static string AESEncrypt(string plainText)
        {
            string encrypted = null;

            try
            {
                //校验参数是否合法
                if (plainText == null || plainText.Length <= 0)
                    throw new ArgumentNullException("plainText");
                if (Key == null || Key.Length <= 0)
                    throw new ArgumentNullException("Key");
                if (IV == null || IV.Length <= 0)
                    throw new ArgumentNullException("Key");

                //加密
                using (AesManaged aesAlg = new AesManaged())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                //将数据写到流中
                                swEncrypt.Write(plainText);
                            }
                            //将结果转为String
                            encrypted = System.Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            //返回加密结果
            return encrypted;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        public static string AESDecrypt(string cipherText)
        {
            //解密结果
            string plaintext = null;

            try
            {
                //校验参数合法性
                if (cipherText == null || cipherText.Length <= 0)
                    throw new ArgumentNullException("cipherText");
                if (Key == null || Key.Length <= 0)
                    throw new ArgumentNullException("Key");
                if (IV == null || IV.Length <= 0)
                    throw new ArgumentNullException("Key");

                byte[] source = System.Convert.FromBase64String(cipherText);

                //解密
                using (AesManaged aesAlg = new AesManaged())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(source))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                //解密结果
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                }

            }
            catch (Exception)
            {
            }

            return plaintext;
        }


        public static string Base64ToString(string strBase64)
        {
            string result = String.Empty;
            try
            {
                Encoding encoding = Encoding.UTF8;
                byte[] buffer = System.Convert.FromBase64String(strBase64);
                if (buffer != null && buffer.Length > 0)
                {
                    result = encoding.GetString(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }
        public static string StringToBase64(string strOrg)
        {
            if (strOrg == null || strOrg.Length <= 0)
                return String.Empty;
            Encoding encoding = Encoding.UTF8;
            byte[] buffer = encoding.GetBytes(strOrg);
            string result = System.Convert.ToBase64String(buffer);
            return result;
        }
    }
}
