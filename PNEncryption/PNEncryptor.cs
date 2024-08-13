#region Using

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace PNEncryption
{
    /// <summary>
    /// Represents class for simple encryption/decription operations
    /// </summary>
    public class PNEncryptor : IDisposable
    {
        private readonly RijndaelManaged m_RijndaelManaged;

        /// <summary>
        /// Initializes new instance of PNEncryptor class
        /// </summary>
        /// <param name="keyString">String used as initial token for encryption/decryption</param>
        public PNEncryptor(string keyString)
        {
            var key = recomputeHash(keyString);
            if (key.Length > 32)
                Array.Resize(ref key, 32);
            else
            {
                var temp = new byte[key.Length];
                Array.Copy(key, temp, key.Length);
                var diff = 32 - key.Length;
                Array.Resize(ref key, 32);
                Array.Reverse(temp);
                Array.Copy(temp, 0, key, key.Length - diff, diff);
            }
            var sIV = convertBytesToString(key);
            var iv = Encoding.ASCII.GetBytes(sIV.Substring(0, 16));

            m_RijndaelManaged = new RijndaelManaged {Key = key, IV = iv, BlockSize = 128, Padding = PaddingMode.PKCS7};
        }

        /// <summary>
        /// Decrypts text file
        /// </summary>
        /// <param name="srcFile">Full file name</param>
        public void DecryptTextFile(string srcFile)
        {
            var tempFile = Path.GetTempFileName();
            using (var fStreamIn = new StreamReader(srcFile))
            {
                using (var fStreamOut = new StreamWriter(tempFile, false))
                {
                    while (fStreamIn.Peek() != -1)
                    {
                        var line = fStreamIn.ReadLine();
                        fStreamOut.WriteLine(DecryptString(line));
                    }
                    fStreamOut.Flush();
                }
            }
            //replace source file
            File.Delete(srcFile);
            File.Move(tempFile, srcFile);
        }

        /// <summary>
        /// Encrypts text file
        /// </summary>
        /// <param name="srcFile">Full file name</param>
        public void EncryptTextFile(string srcFile)
        {
            var tempFile = Path.GetTempFileName();
            using (var fStreamIn = new StreamReader(srcFile))
            {
                using (var fStreamOut = new StreamWriter(tempFile, false))
                {
                    while (fStreamIn.Peek() != -1)
                    {
                        var line = fStreamIn.ReadLine();
                        fStreamOut.WriteLine(EncryptString(line));
                    }
                    fStreamOut.Flush();
                }
            }
            //replace source file
            File.Delete(srcFile);
            File.Move(tempFile, srcFile);
        }

        /// <summary>
        /// Decrypts string
        /// </summary>
        /// <param name="src">String to decrypt</param>
        /// <returns>Decrypted string</returns>
        public string DecryptString(string src)
        {
            return decryptStringFromBytes(convertStringToBytes(src));
        }

        /// <summary>
        /// Decrypts string if it is not empty or null
        /// </summary>
        /// <param name="src">String to decrypt</param>
        /// <returns>Decrypted string or empty string in case the source is empty string or null</returns>
        public string DecryptStringWithTrim(string src)
        {
            return string.IsNullOrEmpty(src) ? "" : decryptStringFromBytes(convertStringToBytes(src));
        }

        /// <summary>
        /// Encrypts string
        /// </summary>
        /// <param name="src">String to encrypt</param>
        /// <returns>Encrypted string</returns>
        public string EncryptString(string src)
        {
            return convertBytesToString(encryptStringToBytes(src));
        }

        /// <summary>
        /// Encrypts string if it is not empty or null
        /// </summary>
        /// <param name="src">String to encrypt</param>
        /// <returns>Encrypted string or empty string in case the source is empty string or null</returns>
        public string EncryptStringWithTrim(string src)
        {
            return string.IsNullOrEmpty(src) ? "" : convertBytesToString(encryptStringToBytes(src));
        }

        /// <summary>
        /// Returns hash string for input string
        /// </summary>
        /// <param name="src">String to return hash for</param>
        /// <returns>Hash string for input string</returns>
        public static string GetHashString(string src)
        {
            return createHashString(src);
        }

        private byte[] encryptStringToBytes(string plainText)
        {
            //declare the streams used to encrypt to an in memory array of bytes
            MemoryStream msEncrypt = null;
            CryptoStream csEncrypt = null;
            StreamWriter swEncrypt = null;

            try
            {
                //create a decryptor to perform the stream transform
                var encryptor = m_RijndaelManaged.CreateEncryptor(m_RijndaelManaged.Key,
                    m_RijndaelManaged.IV);
                //create the streams used for encryption
                msEncrypt = new MemoryStream();
                csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                swEncrypt = new StreamWriter(csEncrypt);
                //write all data to the stream
                swEncrypt.Write(plainText);
            }
            finally
            {
                //clean things up and close the stream
                if (swEncrypt != null)
                    swEncrypt.Close();
                if (csEncrypt != null)
                    csEncrypt.Close();
                if (msEncrypt != null)
                    msEncrypt.Close();
            }
            return msEncrypt.ToArray();
        }

        private string decryptStringFromBytes(byte[] bytes)
        {
            //declare the streams used to decrypt to an in memory array of bytes
            MemoryStream msDecrypt = null;
            CryptoStream csDecrypt = null;
            StreamReader srDecrypt = null;
            //declare the string used to hold the decrypted text
            string plainText;

            try
            {
                //create a decryptor to perform the stream transform
                var decryptor = m_RijndaelManaged.CreateDecryptor(m_RijndaelManaged.Key,
                    m_RijndaelManaged.IV);
                //create the streams used for decryption
                msDecrypt = new MemoryStream(bytes);
                csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                srDecrypt = new StreamReader(csDecrypt);
                //read the decrypted bytes from the decrypted stream and place them in a string
                plainText = srDecrypt.ReadToEnd();
            }
            finally
            {
                //clean things up and close the stream
                if (srDecrypt != null)
                    srDecrypt.Close();
                if (csDecrypt != null)
                    csDecrypt.Close();
                if (msDecrypt != null)
                    msDecrypt.Close();
            }
            return plainText;
        }

        private byte[] convertStringToBytes(string src)
        {
            return Convert.FromBase64String(src);
        }

        private string convertBytesToString(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        private byte[] recomputeHash(string src)
        {
            var md5 = new MD5CryptoServiceProvider();
            var bytes = new UnicodeEncoding().GetBytes(src);
            return md5.ComputeHash(bytes);
        }

        private static string createHashString(string src)
        {
            var sb = new StringBuilder();
            var md5 = new MD5CryptoServiceProvider();
            var bytes = new UnicodeEncoding().GetBytes(src);
            var hash = md5.ComputeHash(bytes);
            foreach (var s in hash.Select(t => string.Format("{0:X}", t)))
            {
                sb.Append(s.ToUpper());
            }
            return sb.ToString();
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (m_RijndaelManaged != null)
                m_RijndaelManaged.Clear();
        }

        #endregion
    }
}