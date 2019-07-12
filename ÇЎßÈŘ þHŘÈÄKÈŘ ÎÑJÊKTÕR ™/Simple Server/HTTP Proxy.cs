using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ÇЎßÈŘ_þHŘÈÄKÈŘ_ÎÑJÊKTÕR__.Simple_Server
{
    class HTTP_Proxy
    {
        private Thread threadListen;
        private int threadCount;
        private TcpListener tcp;
        private List<Thread> thrC;
        private List<Thread> thrS;
        
        private String rproxy, method, url, host, payload, rport ,lp;

        public HTTP_Proxy( string LPORT, string RPROXY, string RPORT, string METHOD ,string URL ,string HOST )
        {
            this.lp = LPORT;
            this.rproxy = RPROXY;
            this.rport = RPORT;
            this.method = METHOD;
            this.url = URL;
            this.host = HOST;
        }

        public void Star()
        {
            if (true)
            {
                this.tcp = new TcpListener(new IPAddress(0), Convert.ToInt32(lp));
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
            else
            {
                List <Thread>.Enumerator enumerator1;
                try
                {
                    enumerator1 = this.thrC.GetEnumerator();
                    while (enumerator1.MoveNext())
                    {
                        Thread current = enumerator1.Current;
                        try
                        {
                            if (current.IsAlive)
                                current.Abort();
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                    }
                }
                finally
                {
                    enumerator1.Dispose();
                }
                List<Thread>.Enumerator enumerator2;
                try
                {
                    enumerator2 = this.thrS.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        Thread current = enumerator2.Current;
                        try
                        {
                            if (current.IsAlive)
                                current.Abort();
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                    }
                }
                finally
                {
                    enumerator2.Dispose();
                }
            }
        }

        public void Starlisten()
        {
            while (true)
            {
                try
                {
                    TcpClient tcpClient = this.tcp.AcceptTcpClient();
                    new Thread((ParameterizedThreadStart)(a0 => this.requestHandler((TcpClient)a0))).Start((object)tcpClient);

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    break;
                }
            }
        }

        private void requestHandler(TcpClient cln)
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
                socket.Connect(this.rproxy, Convert.ToInt32(this.rport));
                this.payload = method + "http://"+ url +"/ HTTP/1.1\r\nHost: "+host+" \r\n\r\n\r\n";
                byte[] BytePayload = Encoding.ASCII.GetBytes (payload);
                socket.Send(BytePayload);
                byte[] buffer = new byte[checked(socket.ReceiveBufferSize + 1)];
                if (socket.Receive(buffer) <= 2)
                {
                    socket.Close();
                    socket = (Socket)null;
                }
            }
            catch (Exception e)
            {
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
            do
            {
                try
                {
                    do
                    {
                        byte[] buffer = new byte[checked(tcpClient.SendBufferSize + 1)];
                        int size = tcpClient.GetStream().Read(buffer, 0, buffer.Length);
                        socket.Send(buffer, size, SocketFlags.None);
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
