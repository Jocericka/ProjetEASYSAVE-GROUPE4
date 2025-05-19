using System;
using System.IO;
using Newtonsoft.Json;

namespace EasySave.Logger
{
    public class Logger
    {
        private readonly string logFilePath;

        public Logger(string logDirectory)
        {
            logFilePath = Path.Combine(logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        // Propriété publique pour accéder au chemin du fichier log
        public string LogFilePath => logFilePath;

        public void LogAction(string backupName, string sourcePath, string destinationPath, long fileSize, long transferTime)
        {
            var logEntry = new
            {
                Timestamp = DateTime.Now,
                BackupName = backupName,
                SourcePath = sourcePath,
                DestinationPath = destinationPath,
                FileSize = fileSize,
                TransferTime = transferTime
            };

            var logContent = JsonConvert.SerializeObject(logEntry, Formatting.Indented);
            File.AppendAllText(logFilePath, logContent + Environment.NewLine);
        }
    }
}