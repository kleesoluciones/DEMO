using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;
namespace Servidor
{
    public class KSI_SocketServidor
    {
        #region Estructuras
        private struct DatosCliente
        {
            public TcpClient oTCPCliente;
            public Thread oThread;
            public string sDatosRecibidos;
        }
        #endregion
        #region Variables
        private TcpListener oTCPListener;
        private Hashtable oListaClientes;
        private Thread oThreadTCP;
        private EndPoint oIDClienteActual; //último cliente conectado
        private string sPortListener;
        private string sPathApplication;
        private Mutex oMutex = new Mutex();
        #endregion

        #region Eventos
        
        #endregion
        #region Propiedades
        public string PortListener { get { return sPortListener; } set { sPortListener = value; } }
        #endregion
        #region Constructores
        public KSI_SocketServidor(string sPath)
        {
            this.sPathApplication = sPath;
        }
        #endregion
        #region Métodos
        public void Listen()
        {

        }
        #region Funciones Privadas
        private void WaitCliente()
        {
            while (true)
            {
                DatosCliente oDataCurrentClient = new DatosCliente();
                oDataCurrentClient.oTCPCliente = oTCPListener.AcceptTcpClient();
                oIDClienteActual = oDataCurrentClient.oTCPCliente.Client.RemoteEndPoint;
                
                oDataCurrentClient.sDatosRecibidos = "";
                lock (this)
                {
                    oListaClientes.Add(oIDClienteActual, oDataCurrentClient);
                }
                oDataCurrentClient.oThread.Name = string.Format("Client {0}", oDataCurrentClient.oTCPCliente.Client.RemoteEndPoint.ToString());
                
            }
        }
        
   
        #endregion
        #endregion

    }
    public class KSI_DatosCliente
    {
        public string sKeyCliente{get;set;}
    }
}
