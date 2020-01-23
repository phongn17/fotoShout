using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutPublishingService {
    public partial class PublishingService : ServiceBase {
        public const string SVC_NAME = "FotoShout Publishing Service";
        
        PublishingEngine _engine = null;

        public PublishingService() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            _engine = new PublishingEngine();
            if (!_engine.Initialize(true))
                this.Stop();
        }

        protected override void OnStop() {
        }
    }
}
