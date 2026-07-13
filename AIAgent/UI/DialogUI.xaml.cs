using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AIAgent.UI
{
    /// <summary>
    /// Логика взаимодействия для DialogUI.xaml
    /// </summary>
    public partial class DialogUI : Window
    {
        private readonly AIAgentLib.OllamaClientWrapper _chatClient;

        public DialogUI()
        {
            InitializeComponent();

            _chatClient = new AIAgentLib.OllamaClientWrapper();
            Loaded += (s, e) => txtInput.Focus();
        }
        
        /// <summary>
        /// Отправляет промт в модель Ollama и возвращает ответ.
        /// </summary>
        public async Task<string> Response(string promt)
        {
            var response = await _chatClient.Response(promt);
            return response;
        }

        public async Task<string> ResponseChat(string promt)
        {
            var response = await _chatClient.ResponseChat(promt);
            return response;
        }

        /// <summary>
        /// Обработка отправки сообщения (асинхронная).
        /// </summary>
        private async Task SendMessageAsync()
        {
            string text = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(text))
                return;

            // Добавляем сообщение пользователя
            lstMessages.Items.Add($"Вы: {text}");
            txtInput.Clear();
            txtInput.IsEnabled = false; // блокируем ввод на время запроса
            btnSend.IsEnabled = false;

            try
            {

                string answer = await ResponseChat(text);
                lstMessages.Items.Add($"Собеседник: {answer}");
            }
            catch (Exception ex)
            {
                // Если произошла ошибка, покажем её в диалоге
                lstMessages.Items.Add($"⚠️ Ошибка: {ex.Message}");
            }
            finally
            {
                txtInput.IsEnabled = true;
                btnSend.IsEnabled = true;
                txtInput.Focus();
            }

            // Прокручиваем список вниз
            if (lstMessages.Items.Count > 0)
                lstMessages.ScrollIntoView(lstMessages.Items[lstMessages.Items.Count -1]);
        }


        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }


        private async void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendMessageAsync();
                e.Handled = true;
            }
        }
    }
}
