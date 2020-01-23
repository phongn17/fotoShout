using FotoShoutUtils.Log;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutApi.Log {
    public class WebLogger: Logger {
        public override void Error(ILog logger, object message) {
            if (_initialized)
                logger.Error(message);
            else if (message is IEnumerable<System.Web.Http.ModelBinding.ModelError>) {
                foreach (System.Web.Http.ModelBinding.ModelError error in (IEnumerable<System.Web.Http.ModelBinding.ModelError>)message) {
                    logger.Error(error.ToString());
                }
            }
            else
                Console.WriteLine(message);
        }
    }
}
