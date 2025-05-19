using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.StateManager
{
    public class JobState
    {
        public string? Name { get; set; }
        public string? Status { get; set; }
        public DateTime LastUpdate { get; set; }
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int FilesRemaining { get; set; }
        public long SizeRemaining { get; set; }
        public int Progress { get; set; }
        public string? CurrentSourceFile { get; set; }
        public string? CurrentTargetFile { get; set; }
    }

    public class StateTracker
    {
        private readonly string _filePath;
        private readonly Dictionary<string, JobState> _jobStates;

        public StateTracker(string filePath)
        {
            _filePath = filePath;
            _jobStates = new Dictionary<string, JobState>();
        }

        public void StartJob(string name, int totalFiles, long totalSize)
        {
            _jobStates[name] = new JobState
            {
                Name = name,
                Status = "Active",
                LastUpdate = DateTime.Now,
                TotalFiles = totalFiles,
                TotalSize = totalSize,
                FilesRemaining = totalFiles,
                SizeRemaining = totalSize,
                Progress = 0,
                CurrentSourceFile = "",
                CurrentTargetFile = ""
            };

            SaveState();
        }

        public void UpdateProgress(string jobName, string currentSourceFile, string currentTargetFile, long fileSize)
        {
            if (!_jobStates.ContainsKey(jobName)) return;

            var state = _jobStates[jobName];
            state.LastUpdate = DateTime.Now;
            state.CurrentSourceFile = currentSourceFile;
            state.CurrentTargetFile = currentTargetFile;
            state.FilesRemaining = Math.Max(0, state.FilesRemaining - 1);
            state.SizeRemaining = Math.Max(0, state.SizeRemaining - fileSize);
            state.Progress = (int)(((state.TotalSize - state.SizeRemaining) / (double)state.TotalSize) * 100);

            SaveState();
            DisplayProgress(state);
        }

        public void EndJob(string name)
        {
            if (_jobStates.ContainsKey(name))
            {
                var state = _jobStates[name];
                state.Status = "Completed";
                state.LastUpdate = DateTime.Now;
                state.Progress = 100;
                state.FilesRemaining = 0;
                state.SizeRemaining = 0;
                SaveState();
            }
        }

        private void SaveState()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(_jobStates, options);
            File.WriteAllText(_filePath, json);
        }

        private void DisplayProgress(JobState state)
        {
            Console.Clear();
            Console.WriteLine($"Job: {state.Name}");
            Console.WriteLine($"Status: {state.Status}");
            Console.WriteLine($"Files Remaining: {state.FilesRemaining}/{state.TotalFiles}");
            Console.WriteLine($"Size Remaining: {state.SizeRemaining} bytes");
            Console.WriteLine($"Current File: {state.CurrentSourceFile} -> {state.CurrentTargetFile}");
            Console.WriteLine($"Last Update: {state.LastUpdate}");

            // Affiche une barre de progression
            int progressBarWidth = 50; // Largeur de la barre de progression
            int progress = state.Progress;
            int filledBars = (progress * progressBarWidth) / 100;
            string progressBar = new string('█', filledBars) + new string('-', progressBarWidth - filledBars);

            Console.WriteLine($"Progress: [{progressBar}] {progress}%");
        }
    }
}