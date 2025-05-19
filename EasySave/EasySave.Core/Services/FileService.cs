using System;
using System.Diagnostics;
using System.IO;
using EasySave.Logger;

namespace EasySave.Services
{
    public class FileService
    {
        private readonly Logger.Logger logger;

        public FileService(Logger.Logger logger)
        {
            this.logger = logger;
        }

        public void BackupDirectory(string sourceDir, string targetDir, bool isDifferential = false)
        {
            var stopwatch = Stopwatch.StartNew(); // Démarre le chronomètre

            try
            {
                Console.WriteLine("Début de la sauvegarde...");

                // Vérifie si le répertoire source existe
                if (!Directory.Exists(sourceDir))
                {
                    Console.WriteLine($"Erreur : Le répertoire source '{sourceDir}' n'existe pas.");
                    return;
                }

                // Crée le répertoire cible s'il n'existe pas
                Directory.CreateDirectory(targetDir);

                // Récupère tous les fichiers du répertoire source
                string[] files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    string relativePath = Path.GetRelativePath(sourceDir, file);
                    string destinationFile = Path.Combine(targetDir, relativePath);

                    // Crée les répertoires nécessaires dans le répertoire cible
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

                    // Si la sauvegarde est différentielle, vérifie si le fichier doit être copié
                    if (isDifferential && !ShouldCopyFileDifferential(file, destinationFile))
                    {
                        Console.WriteLine($"Ignoré (pas de modification) : {relativePath}");
                        continue;
                    }

                    // Copie le fichier
                    File.Copy(file, destinationFile, true);

                    // Log de l'action
                    var fileSize = new FileInfo(file).Length;
                    logger.LogAction("BackupJob", file, destinationFile, fileSize, 0);

                    Console.WriteLine($"Copié : {relativePath}");
                }

                Console.WriteLine("Sauvegarde terminée avec succès.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur pendant la sauvegarde : {ex.Message}");
            }
            finally
            {
                stopwatch.Stop(); // Arrête le chronomètre
                Console.WriteLine($"Temps total de la sauvegarde : {stopwatch.Elapsed.TotalSeconds} secondes");

                // Log du temps total
                logger.LogAction("BackupSummary", sourceDir, targetDir, 0, (int)stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        private bool ShouldCopyFileDifferential(string sourcePath, string destinationPath)
        {
            if (!File.Exists(destinationPath))
            {
                return true; // Le fichier n'existe pas dans le répertoire cible
            }

            var sourceInfo = new FileInfo(sourcePath);
            var destinationInfo = new FileInfo(destinationPath);

            // Vérifie si la date de modification ou la taille du fichier a changé
            return sourceInfo.LastWriteTime > destinationInfo.LastWriteTime || sourceInfo.Length != destinationInfo.Length;
        }
    }
}