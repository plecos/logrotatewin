namespace Logrotate
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.logrotateProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.logrotateInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // logrotateProcessInstaller
            // 
            this.logrotateProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.logrotateProcessInstaller.Password = null;
            this.logrotateProcessInstaller.Username = null;
            // 
            // logrotateInstaller
            // 
            this.logrotateInstaller.DisplayName = "Logrotate";
            this.logrotateInstaller.ServiceName = "Logrotate";
            this.logrotateInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.logrotateProcessInstaller,
            this.logrotateInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller logrotateProcessInstaller;
        private System.ServiceProcess.ServiceInstaller logrotateInstaller;
    }
}