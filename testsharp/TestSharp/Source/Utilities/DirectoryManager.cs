using System;
using System.IO;
using System.Reflection;

namespace TestSharp.Source.Utilities
{
    public class DirectoryManager
    { 
        public static string GetBinDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        
        /// <summary>
        /// Rebases file with path fromPath to folder with baseDir.
        /// </summary>
        /// <param name="_fromPath">Full file path (absolute)</param>
        /// <param name="_baseDir">Full base directory path (absolute)</param>
        /// <returns>Relative path to file in respect of baseDir</returns>
        public static string MakeRelative(String _filePath, String _rootPath)
        {
            Uri filePath = new Uri(_filePath, UriKind.Absolute); 
            Uri rootPath = new Uri(_rootPath, UriKind.Absolute);

            return rootPath.MakeRelativeUri(filePath).ToString();
        }

        public static void CheckReportFolders()
        {
            var reportFolder = GetBinDirectory() + @"\Reports";
            if(Directory.Exists(reportFolder)!=true) Directory.CreateDirectory(reportFolder);

            if (Directory.Exists(reportFolder) != true)
            {
                Directory.CreateDirectory(reportFolder);
            }

        }
    }
}