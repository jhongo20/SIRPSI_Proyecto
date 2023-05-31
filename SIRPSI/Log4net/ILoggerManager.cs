using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvertecApi.Log4net
{
    public interface ILoggerManager
    {
        //Metodo Genera Mensaje Tipo Informativo 
        //con parametro un mensaje
        void LogInformation(string message);

        //Metodo Genera Mensaje Tipo Informativo 
        //con parametro un mensaje y objeto Excepcion
        void LogInformation(string message, Exception ex);

        //Metodo Genera Mensaje Tipo Advertencia 
        //con parametro un mensaje
        void LogAdvertencia(string message);

        //Metodo Genera Mensaje Tipo Advertencia 
        //con parametro un mensaje y objeto Excepcion
        void LogAdvertencia(string message, Exception ex);

        //Metodo Genera Mensaje Tipo Error 
        //con parametro un mensaje
        void LogError(string message);

        //Metodo Genera Mensaje Tipo Error 
        //con parametro un mensaje y objeto Excepcion
        void LogError(string message, Exception ex);

    }
}
