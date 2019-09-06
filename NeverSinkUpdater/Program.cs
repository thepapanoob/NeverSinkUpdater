using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Json;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeverSinkUpdater
{
    class Program
    {
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        static void Main(string[] args)
        {
            WebClient client = new WebClient();
            client.Headers.Add("User-Agent", "Nothing");
            string zipPath = @".\temp.zip";

            JsonValue content = JsonValue.Parse(client.DownloadString("https://api.github.com/repos/NeverSinkDev/NeverSink-Filter/releases/latest"));

            string zipBallUrl = content["zipball_url"].ToString().Substring(1);
            zipBallUrl = zipBallUrl.Remove(zipBallUrl.Length - 1);
            
            client.Headers.Add("User-Agent", "Nothing");
            client.DownloadFile(zipBallUrl, zipPath);
            
            string extractPath = @".\temp";
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            File.Delete(zipPath);

            dynamic directories = Directory.GetDirectories(extractPath);
            DirectoryCopy(directories[0], @".\", true);
            Directory.Delete(extractPath, true);
        }
    }
}
