using AIAgentLib;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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

            var pluginDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var config = Config.Load(pluginDir);
            _aiClient = new OllamaClientWrapper(config.OllamaUrl, config.ModelName);

            Loaded += (s, e) => txtInput.Focus();
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }

        private async void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            // Отправка по Ctrl+Enter
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                await SendMessageAsync();
            }
        }

        private async Task SendMessageAsync()
        {
            string text = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(text) || IsBusy)
                return;

            Messages.Add(new Message { IsUser = true, Text = text });
            txtInput.Clear();
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
                        if (lstMessages.Items.Count > 0)
                            lstMessages.ScrollIntoView(lstMessages.Items[lstMessages.Items.Count - 1]);
                    },
                    _cts.Token);
            }
            catch (OperationCanceledException)
            {
                var last = Messages[Messages.Count -1];
                if (last.IsUser == false)
                    last.Text += " (отменено)";
            }
            catch (Exception ex)
            {
                var last = Messages[Messages.Count - 1];
                if (last.IsUser == false)
                    last.Text = $"Ошибка: {ex.Message}";
                else
                    Messages.Add(new Message { IsUser = false, Text = $"Ошибка: {ex.Message}" });
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
    }
}