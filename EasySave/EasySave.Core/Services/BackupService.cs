using System;
using System.Collections.Generic;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Services
{
    public class BackupService
    {
        private readonly List<BackupJob> backupJobs = new();
        private readonly TranslationService translationService;
        private readonly FileService fileService;

        public BackupService(TranslationService translationService, FileService fileService)
        {
            this.translationService = translationService;
            this.fileService = fileService;
        }

        /// <summary>
        /// Ajoute un nouveau travail de sauvegarde.
        /// </summary>
        /// <param name="job">Le travail de sauvegarde à ajouter.</param>
        public void AddBackupJob(BackupJob job)
        {
            if (backupJobs.Count >= 5)
            {
                Console.WriteLine(translationService.GetTranslation("MaxBackupJobsReached"));
                return;
            }
            backupJobs.Add(job);
            Console.WriteLine(translationService.GetTranslation("BackupJobAdded", job.Name));
        }

        /// <summary>
        /// Exécute un travail de sauvegarde spécifique.
        /// </summary>
        /// <param name="index">Index du travail de sauvegarde à exécuter.</param>
        public void ExecuteBackupJob(int index)
        {
            if (index < 0 || index >= backupJobs.Count)
            {
                Console.WriteLine(translationService.GetTranslation("InvalidBackupJobIndex"));
                return;
            }

            var job = backupJobs[index];

            // Vérifie si les répertoires source et cible sont valides
            if (string.IsNullOrWhiteSpace(job.SourceDirectory) || 
                string.IsNullOrWhiteSpace(job.TargetDirectory) ||
                !System.IO.Directory.Exists(job.SourceDirectory))
            {
                Console.WriteLine($"Invalid directories for backup job '{job.Name}'.");
                return;
            }

            Console.WriteLine(translationService.GetTranslation("ExecutingBackupJob", job.Name));

            // Utilise FileService pour effectuer la sauvegarde
            fileService.BackupDirectory(job.SourceDirectory, job.TargetDirectory, job.BackupType == BackupType.Differential);

            Console.WriteLine(translationService.GetTranslation("BackupJobCompleted", job.Name));
        }

        /// <summary>
        /// Exécute tous les travaux de sauvegarde séquentiellement.
        /// </summary>
        public void ExecuteAllBackupJobs()
        {
            for (int i = 0; i < backupJobs.Count; i++)
            {
                ExecuteBackupJob(i);
            }
        }
    }
}