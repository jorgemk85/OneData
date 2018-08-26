using System;

namespace DataManagement.Exceptions
{
    public class ConfigurationNotFoundException : Exception
    {
        const string errorMessage = "No se encontro la configuracion requerida dentro del archivo de configuracion de la aplicacion.";

        public ConfigurationNotFoundException(string setting) : base(string.Format("Configuracion: {0} | {1}", setting, errorMessage)) { }

        public ConfigurationNotFoundException(string setting, Exception innerException) : base(string.Format("Configuracion: {0} | {1}", setting, errorMessage), innerException) { }
    }
}
