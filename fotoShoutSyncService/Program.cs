using FotoShoutUtils.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutSyncService {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main() {
            Test();
        }

        static void Test() {
            
            LogManager.Configure(new Logger());

            SyncEngine syncEngine = new SyncEngine();
            if (syncEngine.Initialize(false)) {
                syncEngine.Execute();
                syncEngine.DeInitialize();
            }
        }

        static void RunService() {
            LogManager.Configure(new Logger());

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new SyncService() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
