using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using NUnit.Framework;

namespace TestSharp.Source.Utilities
{
    public class EncryptManager
    {
        /// <summary>
        /// Encrypts string using symmetric key
        /// </summary>
        /// <param name="plainText">Value that will be encrypted</param>
        /// <returns></returns>
        public static string EncryptString(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Common.ReadConfig("Settings:Key"));
                aes.IV = iv;

                var symmetricEncryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream as Stream,
                               symmetricEncryptor, CryptoStreamMode.Write))
                    {
                        using (var streamWriter = new StreamWriter(cryptoStream as Stream))
                        {
                            streamWriter.Write(plainText);
                        }

                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// Decrypts string using symmetric key
        /// </summary>
        /// <param name="cipherText">Value that will be decrypted</param>
        /// <returns></returns>
        public static string DecryptString(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Common.ReadConfig("Settings:Key"));
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            var memoryStream = new MemoryStream(buffer);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            var streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// Use to encrypt value
        /// </summary>
        [Test]
        public static void EncryptString()
        {
            var encPass = EncryptString("MyPassword");
            Console.WriteLine(encPass);
        }

        /// <summary>
        /// Use to decrypt value
        /// </summary>
        [Test]
        public static void DecryptString()
        {
            var encPass = DecryptString("ENTER DECRYPTED VALUE");
            Console.WriteLine(encPass);
        }
    }
}