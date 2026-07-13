using System.IO;
using System.Text.Json;

namespace AIAgentLib
{
    public class Config
    {
        public string OllamaUrl { get; set; } = "http://localhost:11434";
        public string ModelName { get; set; } = "llama3";

        private const string ConfigFileName = "AIAgent.json";

        public static Config Load(string pluginDirectory)
        {
            var path = Path.Combine(pluginDirectory, ConfigFileName);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Config>(json) ?? new Config();
            }
            return new Config();
        }
    }
}