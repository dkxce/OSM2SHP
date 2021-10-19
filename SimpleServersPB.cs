using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace SimpleServers
{
    /// <summary>
    ///     Базовые методы серверов
    /// </summary>
    public interface IServer
    {
        void Start();
        void Stop();        
    }

    /// <summary>
    ///     Базовый класс для TCP серверов
    /// </summary>
    public abstract class TTCPServer: IServer, IDisposable
    {
        protected Thread mainThread = null;
        protected TcpListener mainListener = null;

        /// <summary>
        ///     Listen IP Address
        /// </summary>
        protected IPAddress ListenIP = IPAddress.Any;
        /// <summary>
        ///     Listen IP Address
        /// </summary>
        public IPAddress ServerIP { get { return ListenIP; } }        

        /// <summary>
        ///     Listen Port Number
        /// </summary>
        protected int ListenPort = 5000;
        /// <summary>
        ///     Listen Port Number
        /// </summary>
        public int ServerPort { get { return ListenPort; } set { ListenPort = value; } }

        /// <summary>
        ///     Server is running or not
        /// </summary>
        protected bool isRunning = false;
        /// <summary>
        ///     Server is running or not
        /// </summary>
        public bool Running { get { return isRunning; } }

        /// <summary>
        ///     Total Clients Counter
        /// </summary>
        protected ulong counter = 0;
        /// <summary>
        ///     Total Clients Counter
        /// </summary>
        public ulong ClientsCounter { get { return counter; } }

        /// <summary>
        ///     Client Read Timeout in seconds, default 10
        /// </summary>
        protected int readTimeout = 10; // 10 sec
        /// <summary>
        ///     Client Read Timeout in seconds, default 10
        /// </summary>
        public int ReadTimeout { get { return readTimeout; } set { readTimeout = value; } }        

        public TTCPServer(){}
        public TTCPServer(int Port) { this.ListenPort = Port; }
        public TTCPServer(IPAddress IP, int Port) { this.ListenIP = IP; this.ListenPort = Port; }
        
        /// <summary>
        ///     Start Server
        /// </summary>
        public virtual void Start() { }
        /// <summary>
        ///     Stop Server
        /// </summary>
        public virtual void Stop() { }
        /// <summary>
        ///     Stop Server and Dipose
        /// </summary>
        public virtual void Dispose() { this.Stop(); }

        /// <summary>
        ///     Accept TCP Client
        /// </summary>
        /// <param name="client"></param>
        /// <returns>true - connect, false - ignore</returns>
        protected virtual bool AcceptClient(TcpClient client) { return true; }
        /// <summary>
        ///     Get Client Connection
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="clientID">Client Number</param>
        protected virtual void GetClient(TcpClient Client, ulong clientID) { }
        /// <summary>
        ///     On Error
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="clientID">Client Number</param>
        /// <param name="error"></param>
        protected virtual void onError(TcpClient Client, ulong clientID, Exception error) { throw error; }

        /// <summary>
        ///     Is Connection Alive?
        /// </summary>
        /// <param name="Client"></param>
        /// <returns></returns>
        public static bool IsConnected(TcpClient Client)
        {
            if (!Client.Connected) return false;
            if (Client.Client.Poll(0, SelectMode.SelectRead))
            {
                byte[] buff = new byte[1];
                try
                {
                    if (Client.Client.Receive(buff, SocketFlags.Peek) == 0)
                        return false;
                }
                catch
                {
                    return false;
                };
            };
            return true;
        }

        public static string Base64Encode(string plainText)
        {
            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            byte[] base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }    
    
    /// <summary>
    ///     Простейший однопоточный TCP-сервер
    /// </summary>
    public class SingledTCPServer: TTCPServer
    {
        /// <summary>
        ///     Close connection when GetClient method completes
        /// </summary>
        protected bool _closeOnGetClientCompleted = true;
        /// <summary>
        ///     Close connection when GetClient method completes
        /// </summary>
        public bool CloseConnectionOnGetClientCompleted { get { return _closeOnGetClientCompleted; } set { _closeOnGetClientCompleted = value; } }

        public SingledTCPServer() { }
        public SingledTCPServer(int Port) { this.ListenPort = Port; }
        public SingledTCPServer(IPAddress IP, int Port) { this.ListenIP = IP; this.ListenPort = Port; }
        ~SingledTCPServer() { this.Dispose(); }

        public override void Start()
        {
            if (isRunning) throw new Exception("Server Already Running!");

            isRunning = true;
            mainThread = new Thread(MainThread);
            mainThread.Start();
        }

        public override void Stop()
        {
            if (!isRunning) return;

            isRunning = false;

            if (mainListener != null) mainListener.Stop();
            mainListener = null;

            mainThread.Join();
            mainThread = null;
        }           
                
        private void MainThread()
        {
            mainListener = new TcpListener(this.ListenIP, this.ListenPort);
            mainListener.Start();
            while (isRunning)
            {
                try
                {
                    TcpClient client = mainListener.AcceptTcpClient();
                    if (!AcceptClient(client))
                    {
                        client.Client.Close();
                        client.Close();
                        continue;
                    };

                    ulong id = 0;
                    try 
                    {
                        client.GetStream().ReadTimeout = this.readTimeout * 1000;
                        GetClient(client, id = this.counter++); 
                    } 
                    catch (Exception ex) 
                    { 
                        onError(client, id, ex); 
                    };
                    if(_closeOnGetClientCompleted)
                        try { client.Client.Close(); client.Close(); } catch { };                    
                }
                catch { };
                Thread.Sleep(1);
            };
        }

        protected override void GetClient(TcpClient Client, ulong clientID) 
        { 
            //
            // do something
            //

            if (!this._closeOnGetClientCompleted)
            {
                Client.Client.Close();
                Client.Close();
            };
        }
    }       

    /// <summary>
    ///     Простейший однопоточный TCP-сервер, который принимает текст
    /// </summary>
    public class SingledTextTCPServer : SingledTCPServer
    {
        public SingledTextTCPServer() : base() { }
        public SingledTextTCPServer(int Port) : base(Port) { }
        public SingledTextTCPServer(IPAddress IP, int Port) : base(IP, Port) { }
        ~SingledTextTCPServer() { this.Dispose(); }

        protected override void GetClient(TcpClient Client, ulong clientID)
        {
            string Request = "";
            int DCRLF = -1;

            int b = -1;
            while ((b = Client.GetStream().ReadByte()) >= 0)
            {
                Request += Encoding.ASCII.GetString(new byte[] { (byte)b }, 0, 1);
                if (b == 0x0A)
                    DCRLF = Request.IndexOf("\r\n\r\n");
                if (DCRLF >= 0 || Request.Length > 4096) { break; };
            };

            GetClientRequest(Client, clientID, Request);
        }

        /// <summary>
        ///     Get Client with Request Text Data
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="clientID">client number</param>
        /// <param name="Request"></param>
        protected virtual void GetClientRequest(TcpClient Client, ulong clientID, string Request)
        {
            string proto = "tcp://" + Client.Client.RemoteEndPoint.ToString() + "/text/";

            //
            // do something
            //

            if (!this._closeOnGetClientCompleted)
            {
                Client.Client.Close();
                Client.Close();
            };
        }        
    }    
    
    /// <summary>
    ///     Простейший HTTP-сервер
    /// </summary>
    public class SingledHttpServer : SingledTextTCPServer
    {
        public SingledHttpServer() : base(80) { this._closeOnGetClientCompleted = true; }
        public SingledHttpServer(int Port) : base(Port) { this._closeOnGetClientCompleted = true; }
        public SingledHttpServer(IPAddress IP, int Port) : base(IP, Port) { this._closeOnGetClientCompleted = true; }
        ~SingledHttpServer() { this.Dispose(); }

        protected Mutex _h_mutex = new Mutex();
        protected Dictionary<string, string> _headers = new Dictionary<string, string>();
        public Dictionary<string, string> Headers
        {
            get
            {
                _h_mutex.WaitOne();
                Dictionary<string, string> res = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> kvp in _headers)
                    res.Add(kvp.Key, kvp.Value);
                _h_mutex.ReleaseMutex();
                return res;
            }
            set
            {
                _h_mutex.WaitOne();
                _headers.Clear();
                foreach (KeyValuePair<string, string> kvp in value)
                    _headers.Add(kvp.Key, kvp.Value);
                _h_mutex.ReleaseMutex();
            }
        }

        public virtual void HttpClientSendError(TcpClient Client, int Code, Dictionary<string, string> dopHeaders)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
            // Код простой HTML-странички
            string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str = "HTTP/1.1 " + CodeStr + "\r\n";
            this._h_mutex.WaitOne();
            foreach (KeyValuePair<string, string> kvp in this._headers)
                Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            this._h_mutex.ReleaseMutex();
            if (dopHeaders != null)
                foreach (KeyValuePair<string, string> kvp in dopHeaders)
                    Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            Str += "Content-type: text/html\r\nContent-Length:" + Html.Length.ToString() + "\r\n\r\n" + Html;
            // Приведем строку к виду массива байт
            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
            // Отправим его клиенту
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            Client.Client.Close();
            Client.Close();
        }
        public virtual void HttpClientSendError(TcpClient Client, int Code)
        {
            HttpClientSendError(Client, Code, null);
        }
        public virtual void HttpClientSendText(TcpClient Client, string Text, Dictionary<string, string> dopHeaders)
        {
            // Код простой HTML-странички
            string Html = "<html><body>" + Text + "</body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str = "HTTP/1.1 200\r\n";
            this._h_mutex.WaitOne();
            foreach (KeyValuePair<string, string> kvp in this._headers)
                Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            this._h_mutex.ReleaseMutex();
            if (dopHeaders != null)
                foreach (KeyValuePair<string, string> kvp in dopHeaders)
                    Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            Str += "Content-type: text/html\r\nContent-Length:" + Html.Length.ToString() + "\r\n\r\n" + Html;
            // Приведем строку к виду массива байт
            byte[] Buffer = Encoding.GetEncoding(1251).GetBytes(Str);
            // Отправим его клиенту
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            Client.Client.Close();
            Client.Close();
        }
        public virtual void HttpClientSendText(TcpClient Client, string Text)
        {
            HttpClientSendText(Client, Text, null);
        }

        protected override void GetClientRequest(TcpClient Client, ulong clientID, string Request)
        {
            HttpClientSendError(Client, 501);
        }        
    }

    #region SimpleUDP
    /// <summary>
    ///     Простейший UDP-сервер
    /// </summary>
    public class SimpleUDPServer : IServer, IDisposable
    {
        private Thread mainThread = null;
        private Socket udpSocket = null;
        private IPAddress ListenIP = IPAddress.Any;
        private int ListenPort = 5000;
        private bool isRunning = false;
        private int _bufferSize = 4096;

        public SimpleUDPServer() { }
        public SimpleUDPServer(int Port) { this.ListenPort = Port; }
        public SimpleUDPServer(IPAddress IP, int Port) { this.ListenIP = IP; this.ListenPort = Port; }
        ~SimpleUDPServer() { Dispose(); }        

        public bool Running { get { return isRunning; } }
        public IPAddress ServerIP { get { return ListenIP; } }
        public int ServerPort { get { return ListenPort; } }
        public int BufferSize { get { return _bufferSize; } set { _bufferSize = value; } }

        public void MainThread()
        {
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = new IPEndPoint(this.ListenIP, this.ListenPort);
            udpSocket.Bind(ipep);

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            //byte[] data1 = Encoding.ASCII.GetBytes("Hello");
            //udpSocket.SendTo(data1, data1.Length, SocketFlags.None, new IPEndPoint(IPAddress.Parse("127.0.0.1"), this.ListenPort));

            while (isRunning)
            {
                try
                {
                    byte[] data = new byte[_bufferSize];
                    int recv = udpSocket.ReceiveFrom(data, ref Remote);
                    if (recv > 0) ReceiveBuff(Remote, data, recv);
                }
                catch (Exception ex)
                {
                    try { onError(ex); } catch { };
                };
                Thread.Sleep(1);
            };
        }

        public virtual void Stop()
        {
            if (!isRunning) return;
            isRunning = false;

            udpSocket.Close();
            mainThread.Join();

            udpSocket = null;
            mainThread = null;
        }

        public virtual void Start()
        {
            if (isRunning) throw new Exception("Server Already Running!");

            isRunning = true;
            mainThread = new Thread(MainThread);
            mainThread.Start();
        }

        public virtual void Dispose() { Stop(); } 

        protected virtual void onError(Exception ex)
        {

        }

        protected virtual void ReceiveBuff(EndPoint Client, byte[] data, int length)
        {
            // do anything
        }
    }

    /// <summary>
    ///     Простейший UDP-сервер, который принимает текст
    /// </summary>
    public class SimpleTextUDPServer : SimpleUDPServer
    {
        public SimpleTextUDPServer() : base() { }
        public SimpleTextUDPServer(int Port) : base(Port) { }
        public SimpleTextUDPServer(IPAddress IP, int Port) : base(IP, Port) { }
        ~SimpleTextUDPServer() { this.Dispose(); }

        protected override void ReceiveBuff(EndPoint Client, byte[] data, int length)
        {
            string Request = System.Text.Encoding.GetEncoding(1251).GetString(data, 0, length);
            ReceiveData(Client, Request);
        }

        protected virtual void ReceiveData(EndPoint Client, string Request)
        {
            string proto = "udp://" + Client.ToString() + "/text/";
            // do anything
        }
    }
    #endregion    

    /// <summary>
    ///     Многопоточный TCP-сервер
    /// </summary>
    public class ThreadedTCPServer : TTCPServer
    {
        private class ClientTCPSInfo
        {
            public ulong id;
            public TcpClient client;
            public Thread thread;

            public ClientTCPSInfo(TcpClient client, Thread thread)
            {
                this.client = client;
                this.thread = thread;
            }
        }

        /// <summary>
        ///     Working Mode
        /// </summary>
        public enum Mode: byte
        {
            /// <summary>
            ///     Allow all connections
            /// </summary>
            NoRules = 0,
            /// <summary>
            ///     Allow only specified connections
            /// </summary>
            AllowWhiteList = 1,
            /// <summary>
            ///     Allow all but black list
            /// </summary>
            DenyBlackList = 2
        }

        /// <summary>
        ///     Started
        /// </summary>
        private DateTime _started = DateTime.MinValue;
        /// <summary>
        ///     Started
        /// </summary>
        public DateTime Started { get { return isRunning ? _started : DateTime.MinValue; } }

        /// <summary>
        ///     Server Mode
        /// </summary>
        private Mode ipmode = Mode.NoRules;
        /// <summary>
        ///     Server Mode
        /// </summary>
        public Mode ListenIPMode { get { return ipmode; } set { ipmode = value; } }

        private Mutex iplistmutex = new Mutex();
        /// <summary>
        ///     IP white list
        /// </summary>
        private List<string> ipwhitelist = new List<string>();
        /// <summary>
        ///     IP white List
        /// </summary>
        public string[] ListenIPAllow 
        { 
            get 
            { 
                iplistmutex.WaitOne();
                string[] res = ipwhitelist.ToArray();
                iplistmutex.ReleaseMutex();
                return res;
            }
            set 
            {
                iplistmutex.WaitOne();
                ipwhitelist.Clear();
                if (value != null)
                    ipwhitelist.AddRange(value);
                iplistmutex.ReleaseMutex();
            }
        }

        /// <summary>
        ///     IP black list
        /// </summary>
        private List<string> ipblacklist = new List<string>();
        /// <summary>
        ///     IP black list
        /// </summary>
        public string[] ListenIPDeny
        {
            get
            {
                iplistmutex.WaitOne();
                string[] res = ipblacklist.ToArray();
                iplistmutex.ReleaseMutex();
                return res;
            }
            set
            {
                iplistmutex.WaitOne();
                ipblacklist.Clear();
                if (value != null)
                    ipblacklist.AddRange(value);
                iplistmutex.ReleaseMutex();
            }
        }

        /// <summary>
        ///     Current clients count
        /// </summary>
        private int clientsCount = 0;
        /// <summary>
        ///     Currect Connected Clients Count
        /// </summary>
        public int ClientsCounts { get { return clientsCount; } }
        
        /// <summary>
        ///     Max Clients Count
        /// </summary>
        private ushort maxClients = 50;
        /// <summary>
        ///     Max connected clients count
        /// </summary>
        public ushort MaxClients { get { return maxClients; } set { maxClients = value; } }

        /// <summary>
        ///     Abort client connection on stop
        /// </summary>
        private bool abortOnStop = false;
        /// <summary>
        ///     Abort client connections on stop
        /// </summary>
        public bool AbortOnStop { get { return abortOnStop; } set { abortOnStop = value; } }

        /// <summary>
        ///     Mutex for client dictionary
        /// </summary>
        private Mutex stack = new Mutex();
        /// <summary>
        ///     Client dictionary
        /// </summary>
        private Dictionary<ulong, ClientTCPSInfo> clients = new Dictionary<ulong, ClientTCPSInfo>();
        /// <summary>
        ///     Currect connected clients
        /// </summary>
        public KeyValuePair<ulong, TcpClient>[] Clients
        {
            get
            {
                this.stack.WaitOne();
                List<KeyValuePair<ulong, TcpClient>> res = new List<KeyValuePair<ulong, TcpClient>>();
                foreach (KeyValuePair<ulong, ClientTCPSInfo> kvp in this.clients)
                    res.Add(new KeyValuePair<ulong, TcpClient>(kvp.Key, kvp.Value.client));
                this.stack.ReleaseMutex();
                return res.ToArray();
            }
        }       

        public ThreadedTCPServer() { }
        public ThreadedTCPServer(int Port) { this.ListenPort = Port; }
        public ThreadedTCPServer(IPAddress IP, int Port) { this.ListenIP = IP; this.ListenPort = Port; }
        ~ThreadedTCPServer() { Dispose(); }        
        
        private bool AllowedByIPRules(TcpClient client)
        {
            if (ipmode != Mode.NoRules)
            {
                string remoteIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                iplistmutex.WaitOne();
                if (ipmode == Mode.AllowWhiteList)
                {                    
                    if ((ipwhitelist == null) || (ipwhitelist.Count == 0) || (!ipwhitelist.Contains(remoteIP)))
                    {
                        iplistmutex.ReleaseMutex();
                        return false;
                    };
                }
                else
                {
                    if ((ipblacklist != null) && (ipblacklist.Count > 0) && ipblacklist.Contains(remoteIP))
                    {
                        iplistmutex.ReleaseMutex();
                        return false;
                    };
                };
                iplistmutex.ReleaseMutex();
            };
            return true;
        }

        private void MainThread()
        {
            mainListener = new TcpListener(this.ListenIP, this.ListenPort);
            mainListener.Start();
            _started = DateTime.Now;
            while (isRunning)
            {
                try
                {
                    TcpClient client = mainListener.AcceptTcpClient();
                    if ((!AllowedByIPRules(client)) || (!AcceptClient(client)))
                    {
                        client.Client.Close();
                        client.Close();
                        continue;
                    };

                    if (this.maxClients < 2) // single-threaded
                    {
                        RunClientThread(new ClientTCPSInfo(client, null));
                    }
                    else // multi-threaded
                    {
                        while ((this.clientsCount >= this.maxClients) && isRunning) // wait for any closed thread
                            System.Threading.Thread.Sleep(5);
                        if (isRunning)
                        {
                            Thread thr = new Thread(RunThreaded);
                            thr.Start(new ClientTCPSInfo(client, thr));
                        };
                    };
                }
                catch { };
                Thread.Sleep(1);
            };
        }
        
        private void RunThreaded(object client)
        {
            RunClientThread((ClientTCPSInfo)client);
        }

        private void RunClientThread(ClientTCPSInfo Client)
        {
            this.clientsCount++;
            Client.id = this.counter++;
            this.stack.WaitOne();
            this.clients.Add(Client.id, Client);
            this.stack.ReleaseMutex();
            try
            {
                Client.client.GetStream().ReadTimeout = this.readTimeout * 1000;
                GetClient(Client.client, Client.id);                
            }
            catch (Exception ex)
            {
                onError(Client.client, Client.id, ex);
            } 
            finally
            {
                try
                {
                    Client.client.Client.Close();
                    Client.client.Close();
                }
                catch { };
            };

            this.stack.WaitOne();
            if (this.clients.ContainsKey(Client.id))
                this.clients.Remove(Client.id);
            this.stack.ReleaseMutex();
            this.clientsCount--;
        }

        /// <summary>
        ///  Start Server
        /// </summary>
        public override void Start()
        {
            if (isRunning) throw new Exception("Server Already Running!");

            isRunning = true;
            mainThread = new Thread(MainThread);
            mainThread.Start();
        }

        /// <summary>
        ///     Stop Server
        /// </summary>
        public override void Stop()
        {
            if (!isRunning) return;

            isRunning = false;

            if (this.abortOnStop)
            {
                this.stack.WaitOne();
                try
                {
                    foreach (KeyValuePair<ulong, ClientTCPSInfo> kvp in this.clients)
                    {
                        try { if (kvp.Value.thread != null) kvp.Value.thread.Abort(); }
                        catch { };
                        try { kvp.Value.client.Client.Close(); }
                        catch { };
                        try { kvp.Value.client.Close(); }
                        catch { };
                    };
                    this.clients.Clear();
                }
                catch { };
                this.stack.ReleaseMutex();
            };

            if (mainListener != null) mainListener.Stop();
            mainListener = null;

            mainThread.Join();
            mainThread = null;
        }

        /// <summary>
        ///     Get Client, threaded
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="id"></param>
        protected override void GetClient(TcpClient Client, ulong id)
        {
            // loop something  
            // connection will be close after return
        }                      
    }

    public class ClientExample4Bytes
    {
        public ClientExample4Bytes()
        {
            System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
            client.Connect("127.0.0.1", 8011);

            // write
            List<byte> buff = new List<byte>();
            buff.AddRange(System.Text.Encoding.ASCII.GetBytes("PROTOBUF+4"));
            buff.Add(0); buff.Add(0); buff.Add(0); buff.Add(0);
            client.GetStream().Write(buff.ToArray(), 0, buff.Count);

            // read
            byte[] incb = new byte[14];
            int count = client.GetStream().Read(incb, 0, incb.Length);
            string prefix = System.Text.Encoding.GetEncoding(1251).GetString(incb, 0, 10);
            if (prefix != "PROTOBUF+4")
            {
                int length = System.BitConverter.ToInt32(incb, 10);
                incb = new byte[length];
                client.GetStream().Read(incb, 0, incb.Length);
            };
        }
    }    

    /// <summary>
    ///    Protobuf-сервер
    /// </summary>
    public class Threaded4BytesTCPServer : ThreadedTCPServer
    {
        //[ProtoContract]
        //public class Config
        //{
        //    [ProtoMember(1)]
        //    public string inputFileName = "";
        //    [ProtoMember(2)]
        //    public string outputFileName = AppDomain.CurrentDomain.BaseDirectory.Trim('\\') + @"\SHAPES\default.dbf";
        //    [ProtoMember(3)]
        //    public byte selector = 1;
        //    [ProtoMember(4)]
        //    public bool onlyHasName = false;
        //    [ProtoMember(5)]
        //    public Dictionary<string, string> onlyWithTags = new Dictionary<string, string>();
        //    [ProtoMember(7)]
        //    public DateTime onlyMdfAfter = DateTime.MinValue;
        //    [ProtoMember(10)]
        //    public List<int> onlyVersion = new List<int>();
        //};
        // ProtoBuf.Serializer.Serialize(Stream, object);
        // ProtoBuf.Serializer.Deserialize<BlobHeader>(new MemoryStream(data));

        public Threaded4BytesTCPServer() : base() { }
        public Threaded4BytesTCPServer(int Port) : base(Port) { }
        public Threaded4BytesTCPServer(IPAddress IP, int Port) : base(IP, Port) { }
        ~Threaded4BytesTCPServer() { this.Dispose(); }

        protected string _prefix = "PROTOBUF+4";
        public string MessagePrefix { get { return _prefix; } set { _prefix = value; } }

        protected override void GetClient(TcpClient Client, ulong id)
        {
            try
            {                
                // PROTOBUF + 0x00 0x00 0x00 0x00 // 4 bytes length of 1-st block data (BigEndian) // 0x04000000 means 4; 0x00000004 means 67108864                

                byte[] buff = new byte[this._prefix.Length + 4];
                Client.GetStream().Read(buff, 0, buff.Length);
                string prefix = System.Text.Encoding.GetEncoding(1251).GetString(buff, 0, this._prefix.Length);
                if (prefix != this._prefix) return;

                int length = System.BitConverter.ToInt32(buff, this._prefix.Length);
                buff = new byte[length];
                Client.GetStream().Read(buff, 0, buff.Length);
                GetClientData(Client, id, buff);
            }
            catch (Exception ex)
            {
                onError(Client, id, ex);
            };
        }

        /// <summary>
        ///     Get Client with 1st block of data
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="id"></param>
        /// <param name="data"></param>
        protected virtual void GetClientData(TcpClient Client, ulong id, byte[] data)
        {
            // Write No Data To Client
            
            byte[] prfx = System.Text.Encoding.ASCII.GetBytes(this._prefix);
            Client.GetStream().Write(prfx, 0, prfx.Length);
            byte[] buff = System.Text.Encoding.ASCII.GetBytes("Hello " + DateTime.Now.ToString());
            Client.GetStream().Write(BitConverter.GetBytes(buff.Length), 0, 4);
            Client.GetStream().Write(buff, 0, buff.Length);

            // connection will be close after return       
        }
        
        protected override void onError(TcpClient Client, ulong id, Exception error)
        {

        }      

        private static void sample()
        {             
            SimpleServers.Threaded4BytesTCPServer srv  = new SimpleServers.Threaded4BytesTCPServer(8011);
            srv.ReadTimeout = 30;
            srv.Start();
            //
            System.Threading.Thread.Sleep(10000);
            //
            srv.Stop();
            srv.Dispose();
        }
    }

    /// <summary>
    ///     Многопоточный TCP-сервер, который принимает текст
    /// </summary>
    public class ThreadedTextTCPServer : ThreadedTCPServer
    {
        public ThreadedTextTCPServer() : base() { }
        public ThreadedTextTCPServer(int Port) : base(Port) { }
        public ThreadedTextTCPServer(IPAddress IP, int Port) : base(IP, Port) { }
        ~ThreadedTextTCPServer() { this.Dispose(); }

        protected override void GetClient(TcpClient Client, ulong ID)
        {            
            string Request = "";
            byte[] Buffer = new byte[4096];
            int Count, DCRLF = -1;

            while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
            {
                Request += Encoding.ASCII.GetString(Buffer, 0, Count);
                DCRLF = Request.IndexOf("\r\n\r\n");
                if (DCRLF >= 0 || Request.Length > 4096) { break; };
            };
            byte[] afterheader = new byte[0];
            if (DCRLF > 0) afterheader = Encoding.ASCII.GetBytes(Request.Substring(DCRLF + 4));

            ReceiveData(Client, ID, Request, afterheader, DCRLF > 0);
        }

        protected virtual void ReceiveData(TcpClient Client, ulong clientID, string Request, byte[] afterheader, bool validRequest)
        {
            //
            // loop something  
            // connection will be close after return       
            //
        }       
    }

    /// <summary>
    ///     Многопоточный HTTP-сервер
    /// </summary>
    public class ThreadedHttpServer : ThreadedTCPServer
    {
        public ThreadedHttpServer() : base(80) {  }
        public ThreadedHttpServer(int Port) : base(Port) {  }
        public ThreadedHttpServer(IPAddress IP, int Port) : base(IP, Port) { }
        ~ThreadedHttpServer() { this.Dispose(); }

        protected Mutex _h_mutex = new Mutex();
        protected Dictionary<string, string> _headers = new Dictionary<string, string>();
        public Dictionary<string, string> Headers
        {
            get
            {
                _h_mutex.WaitOne();
                Dictionary<string, string> res = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> kvp in _headers)
                    res.Add(kvp.Key, kvp.Value);
                _h_mutex.ReleaseMutex();
                return res;
            }
            set
            {
                _h_mutex.WaitOne();
                _headers.Clear();
                foreach (KeyValuePair<string, string> kvp in value)
                    _headers.Add(kvp.Key, kvp.Value);
                _h_mutex.ReleaseMutex();
            }
        }
        
        private bool _authRequired = false;
        public Dictionary<string, string> AuthentificationCredintals = new Dictionary<string, string>();
        public bool AuthentificationRequired { get { return _authRequired; } set { _authRequired = value; } }        

        public virtual void HttpClientSendError(TcpClient Client, int Code, Dictionary<string, string> dopHeaders)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
            // Код простой HTML-странички
            string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str = "HTTP/1.1 " + CodeStr + "\r\n";
            this._h_mutex.WaitOne();
            foreach (KeyValuePair<string, string> kvp in this._headers)
                Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            this._h_mutex.ReleaseMutex();
            if (dopHeaders != null)
                foreach (KeyValuePair<string, string> kvp in dopHeaders)
                    Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            Str += "Content-type: text/html\r\nContent-Length:" + Html.Length.ToString() + "\r\n\r\n" + Html;
            // Приведем строку к виду массива байт
            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
            // Отправим его клиенту
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            // Закроем соединение
            //Client.Client.Close();
            //Client.Close();
        }
        public virtual void HttpClientSendError(TcpClient Client, int Code)
        {
            HttpClientSendError(Client, Code, null);
        }
        
        protected override void GetClient(TcpClient Client, ulong id)
        {
            string Request = "";
            int DCRLF = -1;

            try
            {
                int b = -1;
                while ((b = Client.GetStream().ReadByte()) >= 0)
                {
                    Request += Encoding.ASCII.GetString(new byte[] { (byte)b }, 0, 1);
                    if (b == 0x0A)
                        DCRLF = Request.IndexOf("\r\n\r\n");
                    if (DCRLF >= 0 || Request.Length > 4096) { break; };
                };
                bool valid = (DCRLF > 0);
                if (!valid)
                {
                    HttpClientSendError(Client, 400); // 400 Bad Request
                    return;
                };

                if (_authRequired && (AuthentificationCredintals.Count > 0))
                {
                    bool accept = false;
                    string sa = "Authorization:";
                    if (Request.IndexOf(sa) > 0)
                    {
                        int iofcl = Request.IndexOf(sa);
                        sa = Request.Substring(iofcl + sa.Length, Request.IndexOf("\r", iofcl + sa.Length) - iofcl - sa.Length).Trim();
                        if (sa.StartsWith("Basic"))
                        {
                            sa = Base64Decode(sa.Substring(6));
                            string[] up = sa.Split(new char[] { ':' }, 2);
                            if (AuthentificationCredintals.ContainsKey(up[0]) && AuthentificationCredintals[up[0]] == up[1])
                                accept = true;
                        };
                    };
                    if (!accept)
                    {
                        Dictionary<string, string> dh = new Dictionary<string, string>();
                        dh.Add("WWW-Authenticate", "Basic realm=\"Authentification required\"");
                        HttpClientSendError(Client, 401, dh); // 401 Unauthorized
                        return;
                    };
                };

                GetClientRequest(Client, id, Request);
            }
            catch (Exception ex)
            {
                onError(Client, id, ex);
            };
        }

        protected virtual void GetClientRequest(TcpClient Client, ulong clientID, string Request)
        {
            HttpClientSendError(Client, 501);
            // connection will be close after return   
        }

        protected override void onError(TcpClient Client, ulong id, Exception error)
        {

        }
    }

    /// <summary>
    ///     Protobuf HTTP sever
    ///     curl http://127.0.0.1:8011/PROTOBUF+4/ --upload-file C:\xxx.bin
    //      curl http://127.0.0.1:8011/PROTOBUF+4/ --upload-file C:\xxx.bin --output c:\yyy.bin
    /// </summary>
    public class Threaded4BytesHttpServer : Threaded4BytesTCPServer
    {
        public Threaded4BytesHttpServer() : base(80) { }
        public Threaded4BytesHttpServer(int Port) : base(Port) { }
        public Threaded4BytesHttpServer(IPAddress IP, int Port) : base(IP, Port) { }
        ~Threaded4BytesHttpServer() { this.Dispose(); }

        private Mutex _h_mutex = new Mutex();
        private Dictionary<string, string> _headers = new Dictionary<string, string>();
        public Dictionary<string, string> Headers
        {
            get
            {
                _h_mutex.WaitOne();
                Dictionary<string, string> res = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> kvp in _headers)
                    res.Add(kvp.Key, kvp.Value);
                _h_mutex.ReleaseMutex();
                return res;
            }
            set
            {
                _h_mutex.WaitOne();
                _headers.Clear();
                foreach (KeyValuePair<string, string> kvp in value)
                    _headers.Add(kvp.Key, kvp.Value);
                _h_mutex.ReleaseMutex();
            }
        }

        private bool _authRequired = false;
        public Dictionary<string, string> AuthentificationCredintals = new Dictionary<string, string>();
        public bool AuthentificationRequired { get { return _authRequired; } set { _authRequired = value; } }

        // Отправка данных
        public virtual void HttpClientSendData(TcpClient Client, byte[] data, Dictionary<string, string> dopHeaders)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            int Code = 200;
            string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str = "HTTP/1.1 " + CodeStr + "\r\n";
            this._h_mutex.WaitOne();
            foreach (KeyValuePair<string, string> kvp in this._headers)
                Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            this._h_mutex.ReleaseMutex();
            if (dopHeaders != null)
                foreach (KeyValuePair<string, string> kvp in dopHeaders)
                    Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            Str += "Content-type: application/" + this._prefix + "\r\nContent-Length:" + data.Length.ToString() + "\r\n\r\n";
            // Приведем строку к виду массива байт
            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
            // Отправим его клиенту
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            Client.GetStream().Write(data, 0, data.Length);
            // Закроем соединение
            //Client.Client.Close();
            //Client.Close();
        }
        public virtual void HttpClientSendData(TcpClient Client, byte[] data)
        {
            HttpClientSendData(Client, data, null);
        }

        // Отправка страницы с ошибкой
        public virtual void HttpClientSendError(TcpClient Client, int Code, Dictionary<string, string> dopHeaders)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
            // Код простой HTML-странички
            string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str = "HTTP/1.1 " + CodeStr + "\r\n";
            this._h_mutex.WaitOne();
            foreach (KeyValuePair<string, string> kvp in this._headers)
                Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            this._h_mutex.ReleaseMutex();
            if (dopHeaders != null)
                foreach (KeyValuePair<string, string> kvp in dopHeaders)
                    Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            Str += "Content-type: text/html\r\nContent-Length:" + Html.Length.ToString() + "\r\n\r\n" + Html;
            // Приведем строку к виду массива байт
            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
            // Отправим его клиенту
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            // Закроем соединение
            //Client.Client.Close();
            //Client.Close();
        }
        public virtual void HttpClientSendError(TcpClient Client, int Code)
        {
            HttpClientSendError(Client, Code, null);
        }      
        
        protected override void GetClient(TcpClient Client, ulong id)
        {
            string s1 = "GET /" + this._prefix + "/";
            string s2 = "POST /" + this._prefix + "/";
            string s3 = "PUT /" + this._prefix + "/";

            string Request = "";
            int DCRLF = -1;

            try
            {
                int b = -1;
                while ((b = Client.GetStream().ReadByte()) >= 0)
                {
                    Request += Encoding.ASCII.GetString(new byte[] { (byte)b }, 0, 1);
                    if(b == 0x0A)
                        DCRLF = Request.IndexOf("\r\n\r\n");
                    if (DCRLF >= 0 || Request.Length > 4096) { break; };
                };
                bool valid = (DCRLF > 0) && ((Request.IndexOf("GET") == 0) || (Request.IndexOf("POST") == 0) || (Request.IndexOf("PUT") == 0));
                if (!valid)
                {
                    HttpClientSendError(Client, 400); // 400 Bad Request
                    return;
                };

                if (_authRequired && (AuthentificationCredintals.Count > 0))
                {
                    bool accept = false;
                    string sa = "Authorization:";
                    if (Request.IndexOf(sa) > 0)
                    {
                        int iofcl = Request.IndexOf(sa);
                        sa = Request.Substring(iofcl + sa.Length, Request.IndexOf("\r", iofcl + sa.Length) - iofcl - sa.Length).Trim();
                        if (sa.StartsWith("Basic"))
                        {
                            sa = Base64Decode(sa.Substring(6));
                            string[] up = sa.Split(new char[] { ':' }, 2);
                            if (AuthentificationCredintals.ContainsKey(up[0]) && AuthentificationCredintals[up[0]] == up[1])
                                accept = true;
                        };
                    };
                    if (!accept)
                    {
                        Dictionary<string, string> dh = new Dictionary<string, string>();
                        dh.Add("WWW-Authenticate", "Basic realm=\"Authentification required\"");
                        HttpClientSendError(Client, 401, dh); // 401 Unauthorized
                        return;
                    };
                };

                string cl = "Content-Length:";
                int bytesCount = 0;
                if (Request.IndexOf(cl) > 0)
                {
                    int iofcl = Request.IndexOf(cl);
                    cl = Request.Substring(iofcl + cl.Length, Request.IndexOf("\r", iofcl + cl.Length) - iofcl - cl.Length);
                    int.TryParse(cl.Trim(), out bytesCount);
                };
                if (bytesCount == 0)
                {
                    HttpClientSendError(Client, 411); // 411 Length Required
                    return;
                }
                if (bytesCount < (this._prefix.Length + 4))
                {
                    HttpClientSendError(Client, 406); // 406 Not Acceptable
                    return;
                };

                byte[] buff = new byte[this._prefix.Length + 4];
                Client.GetStream().Read(buff, 0, buff.Length);
                string prefix = System.Text.Encoding.GetEncoding(1251).GetString(buff, 0, this._prefix.Length);
                if (prefix != this._prefix)
                {
                    HttpClientSendError(Client, 415); // 415 Unsupported Media Type
                    return;
                };

                int length = System.BitConverter.ToInt32(buff, this._prefix.Length);
                if (length > bytesCount)
                {
                    HttpClientSendError(Client, 416); // 416 Range Not Satisfiable
                    return;
                };
                buff = new byte[length];
                Client.GetStream().Read(buff, 0, buff.Length);
                GetClientRequestData(Client, id, Request, buff);                
            }
            catch (Exception ex)
            {
                onError(Client, id, ex);
            };
        }

        /// <summary>
        ///     Get Client Request with 1st block of data
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="id"></param>
        /// <param name="data"></param>
        public virtual void GetClientRequestData(TcpClient Client, ulong id, string Request, byte[] data)
        {
            // Write No Data To Client
            List<byte> result = new List<byte>();
            result.AddRange(System.Text.Encoding.ASCII.GetBytes(this._prefix));
            byte[] buff = System.Text.Encoding.ASCII.GetBytes("Hello " + DateTime.Now.ToString());
            result.AddRange(BitConverter.GetBytes(buff.Length));
            result.AddRange(buff);
            HttpClientSendData(Client, result.ToArray());

            // connection will be close after return   
        }

        protected override void onError(TcpClient Client, ulong id, Exception error)
        {

        }


        private static void sample()
        {
            // curl http://127.0.0.1:8011 --upload-file C:\xxx.bin
            // curl http://127.0.0.1:8011 --upload-file C:\xxx.bin --output c:\yyy.bin
            // curl --user sa:q http://127.0.0.1:8011 --upload-file C:\xxx.bin

            SimpleServers.Threaded4BytesHttpServer svr = new SimpleServers.Threaded4BytesHttpServer(8011);
            svr.Headers.Add("Server", "Threaded4BytesHttpServer/0.1");
            svr.Headers.Add("Server-Name", "TEST SAMPLE");
            svr.Headers.Add("Server-Owner", "I am");
            svr.AuthentificationCredintals.Add("sa", "q");
            svr.AuthentificationRequired = false;
            svr.ListenIPMode = SimpleServers.ThreadedTCPServer.Mode.DenyBlackList;
            svr.ListenIPDeny = new string[] { "127.0.0.2" };
            svr.Start();
            //
            System.Threading.Thread.Sleep(10000);
            //
            svr.Stop();
            svr.Dispose();
        }
    }

    public class AsyncTest
    {
        public class CustomForm : System.Windows.Forms.Form
        {
            public string SetText(string world)
            {
                this.Text = world;
                return this.Text;
            }
        }

        public string Hello(string world)
        {
            return world;
        }

        public delegate string AsyncMethodCaller(string world);

        public static string call_Hello_1(string world)
        {
            AsyncTest obj = new AsyncTest();
            return obj.Hello(world);
        }

        public static string call_Hello_2(string world)
        {
            AsyncTest obj = new AsyncTest();
            AsyncMethodCaller caller = new AsyncMethodCaller(obj.Hello);
            return caller.Invoke(world);
        }

        public static string call_Hello_3(string world)
        {
            AsyncTest obj = new AsyncTest();
            AsyncMethodCaller caller = new AsyncMethodCaller(obj.Hello);
            IAsyncResult res = caller.BeginInvoke(world, null, null);
            return caller.EndInvoke(res);
        }

        public static string call_Hello_4(string world)
        {
            AsyncTest obj = new AsyncTest();
            AsyncMethodCaller caller = new AsyncMethodCaller(obj.Hello);
            IAsyncResult res = caller.BeginInvoke(world, null, null);
            res.AsyncWaitHandle.WaitOne();
            return caller.EndInvoke(res);
        }

        public static string call_Hello_5(string world)
        {
            AsyncTest obj = new AsyncTest();
            AsyncMethodCaller caller = new AsyncMethodCaller(obj.Hello);
            IAsyncResult res = caller.BeginInvoke(world, null, null);
            while (!res.IsCompleted) System.Threading.Thread.Sleep(100);
            return caller.EndInvoke(res);
        }

        public static string call_Hello_6(string world)
        {
            AsyncTest obj = new AsyncTest();
            CallbackMethod6_caller = new AsyncMethodCaller(obj.Hello);
            IAsyncResult res = CallbackMethod6_caller.BeginInvoke(world, new AsyncCallback(CallbackMethod6), CallbackMethod6_caller);
            System.Threading.Thread.Sleep(200);
            return CallbackMethod6_result;
        }

        public static string call_Hello_7(string world)
        {
            CustomForm lbl = new CustomForm();
            lbl.Show();
            
            string res = (string)lbl.Invoke(new AsyncMethodCaller(lbl.SetText), new object[] { world });
            lbl.Invoke(new System.Windows.Forms.MethodInvoker(lbl.Close));

            lbl.Dispose();

            return res;
        }

        private static AsyncMethodCaller CallbackMethod6_caller = null;
        private static string CallbackMethod6_result = null;
        public static void CallbackMethod6(IAsyncResult res)
        {
            CallbackMethod6_result = CallbackMethod6_caller.EndInvoke(res);            
        }
    }
}
