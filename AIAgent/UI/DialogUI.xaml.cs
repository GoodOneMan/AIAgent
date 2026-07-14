using AIAgent.Model;
using AIAgent.Structures;
using AIAgentLib;
using Autodesk.Navisworks.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace AIAgent.UI
{
    public class Message : INotifyPropertyChanged
    {
        private string _text;
        public bool IsUser { get; set; }

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class UserBackgroundConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isUser && isUser)
                return new SolidColorBrush(Colors.LightBlue);
            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public partial class DialogUI : Window
    {
        private readonly OllamaClientWrapper _aiClient;
        private CancellationTokenSource _cts;
        private bool _isBusy;

        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotBusy)); }
        }
        public bool IsNotBusy => !IsBusy;

        public DialogUI()
        {
            InitializeComponent();
            DataContext = this;
            _aiClient = new OllamaClientWrapper(Configuration.Instance.OllamaUrl, Configuration.Instance.ModelName);
            Loaded += async (s, e) =>
            {
                txtInput.Focus();
                //await HandlerModel();
            };
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }

        private async void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                await SendMessageAsync();
            }
        }

        /// <summary>
        /// Прокручивает внешний ScrollViewer до самого низа.
        /// Использует Dispatcher для отложенного выполнения после перерисовки.
        /// </summary>
        private void ScrollToEnd()
        {
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    // Прокручиваем внешний скроллер до конца
                    MainScrollViewer.ScrollToEnd();
                }),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        /// <summary>
        /// Единый метод отправки сообщения. Если text == null, текст берётся из txtInput.
        /// </summary>
        private async Task SendMessageAsync(string text = null)
        {
            // Если текст не передан, берём из поля ввода
            if (string.IsNullOrEmpty(text))
                text = txtInput.Text.Trim();

            if (string.IsNullOrEmpty(text) || IsBusy)
                return;

            // Если текст передан извне (не из поля ввода), не очищаем поле
            if (text == txtInput.Text.Trim())
                txtInput.Clear();

            Messages.Add(new Message { IsUser = true, Text = text });
            txtInput.IsEnabled = false;
            btnSend.IsEnabled = false;
            IsBusy = true;

            _cts = new CancellationTokenSource();

            try
            {
                var assistantMessage = new Message { IsUser = false, Text = "" };
                Messages.Add(assistantMessage);

                await _aiClient.GetStreamingResponseAsync(
                    text,
                    token =>
                    {
                        assistantMessage.Text += token;
                        ScrollToEnd();
                    },
                    _cts.Token);

                ScrollToEnd(); // финальная прокрутка
            }
            catch (OperationCanceledException)
            {
                var last = Messages.Count > 0 ? Messages[Messages.Count - 1] : null;
                if (last != null && !last.IsUser)
                    last.Text += " (отменено)";
                ScrollToEnd();
            }
            catch (Exception ex)
            {
                var last = Messages.Count > 0 ? Messages[Messages.Count - 1] : null;
                if (last != null && !last.IsUser)
                    last.Text = $"Ошибка: {ex.Message}";
                else
                    Messages.Add(new Message { IsUser = false, Text = $"Ошибка: {ex.Message}" });
                ScrollToEnd();
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                IsBusy = false;
                txtInput.IsEnabled = true;
                btnSend.IsEnabled = true;
                txtInput.Focus();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        private async Task HandlerModel()
        {
            try
            {
                var elements = await Task.Run(() => new GeometryExtractor().Run());
                if (elements == null) return;

                var elementDtos = new List<ElementDto>();
                foreach (var item in elements)
                {
                    var extracted = new InformationExtractor(item.Value).GetElements();
                    foreach (var elem in extracted)
                    {
                        var dto = new ElementDto(elem);
                        if (dto.Properties.Count > 0)
                            elementDtos.Add(dto);
                    }
                }

                var json = Configuration.Instance.GetJsonElementDto(elementDtos);
                await SendMessageAsync(json);
            }
            catch (Exception ex)
            {
                Messages.Add(new Message { IsUser = false, Text = $"Ошибка: {ex.Message}" });
                ScrollToEnd();
            }
        }
    }
}