using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Log {
    public class Logger: LoggerBase {
        protected bool _initialized = false;

        public override void Configure() {
            log4net.Config.XmlConfigurator.Configure();
            _initialized = true;
        }

        public override void Configure(string configFile) {
            if (!File.Exists(configFile)) {
                Console.WriteLine(string.Format("The {0} configuration file for log4net not found.", configFile));
                _initialized = false;
                return;
            }

            FileInfo fileInfo = new FileInfo(configFile);
            log4net.Config.XmlConfigurator.Configure(fileInfo);
            _initialized = true;
        }

        public override void Info(ILog logger, object message) {
            if (_initialized)
                logger.Info(message);
            else
                Console.WriteLine(message);
        }

        public override void Error(ILog logger, object message) {
            if (_initialized)
                logger.Error(message);
            else
                Console.WriteLine(message);
        }

        public override void Debug(ILog logger, string message) {
            if (_initialized)
                logger.Debug(message);
            else
                Console.WriteLine(message);
        }
    }
}
