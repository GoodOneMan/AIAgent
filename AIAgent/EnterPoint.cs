using Autodesk.Navisworks.Api.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            LoadAssembly();

            switch (commandId)
            {
                case "ID_AI_Agent":
                    //RunAgent();
                    ShowDialog();
                    break;
            }
            return 0;
        }

        public void ShowDialog()
        {
            var dialog_ui = new UI.DialogUI();
            dialog_ui.ShowDialog();
            //dialog_ui.Show();
        }

        public async Task RunAgent()
        {
            try
            {
                AIAgentLib.EntryPoint entryPoint = new AIAgentLib.EntryPoint();

                //var result = await entryPoint.Start();
                //var result = await entryPoint.Response("что знаешь о программе Navisworks, расскажи подробно, выведи в виде таблицы в консоль");
                var result = await entryPoint.Response("найди информацию в интернете о программе Navisworks, выведи ссылки");

                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void LoadAssembly()
        {
            foreach (string path in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "Assembly"))
            {
                try
                {
                    Assembly.LoadFrom(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }
    }
}
