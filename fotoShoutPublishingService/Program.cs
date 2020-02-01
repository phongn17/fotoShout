using FotoShoutUtils.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutPublishingService {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main() {
            RunService();
        }

        static void Test() {
            LogManager.Configure(new Logger());

            PublishingEngine publishEngine = new PublishingEngine();
            if (publishEngine.Initialize(false))
                publishEngine.Execute();
        }

        static void RunService() {
            LogManager.Configure(new Logger());
            
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new PublishingService() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
