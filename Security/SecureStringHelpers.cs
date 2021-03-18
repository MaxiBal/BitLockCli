using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace BitLockCli.Security
{
    public static class SecureStringHelpers
    {
        /// <summary>
        /// Converts a <c>SecureString</c> to a <c>string</c>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ContentsToString(this SecureString value)
        {
            IntPtr bstr = Marshal.SecureStringToBSTR(value);

            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }
    }
}
