using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace BitLockCli.Security
{
    public static class GetPassword
    {
        public static SecureString GetPasswordFromCMD()
        {
            Console.WriteLine("Enter the file's password: ");
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
