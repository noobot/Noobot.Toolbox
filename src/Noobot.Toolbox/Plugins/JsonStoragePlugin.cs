using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Noobot.Core.Plugins;

namespace Noobot.Toolbox.Plugins
{
    public class JsonStoragePlugin : IPlugin
    {
        private readonly ILogger _log;
        private string _directory;

        public JsonStoragePlugin(ILogger log)
        {
            _log = log;
        }

        public void Start()
        {
            _directory = Path.Combine(Environment.CurrentDirectory, "data");
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }
        }

        public void Stop()
        {
        }

        public T[] ReadFile<T>(string fileName) where T : class, new()
        {
            string filePath = GetFilePath(fileName);
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }

            T[] result = new T[0];

            try
            {
                string file = File.ReadAllText(filePath);

                if (!string.IsNullOrEmpty(file))
                {
                    result = JsonConvert.DeserializeObject<T[]>(file);
                }
            }
            catch (Exception ex)
            {
                _log.LogInformation($"Error loading file '{filePath}' - {ex}");
            }

            return result;
        }

        public void SaveFile<T>(string fileName, T[] objects) where T : class, new()
        {
            string filePath = GetFilePath(fileName);
            File.WriteAllText(filePath, JsonConvert.SerializeObject(objects, Formatting.Indented));
        }
        
        public void DeleteFile(string fileName)
        {
            string filePath = GetFilePath(fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(_directory, fileName + ".json");
        }
    }
}