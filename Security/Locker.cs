using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace BitLockCli.Security
{
    /// <summary>
    /// Locks and unlocks a file using a password
    /// </summary>
    public class Locker : IDisposable
    {
        // a SecureString with the file's password
        private readonly SecureString Password;

        // automatically lock the file after TimeTillDeath
        // this prevents forgetting the file is unlocked and leaving sensitive data open
        private readonly long TimeTillDeath;

        // the file to encrypt
        private readonly string File;

        // salt has to be static and can be re-used
        // this prevents against Rainbow Table attacks
        // this salt can be replaced with any other byte[8]
        private static readonly byte[] Salt = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };

        // have high iterations to protect against brute force
        // this also makes the encryption considerably slower
        // for faster encryption, use a number between 1000 - 2000
        private const int Iterations = 10000;

        /// <summary>
        /// Creates a file locker and immediately locks <paramref name="file"/>
        /// </summary>
        /// <param name="file">The file to lock/unlock</param>
        /// <param name="timeTillDeath">the time until the file automatically locks itself</param>
        public Locker(string file, long timeTillDeath)
        {
            TimeTillDeath = timeTillDeath;
            File = file;
            Password = GetPassword.GetPasswordFromCMD();
            // start new line from GetPasswordFromCMD()
            Console.WriteLine();

            // if the file is already locked
            if (Path.GetExtension(file) == ".lock")
            {
                // let File be the original file name without .lock
                File = file.Remove(file.Length - 5);
                UnlockFile(File);

                Console.WriteLine("File is unlocked.");

                // in case of ctrl-c, lock the file
                Console.CancelKeyPress += new ConsoleCancelEventHandler(HandleConsoleEnd);

                // wait until timer dies
                Thread.Sleep((int)TimeTillDeath);
            }
        }

        /// <summary>
        /// Locks a file using the prompted password as a key
        /// </summary>
        /// <param name="outfile">the new file location of the locked file</param>
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
                using CryptoStream cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write);
                using FileStream source = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read);

                source.CopyTo(cryptoStream);
            }

            System.IO.File.Delete(File);
        }

        /// <summary>
        /// Unlocks the file with the prompted password
        /// </summary>
        /// <param name="outfile">The file location where the unlocked file should be stored</param>
        private void UnlockFile(string outfile)
        {
            AesManaged aes = new AesManaged();

            // get aes' max block and max key size
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
                using CryptoStream cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write);
                using FileStream source = new FileStream(File + ".lock", FileMode.Open, FileAccess.Read, FileShare.Read);
                source.CopyTo(cryptoStream);
            }

            System.IO.File.Delete(File + ".lock");
        }
        
        /// <summary>
        /// Disposes the object and locks the file
        /// </summary>
        public void Dispose()
        {
            LockFile(File + ".lock");
            Console.WriteLine("Locked file successfully.");
        }

        /// <summary>
        /// Disposes the object when Ctrl-c is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void HandleConsoleEnd(object sender, ConsoleCancelEventArgs eventArgs)
        {
            Dispose();
        }
    }
}
