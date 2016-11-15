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
        //public event ClienteConectado(EndPoint o)
        //Public Event ClienteConectado(ByVal IDTerminal As Net.IPEndPoint)
        //Public Event DatosRecibidos(ByVal IDTerminal As Net.IPEndPoint, ByRef datos As String)
        //Public Event ClienteDesconectado(ByVal IpAddressClient As String, ByVal portCliente As Integer)
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
                //oDataCurrentClient.oThread = new Thread()
                oDataCurrentClient.sDatosRecibidos = "";
                lock (this)
                {
                    oListaClientes.Add(oIDClienteActual, oDataCurrentClient);
                }
                oDataCurrentClient.oThread.Name = string.Format("Client {0}", oDataCurrentClient.oTCPCliente.Client.RemoteEndPoint.ToString());
                
            }
        }
        
    //        While True
    //            Dim InfoClienteActual As New InfoDeUnCliente
    //            With InfoClienteActual
    //                'Cuando se recibe la conexion, guardo la informacion del cliente
    //                'Guardo el Socket que utilizo para mantener la conexion con el cliente
    //                .tcpClient = tcpLsn.AcceptTcpClient 'Se queda esperando la conexion de un cliente 

    //                'Guardo el el RemoteEndPoint, que utilizo para identificar al cliente
    //                IDClienteActual = .tcpClient.Client.RemoteEndPoint

    //                'Creo un Thread para que se encargue de escuchar los mensaje del cliente 
    //                .Thread = New Thread(AddressOf LeerSocket)

    //                'Agrego la informacion del cliente al HashArray Clientes, donde esta la
    //                .UltimosDatosRecibidos = ""
    //                'informacion de todos estos
    //                SyncLock Me
    //                    Clientes.Add(IDClienteActual, InfoClienteActual)
    //                End SyncLock

    //                .Thread.Name = "Cliente" & .tcpClient.Client.RemoteEndPoint.ToString

    //                ''Genero el evento Nueva conexion 
    //                RaiseEvent ClienteConectado(IDClienteActual)

    //                'Inicio el thread encargado de escuchar los mensajes del cliente 
    //                .Thread.Start()
    //            End With
    //        End While
    //    Catch ex As ThreadAbortException
    //    Catch ex As SocketException
    //    Finally
    //        tcpLsn.Stop()
    //        Console.WriteLine(Thread.CurrentThread.Name & " [END]")
    //    End Try
    //End Sub
        #endregion
        #endregion

    }
    public class KSI_DatosCliente
    {
        public string sKeyCliente{get;set;}
    }
}
