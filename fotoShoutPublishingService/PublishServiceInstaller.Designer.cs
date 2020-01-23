using System.ServiceProcess;
namespace FotoShoutPublishingService {
    partial class PublishServiceInstaller {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.serviceProcessInstallerPublishing = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstallerPublishing = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstallerPublishing
            // 
            this.serviceProcessInstallerPublishing.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceProcessInstallerPublishing.Password = null;
            this.serviceProcessInstallerPublishing.Username = null;
            // 
            // serviceInstallerPublishing
            // 
            this.serviceInstallerPublishing.DisplayName = "FotoShout Publishing Service";
            this.serviceInstallerPublishing.ServiceName = "FotoShout Publishing Service";
            this.serviceInstallerPublishing.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstallerPublishing,
            this.serviceInstallerPublishing});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstallerPublishing;
        private System.ServiceProcess.ServiceInstaller serviceInstallerPublishing;
    }
}