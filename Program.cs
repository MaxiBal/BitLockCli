﻿using BitLockCli.Security;
using CommandLine;
using System;
using System.Collections.Generic;

namespace BitLockCli
{
    public class BitLockCliOptions
    {
        [Value(0, Required = true, HelpText = "The files or directories to lock/unlock.", MetaName = "Items")]
        public IEnumerable<string> Files { get; set; }

        [Option('t', "time", Required = false, HelpText = "Time until automatic lockout (default 5 minutes)", Default = 300)]
        public int Timeout { get; set; }

        [Option("change", Required = false, HelpText = "Changing a file/directory's password.", Default = false)]
        public bool ChangePassword { get; set; }
    }

    public class Program
    {
        /// <summary>
        /// The entry point of the application
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<BitLockCliOptions>(args).WithParsed(o =>
            {
                // o.Timeout is mulitplied by 1000 to cvt it from milliseconds to seconds
                using (var locker = new Locker(new List<string>(o.Files), o.Timeout * 1000, o.ChangePassword)) ;
            });
        }
    }
}
