using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace BitLockCli.Security
{
    public static class GetPassword
    {
        /// <summary>
        /// Prompts the user for a password and hides it in the command line
        /// </summary>
        /// <returns>A SecureString with the password</returns>
        public static SecureString GetPasswordFromCMD(string askText = "Enter the file's password: ")
        {
            Console.WriteLine(askText);
            var pass = new SecureString();
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass.RemoveAt(pass.Length - 1);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass.AppendChar(keyInfo.KeyChar);
                }
            } while (key != ConsoleKey.Enter);

            return pass;
        }
    }
}
