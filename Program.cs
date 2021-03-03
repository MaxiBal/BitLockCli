using BitLockCli.Security;
using System;

namespace BitLockCli
{
    public class Program
    {
        private const string Usage = "Usage: bitlockcli <file> <time (optional)>";

        public static void Main(string[] args)
        {
            if (args.Length == 0 || args.Length > 2)
            {
                Console.WriteLine(Usage);
            }

            // timeTillDeath defaults to 5 minutes
            int timeTillDeath = 300 * 1000;

            if (args.Length == 2)
            {
                try
                {
                    timeTillDeath = Convert.ToInt32(args[1]) * 1000;
                } catch(Exception _)
                {
                    Console.WriteLine("<time> argument must be a number.");
                }
            }

            using (_ = new Locker(args[0], timeTillDeath)) ;
        }
    }
}
