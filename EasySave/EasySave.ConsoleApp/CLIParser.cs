using System;
using System.Linq;

namespace EasySave.ConsoleApp
{
    public static class CLIParser
    {
        /// <summary>
        /// Analyse les arguments de la ligne de commande ou les entrées utilisateur pour déterminer les indices des travaux de sauvegarde à exécuter.
        /// </summary>
        /// <param name="input">Chaîne d'entrée de l'utilisateur (ex : "1-3" ou "4-5").</param>
        /// <returns>Un tableau d'indices des travaux de sauvegarde.</returns>
        public static int[] ParseBackupJobs(string input)
        {
            try
            {
                if (input.Contains('-'))
                {
                    // Gestion des plages (ex : "1-3" ou "4-5")
                    var range = input.Split('-').Select(int.Parse).ToArray();
                    if (range.Length == 2 && range[0] >= 1 && range[1] <= 5 && range[0] <= range[1])
                    {
                        return Enumerable.Range(range[0], range[1] - range[0] + 1).ToArray();
                    }
                }
                else if (input.Contains(';'))
                {
                    // Gestion des indices séparés par des points-virgules (ex : "1;3;5")
                    var indices = input.Split(';').Select(int.Parse).ToArray();
                    if (indices.All(index => index >= 1 && index <= 5))
                    {
                        return indices;
                    }
                }
                else
                {
                    // Gestion d'un seul index (ex : "1")
                    int singleIndex = int.Parse(input);
                    if (singleIndex >= 1 && singleIndex <= 5)
                    {
                        return new[] { singleIndex };
                    }
                }
            }
            catch
            {
                Console.WriteLine("Invalid input format. Use formats like '1-3', '4-5', or '1;3;5'.");
            }

            return Array.Empty<int>();
        }
    }
}