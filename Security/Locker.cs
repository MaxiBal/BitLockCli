using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace BitLockCli.Security
{
    public class Locker : IDisposable
    {
        private readonly SecureString Password;
        private readonly long TimeTillDeath;
        private readonly string File;
        private static readonly byte[] Salt = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };
        private const int Iterations = 10000;

        public Locker(string file, long timeTillDeath)
        {
            TimeTillDeath = timeTillDeath;
            File = file;
            Password = GetPassword.GetPasswordFromCMD();
            // start new line from GetPasswordFromCMD()
            Console.WriteLine();

            if (Path.GetExtension(file) == ".lock")
            {
                File = file.Remove(file.Length - 5);
                UnlockFile(File);

                Console.WriteLine("File is unlocked.");

                // in case of ctrl-c, lock the file
                Console.CancelKeyPress += new ConsoleCancelEventHandler(HandleConsoleEnd);

                Thread.Sleep((int)TimeTillDeath);
            }
        }
        private void LockFile(string outfile)
        {
            AesManaged aes = new AesManaged();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            // NB: Rfc2898DeriveBytes initialization and subsequent calls to   GetBytes   must be eactly the same, including order, on both the encryption and decryption sides.
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(Password.ToString(), Salt, Iterations);
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            aes.Mode = CipherMode.CBC;
            ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);

            using (FileStream destination = new FileStream(outfile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (CryptoStream cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write))
                {
                    try
                    {
                        using (FileStream source = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            source.CopyTo(cryptoStream);
                        }
                    }
                    catch (CryptographicException exception)
                    {
                        if (exception.Message == "Padding is invalid and cannot be removed.")
                            throw new ApplicationException("Universal Microsoft Cryptographic Exception (Not to be believed!)", exception);
                        else
                            throw;
                    }
                }
            }

            System.IO.File.Delete(File);
        }

        private void UnlockFile(string outfile)
        {
            AesManaged aes = new AesManaged();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            // NB: Rfc2898DeriveBytes initialization and subsequent calls to   GetBytes   must be eactly the same, including order, on both the encryption and decryption sides.
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(Password.ToString(), Salt, Iterations);
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            aes.Mode = CipherMode.CBC;
            ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);

            using (FileStream destination = new FileStream(outfile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (CryptoStream cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write))
                {
                    try
                    {
                        using (FileStream source = new FileStream(File + ".lock", FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            source.CopyTo(cryptoStream);
                        }
                    }
                    catch (CryptographicException exception)
                    {
                        if (exception.Message == "Padding is invalid and cannot be removed.")
                            throw new ApplicationException("Universal Microsoft Cryptographic Exception (Not to be believed!)", exception);
                        else
                            throw;
                    }
                }
            }

            System.IO.File.Delete(File + ".lock");
        }

        public void Dispose()
        {
            LockFile(File + ".lock");
            Console.WriteLine("Locked file successfully.");
        }

        private void HandleConsoleEnd(object sender, ConsoleCancelEventArgs eventArgs)
        {
            LockFile(File + ".lock");
            Console.WriteLine("Locked file successfully.");
        }
    }
}
