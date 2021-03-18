using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BitLockCli.Filesystem
{
    public static class GetAllFiles
    {
        /// <summary>
        /// Gets all files from a List&lt;string&gt; of files and directories
        /// </summary>
        /// <param name="input">A list of files and directories</param>
        /// <returns>A List of all files found</returns>
        public static List<string> GetAllFilesFromPaths(List<string> input)
        {
            List<string> filesList = new List<string>();

            foreach (string singleItem in input)
            {
                if (Directory.Exists(singleItem))
                {
                    filesList.AddRange(GetRecursiveFiles(singleItem));
                }
                else
                {
                    filesList.Add(singleItem);
                }
            }

            return filesList;
        }

        /// <summary>
        /// Get all files from a directory recursively
        /// </summary>
        /// <param name="directory">The starting directory</param>
        /// <returns>A List of all the files found in every sub-directory of <paramref name="directory"/></returns>
        public static List<string> GetRecursiveFiles(string directory)
        {
            List<string> files = new List<string>(Directory.GetFiles(directory));
            foreach (string dir in Directory.GetDirectories(directory))
            {
                files.AddRange(GetRecursiveFiles(dir));
            }

            return files;
        }
    }
}
