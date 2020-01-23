using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Log {
    public abstract class LoggerBase {
        public abstract void Configure();
        public abstract void Configure(string configFile);
        public abstract void Info(ILog logger, object message);
        public abstract void Error(ILog logger, object message);
        public abstract void Debug(ILog logger, string message);
    }
}
