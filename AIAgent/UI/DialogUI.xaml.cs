using System.Windows;
using System.Windows.Input;

namespace AIAgent.UI
{
    public partial class DialogUI : Window
    {
        private DialogViewModel _vm;

        public DialogUI()
        {
            InitializeComponent();
            _vm = new DialogViewModel();
            DataContext = _vm;
            _vm.OnMessageAdded += () =>
            {
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    var sv = FindVisualChild<System.Windows.Controls.ScrollViewer>(lstMessages);
                    sv?.ScrollToEnd();
                }), System.Windows.Threading.DispatcherPriority.Background);
            };
            Loaded += (s, e) => txtInput.Focus();
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                if (_vm.SendCommand.CanExecute(null))
                    _vm.SendCommand.Execute(null);
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}