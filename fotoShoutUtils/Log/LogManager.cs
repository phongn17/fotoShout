using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Log {
    public static class LogManager {
        public static LoggerBase Instance { get; set; }

        public static void Configure(LoggerBase logger) {
            Instance = logger;
            Instance.Configure();
        }
        
        public static void Configure(LoggerBase logger, string configFile) {
            Instance = logger;
            Instance.Configure(configFile);
        }

        public static void Info(ILog logger, object message) {
            Instance.Info(logger, message);
        }
        
        public static void Error(ILog logger, object message) {
            Instance.Error(logger, message);
        }

        public static void Debug(ILog logger, string message) {
            Instance.Debug(logger, message);
        }
    }
}
