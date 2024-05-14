using slock4net.Commands;
using slock4net.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace slock4net
{
    public class SlockClient : ISlockClient
    {
        private static readonly char[] DIGITS_LOWER = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        private static string EncodeHex(byte[] data)
        {
            int l = data.Length;
            char[] outStr = new char[l << 1];
            for (int i = 0, j = 0; i < l; i++)
            {
                outStr[j++] = DIGITS_LOWER[(240 & data[i]) >> 4];
                outStr[j++] = DIGITS_LOWER[15 & data[i]];
            }
            return new string(outStr);
        }

        protected string host;
        protected int port;
        private ushort defaultTimeoutFlag;
        private ushort defaultExpriedFlag;
        protected Socket socket;
        protected Thread thread;
        protected SlockDatabase[] databases;
        protected ConcurrentDictionary<string, Command> requests;
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
            this.defaultTimeoutFlag = 0;
            this.defaultExpriedFlag = 0;
            this.databases = new SlockDatabase[256];
            this.requests = new ConcurrentDictionary<string, Command>();
        }

        public SlockClient(string host, int port, SlockReplsetClient replsetClient, SlockDatabase[] databases)
        {
            this.host = host;
            this.port = port;
            this.replsetClient = replsetClient;
            this.databases = databases;
            this.requests = new ConcurrentDictionary<string, Command>();
        }

        public virtual void SetDefaultTimeoutFlag(ushort defaultTimeoutFlag)
        {
            this.defaultTimeoutFlag = defaultTimeoutFlag;
            foreach (SlockDatabase database in databases)
            {
                if (database != null)
                {
                    database.DefaultTimeoutFlag = defaultTimeoutFlag;
                }
            }
        }

        public virtual void SetDefaultExpriedFlag(ushort defaultExpriedFlag)
        {
            this.defaultExpriedFlag = defaultExpriedFlag;
            foreach (SlockDatabase database in databases)
            {
                if (database != null)
                {
                    database.DefaultExpriedFlag = defaultExpriedFlag;
                }
            }
        }

        public void Open()
        {
            if (this.thread != null)
            {
                return;
            }

            this.Connect();
            this.thread = new Thread(new ThreadStart(this.Run));
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        public Task OpenAsync()
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    this.Open();
                } catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                    return;
                }
                taskCompletionSource.SetResult(true);
            }, TaskCreationOptions.LongRunning);
            return taskCompletionSource.Task;
        }

        public ISlockClient TryOpen()
        {
            try
            {
                if(this.thread != null)
                {
                    return this;
                }
                this.Connect();
            } catch(Exception)
            {
                this.thread = new Thread(new ThreadStart(this.Run));
                this.thread.IsBackground = true;
                this.thread.Start();
                return null;
            }
            this.thread = new Thread(new ThreadStart(this.Run));
            this.thread.IsBackground = true;
            this.thread.Start();
            return this;
        }

        public Task<ISlockClient> TryOpenAsync()
        {
            TaskCompletionSource<ISlockClient> taskCompletionSource = new TaskCompletionSource<ISlockClient>();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    taskCompletionSource.SetResult(this.TryOpen());
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            }, TaskCreationOptions.LongRunning);
            return taskCompletionSource.Task;
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
                foreach (string requestId in this.requests.Keys.ToArray())
                {
                    if (this.requests.TryRemove(requestId, out Command command) && command != null)
                    {
                        command.CommandResult = null;
                        if (!command.WakeupWaiter())
                        {
                            command.WakeupTask();
                        }
                    }
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

        public Task CloseAsync()
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    this.Close();
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                    return;
                }
                taskCompletionSource.SetResult(true);
            }, TaskCreationOptions.LongRunning);
            return taskCompletionSource.Task;
        }

        protected void Connect()
        {
            SocketException connectErr = null;
            if (IPAddress.TryParse(this.host, out IPAddress iPAddress))
            {
                IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);
                Socket tempSocket = new Socket(iPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    tempSocket.Connect(iPEndPoint);
                    if (tempSocket.Connected)
                    {
                        this.socket = tempSocket;
                    }
                }
                catch (SocketException e)
                {
                    connectErr = e;
                }
            }
            else
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(this.host);
                foreach (IPAddress address in hostEntry.AddressList)
                {
                    IPEndPoint iPEndPoint = new IPEndPoint(address, port);
                    Socket tempSocket = new Socket(iPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        tempSocket.Connect(iPEndPoint);
                        if (tempSocket.Connected)
                        {
                            this.socket = tempSocket;
                            break;
                        }
                        continue;
                    }
                    catch (SocketException e)
                    {
                        connectErr = e;
                    }
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

            this.socket.NoDelay = true;
            InitCommandResult initCommandResult = this.InitClient();
            if (this.replsetClient != null)
            {
                replsetClient.AddLivedClient(this, initCommandResult.InitType == 1);
            }
        }

        protected void Reconnect()
        {
            lock (this)
            {
                foreach (string requestId in this.requests.Keys.ToArray())
                {
                    if (this.requests.TryRemove(requestId, out Command command) && command != null)
                    {
                        command.CommandResult = null;
                        if (!command.WakeupWaiter())
                        {
                            command.WakeupTask();
                        }
                    }
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
                        this.socket.Disconnect(false);
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

        protected InitCommandResult InitClient()
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
            catch (Exception)
            {
                this.CloseSocket();
                throw;
            }

            try
            {
                if (ReceiveBytes(buffer, 64) < 64)
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
                return initCommandResult;
            }
            catch (Exception)
            {
                this.CloseSocket();
                throw;
            }
        }

        private int ReceiveBytes(byte[] buffer, int length)
        {
            int n = this.socket.Receive(buffer, length, SocketFlags.None);
            while (n > 0 && n < length) {
                int nn = this.socket.Receive(buffer, n, length - n, SocketFlags.None);
                if (nn <= 0) {
                    break;
                }
                n += nn;
            }
            return n;
        }

        private int ReceiveBytes(byte[] buffer, int offset, int length)
        {
            int n = this.socket.Receive(buffer, offset, length, SocketFlags.None);
            while (n > 0 && n < length)
            {
                int nn = this.socket.Receive(buffer, offset + n, length - n, SocketFlags.None);
                if (nn <= 0)
                {
                    break;
                }
                n += nn;
            }
            return n;
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
                                int n = this.ReceiveBytes(buffer, 64);
                                if (n < 64)
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
                                        if (lockCommandResult.HasExtraData())
                                        {
                                            int n = this.ReceiveBytes(buffer, 4);
                                            if (n < 4)
                                            {
                                                break;
                                            }
                                            int dataLength = buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24;
                                            byte[] data = new byte[dataLength + 4];
                                            if (ReceiveBytes(data, 4, dataLength) < dataLength)
                                            {
                                                break;
                                            }
                                            lockCommandResult.LoadCommandData(data);
                                        }
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
            string requestId = EncodeHex(commandResult.GetRequestId());
            if (this.requests.TryRemove(requestId, out Command command) && command != null)
            {
                command.CommandResult = commandResult;
                if (!command.WakeupWaiter())
                {
                    command.WakeupTask();
                }
            }
        }

        private int SendBytes(byte[] buffer)
        {
            int n = this.socket.Send(buffer);
            while (n > 0 && n < buffer.Length)
            {
                int nn = this.socket.Send(buffer, n, buffer.Length - n, SocketFlags.None);
                if (nn <= 0)
                {
                    break;
                }
                n += nn;
            }
            return n;
        }

        public CommandResult SendCommand(Command command)
        {
            if (this.closed)
            {
                throw new ClientClosedException();
            }

            byte[] buffer = command.DumpCommand();
            if (!command.CreateWaiter())
            {
                throw new ClientCommandException();
            }

            string requestId = EncodeHex(command.GetRequestId());
            lock (this)
            {
                if (this.socket == null)
                {
                    throw new ClientUnconnectException();
                }
                if (!this.requests.TryAdd(requestId, command))
                {
                    throw new ClientCommandException();
                }

                try
                {
                    if(SendBytes(buffer) < 64)
                    {
                        throw new ClientOutputStreamException();
                    }
                    byte[] extraData = command.GetExtraData();
                    if (extraData != null)
                    {
                        if (SendBytes(extraData) < extraData.Length)
                        {
                            throw new ClientOutputStreamException();
                        }
                    }
                }
                catch (SocketException)
                {
                    this.requests.TryRemove(requestId, out Command c);
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
                this.requests.TryRemove(requestId, out Command c);
                throw new ClientCommandTimeoutException();
            }
            if (command.CommandResult == null)
            {
                throw new ClientClosedException();
            }
            return command.CommandResult;
        }

        public async Task<CommandResult> SendCommandAsync(Command command)
        {
            if (this.closed)
            {
                throw new ClientClosedException();
            }
            byte[] buffer = command.DumpCommand();
            if (!command.CreateTask())
            {
                throw new ClientCommandException();
            }

            string requestId = EncodeHex(command.GetRequestId());
            lock (this)
            {
                if (this.socket == null)
                {
                    throw new ClientUnconnectException();
                }
                if (!this.requests.TryAdd(requestId, command))
                {
                    throw new ClientCommandException();
                }

                try
                {
                    if (SendBytes(buffer) < 64)
                    {
                        throw new ClientOutputStreamException();
                    }
                    byte[] extraData = command.GetExtraData();
                    if (extraData != null)
                    {
                        if(SendBytes(extraData) < extraData.Length)
                        {
                            throw new ClientOutputStreamException();
                        }
                    }
                }
                catch (SocketException)
                {
                    this.requests.TryRemove(requestId, out Command c);
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

            Task<bool> task = command.WaitTask();
            if (task == null)
            {
                this.requests.TryRemove(requestId, out Command c);
                throw new ClientCommandTimeoutException();
            }
            await task;
            if (command.CommandResult == null)
            {
                throw new ClientClosedException();
            }
            return command.CommandResult;
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

        public async Task<bool> PingAsync()
        {
            PingCommand pingCommand = new PingCommand();
            PingCommandResult pingCommandResult = (PingCommandResult) await this.SendCommandAsync(pingCommand);
            if (pingCommandResult != null && pingCommandResult.Result == ICommand.COMMAND_RESULT_SUCCED)
            {
                return true;
            }
            return false;
        }

        public SlockDatabase SelectDatabase(byte databaseId)
        {
            if (this.databases[databaseId] == null)
            {
                lock (this)
                {
                    if (this.databases[databaseId] == null)
                    {
                        this.databases[databaseId] = new SlockDatabase(this, databaseId, defaultTimeoutFlag, defaultExpriedFlag);
                    }
                }
            }
            return this.databases[databaseId];
        }

        public Lock NewLock(byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewLock(lockKey, timeout, expried);
        }

        public Lock NewLock(string lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewLock(lockKey, timeout, expried);
        }

        public Event NewEvent(byte[] eventKey, uint timeout, uint expried, bool defaultSeted)
        {
            return this.SelectDatabase(0).NewEvent(eventKey, timeout, expried, defaultSeted);
        }

        public Event NewEvent(string eventKey, uint timeout, uint expried, bool defaultSeted)
        {
            return this.SelectDatabase(0).NewEvent(eventKey, timeout, expried, defaultSeted);
        }

        public ReentrantLock NewReentrantLock(byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewReentrantLock(lockKey, timeout, expried);
        }

        public ReentrantLock NewReentrantLock(string lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewReentrantLock(lockKey, timeout, expried);
        }

        public ReadWriteLock NewReadWriteLock(byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewReadWriteLock(lockKey, timeout, expried);
        }

        public ReadWriteLock NewReadWriteLock(string lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewReadWriteLock(lockKey, timeout, expried);
        }

        public Semaphore NewSemaphore(byte[] semaphoreKey, ushort count, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewSemaphore(semaphoreKey, count, timeout, expried);
        }

        public Semaphore NewSemaphore(string semaphoreKey, ushort count, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewSemaphore(semaphoreKey, count, timeout, expried);
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(byte[] flowKey, ushort count, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewMaxConcurrentFlow(flowKey, count, timeout, expried);
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(string flowKey, ushort count, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewMaxConcurrentFlow(flowKey, count, timeout, expried);
        }

        public TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, ushort count, uint timeout, double period)
        {
            return this.SelectDatabase(0).NewTokenBucketFlow(flowKey, count, timeout, period);
        }

        public TokenBucketFlow NewTokenBucketFlow(string flowKey, ushort count, uint timeout, double period)
        {
            return this.SelectDatabase(0).NewTokenBucketFlow(flowKey, count, timeout, period);
        }

        public GroupEvent NewGroupEvent(byte[] groupKey, ulong clientId, ulong versionId, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewGroupEvent(groupKey, clientId, versionId, timeout, expried);
        }

        public GroupEvent NewGroupEvent(string groupKey, ulong clientId, ulong versionId, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewGroupEvent(groupKey, clientId, versionId, timeout, expried);
        }

        public TreeLock NewTreeLock(byte[] parentKey, byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewTreeLock(parentKey, lockKey, timeout, expried);
        }

        public TreeLock NewTreeLock(string parentKey, string lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewTreeLock(parentKey, lockKey, timeout, expried);
        }

        public TreeLock NewTreeLock(byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewTreeLock(lockKey, timeout, expried);
        }

        public TreeLock NewTreeLock(string lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewTreeLock(lockKey, timeout, expried);
        }
    }
}