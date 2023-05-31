using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Xml;
using log4net;
using log4net.Config;

namespace EvertecApi.Log4net
{
    public class LoggerManager : ILoggerManager
    {

        private readonly ILog _log = LogManager.GetLogger(typeof(LoggerManager));
        public LoggerManager()
        {
            try
            {
                XmlDocument log4netConfig = new XmlDocument();

                using (var fs = File.OpenRead("log4net.config"))
                {
                    log4netConfig.Load(fs);

                    var repo = LogManager.CreateRepository(
                            Assembly.GetEntryAssembly(),
                            typeof(log4net.Repository.Hierarchy.Hierarchy));
                    //Configuracion de la ruta.
                    string ruta = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                    GlobalContext.Properties["FilePath"] = ruta.Substring(6, ruta.Length - 6);
                    
                    XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error", ex);
            }
        }

        public void LogAdvertencia(string message)
        {
            _log.Warn(message);
        }

        public void LogAdvertencia(string message, Exception ex)
        {
            _log.Warn(message, ex);
        }

        public void LogError(string message)
        {
            _log.Error(message);
        }

        public void LogError(string message, Exception ex)
        {
            _log.Error(message, ex);
        }

        public void LogInformation(string message)
        {
            _log.Info(message);
        }

        public void LogInformation(string message, Exception ex)
        {
            _log.Info(message, ex);
        }
    }
}
