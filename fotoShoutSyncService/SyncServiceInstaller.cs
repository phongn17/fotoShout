using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace FotoShoutSyncService {
    [RunInstaller(true)]
    public partial class SyncServiceInstaller : System.Configuration.Install.Installer {
        public SyncServiceInstaller() {
            InitializeComponent();
        }
    }
}
