using System;
using EasySave.Services;
using EasySave.Logger;
using EasySave.StateManager;

namespace EasySave.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Spécifiez le chemin du répertoire des logs
            string logDirectory = @"C:\Logs"; // Remplacez par le chemin souhaité pour les logs

            // Spécifiez le chemin du fichier d'état
            string stateFilePath = @"C:\State.json"; // Remplacez par le chemin souhaité pour le fichier d'état

            // Initialisation des services
            var logger = new Logger.Logger(logDirectory);
            var fileService = new FileService(logger);
            var stateTracker = new StateTracker(stateFilePath);

            while (true)
            {
                // Affiche le menu principal
                Menu.DisplayMainMenu();

                // Gère le choix de l'utilisateur
                Menu.HandleUserChoice(fileService, stateTracker);
            }
        }
    }
}