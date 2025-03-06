using System.IO;
using System.Security.Cryptography;

namespace WeatherPlusZero
{
    public static class Encryption
    {
        private static readonly byte[] key = new byte[32]
        {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
                0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
                0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
        };

        private static readonly byte[] iv = new byte[16]
        {
                0xA1, 0xB2, 0xC3, 0xD4, 0xE5, 0xF6, 0x07, 0x08,
                0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10
        };

        /// <summary>
        /// Encrypts the given data using AES encryption.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <returns>The encrypted data as a byte array.</returns>
        public static byte[] EncryptData(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Decrypts the given data using AES decryption.
        /// </summary>
        /// <param name="data">The data to decrypt.</param>
        /// <returns>The decrypted data as a byte array.</returns>
        public static byte[] DecryptData(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var memoryStream = new MemoryStream(data);
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var resultStream = new MemoryStream();

            cryptoStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
    }
}
