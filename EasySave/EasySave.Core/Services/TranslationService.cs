using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace EasySave.Services
{
    public class TranslationService
    {
        private readonly Dictionary<string, string>? translations;

        public TranslationService(string language)
        {
            // Utilise un chemin absolu pour éviter les problèmes de chemin relatif
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", $"{language}.json");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Translation file not found: {filePath}");
            }

            var jsonContent = File.ReadAllText(filePath);
            translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
        }

        public string GetTranslation(string key, params object[] args)
        {
            if (translations != null && translations.TryGetValue(key, out var value))
            {
                return string.Format(value, args);
            }
            return key; // Retourne la clé si la traduction n'est pas trouvée
        }
    }
}