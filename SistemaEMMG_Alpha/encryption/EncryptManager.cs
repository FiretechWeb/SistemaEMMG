using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace SistemaEMMG_Alpha
{
    public static class EncryptManager
    {
        private static byte[] generateAESEncryptionKey(string key)
        {
            byte[] bytesKey = new byte[16];
            for (int i = 0; i < 16; i += 2)
            {
                byte[] unicodeBytes = BitConverter.GetBytes(key[i % key.Length]);
                Array.Copy(unicodeBytes, 0, bytesKey, i, 2);
            }

            return bytesKey;
        }

        public static string DecryptString(string cipherText, string encodingKey)
        {
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = generateAESEncryptionKey(encodingKey);

                // Extract intialization vector (IV) from the cipherText
                byte[] iv = Convert.FromBase64String(cipherText.Substring(0, 24));
                aesAlg.IV = iv;

                //Create a decryptor to perform the stream transform
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                //Create the streams used for decryption

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText.Substring(24))))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static string EncryptString(string plainText, string encondingKey)
        {
            using (AesManaged aesAlg = new AesManaged())
            {
    
                aesAlg.Key = generateAESEncryptionKey(encondingKey);

                //Generate a random intialization vector (IV) for each encryption
                aesAlg.GenerateIV();

                //Create an ecnryptor to perform the stream transform

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                //Create the streams used for encryption

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all date to the stream
                            swEncrypt.Write(plainText);
                        }
                    }

                    return Convert.ToBase64String(aesAlg.IV) + Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
    }
}
