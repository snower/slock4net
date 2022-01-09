using slock4net.Commands;
using slock4net.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace slock4net
{
    public class SlockClient : IClient
    {
        protected string host;
        protected int port;
        protected Socket socket;
        protected Thread thread;
        protected Database[] databases;
        protected Dictionary<String, Command> requests;
        protected SlockReplsetClient replsetClient;
        protected byte[] clientId;
        protected bool closed = false;

        public SlockClient() : this("127.0.0.1", 5658)
        {
        }

        public SlockClient(string host) : this(host, 5658)
        {
        }

        public SlockClient(string host, int port)
        {
            this.host = host;
            this.port = port;
            this.databases = new Database[256];
            this.requests = new Dictionary<String, Command>();
        }

        public SlockClient(String host, int port, SlockReplsetClient replsetClient, Database[] databases)
        {
            this.host = host;
            this.port = port;
            this.replsetClient = replsetClient;
            this.databases = databases;
            this.requests = new Dictionary<String, Command>();
        }

        public void Open()
        {
            if (this.socket != null)
            {
                return;
            }

            this.Connect();
            this.thread = new Thread(new ThreadStart(this.Run));
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        public bool TryOpen()
        {
            try
            {
                if(this.socket != null)
                {
                    return true;
                }
                this.Connect();
            } catch(Exception)
            {
                this.thread = new Thread(new ThreadStart(this.Run));
                this.thread.IsBackground = true;
                this.thread.Start();
                return false;
            }
            this.thread = new Thread(new ThreadStart(this.Run));
            this.thread.IsBackground = true;
            this.thread.Start();
            return true;
        }

        public void Close()
        {
            this.closed = true;
            if (this.socket != null)
            {
                try
                {
                    this.socket.Close();
                }
                catch (SocketException)
                {
                }
            }

            lock (this)
            {
                foreach (string requestId in this.requests.Keys)
                {
                    Command command = this.requests[requestId];
                    this.requests.Remove(requestId);
                    if (command == null)
                    {
                        continue;
                    }
                    command.CommandResult = null;
                    command.WakeupWaiter();
                }

                for (int i = 0; i < databases.Length; i++)
                {
                    if (databases[i] != null)
                    {
                        databases[i].Close();
                    }
                    databases[i] = null;
                }
            }
        }

        protected void Connect()
        {
            SocketException connectErr = null;
            IPHostEntry hostEntry = Dns.GetHostEntry(this.host);
            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, port);
                Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    tempSocket.Connect(ipe);
                    if (tempSocket.Connected)
                    {
                        this.socket = tempSocket;
                        break;
                    }
                    continue;
                } catch(SocketException e)
                {
                    connectErr = e;
                }
            }
            if(this.socket == null)
            {
                if(connectErr != null)
                {
                    throw connectErr;
                }
                throw new SocketException((int)SocketError.SocketError);
            }

            this.InitClient();
            if (this.replsetClient != null)
            {
                replsetClient.AddLivedClient(this);
            }
        }

        protected void Reconnect()
        {
            lock (this)
            {
                foreach (String requestId in this.requests.Keys)
                {
                    Command command = this.requests[requestId];
                    this.requests.Remove(requestId);
                    if (command == null)
                    {
                        continue;
                    }
                    command.CommandResult = null;
                    command.WakeupWaiter();
                }
            }

            while (!this.closed)
            {
                try
                {
                    this.Connect();
                    if (this.socket != null)
                    {
                        return;
                    }
                }
                catch (Exception) { }
                Thread.Sleep(2000);
            }
        }

        protected void CloseSocket()
        {
            lock (this)
            {
                if (this.socket != null)
                {
                    try
                    {
                        this.socket.Close();
                    }
                    catch (Exception)
                    {
                    }
                    this.socket = null;
                    if (this.replsetClient != null)
                    {
                        this.replsetClient.RemoveLivedClient(this);
                    }
                }
            }
        }

        protected void InitClient()
        {
            if (clientId == null)
            {
                clientId = InitCommand.GenClientId();
            }
            InitCommand initCommand = new InitCommand(clientId);
            byte[] buffer = initCommand.DumpCommand();
            try
            {
                this.socket.Send(buffer);
            }
            catch (Exception e)
            {
                this.CloseSocket();
                throw e;
            }

            try
            {
                int n = this.socket.Receive(buffer, 64, SocketFlags.None);
                while (n > 0 && n < 64)
                {
                    int nn = this.socket.Receive(buffer, n, 64 - n, SocketFlags.None);
                    if (nn <= 0)
                    {
                        break;
                    }
                    n += nn;
                }
                if (n < 64)
                {
                    throw new SocketException(-2);
                }
                InitCommandResult initCommandResult = new InitCommandResult();
                if (initCommandResult.LoadCommand(buffer) != null)
                {
                    if (initCommandResult.Result != ICommand.COMMAND_RESULT_SUCCED)
                    {
                        throw new SocketException(-2);
                    }
                }
            }
            catch (Exception e)
            {
                this.CloseSocket();
                throw e;
            }
        }

        protected void Run()
        {
            try
            {
                while (!this.closed)
                {
                    try
                    {
                        if (this.socket == null)
                        {
                            this.Reconnect();
                            continue;
                        }

                        byte[] buffer = new byte[64];
                        while (!this.closed)
                        {
                            try
                            {
                                int n = this.socket.Receive(buffer, 64, SocketFlags.None);
                                while (n > 0 && n < 64)
                                {
                                    int nn = this.socket.Receive(buffer, n, 64 - n, SocketFlags.None);
                                    if(nn <= 0)
                                    {
                                        break;
                                    }
                                    n += nn;
                                }
                                if(n < 64)
                                {
                                    break;
                                }
                            }
                            catch (SocketException)
                            {
                                break;
                            }

                            switch (buffer[2])
                            {
                                case ICommand.COMMAND_TYPE_LOCK:
                                case ICommand.COMMAND_TYPE_UNLOCK:
                                    LockCommandResult lockCommandResult = new LockCommandResult();
                                    if (lockCommandResult.LoadCommand(buffer) != null)
                                    {
                                        this.HandleCommand(lockCommandResult);
                                    }
                                    break;
                                case ICommand.COMMAND_TYPE_PING:
                                    PingCommandResult pingCommandResult = new PingCommandResult();
                                    if (pingCommandResult.LoadCommand(buffer) != null)
                                    {
                                        this.HandleCommand(pingCommandResult);
                                    }
                                    break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(2000);
                    }

                    this.CloseSocket();
                }

            }
            finally
            {
                this.CloseSocket();
                this.closed = true;
                this.replsetClient = null;
            }
        }

        protected void HandleCommand(CommandResult commandResult)
        {
            String requestId = Encoding.Unicode.GetString(commandResult.GetRequestId());
            if (!this.requests.ContainsKey(requestId))
            {
                return;
            }
            Command command = this.requests[requestId];
            this.requests.Remove(requestId);
            if (command == null)
            {
                return;
            }
            command.CommandResult = commandResult;
            command.WakeupWaiter();
        }

        public CommandResult SendCommand(Command command)
        {
            if (this.closed)
            {
                throw new ClientClosedException();
            }

            byte[] buf = command.DumpCommand();
            if (!command.CreateWaiter())
            {
                throw new ClientCommandException();
            }

            String requestId = Encoding.Unicode.GetString(command.GetRequestId());
            lock (this)
            {
                if (this.socket == null)
                {
                    throw new ClientUnconnectException();
                }

                this.requests.Add(requestId, command);
                try
                {
                    int n = this.socket.Send(buf);
                    while (n > 0 && n < 64)
                    {
                        int nn = this.socket.Send(buf, n, 64 - n, SocketFlags.None);
                        if(nn <= 0)
                        {
                            break;
                        }
                        n += nn;
                    }
                    if(n < 64)
                    {
                        throw new ClientOutputStreamException();
                    }
                }
                catch (SocketException)
                {
                    this.requests.Remove(requestId);
                    try
                    {
                        this.socket.Close();
                    }
                    catch (Exception)
                    {
                    }
                    throw new ClientOutputStreamException();
                }
            }

            if (!command.WaitWaiter())
            {
                this.requests.Remove(requestId);
                throw new ClientCommandTimeoutException();
            }

            if (command.CommandResult == null)
            {
                throw new ClientClosedException();
            }
            return command.CommandResult;
        }

        public Database SelectDatabase(byte databaseId)
        {
            if (this.databases[databaseId] == null)
            {
                lock (this)
                {
                    if (this.databases[databaseId] == null)
                    {
                        this.databases[databaseId] = new Database(this, databaseId);
                    }
                }
            }
            return this.databases[databaseId];
        }

        public bool Ping()
        {
            PingCommand pingCommand = new PingCommand();
            PingCommandResult pingCommandResult = (PingCommandResult)this.SendCommand(pingCommand);
            if (pingCommandResult != null && pingCommandResult.Result == ICommand.COMMAND_RESULT_SUCCED)
            {
                return true;
            }
            return false;
        }

        public Lock NewLock(byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewLock(lockKey, timeout, expried);
        }

        public Event NewEvent(byte[] eventKey, uint timeout, uint expried, bool defaultSeted)
        {
            return this.SelectDatabase(0).NewEvent(eventKey, timeout, expried, defaultSeted);
        }

        public ReentrantLock NewReentrantLock(byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewReentrantLock(lockKey, timeout, expried);
        }

        public ReadWriteLock NewReadWriteLock(byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewReadWriteLock(lockKey, timeout, expried);
        }

        public Semaphore NewSemaphore(byte[] semaphoreKey, ushort count, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewSemaphore(semaphoreKey, count, timeout, expried);
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(byte[] flowKey, ushort count, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewMaxConcurrentFlow(flowKey, count, timeout, expried);
        }

        public TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, ushort count, uint timeout, double period)
        {
            return this.SelectDatabase(0).NewTokenBucketFlow(flowKey, count, timeout, period);
        }
    }
}