using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ModelBenchmark
{
    /// <summary>
    /// Выполняет тестовые запросы для списка моделей с замером времени.
    /// </summary>
    public class ModelTester
    {
        private readonly OllamaClient _client;

        public ModelTester(OllamaClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Асинхронно прогоняет один и тот же промпт через все переданные модели.
        /// </summary>
        /// <param name="models">Список имён моделей</param>
        /// <param name="prompt">Вопрос/запрос</param>
        /// <returns>Коллекция результатов ModelTestResult</returns>
        public async Task<List<ModelTestResult>> RunTestsAsync(IEnumerable<string> models, string prompt)
        {
            var results = new List<ModelTestResult>();

            foreach (string model in models)
            {
                var result = new ModelTestResult { ModelName = model };

                // Засекаем время
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    // Отправляем запрос
                    string response = await _client.SendPromptAsync(model, prompt);
                    stopwatch.Stop();

                    result.IsSuccess = true;
                    result.Response = response;
                    result.ElapsedTime = stopwatch.Elapsed;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    result.ElapsedTime = stopwatch.Elapsed; // сколько времени прошло до ошибки
                }

                results.Add(result);

                // Небольшая задержка между запросами, чтобы не перегружать Ollama (опционально)
                await Task.Delay(500);
            }

            return results;
        }
    }
}