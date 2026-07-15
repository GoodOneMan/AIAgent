using AIAgent.Model;
using AIAgent.Structures;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using System.Xml.Linq;

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
        public EnterPoint() 
        {
            LoadDependencies();
        }

        public override int ExecuteCommand(string commandId, params string[] parameters)
        {
            if (commandId == "ID_AI_Agent")
            {
                ShowDialog();
            }
            return 0;
        }

        private void ShowDialog()
        {
            try
            {
                Window dialog = new UI.DialogUI();
                ElementHost.EnableModelessKeyboardInterop(dialog);
                dialog.Show();


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