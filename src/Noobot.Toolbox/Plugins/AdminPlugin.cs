using System.Collections.Generic;
using Common.Logging;
using Noobot.Core.Configuration;
using Noobot.Core.Plugins;

namespace Noobot.Toolbox.Plugins
{
    /// <summary>
    /// Given a user authenticated, this plugin can give users extra abilities/functions.
    /// A good example of how Middleware and Plugins can work together.
    /// </summary>
    public class AdminPlugin : IPlugin
    {
        private readonly IConfigReader _configReader;
        private readonly ILog _log;
        private readonly HashSet<string> _admins = new HashSet<string>();
        private readonly object _lock = new object();
        private int? _adminPin;

        public AdminPlugin(IConfigReader configReader, ILog log)
        {
            _configReader = configReader;
            _log = log;
        }

        public void Start()
        {
            _adminPin = _configReader.GetConfigEntry<int?>("adminPin");

            if (_adminPin.HasValue)
            {
                _log.Info($"Admin pin is '{_adminPin.Value}'");
            }
            else
            {
                _log.Info("No admin pin detected. Admin mode deactivated.");
            }
        }

        public void Stop()
        { }

        public bool AdminModeEnabled()
        {
            return _adminPin.HasValue;
        }

        public bool AuthoriseUser(string userId, int pin)
        {
            bool authorised = false;

            if (_adminPin.HasValue)
            {
                authorised = pin == _adminPin.Value;

                if (authorised)
                {
                    lock (_lock)
                    {
                        if (!_admins.Contains(userId))
                        {
                            _admins.Add(userId);
                        }
                    }
                }
            }

            return authorised;
        }

        public bool AuthenticateUser(string userId)
        {
            lock (_lock)
            {
                return _admins.Contains(userId);
            }
        }
    }
}