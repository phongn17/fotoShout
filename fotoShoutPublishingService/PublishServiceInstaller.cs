﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace FotoShoutPublishingService {
    [RunInstaller(true)]
    public partial class PublishServiceInstaller : System.Configuration.Install.Installer {
        public PublishServiceInstaller() {
            InitializeComponent();
        }
    }
}
