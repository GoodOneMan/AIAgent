using AIAgent.Structures;
using Autodesk.Navisworks.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Windows.Documents;

namespace AIAgent
{
    public class Configuration
    {
        private const string ConfigFileName = "Configuration.json";
        private static Configuration _instance = null;
        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Init();

                return _instance;
            }
        }

        private static Configuration Init()
        {
            string path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), ConfigFileName);
            try
            {
                if (File.Exists(path))
                {
                    return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(path));
                }
            }
            catch (System.Exception ex)
            {

            }
            
            return new Configuration();
        }

        public string OllamaUrl { get; set; } = "http://192.168.2.162:11434";
        public string ModelName { get; set; } = "qwen2.5-coder:14b";
        public int GeometryHandler { get; set; } = 1;
        public int Optimization { get; set; } = 0;
        
        public List<string> Categories { get; set; } = new List<string>();

        public string GetJsonElementDto(List<ElementDto> elementDtos)
        {
            string json = "";
            try
            {
                json = JsonConvert.SerializeObject(elementDtos, Formatting.Indented);
            }
            catch(Exception ex)
            {

            }
            return json;
        }
    }
}
