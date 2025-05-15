namespace test;

public enum BackupState
{
    Full,
    Differential
}

public class BackupJob
{
    public required string Name { get; set; }
    public required string SourcePath { get; set; }
    public required string TargetPath { get; set; }
    public required BackupJob Type { get; set; }
    public DateTime LastBackup { get; set; }
}
