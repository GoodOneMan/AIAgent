using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ModelBenchmark
{
    /// <summary>
    /// Простая обёртка для взаимодействия с локальным сервером Ollama.
    /// Использует HttpClient для отправки HTTP-запросов.
    /// </summary>
    public class OllamaClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        /// <summary>
        /// Конструктор, принимает базовый адрес Ollama API (по умолчанию http://192.168.2.162:11434)
        /// </summary>
        public OllamaClient(string baseUrl = "http://192.168.2.162:11434")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            // Устанавливаем таймаут, чтобы запросы не висели бесконечно
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Получает список доступных моделей из Ollama.
        /// Обращается к эндпоинту GET /api/tags.
        /// </summary>
        /// <returns>Список имён моделей (например, ["llama2:latest", "mistral:7b"])</returns>
        public async Task<List<string>> GetAvailableModelsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                // Парсим JSON: {"models":[{"name":"llama2:latest"}, ...]}
                JObject obj = JObject.Parse(json);
                var models = obj["models"] as JArray;
                if (models == null)
                    return new List<string>();

                var result = new List<string>();
                foreach (var item in models)
                {
                    string name = item["name"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                        result.Add(name);
                }
                return result;
            }
            catch (Exception ex)
            {
                // Если сервер недоступен – возвращаем пустой список
                Console.WriteLine($"Ошибка при получении списка моделей: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Отправляет запрос к указанной модели и возвращает сгенерированный ответ.
        /// Использует эндпоинт POST /api/generate.
        /// </summary>
        /// <param name="model">Имя модели</param>
        /// <param name="prompt">Текст запроса</param>
        /// <returns>Строка с ответом модели</returns>
        public async Task<string> SendPromptAsync(string model, string prompt)
        {
            // Формируем JSON-тело запроса согласно спецификации Ollama:
            // { "model": "llama2", "prompt": "Hello", "stream": false }
            var requestBody = new
            {
                model = model,
                prompt = prompt,
                stream = false // отключаем потоковую передачу, чтобы получить сразу полный ответ
            };

            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync();
            JObject obj = JObject.Parse(responseJson);
            // В ответе приходит {"response": "текст ответа", ...}
            string answer = obj["response"]?.ToString();
            return answer ?? string.Empty;
        }
    }
}