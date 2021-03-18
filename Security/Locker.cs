using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using BitLockCli.Filesystem;

namespace BitLockCli.Security
{
    /// <summary>
    /// Locks and unlocks a file using a password
    /// </summary>
    public sealed class Locker : IDisposable
    {
        // a SecureString with the file's password
        private SecureString Password;

        // automatically lock the file after TimeTillDeath
        // this prevents forgetting the file is unlocked and leaving sensitive data open
        private readonly long TimeTillDeath;

        // the files to encrypt
        private readonly List<string> Files;

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
        public Locker(List<string> file, long timeTillDeath, bool shouldChangePass = false)
        {
            TimeTillDeath = timeTillDeath;
            Files = GetAllFiles.GetAllFilesFromPaths(file);
            Password = GetPassword.GetPasswordFromCMD();
            // start new line from GetPasswordFromCMD()
            Console.WriteLine();

            for (int i = 0; i < file.Count(); i++)
            {
                var f = file[i];

                // if the file is already locked
                if (Path.GetExtension(f) == ".lock")
                {
                    // let File be the original file name without .lock
                    Files[i] = f.Remove(f.Length - 5);
                    try
                    {
                        UnlockFile(f, Files[i]);
                    }
                    catch (CryptographicException)
                    {
                        Console.WriteLine("Password is incorrect.");
                        File.Delete(Files[i]);
                        Environment.Exit(1);
                    }
                }
            }

            // in case of ctrl-c, lock the file
            Console.CancelKeyPress += new ConsoleCancelEventHandler(HandleConsoleEnd);

            if (shouldChangePass)
            {
                ChangePassword();
            }
            else
            {
                // wait until timer dies
                Thread.Sleep((int)TimeTillDeath);
            }
        }

        // NOTE: this shares the same code as UnlockFile, but it improves readability when calling the functions.

        /// <summary>
        /// Locks a file using the prompted password as a key
        /// </summary>
        /// <param name="infile">the file to read and encrypt the contents from</param>
        /// <param name="outfile">the new file location of the locked file</param>
        private void LockFile(string infile, string outfile)
        {
            AesManaged aes = new AesManaged();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            // NB: Rfc2898DeriveBytes initialization and subsequent calls to   GetBytes   must be eactly the same, including order, on both the encryption and decryption sides.
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(Password.ContentsToString(), Salt, Iterations);
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            aes.Mode = CipherMode.CBC;
            ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);

            using (FileStream destination = new FileStream(outfile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using CryptoStream cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write);
                using FileStream source = new FileStream(infile, FileMode.Open, FileAccess.Read, FileShare.Read);

                source.CopyTo(cryptoStream);
            }

            File.Delete(infile);
        }

        /// <summary>
        /// Unlocks the file with the prompted password
        /// </summary>
        /// <param name="infile">the file to read and decrypt the contents from</param>
        /// <param name="outfile">The file location where the unlocked file should be stored</param>
        private void UnlockFile(string infile, string outfile)
        {
            AesManaged aes = new AesManaged();

            // get aes' max block and max key size
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            // NB: Rfc2898DeriveBytes initialization and subsequent calls to   GetBytes   must be eactly the same, including order, on both the encryption and decryption sides.
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(Password.ContentsToString(), Salt, Iterations);

            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            aes.Mode = CipherMode.CBC;
            ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);

            using (FileStream destination = new FileStream(outfile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using CryptoStream cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write);
                using FileStream source = new FileStream(infile, FileMode.Open, FileAccess.Read, FileShare.Read);
                source.CopyTo(cryptoStream);
            }

            File.Delete(infile);
        }

        /// <summary>
        /// Changes the file/directory's password
        /// </summary>
        public void ChangePassword()
        {
            // Get new password
            SecureString newPassword = GetPassword.GetPasswordFromCMD(askText : "Enter the new password: ");

            Password.Clear();

            Password = newPassword;
        }
        
        /// <summary>
        /// Disposes the object and locks the file
        /// </summary>
        public void Dispose()
        {
            foreach (string file in Files)
            {
                LockFile(file, file + ".lock");
            }

            Password.Dispose();

            Console.WriteLine();

            Console.WriteLine("Locked file(s) successfully.");
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
