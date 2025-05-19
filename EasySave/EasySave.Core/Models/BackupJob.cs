using System;
using System.IO;
using EasySave.Services;
using EasySave.Logger;

namespace EasySave.Models
{
    public class BackupJob
    {
        public string Name { get; set; }
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public BackupType BackupType { get; set; }

        // Propriété calculée pour déterminer si la sauvegarde est différentielle
        public bool IsDifferential => BackupType == BackupType.Differential;

        public BackupJob(string name, string sourceDirectory, string targetDirectory, BackupType backupType)
        {
            Name = name;
            SourceDirectory = sourceDirectory;
            TargetDirectory = targetDirectory;
            BackupType = backupType;
        }

        public void Execute(FileService fileService, Logger.Logger logger)
        {
            try
            {
                Console.WriteLine($"Début de la sauvegarde : {Name}");

                // Effectue la sauvegarde
                fileService.BackupDirectory(SourceDirectory, TargetDirectory, IsDifferential);

                // Log global pour le travail de sauvegarde
                var totalSize = CalculateTotalSize(SourceDirectory);
                logger.LogAction(Name, SourceDirectory, TargetDirectory, totalSize, 0);

                Console.WriteLine($"Sauvegarde terminée : {Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur pendant la sauvegarde : {ex.Message}");
            }
        }

        private long CalculateTotalSize(string directory)
        {
            long totalSize = 0;
            string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                totalSize += new FileInfo(file).Length;
            }
            return totalSize;
        }
    }
}