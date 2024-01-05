using System.Text;

namespace LIBS
{
    /// <summary>
    /// 异或加解密
    /// </summary>
    public static class XorCrypto
    {

        private static byte[] _KeyBytes;
        private static int _KeySize;
        private static byte[] KeyBytes
        {
            get
            {
                return _KeyBytes;
            }
        }

        public static void SetKey(string key)
        {
            _KeyBytes = Encoding.ASCII.GetBytes(key);
            _KeySize = KeyBytes.Length;
        }

        private static int KeySize
        {
            get
            {
                return _KeySize;
            }
        }

        /// <summary>
        /// Xor the specified buf and size.
        /// 异或加解密码
        /// </summary>
        /// <param name="buf">Buffer.</param>
        /// <param name="size">Size.</param>
        private static void Xor(byte[] buf, int offset, int size, int keyOffset)
        {
            //var len = buf.Length;
            for (int i = offset; i < size; i++)
            {
                //buf[i % len] = (byte)(buf[i % len] ^ KeyBytes[i % KeySize]);
                buf[i] = (byte)(buf[i] ^ KeyBytes[(i + keyOffset) % KeySize]);
            }
        }

        /// <summary>
        /// Decrypt the specified buf and size.
        /// </summary>
        /// <param name="buf">Buffer.</param>
        /// <param name="size">Size.</param>
        public static void Decrypt(byte[] buf, int offset, int size, int keyOffset = 0)
        {
            Xor(buf, offset, size, keyOffset);
        }

        /// <summary>
        /// Encrypt the specified buf and size.
        /// </summary>
        /// <param name="buf">Buffer.</param>
        /// <param name="size">Size.</param>
        public static void Encrypt(byte[] buf, int offset, int size)
        {
            Xor(buf, offset, size, 0);
        }
    }
}

