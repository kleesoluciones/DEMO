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
using System.Xml;
using System.Net;
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
        private IPEndPoint oIDClienteActual; 
        private int iPortListener;
        private string sPathApplication;
        private Mutex oMutex = new Mutex();
        #endregion
        #region Delegados
        public delegate void ClientConnected(IPEndPoint oIDTerminal);
        public delegate void DataRecieved (IPEndPoint oIDTerminal, string sData);
        public delegate void ClientDisconnected(IPEndPoint oIDTerminal, int iPuertoCliente);
        
        #endregion
        #region Eventos
        public event ClientConnected oClienteConectado;
        
        public event DataRecieved oDataRecibido;
        public event ClientDisconnected oClienteDesonectado;
        #endregion
        #region Propiedades
        public int PortListener { get { return iPortListener; } set { iPortListener = value; } }
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
            oTCPListener = new TcpListener (System.Net.IPAddress.Any, PortListener);
            oTCPListener.Start();
            Thread oHiloEspera= new Thread(new ThreadStart(WaitCliente));
            oHiloEspera.Name="SocketServer Start";
            oHiloEspera.Start();
        }
        private void EliminarArchivosTemporales(int iMinutosAntiguedad = 10080)
        {
            try
            {
                if (Directory.Exists(this.sPathApplication))
                {
                    foreach (String nombreArchivo in Directory.GetFiles(this.sPathApplication, "*.*", SearchOption.TopDirectoryOnly))
                    {
                        FileInfo archivo = new FileInfo(nombreArchivo);
                        if (archivo.LastWriteTime < DateTime.Now.AddMinutes(-iMinutosAntiguedad))
                        {
                            archivo.Delete();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                GuardarLogErrores(ex, "EliminarArchivosTemporales", "Error logs de aplicación");
            }
        }
        private void WaitCliente()
        {
            try
            {
                while (true)
                {
                    DatosCliente oDataCurrentClient = new DatosCliente();
                    oDataCurrentClient.oTCPCliente = oTCPListener.AcceptTcpClient();
                    oIDClienteActual = (IPEndPoint) oDataCurrentClient.oTCPCliente.Client.RemoteEndPoint;
                    oDataCurrentClient.sDatosRecibidos = "";
                    oDataCurrentClient.oThread = new Thread(new ThreadStart(()=> ReadSocket(oIDClienteActual)));
                    lock (this)
                    {
                        oListaClientes.Add(oIDClienteActual, oDataCurrentClient);
                    }
                    oDataCurrentClient.oThread.Name = string.Format("Client {0}", oDataCurrentClient.oTCPCliente.Client.RemoteEndPoint.ToString());
                    oClienteConectado(oIDClienteActual);
                    oDataCurrentClient.oThread.Start();
                }
            }
            catch (ThreadAbortException tEx) {
                Console.WriteLine(string.Format("ERROR ThreadAbortException: {0}", tEx.ToString()));
                
            }
            catch (SocketException sEx) 
            {
                Console.WriteLine(string.Format("ERROR SocketException: {0}", sEx.ToString()));
            }
            catch (Exception oEx)
            {
                GuardarLogErrores(oEx, "WaitClient", "Error general al esperar cliente conectado", true);
            }
            finally
            {
                oTCPListener.Stop();
            }
                
        }
        private void ReadSocket(EndPoint oIDActual)
        {

            try
            {
                DatosCliente oClienteActual = new DatosCliente();
                oIDActual =this.oIDClienteActual;
                oClienteActual = (DatosCliente)this.oListaClientes[oIDActual];
                oIDClienteActual = (IPEndPoint)oClienteActual.oTCPCliente.Client.RemoteEndPoint;
                NetworkStream oNetworkStream =  oClienteActual.oTCPCliente.GetStream();
                string sDataLeido="";
                oClienteActual.sDatosRecibidos = "";
                while (true)
                {
                    try
                    {
                        var oBuffer = new byte[4096];
                        int iCantidad = 0;
                        do
                        {
                            iCantidad = oClienteActual.oTCPCliente.GetStream().Read(oBuffer, 0, oBuffer.Length);
                            if (iCantidad > 0)
                                sDataLeido += Encoding.ASCII.GetString(oBuffer, 0, oBuffer.Length);
                            else
                                break;

                        } while (oClienteActual.oTCPCliente.Available > 0);
                        if (!string.IsNullOrEmpty(sDataLeido))
                            oClienteActual.sDatosRecibidos += sDataLeido;
                        oDataRecibido(oIDClienteActual, oClienteActual.sDatosRecibidos);
                    }
                    catch (Exception oError)
                    {
                        GuardarLogErrores(oError, "ReadSocket", "Error al leer datos");
                        break;
                    }
                }
            }
            catch (Exception oEx)
            {
                GuardarLogErrores(oEx, "ReadSocket", "Error al leer datos de cliente conectado");
            }

        }
        private void GuardarLogErrores(Exception oError, string sMetodo, string sOtrosDatos = "", bool bBorrarErroresAntiguos = false)
        {
            if (bBorrarErroresAntiguos)
                EliminarArchivosTemporales(10080);
            string sRutaDirectorioErrores = this.sPathApplication + "\\Errores";
            if (!Directory.Exists(sRutaDirectorioErrores))
                Directory.CreateDirectory(sRutaDirectorioErrores);
            sRutaDirectorioErrores += "\\" + DateTime.Now.ToString("ddMMyyyyHH") + ".xml";
            if (!(File.Exists(sRutaDirectorioErrores)))
            {
                StreamWriter strWr;
                strWr = File.CreateText(sRutaDirectorioErrores);
                strWr.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
                strWr.WriteLine("<errores>");
                strWr.WriteLine("</errores>");
                strWr.Close();
            }
            XmlDocument docXML = new XmlDocument();
            docXML.Load(sRutaDirectorioErrores);

            XmlElement nodoMetodo = docXML.CreateElement("metodo");
            nodoMetodo.InnerText = sMetodo;
            XmlElement nodoDescripcion = docXML.CreateElement("descripcion");
            nodoDescripcion.InnerText = "Message: " + oError.Message + "  StackTrace: " + oError.StackTrace + " InnerException:" + oError.InnerException +
                " Source: " + oError.Source + " Data: " + oError.Data + " TargetSite: " + oError.TargetSite;
            XmlElement nodoFecha = docXML.CreateElement("fecha");
            nodoFecha.InnerText = Convert.ToString(DateTime.Now);
            XmlElement nodoOtros = docXML.CreateElement("otrosDatosError");
            nodoOtros.InnerText = sOtrosDatos;

            
            
            XmlNode nuevoError = docXML.DocumentElement;
            nuevoError = docXML.CreateElement("error");
            nuevoError.AppendChild(nodoFecha);
            nuevoError.AppendChild(nodoMetodo);
            nuevoError.AppendChild(nodoDescripcion);
            nuevoError.AppendChild(nodoOtros);
            docXML.DocumentElement.AppendChild(nuevoError);
            docXML.Save(sRutaDirectorioErrores);


        }
        public bool SendDataToClient(IPEndPoint oIDCliente, string sDataEnviar)
        {
            bool bExitoso = true;
            try
            {
                DatosCliente oClienteActual = new DatosCliente();
                oClienteActual = (DatosCliente)this.oListaClientes[oIDCliente];
                byte [] oBufferEnvio = Encoding.ASCII.GetBytes(sDataEnviar);
                NetworkStream oNetworkStream = oClienteActual.oTCPCliente.GetStream();
                if (oNetworkStream.CanWrite)
                {
                    oNetworkStream.Write(oBufferEnvio, 0, oBufferEnvio.Length);
                }

            }
            catch (Exception oError)
            {
                bExitoso = false;
                GuardarLogErrores(oError, "SendDataToCliente", string.Format("Fallo de envío de datos: {0}", sDataEnviar));

            }
            return bExitoso;
        }
        public void DisconnectClient(IPEndPoint oIDCliente)
        {
            DatosCliente oClienteActual = new DatosCliente();
            oClienteActual = (DatosCliente)this.oListaClientes[oIDCliente];
            string sIPAddress = oIDCliente.Address.ToString();
            int iPuerto = oIDCliente.Port;
            oIDClienteActual = (IPEndPoint)oClienteActual.oTCPCliente.Client.RemoteEndPoint;
            try
            {
                if (oClienteActual.oThread != null)
                {
                    oClienteActual.oThread.Abort();
                }
                try { oClienteActual.oTCPCliente.Close(); }
                catch { }
                oClienteDesonectado(oIDClienteActual, iPuerto);
            }
            catch (Exception oError)
            {
                GuardarLogErrores(oError, "DisconnectClient", "Error al desconectar cliente");
            }

        }
        public void StopSocketServer()
        {
            try
            {
                //int indice = 0;
                //while (indice < oListaClientes.Keys.Count)
                //{
                //    IPEndPoint oKey = (IPEndPoint)oListaClientes.Keys;
                //    DatosCliente oData = (DatosCliente) oListaClientes[oKey];
                //    indice++;
                //}
            }
            catch (Exception oError)
            {
                GuardarLogErrores(oError, "StopSocketServer");
            }
        }
   
        
        #endregion

    }
    public class KSI_DatosCliente
    {
        public string sKeyCliente{get;set;}
    }
}
 