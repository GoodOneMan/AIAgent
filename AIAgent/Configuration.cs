using AIAgent.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace AIAgent
{
    public class Configuration
    {
        private const string ConfigFileName = "Configuration.json";
        private static readonly Lazy<Configuration> _instance = new Lazy<Configuration>(Init);

        public static Configuration Instance => _instance.Value;

        private static Configuration Init()
        {
            string path = Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                ConfigFileName);
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    return JsonConvert.DeserializeObject<Configuration>(json) ?? new Configuration();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Ошибка загрузки конфигурации: {ex.Message}");
            }
            return new Configuration();
        }

        

        public string OllamaUrl { get; set; } = "http://192.168.2.162:11434";
        public string ModelName { get; set; } = "qwen2.5-coder:14b";
        public int GeometryHandler { get; set; } = 1;
        public int Optimization { get; set; } = 0;
        public List<string> Categories { get; set; } = new List<string>();

        // Метод перенесён в отдельный сервис, но оставлен для совместимости
        public string GetJsonElementDto(List<ElementDto> elementDtos)
        {
            try
            {
                return JsonConvert.SerializeObject(elementDtos, Formatting.Indented);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Ошибка сериализации: {ex.Message}");
                return string.Empty;
            }
        }

        //hf.co/yuxinlu1/gemma-4-12B-coder-fable5-composer2.5-v1-GGUF:Q8_0
        //hf.co/google/gemma-4-12B-it-qat-q4_0-gguf:Q4_0
        //hf.co/huihui-ai/Huihui-Qwythos-9B-Claude-Mythos-5-1M-abliterated-GGUF:Q8_0
        //hf.co/yuxinlu1/gemma-4-12B-agentic-fable5-composer2.5-v2-3.5x-tau2-GGUF:Q8_0
        //hf.co/empero-ai/Qwythos-9B-Claude-Mythos-5-1M-GGUF:Q4_K_M
        //hf.co/empero-ai/Qwythos-9B-Claude-Mythos-5-1M-GGUF:Q8_0
        //gemma4-heavy:latest
        //gemma4-abliterated:latest
        //nomic-embed-text:latest
        //qwen2.5:7b
        //qwen2.5:14b
        //gemma3:12b
        //deepseek-r1:14b
        //qwen2.5-coder:14b
        public string[] GetModelsName()
        {
            return new string[] {
                "hf.co/yuxinlu1/gemma-4-12B-coder-fable5-composer2.5-v1-GGUF:Q8_0",
                "hf.co/google/gemma-4-12B-it-qat-q4_0-gguf:Q4_0",
                "hf.co/huihui-ai/Huihui-Qwythos-9B-Claude-Mythos-5-1M-abliterated-GGUF:Q8_0",
                "hf.co/yuxinlu1/gemma-4-12B-agentic-fable5-composer2.5-v2-3.5x-tau2-GGUF:Q8_0",
                "hf.co/empero-ai/Qwythos-9B-Claude-Mythos-5-1M-GGUF:Q4_K_M",
                "hf.co/empero-ai/Qwythos-9B-Claude-Mythos-5-1M-GGUF:Q8_0",
                "gemma4-heavy:latest",
                "gemma4-abliterated:latest",
                "nomic-embed-text:latest",
                "qwen2.5:7b",
                "qwen2.5:14b",
                "gemma3:12b",
                "deepseek-r1:14b",
                "qwen2.5-coder:14b"
            };
        }
    }
}