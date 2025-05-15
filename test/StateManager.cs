using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace test
{
    public class JobState
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdate { get; set; }
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int FilesRemaining { get; set; }
        public long SizeRemaining { get; set; }
        public int Progress { get; set; }
        public string CurrentSource { get; set; }
        public string CurrentDestination { get; set; }
    }

    public class StateTracker
    {
        private readonly string _filePath;
        private readonly Dictionary<string, JobState> jobStates;

        public StateTracker(string filePath)
        {
            _filePath = filePath;
            jobStates = new Dictionary<string, JobState>();
        }

        public void StartJob(string name, int totalFiles, long totalSize)
        {
            jobStates[name] = new JobState
            {
                Name = name,
                Status = "Active",
                LastUpdate = DateTime.Now,
                TotalFiles = totalFiles,
                TotalSize = totalSize,
                FilesRemaining = totalFiles,
                SizeRemaining = totalSize,
                Progress = 0,
                CurrentSource = "",
                CurrentDestination = ""
            };

            SaveState();
        }

        public void UpdateProgress(string jobName, string currentSource, string currentDestination, long fileSize)
        {
            if (!jobStates.ContainsKey(jobName)) return;

            var state = jobStates[jobName];
            state.LastUpdate = DateTime.Now;
            state.CurrentSource = currentSource;
            state.CurrentDestination = currentDestination;
            state.FilesRemaining = Math.Max(0, state.FilesRemaining - 1);
            state.SizeRemaining = Math.Max(0, state.SizeRemaining - fileSize);
            state.Progress = (int)(((state.TotalSize - state.SizeRemaining) / (double)state.TotalSize) * 100);

            SaveState();
        }

        public void EndJob(string name)
        {
            if (jobStates.ContainsKey(name))
            {
                var state = jobStates[name];
                state.Status = "Non Actif";
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
            string json = JsonSerializer.Serialize(jobStates, options);

            // Ajout de sauts de ligne entre les éléments JSON pour meilleure lisibilité
            json = json.Replace("},", "},\n");

            File.WriteAllText(_filePath, json);
        }
    }
}
