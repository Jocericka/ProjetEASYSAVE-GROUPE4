using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using test;

class Program
{
    static void Main(string[] args)
    {
        // Création du dossier Output
        Directory.CreateDirectory("Output");

        var stateFilePath = "Output/state.json";
        var tracker = new StateTracker(stateFilePath);

        var stopwatch = Stopwatch.StartNew();

        // Initialisation du job
        tracker.StartJob("Backup-1", totalFiles: 3, totalSize: 3000);
        PrintStateFile(stateFilePath, stopwatch);
        Thread.Sleep(1000);

        // Fichier 1
        tracker.UpdateProgress("Backup-1", @"C:\source\file1.txt", @"D:\target\file1.txt", 1000);
        PrintStateFile(stateFilePath, stopwatch);
        Thread.Sleep(1000);

        // Fichier 2
        tracker.UpdateProgress("Backup-1", @"C:\source\file2.txt", @"D:\target\file2.txt", 1000);
        PrintStateFile(stateFilePath, stopwatch);
        Thread.Sleep(1000);

        // Fichier 3
        tracker.UpdateProgress("Backup-1", @"C:\source\file3.txt", @"D:\target\file3.txt", 1000);
        PrintStateFile(stateFilePath, stopwatch);
        Thread.Sleep(1000);

        // Fin du job
        tracker.EndJob("Backup-1");
        PrintStateFile(stateFilePath, stopwatch);

        Console.WriteLine("\n Sauvegarde terminée.");
    }

    static void PrintStateFile(string path, Stopwatch stopwatch)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine("❌ Le fichier state.json n'existe pas encore.\n");
            return;
        }

        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var stateDict = JsonSerializer.Deserialize<Dictionary<string, test.JobState>>(json, options);

        Console.Clear();
        Console.WriteLine("État en temps réel :\n");

        foreach (var kvp in stateDict)
        {
            var state = kvp.Value;

            double transferred = state.TotalSize - state.SizeRemaining;
            double percent = state.Progress;
            double seconds = stopwatch.Elapsed.TotalSeconds;

            Console.WriteLine($"Sauvegarde : {state.Name}");
            Console.WriteLine($"Statut : {state.Status}");
            Console.WriteLine($"Fichier en cours : {Path.GetFileName(state.CurrentSource)}");
            Console.WriteLine($"De : {state.CurrentSource}");
            Console.WriteLine($"Vers : {state.CurrentDestination}");
            Console.WriteLine($"Transféré : {transferred} o / {state.TotalSize} o");
            Console.WriteLine($"Progression : {GetProgressBar(percent)}");
            Console.WriteLine($"Temps écoulé : {seconds:F1} secondes");

            Console.WriteLine("\n-----------------------------------------\n");
        }
    }

    static string GetProgressBar(double percentage, int width = 30)
    {
        int filled = (int)(percentage / 100 * width);
        int empty = width - filled;
        return $"[{new string('█', filled)}{new string('░', empty)}] {percentage:0}%";
    }
}
