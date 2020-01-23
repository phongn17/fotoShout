using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutSyncService {
    public partial class SyncService : ServiceBase {
        public const string SVC_NAME = "FotoShout Sync Service";

        SyncEngine _engine = null;

        public SyncService() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            _engine = new SyncEngine();
            if (!_engine.Initialize(true))
                this.Stop();
        }

        protected override void OnStop() {
            if (_engine != null)
                _engine.DeInitialize();
        }
    }
}
