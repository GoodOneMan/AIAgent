using AIAgent.Model;
using AIAgent.Structures;
using AIAgentLib;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AIAgent.UI
{
    public class DialogViewModel : INotifyPropertyChanged
    {
        private readonly OllamaClientWrapper _aiClient;
        private CancellationTokenSource _cts;
        private bool _isBusy;
        private string _inputText;

        public DialogViewModel()
        {
            _aiClient = new OllamaClientWrapper(Configuration.Instance.OllamaUrl, Configuration.Instance.ModelName);
            Messages = new ObservableCollection<Message>();
            SendCommand = new RelayCommand(async () => await SendMessageAsync(), () => CanSend);
            CancelCommand = new RelayCommand(() => _cts?.Cancel(), () => IsBusy);
            LoadModelCommand = new RelayCommand(async () => await LoadModelDataAsync(), () => !IsBusy);
        }

        public ObservableCollection<Message> Messages { get; }

        public string InputText
        {
            get => _inputText;
            set { _inputText = value; OnPropertyChanged(); (SendCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotBusy)); RaiseCommandsCanExecute(); }
        }
        public bool IsNotBusy => !IsBusy;

        public ICommand SendCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand LoadModelCommand { get; }

        private bool CanSend => !IsBusy && !string.IsNullOrWhiteSpace(InputText);

        private void RaiseCommandsCanExecute()
        {
            (SendCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (LoadModelCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task SendMessageAsync()
        {
            if (!CanSend) return;
            var userText = InputText.Trim();
            InputText = string.Empty;
            Messages.Add(new Message { IsUser = true, Text = userText });
            IsBusy = true;
            _cts = new CancellationTokenSource();

            var assistantMsg = new Message { IsUser = false, Text = "" };
            Messages.Add(assistantMsg);

            try
            {
                await _aiClient.GetStreamingResponseAsync(
                    userText,
                    token => { assistantMsg.Text += token; OnMessageAdded?.Invoke(); },
                    _cts.Token);
            }
            catch (OperationCanceledException)
            {
                assistantMsg.Text += " (отменено)";
            }
            catch (Exception ex)
            {
                assistantMsg.Text = $"Ошибка: {ex.Message}";
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                IsBusy = false;
                OnMessageAdded?.Invoke();
            }
        }

        private async Task LoadModelDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var elements = await Task.Run(() => new GeometryExtractor().Run());
                if (elements == null || elements.Count == 0)
                {
                    Messages.Add(new Message { IsUser = false, Text = "Не удалось извлечь геометрию." });
                    OnMessageAdded?.Invoke();
                    return;
                }

                var dtos = new System.Collections.Generic.List<ElementDto>();
                foreach (var kvp in elements)
                {
                    var extracted = new InformationExtractor(kvp.Value).GetElements();
                    foreach (var elem in extracted)
                    {
                        var dto = new ElementDto(elem);
                        if (dto.Properties.Count > 0) dtos.Add(dto);
                    }
                }
                if (dtos.Count > 100) dtos = dtos.GetRange(0, 100);
                var json = Configuration.Instance.GetJsonElementDto(dtos);
                if (string.IsNullOrEmpty(json))
                    Messages.Add(new Message { IsUser = false, Text = "Не удалось сериализовать данные." });
                else
                {
                    InputText = json;
                    await SendMessageAsync();
                }
            }
            catch (Exception ex)
            {
                Messages.Add(new Message { IsUser = false, Text = $"Ошибка: {ex.Message}" });
            }
            finally
            {
                IsBusy = false;
                OnMessageAdded?.Invoke();
            }
        }

        public event Action OnMessageAdded;
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}