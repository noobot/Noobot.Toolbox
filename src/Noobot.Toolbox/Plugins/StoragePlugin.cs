using System;
using System.IO;
using Newtonsoft.Json;
using Noobot.Core.Logging;
using Noobot.Core.Plugins;

namespace Noobot.Toolbox.Plugins
{
    public class StoragePlugin : IPlugin
    {
        private readonly ILog _log;
        private string _directory;

        public StoragePlugin(ILog log)
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
                result = JsonConvert.DeserializeObject<T[]>(File.ReadAllText(filePath));
            }
            catch (Exception ex)
            {
                _log.Log($"Error loading file '{filePath}' - {ex}");
            }

            return result;
        }

        public void SaveFile<T>(string fileName, T[] objects) where T : class, new()
        {
            string filePath = GetFilePath(fileName);
            File.WriteAllText(filePath, JsonConvert.SerializeObject(objects));
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
            return Path.Combine(_directory, fileName + ".txt");
        }
    }
}