using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Helper.Common
{
    public class AesBase64Wrapper
    {
        private static string IV = @"Qz-N!p#ATb9_2MkL";
        private static string KEY = @"ledV\K""zRaNF]WXki,RMtLLZ{Cyr_1";

        public static string EncryptAndEncode(string raw)
        {
            var SALT_INDEX = RandomString(2);
            var SALT_TO_EXCLUDE = RandomString(3);
            var SALT = RandomString(16);
            var SALT0_9 = SALT.Substring(0, 10);
            var SALT10_15 = SALT.Substring(10, 6);
            using (var csp = new AesCryptoServiceProvider())
            {
                ICryptoTransform cryptoTransform = GetCryptoTransform(csp, true, SALT);
                byte[] inputBuffer = Encoding.UTF8.GetBytes(raw);
                byte[] output = cryptoTransform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
                string cipher = Convert.ToBase64String(output);

                var INDEX_TO_INSERT = ((Encoding.UTF8.GetBytes(SALT_INDEX.Substring(0, 1))[0] + Encoding.UTF8.GetBytes(SALT_INDEX.Substring(1, 1))[0])) % 3;
                StringBuilder finalCipher = new StringBuilder();
                finalCipher.Append(cipher);
                finalCipher.Insert(INDEX_TO_INSERT, SALT10_15, 1);
                finalCipher.Insert(0, SALT_INDEX, 1);
                finalCipher.Insert(SALT_INDEX.Length, SALT_TO_EXCLUDE, 1);
                finalCipher.Insert((SALT_INDEX.Length + SALT_TO_EXCLUDE.Length), SALT0_9, 1);
                return finalCipher.ToString();
            }
        }

        public static string DecodeAndDecrypt(string encrypted)
        {
            var SALT_INDEX = encrypted.Substring(0, 2);
            var INDEX_TO_INSERT = ((Encoding.UTF8.GetBytes(SALT_INDEX.Substring(0, 1))[0] + Encoding.UTF8.GetBytes(SALT_INDEX.Substring(1, 1))[0])) % 3;
            var SALTedCIPHER = encrypted.Substring(5, encrypted.Length - 5);
            var SALT0_9 = SALTedCIPHER.Substring(0, 10);
            var SALT10_15 = SALTedCIPHER.Substring(SALT0_9.Length + INDEX_TO_INSERT, 6);
            var finalSALT = SALT0_9 + SALT10_15;
            var cipherFirstHalf = (SALTedCIPHER.Substring(SALT0_9.Length, INDEX_TO_INSERT));
            var cipherSecondHalf = SALTedCIPHER.Substring(SALT0_9.Length + INDEX_TO_INSERT + SALT10_15.Length, SALTedCIPHER.Length - (INDEX_TO_INSERT + SALT0_9.Length + SALT10_15.Length));
            var originalCipher = cipherFirstHalf + cipherSecondHalf;
            using (var csp = new AesCryptoServiceProvider())
            {
                var d = GetCryptoTransform(csp, false, finalSALT);
                byte[] output = Convert.FromBase64String(originalCipher);
                byte[] decryptedOutput = d.TransformFinalBlock(output, 0, output.Length);
                string decypted = Encoding.UTF8.GetString(decryptedOutput);
                return decypted;
            }
        }

        private static ICryptoTransform GetCryptoTransform(AesCryptoServiceProvider csp, bool encrypting, string salt)
        {
            csp.Mode = CipherMode.CBC;
            csp.Padding = PaddingMode.PKCS7;
            var a = csp.BlockSize;
            var spec = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(KEY), Encoding.UTF8.GetBytes(salt), 65536);
            byte[] key = spec.GetBytes(16);

            csp.IV = Encoding.UTF8.GetBytes(IV);
            csp.Key = key;
            if (encrypting)
            {
                return csp.CreateEncryptor();
            }
            return csp.CreateDecryptor();
        }

        private static string RandomString(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }

            return res.ToString();
        }
    }
}
