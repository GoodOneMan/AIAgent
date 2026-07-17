using System;

namespace ModelBenchmark
{
    /// <summary>
    /// Результат тестирования одной модели.
    /// </summary>
    public class ModelTestResult
    {
        /// <summary>Имя модели (например, "llama2:latest")</summary>
        public string ModelName { get; set; }

        /// <summary>Текст ответа модели</summary>
        public string Response { get; set; }

        /// <summary>Затраченное время на выполнение запроса</summary>
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>Флаг успешности запроса</summary>
        public bool IsSuccess { get; set; }

        /// <summary>Сообщение об ошибке (если IsSuccess == false)</summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Возвращает строковое представление результата для вывода в консоль.
        /// </summary>
        public override string ToString()
        {
            if (IsSuccess)
            {
                return $@"Модель: {ModelName}
Время : {ElapsedTime.TotalSeconds:F2} сек.
Ответ :
{Response}";
            }
            else
            {
                return $@"Модель: {ModelName} - ОШИБКА
{ErrorMessage}";
            }
        }
    }
}