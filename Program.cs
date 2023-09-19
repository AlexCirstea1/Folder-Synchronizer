using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Collections.Generic;


namespace Program
{
    internal class Program
    {
        private static string sourcePath;
        private static string replicaPath;
        private static string logPath;
        private static int syncInterval;

        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine(
                    "Usage: FolderSync.exe <SourceFolderPath> <ReplicaFolderPath> <SyncIntervalInSeconds> <LogFilePath>");
                return;
            }

            sourcePath = args[0];
            replicaPath = args[1];
            syncInterval = int.Parse(args[2]);
            logPath = args[3];

            Timer timer = new Timer(state => SynchronizeFolders(), null, 0, syncInterval);

            Console.WriteLine("Press 'Q' to quit.");
            while (Console.ReadKey().Key != ConsoleKey.Q) ;
        }

        public static void SynchronizeFolders()
        {
            try
            {
                SynchronizeFolder(sourcePath, replicaPath);
                LogFile("Synchronization completed successfully.");
            }
            catch (Exception e)
            {
                LogFile("Error during synchronization: " + e.Message);
            }
        }

        public static void SynchronizeFolder(string sourceFolder, string replicaFolder)
        {
            var sourceFilePaths = Directory.GetFiles(sourceFolder);
            var replicaFilePaths = Directory.GetFiles(replicaFolder);

            var sourceFileHashes = new Dictionary<string, string>();
            foreach (var sourceFilePath in sourceFilePaths)
            {
                var fileName = Path.GetFileName(sourceFilePath);
                var hash = CalculateMD5(sourceFilePath);
                sourceFileHashes[fileName] = hash;

                var replicaFilePath = Path.Combine(replicaFolder, fileName);
                if (!File.Exists(replicaFilePath) || !hash.Equals(CalculateMD5(replicaFilePath)))
                {
                    File.Copy(sourceFilePath, replicaFilePath, true);
                    LogFile("File copied/updated: " + fileName);
                }
            }

            foreach (var replicaFilePath in replicaFilePaths)
            {
                var fileName = Path.GetFileName(replicaFilePath);

                if (!sourceFileHashes.ContainsKey(fileName))
                {
                    File.Delete(replicaFilePath);
                    LogFile("File deleted from replica: " + fileName);
                }
            }
        }

        public static void LogFile(string message)
        {
            try
            {
                var entry = DateTime.Now + ": " + message;
                Console.WriteLine(entry);
                File.AppendAllText(logPath, entry + Environment.NewLine);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error logging: " + e.Message);
            }
        }

        private static string CalculateMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
