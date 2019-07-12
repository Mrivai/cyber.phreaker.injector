using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ÇЎßÈŘ_þHŘÈÄKÈŘ_ÎÑJÊKTÕR__.Handler
{
    class UniversalHandler
    {

        private Encoding ASCII = Encoding.ASCII;
        private Thread threadListen;
        private int threadCount;
        private TcpListener tcp;
        private List<Thread> thrC;
        private List<Thread> thrS;
        private int thisPort = 8080;
        private string fwdServer = "";
        internal Socket fromClient;
        internal string proxy;
        internal int port;
        internal long timeout;
        public const int DEFAULT_TIMEOUT = 20000;
        private int socketTimeout = 20000;
        internal string FQ = "";
        internal string MQ = "";
        internal string BQ = "";
        internal string CmT = "";
        internal string PT = "";
        
        public UniversalHandler(string proxy, int port, long tout)
        {
            this.proxy = proxy;
            this.port = port;
            this.timeout = tout;
        }
        
        public UniversalHandler(string proxy, int port, long tout, string FQue, string MQue, string BQue, string CmTyp, string PTyp)
        {
            this.proxy = proxy;
            this.port = port;
            this.timeout = tout;
            this.FQ = FQue;
            this.MQ = MQue;
            this.BQ = BQue;
            this.CmT = CmTyp;
            this.PT = PTyp;
        }

        public virtual StringBuilder modifHeader(StringBuilder data)
        {
            int fq1 = -1;
            int fq2 = -1;
            int fq3 = -1;
            int ptx = -1;
            string fqs1 = "";
            string fqs2 = "";
            string fqs3 = "";
            StringBuilder hasil = new StringBuilder("");
            string que1 = "";
            string que2 = "";
            
            StringBuilder gabung = new StringBuilder("");
            StringBuilder hosts = new StringBuilder("");
             if (data.ToString().ToLower().IndexOf("\r\n\r\n") >= 0)
                        {
                            string[] arrHead = data.ToString().Split(new string[1] { "\r\n" }, StringSplitOptions.None);
                            for (int i = 0; i < arrHead.Length; i++)
                            {
                                fq1 = arrHead[i].ToLower().IndexOf("get http://");
                                fq2 = arrHead[i].ToLower().IndexOf("post http://");
                                fq3 = arrHead[i].ToLower().IndexOf("connect ");
                                ptx = arrHead[i].ToLower().IndexOf("host:");

                                if (fq1 >= 0)
                                {
                                    fqs1 = arrHead[i].Substring(fq1 + 11).Trim();
                                    que1 = fqs1.Substring(0, fqs1.ToLower().IndexOf("/"));
                                    que2 = fqs1.Substring(fqs1.ToLower().IndexOf("/"), fqs1.ToLower().IndexOf(" http/1.") - (fqs1.ToLower().IndexOf("/")));
                                    arrHead[i] = arrHead[i].Replace(que1, this.FQ + que1 + this.MQ);
                                    arrHead[i] = arrHead[i].Replace(que2, que2 + this.BQ);
                                }

                                else if (fq2 >= 0)
                                {
                                    fqs2 = arrHead[i].Substring(fq2 + 12).Trim();
                                    que1 = fqs2.Substring(0, fqs2.ToLower().IndexOf("/"));
                                    que2 = fqs2.Substring(fqs2.ToLower().IndexOf("/"), fqs2.ToLower().IndexOf(" http/") - (fqs2.ToLower().IndexOf("/")));
                                    arrHead[i] = arrHead[i].Replace(que1, this.FQ + que1 + this.MQ);
                                    arrHead[i] = arrHead[i].Replace(que2, que2 + this.BQ);
                                }

                                else if (fq3 >= 0)
                                {
                                    fqs3 = arrHead[i].Substring(fq3 + 8).Trim();
                                    arrHead[i] = arrHead[i].Replace(fqs3, this.FQ + fqs3);
                                }

                                else if (ptx >= 0)
                                {
                                    if (this.CmT.Equals("host"))
                                    {
                                        gabung.Append("X-Online-Host: ").Append(this.PT).Append("\r\n");
                                    }

                                    else if (this.CmT.Equals("http"))
                                    {
                                        hosts.Length = 0;
                                        hosts.Append(data.ToString().Substring(ptx + 5).Trim());
                                        arrHead[i] = arrHead[i].Replace(hosts.ToString(), this.PT);
                                    }
                                }

                                gabung.Append(arrHead[i]);
                                gabung.Append("\r\n");
                            }

                            gabung.Append("\r\n");
                            hasil = gabung;
                        }
                        else
                        {
                            hasil = gabung;
                        }
                        return hasil;
        }

        public void Starlistener()
        {
            if (true)
            {
                this.tcp = new TcpListener(new IPAddress(0), Convert.ToInt32(port));
                try
                {
                    tcp.Start();
                    this.threadListen = new Thread(Starlisten);
                    this.threadListen.Start();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return;
                }
            }
        }

        private void Starlisten()
        {
            while (true)
            {
                try
                {
                    TcpClient tcpClient = this.tcp.AcceptTcpClient();
                    new Thread((ParameterizedThreadStart)(a0 => this.requesthandler((TcpClient)a0))).Start((object)tcpClient);

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    break;
                }
            }
        }

        private void requesthandler(TcpClient cln)
        {
            Socket socket = this.konekProxyServer();
            if (socket == null)
                cln.Close();
            else if (socket.Connected)
            {
                Thread thread1 = new Thread((ParameterizedThreadStart)(a0 => this.transferClientToServer((object[])a0)));
                thread1.Start((object)new object[3]
                {
                (object) cln,
                (object) socket,
                (object) this.thrC.Count
                });
                this.thrC.Add(thread1);
                Thread thread2 = new Thread((ParameterizedThreadStart)(a0 => this.transferServerToClient((object[])a0)));
                thread2.Start((object)new object[3]
                {
                (object) cln,
                (object) socket,
                (object) this.thrS.Count
                });
                this.thrS.Add(thread2);
            }
            else
                cln.Close();
        }

        private Socket konekProxyServer()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(this.proxy, Convert.ToInt32(this.port));
                byte[] buffer = new byte[checked(socket.ReceiveBufferSize + 1)];
                if (socket.Receive(buffer) <= 2)
                {
                    socket.Close();
                    socket = (Socket)null;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                if (socket.Connected)
                    socket.Close();
                socket = (Socket)null;
            }
            return socket;
        }

        private void transferClientToServer(object[] param)
        {
            
            TcpClient tcpClient = (TcpClient)param[0];
            Socket socket = (Socket)param[1];
            int index = Convert.ToInt32(param[2]);
            bool flag = true;
            int num1 = 0;
            int num2 = 0;
            StringBuilder dat = new StringBuilder();
            
            do
            {
                try
                {
                    do
                    {
                        dat.Append(tcpClient.GetStream());
                        StringBuilder modif = modifHeader(dat);
                        Byte[] ReadBuff = new byte[checked(tcpClient.SendBufferSize + 1)];
                        byte[] Data = Encoding.ASCII.GetBytes(modif.ToString());
                        int size = tcpClient.GetStream().Read(Data, 0, Data.Length);
                        socket.Send(Data, size, SocketFlags.None);
                        checked { ++num1; }
                        checked { num2 += size; }
                        if (size < 2)
                        {
                            if (tcpClient != null)
                            {
                                if (tcpClient.Connected)
                                {
                                    tcpClient.Close();
                                    break;
                                }
                                break;
                            }
                            break;
                        }
                    }
                    while (tcpClient.GetStream().DataAvailable);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    if (tcpClient.Connected)
                        tcpClient.Close();
                    break;
                }
            }
            while (flag & tcpClient.Connected & socket.Connected);
            try
            {
                if (this.thrS[index] != null && this.thrS[index].IsAlive)
                    this.thrS[index].Abort();
                if (tcpClient.Connected)
                    tcpClient.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            try
            {
                if (socket == null || !socket.Connected)
                    return;
                socket.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void transferServerToClient(object[] param)
        {
            TcpClient tcpClient = (TcpClient)param[0];
            Socket socket = (Socket)param[1];
            int index = Convert.ToInt32(param[2]);
            int num1 = 0;
            int num2 = 0;
            do
            {
                try
                {
                    do
                    {
                        checked { ++num1; }
                        byte[] buffer = new byte[checked(socket.ReceiveBufferSize + 1)];
                        int size = socket.Receive(buffer);
                        tcpClient.GetStream().Write(buffer, 0, size);
                        checked { num2 += size; }
                        if (size < 2)
                        {
                            if (socket != null)
                            {
                                if (socket.Connected)
                                {
                                    socket.Close();
                                    break;
                                }
                                break;
                            }
                            break;
                        }
                    }
                    while (socket.Available > 0);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    if (socket.Connected)
                        socket.Close();
                    break;
                }
            }
            while (tcpClient.Connected & socket.Connected);
            try
            {
                if (this.thrC[index] != null && this.thrC[index].IsAlive)
                    this.thrC[index].Abort();
                if (tcpClient != null && tcpClient.Connected)
                    tcpClient.Close();
                if (!socket.Connected)
                    return;
                socket.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
