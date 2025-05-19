using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EasySave.Services;
using EasySave.Models;
using EasySave.StateManager;

namespace EasySave.ConsoleApp
{
    public static class Menu
    {
        private static List<BackupJob> backupJobs = new List<BackupJob>();

        public static void DisplayMainMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("========================================");
            Console.WriteLine("         Welcome to EasySave!           ");
            Console.WriteLine("========================================");
            Console.ResetColor();
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Create and execute a backup job");
            Console.WriteLine("2. Execute a specific backup job");
            Console.WriteLine("3. Execute a range of backup jobs (e.g., 1-3 or 4-5)");
            Console.WriteLine("4. Exit");
            Console.WriteLine("========================================");
            Console.Write("Your choice: ");
        }

        public static void HandleUserChoice(FileService fileService, StateTracker stateTracker)
        {
            string choice = Console.ReadLine() ?? string.Empty;

            switch (choice)
            {
                case "1":
                    CreateAndExecuteBackupJob(fileService, stateTracker);
                    break;

                case "2":
                    ExecuteSpecificBackupJob(fileService, stateTracker);
                    break;

                case "3":
                    ExecuteBackupJobRange(fileService, stateTracker);
                    break;

                case "4":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Exiting... Thank you for using EasySave!");
                    Console.ResetColor();
                    Environment.Exit(0);
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid choice. Please try again.");
                    Console.ResetColor();
                    break;
            }
        }

        private static void CreateAndExecuteBackupJob(FileService fileService, StateTracker stateTracker)
        {
            try
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("=== Create and Execute a Backup Job ===");
                Console.ResetColor();

                Console.Write("Enter the name of the backup job: ");
                string name = Console.ReadLine()!;

                string sourceDir = GetValidDirectory("Enter the source directory: ");
                string targetDir = GetValidDirectory("Enter the target directory: ", createIfNotExists: true);

                Console.WriteLine("Choose the backup type:");
                Console.WriteLine("1. Full backup");
                Console.WriteLine("2. Differential backup");
                Console.Write("Your choice: ");
                string backupTypeChoice = Console.ReadLine()!;

                bool isDifferential = backupTypeChoice == "2";
                BackupType backupType = isDifferential ? BackupType.Differential : BackupType.Full;

                var backupJob = new BackupJob(name, sourceDir, targetDir, backupType);
                backupJobs.Add(backupJob);

                string[] files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
                int totalFiles = files.Length;
                long totalSize = 0;
                foreach (var file in files)
                {
                    totalSize += new FileInfo(file).Length;
                }

                stateTracker.StartJob(name, totalFiles, totalSize);

                Stopwatch stopwatch = Stopwatch.StartNew();

                foreach (var file in files)
                {
                    string relativePath = Path.GetRelativePath(sourceDir, file);
                    string destinationFile = Path.Combine(targetDir, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
                    File.Copy(file, destinationFile, true);

                    long fileSize = new FileInfo(file).Length;
                    stateTracker.UpdateProgress(name, file, destinationFile, fileSize);
                }

                stopwatch.Stop();
                stateTracker.EndJob(name);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("========================================");
                Console.WriteLine($"Backup job '{name}' executed successfully!");
                Console.WriteLine($"Total Files: {totalFiles}");
                Console.WriteLine($"Total Size: {totalSize} bytes");
                Console.WriteLine($"Time Taken: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
                Console.WriteLine("========================================");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static string GetValidDirectory(string prompt, bool createIfNotExists = false)
        {
            while (true)
            {
                Console.Write(prompt);
                string? directory = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    if (Directory.Exists(directory))
                    {
                        return directory;
                    }
                    else if (createIfNotExists)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Warning: The directory '{directory}' does not exist. It will be created.");
                        Console.ResetColor();
                        Directory.CreateDirectory(directory);
                        return directory;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: The directory '{directory}' does not exist. Please enter a valid directory.");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: The directory path cannot be empty. Please try again.");
                    Console.ResetColor();
                }
            }
        }

        private static void ExecuteSpecificBackupJob(FileService fileService, StateTracker stateTracker)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=== Execute a Specific Backup Job ===");
            Console.ResetColor();

            if (backupJobs.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No backup jobs available. Please create a backup job first.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("Available backup jobs:");
            for (int i = 0; i < backupJobs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {backupJobs[i].Name}");
            }
            Console.Write("Enter the number of the backup job to execute: ");
            if (int.TryParse(Console.ReadLine(), out int jobIndex) && jobIndex > 0 && jobIndex <= backupJobs.Count)
            {
                var backupJob = backupJobs[jobIndex - 1];
                string[] files = Directory.GetFiles(backupJob.SourceDirectory, "*", SearchOption.AllDirectories);
                int totalFiles = files.Length;
                long totalSize = 0;
                foreach (var file in files)
                {
                    totalSize += new FileInfo(file).Length;
                }

                stateTracker.StartJob(backupJob.Name, totalFiles, totalSize);

                Stopwatch stopwatch = Stopwatch.StartNew();

                foreach (var file in files)
                {
                    string relativePath = Path.GetRelativePath(backupJob.SourceDirectory, file);
                    string destinationFile = Path.Combine(backupJob.TargetDirectory, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
                    File.Copy(file, destinationFile, true);

                    long fileSize = new FileInfo(file).Length;
                    stateTracker.UpdateProgress(backupJob.Name, file, destinationFile, fileSize);
                }

                stopwatch.Stop();
                stateTracker.EndJob(backupJob.Name);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("========================================");
                Console.WriteLine($"Backup job '{backupJob.Name}' executed successfully!");
                Console.WriteLine($"Total Files: {totalFiles}");
                Console.WriteLine($"Total Size: {totalSize} bytes");
                Console.WriteLine($"Time Taken: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
                Console.WriteLine("========================================");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid selection. Please try again.");
                Console.ResetColor();
            }
        }

        private static void ExecuteBackupJobRange(FileService fileService, StateTracker stateTracker)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=== Execute a Range of Backup Jobs ===");
            Console.ResetColor();

            if (backupJobs.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No backup jobs available. Please create a backup job first.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("Available backup jobs:");
            for (int i = 0; i < backupJobs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {backupJobs[i].Name}");
            }
            Console.Write("Enter the range of backup jobs to execute (e.g., 1-3): ");
            string? rangeInput = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(rangeInput) && rangeInput.Contains('-'))
            {
                var parts = rangeInput.Split('-');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int start) &&
                    int.TryParse(parts[1], out int end) &&
                    start > 0 && end > 0 && start <= end && end <= backupJobs.Count)
                {
                    for (int i = start - 1; i < end; i++)
                    {
                        var backupJob = backupJobs[i];
                        string[] files = Directory.GetFiles(backupJob.SourceDirectory, "*", SearchOption.AllDirectories);
                        int totalFiles = files.Length;
                        long totalSize = 0;
                        foreach (var file in files)
                        {
                            totalSize += new FileInfo(file).Length;
                        }

                        stateTracker.StartJob(backupJob.Name, totalFiles, totalSize);

                        Stopwatch stopwatch = Stopwatch.StartNew();

                        foreach (var file in files)
                        {
                            string relativePath = Path.GetRelativePath(backupJob.SourceDirectory, file);
                            string destinationFile = Path.Combine(backupJob.TargetDirectory, relativePath);

                            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
                            File.Copy(file, destinationFile, true);

                            long fileSize = new FileInfo(file).Length;
                            stateTracker.UpdateProgress(backupJob.Name, file, destinationFile, fileSize);
                        }

                        stopwatch.Stop();
                        stateTracker.EndJob(backupJob.Name);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("========================================");
                        Console.WriteLine($"Backup job '{backupJob.Name}' executed successfully!");
                        Console.WriteLine($"Total Files: {totalFiles}");
                        Console.WriteLine($"Total Size: {totalSize} bytes");
                        Console.WriteLine($"Time Taken: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
                        Console.WriteLine("========================================");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid range. Please enter a valid range (e.g., 1-3).");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid input. Please enter a valid range (e.g., 1-3).");
                Console.ResetColor();
            }
        }
    }
}