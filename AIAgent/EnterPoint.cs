using Autodesk.Navisworks.Api.Plugins;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace AIAgent
{
    [Plugin("AIAgent", "TZ", DisplayName = "Assistant")]
    [Strings("template.name")]
    [RibbonLayout("template.xaml")]
    [RibbonTab("ID_Template_Tab")]
    [Command("ID_Select_Button", LargeIcon = @"Images\Select_Button.ico", ToolTip = "Выбор действия")]
    [Command("ID_AI_Agent", LargeIcon = @"Images\ai_icon.ico", ToolTip = "агент")]
    public class EnterPoint : CommandHandlerPlugin
    {
        public override int ExecuteCommand(string commandId, params string[] parameters)
        {
            if (commandId == "ID_AI_Agent")
            {
                LoadDependencies();
                ShowDialog();
            }
            return 0;
        }

        private void ShowDialog()
        {
            try
            {
                var dialog = new UI.DialogUI();
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть диалог: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDependencies()
        {
            var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(pluginDir))
                return;

            var assemblyFolder = Path.Combine(pluginDir, "Assembly");
            if (!Directory.Exists(assemblyFolder))
                return;

            foreach (var dllPath in Directory.GetFiles(assemblyFolder, "*.dll"))
            {
                try
                {
                    Assembly.LoadFrom(dllPath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки {dllPath}: {ex.Message}");
                }
            }
        }
    }
}