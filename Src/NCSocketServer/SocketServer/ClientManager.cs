// Copyright (c) 2018 Alachisoft
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using Alachisoft.NCache.Common.DataStructures.Clustered;
using Alachisoft.NCache.SocketServer.Util;
using Alachisoft.NCache.Common.Net;
using Alachisoft.NCache.Common.Util;
using System.Collections.Generic;
using Alachisoft.NCache.Common.DataStructures;

namespace Alachisoft.NCache.SocketServer
{
    /// <summary>
    /// One instance of this class is created per client connection, and it remains 
    /// valid as long as the client is connected. 
    /// </summary>
    public sealed class ClientManager
    {
        /// <summary> True if socket has read the size of incoming command and data, false otherwise</summary>
        internal bool IsSizeRead = true;

        /// <summary> True if client connection is closed, false otherwise</summary>
        internal bool ConnectionLost = false;

        /// <summary> True if this client is a .Net client, false otherwise</summary>
        internal bool IsDotNetClient = false;

        /// <summary> Holds the data packet received from client</summary>
        internal byte[] Buffer = null;

        internal byte[] PinnedBuffer = null;

        internal byte[] tempDataBuffer = null;

        internal byte[] discardingBuffer = new byte[20];

        internal byte[] sendBuffer = null;

        /// <summary> Underlying client socket object</summary>
        private Socket _clientSocket = null;

        /// <summary> Unique clientId holder</summary>
        private string _clientSocketId = null;

        /// <summary>A unique id for the each client connected to the socket server.</summary>
        private string _clientID = "NULL";

        private event ClientDisposedCallback _clientDisposed;

        private ICommandExecuter _cmdExecuter;

        private bool _cacheStopped;

        private object _disposeSync = new object();

        private object _send_mutex = new object();
        private string _toString;

        private float _clientsRequest = 0;
        private float _clientsBytesSent = 0;
        private float _clientsBytesRecieved = 0;
        internal bool _leftGracefully;
        private DateTime _cmdStartTime;
        private TimeSpan _cmdExecurionTime;
        private bool _disposed;
        private DateTime _lastActivityTime;
        private int maxIdleTimeAllowed = 1; // time in minutes till which client can remain idle
        private string _uniqueCacheID = string.Empty;

        private int _clientVersion = 0;

        private string _clientIp;
        private bool _isAzureClient = false;
        
        private bool _operationInProcess;
        private DateTime _beginSendTime;
        private string _beginSendTimeString; // Only for dump analysis
        private DateTime _endSendTime;

        private int _eventPriorityRatio = 30;
        private static int _eventBulkCount = 50;
        private bool supportAcknowledgement = false;
        private bool _isOptimized = false;

        private Dictionary<string, EnumerationPointer> _enumerationPointers = new Dictionary<string, EnumerationPointer>();

        private EventsQueue _eventsQueue;
        private DateTime _lastEventCollectionTime;
        private string _slaveId;
        private bool _raiseClientDisconnectedEvent = true;
        private ConnectionManager _connectionManager;

        #region Aync response to client stuff

        private bool _isSending;
        private SocketAsyncEventArgs _sendAsyncArgs;
        private SocketAsyncEventArgs _receiveAsyncArgs;

        public SocketAsyncEventArgs SendEventArgs
        {
            get { return _sendAsyncArgs; }
            set { _sendAsyncArgs = value; }
        }

        public SocketAsyncEventArgs ReceiveEventArgs
        {
            get { return _receiveAsyncArgs; }
            set { _receiveAsyncArgs = value; }
        }

        public bool SendingResponse
        {
            get { return _isSending; }
            set { _isSending = value; }
        }

        #endregion

        public Dictionary<string, EnumerationPointer> EnumerationPointers
        {
            get { return _enumerationPointers; }
            set { _enumerationPointers = value; }
        }

        public string SlaveId
        {
            get { return _slaveId; }
            set { _slaveId = value; }
        }

        public ConnectionManager ConnectionManager
        {
            get { return _connectionManager; }
            set { _connectionManager = value; }
        }

        public string ClientIP
        {
            get { return _clientSocketId; }
        }

        public bool IsAzureClient
        {
            get { return _isAzureClient; }
            set { _isAzureClient = value; }
        }

        public bool IsMarkedAsClient { get; set; }

        ///<summary>This Property returns 'true' if the current instance of ClientManager has a secure-connection with its client.</summary>
        internal bool HasSecureConnection { get; private set; }
        
        private IPAddress _clientAddress = null;

        static ClientManager()
        {
            _eventBulkCount = ServiceConfiguration.EventBulkCount;
        }

        /// <summary>
        /// Construct client connection object and initialized the data buffer
        /// </summary>
        /// <param name="clientSocketId"> Underlying client socket connection object</param>
        /// <param name="size"> Size of the data packet to receive in buffer.</param>
        internal ClientManager(ConnectionManager conectionManager, Socket clientSocket, int size, int pinnedBufferSize)
        {
            _connectionManager = conectionManager;
            InitializeBuffer(size, pinnedBufferSize);
            _eventPriorityRatio = ServiceConfiguration.EventPriorityRatio;

            _asyncSendQueue = new Alachisoft.NCache.Common.DataStructures.WeightageBasedPriorityQueue(_eventPriorityRatio);

            _clientSocket = clientSocket;

            _clientSocketId = new Address(((IPEndPoint)clientSocket.RemoteEndPoint).Address, ((IPEndPoint)clientSocket.RemoteEndPoint).Port).ToString();

            _clientAddress = ((IPEndPoint)clientSocket.RemoteEndPoint).Address;
        }

        internal IPAddress ClientAddress
        {
            get { return _clientAddress; }
        }

        internal bool IsLocalClient
        {
            get
            {
                try
                {
                    string hostName = Dns.GetHostName();
                    IPHostEntry hostEntry = Dns.GetHostByName(hostName);
                    IPAddress[] addressList = hostEntry.AddressList;
                    if (addressList != null)
                    {
                        for (int i = 0; i < addressList.Length; i++)
                        {
                            IPAddress address = addressList[i];
                            if (address.Equals(_clientAddress)) return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
                return false;
            }
        }

        #region Pending response queue modifications.

        private int _topChunkResps = 0;
        private int _respsQueued = 0;
        private int _currChunckIndex = 0;

        private readonly WeightageBasedPriorityQueue _asyncSendQueue;
        private readonly ClusteredList<long> _commandsSizes = new ClusteredList<long>();
        private readonly object _queueLock = new object();

        /// <summary>
        /// A flag to indicate whether there are any pending commands to be sent to the server.
        /// </summary>
        internal bool AreResponsesPending
        {
            get
            {
                lock (_queueLock)
                {
                    return _asyncSendQueue.Count > 0;
                }
            }
        }

        /// <summary>
        /// Gets a composite binary response of the number of responses queued/getting queued. 
        /// </summary>
        internal ClusteredArrayList CompositeResponse
        {
            get
            {
                if (!_isOptimized){
                    ClusteredArrayList serCommand;
                    lock (_queueLock){
                      serCommand = (ClusteredArrayList)_asyncSendQueue.remove();
                      if (SocketServer.IsServerCounterEnabled)
                      {
                          ConnectionManager.PerfStatsColl.DecrementResponsesQueueCountStats();
                          ConnectionManager.PerfStatsColl.DecrementResponsesQueueSizeStats(serCommand.Count);
                      }
                      return serCommand;
                    }
                }

                using (ClusteredMemoryStream data = new ClusteredMemoryStream(256)){

                    byte[] chunkSize = new byte[10];
                    data.Write(chunkSize, 0, chunkSize.Length);
                    int respSize = 0;
                    int respsCopied = 0;

                    while (respsCopied < ConnectionManager.MaxRspThreshold && AreResponsesPending){
                        IList serCommand;
                        lock (_queueLock){
                            serCommand = (IList) _asyncSendQueue.remove();
                            if (SocketServer.IsServerCounterEnabled)
                            {
                                ConnectionManager.PerfStatsColl.DecrementResponsesQueueCountStats();
                                ConnectionManager.PerfStatsColl.DecrementResponsesQueueSizeStats(serCommand.Count);
                            }
                        }
                        foreach (byte[] chunk in serCommand){
                            respSize += chunk.Length;
                            data.Write(chunk, 0, chunk.Length);
                        }
                        respsCopied++;
                    }
                    chunkSize = HelperFxn.ToBytes(respSize.ToString());
                    data.Position = 0;
                    data.Write(chunkSize, 0, chunkSize.Length);
                    return data.GetInternalBuffer();
                }
            }
        }

        /// <summary>
        /// Add a response to the pending send operation queue.
        /// Previously the instance of queue was directly accessed by the container scopes.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="priority"></param>
        internal void AddPendingQueueOperation(IList buffer, Common.Enum.Priority priority)
        {
            lock (_queueLock)
            {
                _asyncSendQueue.add(buffer, priority);
                if (SocketServer.IsServerCounterEnabled)
                {
                    ConnectionManager.PerfStatsColl.IncrementResponsesQueueCountStats();
                    ConnectionManager.PerfStatsColl.IncrementResponsesQueueSizeStats(buffer.Count);
                }
            }
        }

        #endregion

        public int ClientVersion
        {
            get { return _clientVersion; }
            set
            {
                _clientVersion = value;

                if (_clientVersion >= 4610)
                {
                    _isOptimized = true;
                }
            }
        }

        public bool IsOptimized
        {
            get { return _isOptimized; }
        }

        public bool OperationInProgress
        {
            get { return _operationInProcess; }
        }

        public string ClientSocketId
        {
            get { return _clientSocketId; }
        }

        /// <summary>
        /// Gets/sets the ICommandExecuter.
        /// </summary>
        public ICommandExecuter CmdExecuter
        {
            get { return _cmdExecuter; }
            set { _cmdExecuter = value; }
        }

        public event ClientDisposedCallback ClientDisposed
        {
            add { _clientDisposed += value; }
            remove { _clientDisposed -= value; }
        }

        public bool SupportAcknowledgement
        {
            get { return supportAcknowledgement; }
            set { supportAcknowledgement = value; }
        }

        /// <summary>
        /// Initializes buffer to hold new data packet.
        /// </summary>
        /// <param name="size"> Size of the data packet to receive in buffer.</param>
        internal void InitializeBuffer(int size, int pinnedBufferSize)
        {
            PinnedBuffer = BufferPool.CheckoutBuffer(-1);
            sendBuffer = BufferPool.CheckoutBuffer(-1);
            Buffer = new byte[size];
        }

        internal bool MarkOperationInProcess()
        {
            lock (this)
            {
                if (!_operationInProcess)
                {
                    _operationInProcess = true;
                    _beginSendTime = DateTime.Now;
                    _beginSendTimeString = _beginSendTime.ToString();
                    return true;
                }
                return false;
            }
        }

        internal void DeMarkOperationInProcess()
        {
            lock (this)
            {
                if (_operationInProcess)
                {
                    _operationInProcess = false;
                }
            }
        }

        internal void ResetAsyncSendTime()
        {
            _beginSendTime = DateTime.Now;
            _beginSendTimeString = _beginSendTime.ToString();
        }

        // Manipulate send operation time taken, disconnect socket if interval exceeds configured time.
        internal double TimeTakenByOperation()
        {
            double timeTaken = 0;
            lock (this)
            {
                if (_operationInProcess)
                {
                    TimeSpan sendDifference = DateTime.Now - _beginSendTime;
                    timeTaken = sendDifference.TotalSeconds;
                }
            }
            return timeTaken;
        }

        public void EnqueueEvent(Object eventObj)
        {
            lock (_eventsQueue)
            {
                if (IsDisposed)
                    return;
                _eventsQueue.Enqueue(eventObj);
            }
        }

        internal Alachisoft.NCache.Common.Protobuf.Response GetEvents(out bool hasMessages)
        {
            object eventItem;
            try
            {
                bool removeMessageFromQueue = false;
                if (_eventsQueue.Count > ServiceConfiguration.EventBulkCount)
                {
                    removeMessageFromQueue = true;
                }
                else
                {
                    if ((DateTime.Now - _lastEventCollectionTime).TotalMilliseconds > ServiceConfiguration.BulkEventCollectionInterval)
                    {
                        removeMessageFromQueue = true;
                    }
                }

                if (removeMessageFromQueue)
                {
                    if (_eventsQueue.Count > 0)
                    {
                        Alachisoft.NCache.Common.Protobuf.Response response = null;
                        Alachisoft.NCache.Common.Protobuf.BulkEventResponse bulkEvent = new Common.Protobuf.BulkEventResponse();

                        for (int i = 0; i < ServiceConfiguration.EventBulkCount; i++)
                        {
                            eventItem = null;
                            lock (_eventsQueue)
                            {
                                if (_eventsQueue.Count > 0)
                                {
                                    eventItem = _eventsQueue.Dequeue();
                                    if (SocketServer.IsServerCounterEnabled) _connectionManager.PerfStatsColl.DecrementEventQueueCountStats();
                                }
                            }

                            if (eventItem == null)
                                break;

                            bulkEvent.eventList.Add((Alachisoft.NCache.Common.Protobuf.BulkEventItemResponse)eventItem);
                        }

                        if (bulkEvent.eventList.Count > 0)
                        {
                            _lastEventCollectionTime = DateTime.Now;
                            response = new Common.Protobuf.Response();
                            response.bulkEventResponse = bulkEvent;
                            response.responseType = Common.Protobuf.Response.Type.BULK_EVENT;
                        }

                        hasMessages = _eventsQueue.Count > ServiceConfiguration.EventBulkCount;
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                if (SocketServer.IsServerCounterEnabled) SocketServer.Logger.NCacheLog.Error("ClientManager.GetEvents", ex.ToString());
            }
            hasMessages = _eventsQueue.Count > ServiceConfiguration.EventBulkCount;
            return null;
        }

        /// <summary>
        /// Increment the client requests by specified value
        /// </summary>
        /// <param name="value"></param>
        internal void AddToClientsRequest(long value)
        {
            Interlocked.Exchange(ref this._clientsRequest, value);
        }

        /// <summary>
        /// Increment the bytes sent to clients by specified value
        /// </summary>
        /// <param name="value"></param>
        internal void AddToClientsBytesSent(long value)
        {
            Interlocked.Exchange(ref this._clientsBytesSent, value);
        }

        /// <summary>
        /// Increment the bytes received from clients by specified value
        /// </summary>
        /// <param name="value"></param>
        internal void AddToClientsBytesRecieved(long value)
        {
            Interlocked.Exchange(ref this._clientsBytesRecieved, value);
        }

        /// <summary>
        /// Get number of clients requests
        /// </summary>
        internal float ClientsRequests
        {
            get { return this._clientsRequest; }
        }

        /// <summary>
        /// Get number of bytes sent to clients
        /// </summary>
        internal float ClientsBytesSent
        {
            get { return this._clientsBytesSent; }
        }

        /// <summary>
        /// Get number of bytes sent to clients
        /// </summary>
        internal float ClientsBytesRecieved
        {
            get { return this._clientsBytesRecieved; }
        }

        public Object SendMutex
        {
            get { return _send_mutex; }
        }

        /// <summary>
        /// Unique source cache id which the 
        /// </summary>
        public string UniqueCacheID
        {
            get { return _uniqueCacheID; }
            set { _uniqueCacheID = value; }
        }

        /// <summary>
        /// Gets the value indicating whether client has disconnected from server or not.
        /// </summary>
        public bool IsDisposed
        {
            get { return _disposed; }
            set { _disposed = value; }
        }

        /// <summary>
        /// Gets whether cache is running or stopped
        /// </summary>
        public bool IsCacheStopped
        {
            get { return _cacheStopped; }
        }

        /// <summary>
        /// Get underlying client socket connection object
        /// </summary>        
        internal Socket ClientSocket
        {
            get { return _clientSocket; }
        }

        /// <summary>
        /// Gets/sets the id of the client. This id must be unique for the
        /// client. No two clients can have the same id.
        /// </summary>
        public string ClientID
        {
            get { return _clientID; }
            set { _clientID = value; }
        }

        public bool RaiseClientDisconnectEvent
        {
            get { return _raiseClientDisconnectedEvent; }
            set { _raiseClientDisconnectedEvent = value; }
        }
        public void ReinitializeBuffer()
        {
            if (Buffer != null)
            {
                for (int i = 0; i < Buffer.Length; i++)
                {
                    Buffer[i] = 0;
                }
            }
        }

        /// <summary>
        /// this function is called when cache stops 
        /// </summary>
        /// <param name="cacheId"></param>
        public void OnCacheStopped(string cacheId)
        {
            _cacheStopped = true;
            Dispose(false);
        }

        /// <summary>
        /// Construct the value package that can be send to the client
        /// </summary>
        /// <param name="result">result string</param>
        /// <param name="resultData">result value</param>
        /// <returns>constructed packet</returns>
        internal IList<byte[]> ReplyPacket(string result, byte[] resultData)
        {
            IList<byte[]> bufferList = new List<byte[]>();

            byte[] command = HelperFxn.ToBytes(result);
            byte[] buffer = new byte[ConnectionManager.cmdSizeHolderBytesCount + ConnectionManager.valSizeHolderBytesCount + command.Length + resultData.Length];

            byte[] commandSize = HelperFxn.ToBytes(command.Length.ToString());
            byte[] dataSize = HelperFxn.ToBytes(resultData.Length.ToString());

            commandSize.CopyTo(buffer, 0);
            dataSize.CopyTo(buffer, ConnectionManager.cmdSizeHolderBytesCount);
            command.CopyTo(buffer, ConnectionManager.totSizeHolderBytesCount);
            resultData.CopyTo(buffer, ConnectionManager.totSizeHolderBytesCount + command.Length);
            
            bufferList.Add(buffer);
            
            return bufferList;
        }

        /// <summary>
        /// Construct the value package that can be send to the client
        /// </summary>
        /// <param name="result">result string</param>
        /// <param name="dataLength">datalength int</param>
        /// <returns>constructed packet</returns>
        internal byte[] ReplyPacket(string result, int dataLength)
        {
            byte[] command = HelperFxn.ToBytes(result);
            byte[] buffer = new byte[ConnectionManager.cmdSizeHolderBytesCount + ConnectionManager.valSizeHolderBytesCount + command.Length];

            byte[] commandSize = HelperFxn.ToBytes(command.Length.ToString());
            byte[] dataSize = HelperFxn.ToBytes(dataLength.ToString());

            commandSize.CopyTo(buffer, 0);
            dataSize.CopyTo(buffer, ConnectionManager.cmdSizeHolderBytesCount);
            command.CopyTo(buffer, ConnectionManager.totSizeHolderBytesCount);

            return buffer;
        }

        /// <summary>
        /// Dispose client manager and connection objects
        /// </summary>
        internal void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose client manager and connection objects
        /// </summary>
        internal void Dispose(bool disposingIntentionally)
        {
            if (_disposed) return;
            lock (_disposeSync)
            {
                _disposed = true;

                if (_sendAsyncArgs != null)
                {
                    _sendAsyncArgs.Dispose();
                    _sendAsyncArgs = null;
                }

                if ( _receiveAsyncArgs != null)
                {
                    _receiveAsyncArgs.Dispose();
                    _receiveAsyncArgs = null;
                }

                if (_enumerationPointers != null && !_cacheStopped)
                {
                    foreach (string key in _enumerationPointers.Keys)
                    {
                        _cmdExecuter.DisposeEnumerator(_enumerationPointers[key]);
                    }
                    _enumerationPointers = null;
                }

                if (_clientSocket != null )
                {
#if !NET20
                    if (_clientSocket.Connected)
                    {
                        try
                        {
                            _clientSocket.Dispose();
                        }
                        catch (SocketException e) { }
                        catch (ObjectDisposedException e) { }
                    }
#endif
                    try
                    {
                        if (_clientSocket != null) _clientSocket.Close();
                    }
                    catch (Exception) { }
                    GC.SuppressFinalize(_clientSocket);
                    _clientSocket = null;
                }
                Buffer = null;
                BufferPool.CheckinBuffer(PinnedBuffer);
                BufferPool.CheckinBuffer(sendBuffer);

                PinnedBuffer = null;
                sendBuffer = null;

                ConnectionManager.EventsAndCallbackQueue.UnRegisterSlaveQueue(_slaveId);

                if (_cmdExecuter != null)
                {
                    if (!_cacheStopped && _raiseClientDisconnectedEvent)
                    {
                        try
                        {
                            _cmdExecuter.OnClientDisconnected(ClientID, UniqueCacheID);
                        }
                        catch (Exception e)
                        {
                            if (SocketServer.Logger.IsErrorLogsEnabled) SocketServer.Logger.NCacheLog.Error("ClientManager.Dispose", e.ToString());
                        }
                    }
                    if (_cmdExecuter != null)
                    {
                        _cmdExecuter.Dispose();
                        _cmdExecuter = null;
                    }
                }
                if (!disposingIntentionally) _clientDisposed(this.ClientID);
            }
        }

        public override string ToString()
        {
            return "[" + _clientSocketId + " ->" + _clientID + "]";
        }

        internal void StartCommandExecution()
        {
            _cmdStartTime = DateTime.Now;
            _lastActivityTime = DateTime.Now;
        }

        public void MarkActivity()
        {
            lock (this) { _lastActivityTime = DateTime.Now; }
        }

        public bool IsIdle
        {
            get
            {
                DateTime currentTime = DateTime.Now;
                TimeSpan idleTime = currentTime.Subtract(_lastActivityTime);
                if (idleTime.TotalMinutes > maxIdleTimeAllowed)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal void StopCommandExecution()
        {
            DateTime now = DateTime.Now;
            _cmdExecurionTime = now - _cmdStartTime;
        }

        /// <summary>
        /// Returns the pinned buffer. if the size of the command is grater then new buffer is created and returned.
        /// </summary>
        /// <param name="size"> Size required</param>
        /// <returns> The PinnedBuffer</returns>
        internal byte[] GetPinnedBuffer(int size)
        {
            if (this.PinnedBuffer.Length < size)
            {
                BufferPool.CheckinBuffer(this.PinnedBuffer);
                this.PinnedBuffer = new byte[size];
            }
            return this.PinnedBuffer;
        }

        /// <summary>
        /// Returns the pinned buffer. if the size of the command is grater then new buffer is created and returned.
        /// </summary>
        /// <param name="size"> Size required</param>
        /// <returns> The PinnedBuffer</returns>
        internal byte[] GetTempPinnedBuffer(int size)
        {
            if (this.tempDataBuffer == null || this.tempDataBuffer.Length < size)
            {
                this.tempDataBuffer = new byte[size];
            }
            return this.tempDataBuffer;
        }

        public EventsQueue EventQueue { get { return _eventsQueue; } set { _eventsQueue = value; } }

        public static int EventBulkCount { get { return ServiceConfiguration.EventBulkCount; } }
    }
}
