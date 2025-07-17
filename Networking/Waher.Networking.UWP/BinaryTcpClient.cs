﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage.Streams;
#else
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
#endif
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Security;
using Waher.Runtime.Collections;

namespace Waher.Networking
{
	/// <summary>
	/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also makes the use of <see cref="TcpClient"/>
	/// safe, making sure it can be disposed, even during an active connection attempt. Outgoing data is queued and transmitted in the
	/// permitted pace.
	/// </summary>
	public class BinaryTcpClient : CommunicationLayer, IDisposableAsync, IBinaryTransportLayer
	{
		private const int BufferSize = 65536;

		private readonly ChunkedList<Rec> outputQueue = new ChunkedList<Rec>();
		private ChunkedList<TaskCompletionSource<bool>> idleQueue = null;
		private ChunkedList<TaskCompletionSource<bool>> cancelledQueue = null;
		private ChunkedList<TaskCompletionSource<bool>> waitingQueue = null;
#if WINDOWS_UWP
		private readonly MemoryBuffer memoryBuffer = new MemoryBuffer(BufferSize);
		private readonly StreamSocket client;
		private readonly IBuffer buffer = null;
		private DataWriter dataWriter = null;
#else
		private readonly byte[] buffer = new byte[BufferSize];
		private readonly TcpClient tcpClient;
		private readonly Guid id = Guid.NewGuid();
		private Stream stream = null;
		private CancellationTokenSource currentCancelReading;
#endif
		private readonly object synchObj = new object();
		private readonly bool sniffBinary;
		private string hostName;
		private string domainName;
		private string remoteEndPoint = string.Empty;
		private string localEndPoint = string.Empty;
		private int outputQueueSize = 0;
		private int maxOutputQueueSize = 2 * 65536; // 128 kB by default.
		private bool connecting = false;
		private bool connected = false;
		private bool disposing = false;
		private bool disposed = false;
		private bool sending = false;
		private bool reading = false;
		private bool upgrading = false;
		private bool trustRemoteEndPoint = false;
		private bool remoteCertificateValid = false;
		private bool disposeWhenDone = false;
		private bool cancelRead = false;

		private class Rec
		{
			public byte[] Data;
			public int Offset;
			public int Count;
			public EventHandlerAsync<DeliveryEventArgs> Callback;
			public object State;
			public TaskCompletionSource<bool> Task;
		}

		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		/// <param name="DecoupledEvents">If events raised from the communication 
		/// layer are decoupled, i.e. executed in parallel with the source that raised 
		/// them.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public BinaryTcpClient(bool DecoupledEvents, params ISniffer[] Sniffers)
			: this(true, DecoupledEvents, Sniffers)
		{
		}

		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		/// <param name="SniffBinary">If binary communication is to be forwarded to registered sniffers.</param>
		/// <param name="DecoupledEvents">If events raised from the communication 
		/// layer are decoupled, i.e. executed in parallel with the source that raised 
		/// them.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public BinaryTcpClient(bool SniffBinary, bool DecoupledEvents, params ISniffer[] Sniffers)
			: base(DecoupledEvents, Sniffers)
		{
			this.sniffBinary = SniffBinary;
#if WINDOWS_UWP
			this.buffer = Windows.Storage.Streams.Buffer.CreateCopyFromMemoryBuffer(this.memoryBuffer);
			this.client = new StreamSocket();
#else
			this.tcpClient = new TcpClient();
#endif
		}

#if WINDOWS_UWP
		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="DecoupledEvents">If events raised from the communication 
		/// layer are decoupled, i.e. executed in parallel with the source that raised 
		/// them.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public BinaryTcpClient(StreamSocket Client, bool DecoupledEvents, params ISniffer[] Sniffers)
			: this(Client, true, DecoupledEvents, Sniffers)
		{
		}

		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="SniffBinary">If binary communication is to be forwarded to registered sniffers.</param>
		/// <param name="DecoupledEvents">If events raised from the communication 
		/// layer are decoupled, i.e. executed in parallel with the source that raised 
		/// them.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public BinaryTcpClient(StreamSocket Client, bool SniffBinary, bool DecoupledEvents, params ISniffer[] Sniffers)
			: base(DecoupledEvents, Sniffers)
		{
			this.sniffBinary = SniffBinary;
			this.buffer = Windows.Storage.Streams.Buffer.CreateCopyFromMemoryBuffer(this.memoryBuffer);
			this.client = Client;

			this.SetTcpEndPoints();
		}

		private void SetTcpEndPoints()
		{
			if (this.remoteCertificate is null || !this.remoteCertificateValid)
				this.remoteEndPoint = this.client.Information.RemoteAddress.ToString() + ":" + this.client.Information.RemotePort;
			else
			{
				this.remoteEndPoint = GetDomainFromSubject(this.remoteCertificate.Subject);
				if (string.IsNullOrEmpty(this.remoteEndPoint))
					this.remoteEndPoint = this.client.Information.RemoteAddress.ToString() + ":" + this.client.Information.RemotePort;
			}

			this.localEndPoint = this.client.Information.LocalAddress.ToString() + ":" + this.client.Information.LocalPort;
		}
#else
		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="DecoupledEvents">If events raised from the communication 
		/// layer are decoupled, i.e. executed in parallel with the source that raised 
		/// them.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public BinaryTcpClient(TcpClient Client, bool DecoupledEvents, params ISniffer[] Sniffers)
			: this(Client, true, DecoupledEvents, Sniffers)
		{
		}

		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="SniffBinary">If binary communication is to be forwarded to registered sniffers.</param>
		/// <param name="DecoupledEvents">If events raised from the communication 
		/// layer are decoupled, i.e. executed in parallel with the source that raised 
		/// them.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public BinaryTcpClient(TcpClient Client, bool SniffBinary, bool DecoupledEvents, params ISniffer[] Sniffers)
			: base(DecoupledEvents, Sniffers)
		{
			this.sniffBinary = SniffBinary;
			this.tcpClient = Client;

			this.SetTcpEndPoints();
		}

		private void SetTcpEndPoints()
		{
			Socket UnderlyingSocket = this.tcpClient.Client;
			if (UnderlyingSocket is null)
				return;

			if (this.remoteCertificate is null || !this.remoteCertificateValid)
				this.remoteEndPoint = UnderlyingSocket.RemoteEndPoint.ToString();
			else
			{
				this.remoteEndPoint = GetDomainFromSubject(this.remoteCertificate.Subject);
				if (string.IsNullOrEmpty(this.remoteEndPoint))
					this.remoteEndPoint = UnderlyingSocket.RemoteEndPoint.ToString();
			}

			this.localEndPoint = UnderlyingSocket.LocalEndPoint.ToString();
		}
#endif
		/// <summary>
		/// Extracts the domain name from a certificate subject string.
		/// </summary>
		/// <param name="Subject">Certificate subject string.</param>
		/// <returns>Domain name, if available.</returns>
		public static string GetDomainFromSubject(string Subject)
		{
			if (string.IsNullOrEmpty(Subject))
				return string.Empty;

			int i = Subject.IndexOf("CN=", StringComparison.InvariantCultureIgnoreCase);
			if (i < 0)
				return string.Empty;

			i += 3;
			int j = Subject.IndexOf(',', i);
			if (j < 0)
				return Subject.Substring(i).Trim();
			else
				return Subject.Substring(i, j - i).Trim();
		}

#if WINDOWS_UWP
		/// <summary>
		/// Underlying <see cref="StreamSocket"/> object.
		/// </summary>
		public StreamSocket Client => this.client;
#else
		/// <summary>
		/// Underlying <see cref="TcpClient"/> object.
		/// </summary>
		public TcpClient Client => this.tcpClient;

		/// <summary>
		/// Stream object currently being used.
		/// </summary>
		public Stream Stream => this.stream;
#endif

		/// <summary>
		/// Remote End-point of connection. This corresponds to the IP Endpoint of the 
		/// remote party in normal cases. It can also be the domain name of the remote
		/// party, if mTLS is used.
		/// </summary>
		public string RemoteEndPoint => this.remoteEndPoint;

		/// <summary>
		/// Local End-point of connection. This corresponds to the IP Endpoint of the 
		/// connection.
		/// </summary>
		public string LocalEndPoint => this.localEndPoint;

		/// <summary>
		/// Current output queue size, in bytes.
		/// </summary>
		public int OutputQueueSize
		{
			get
			{
				lock (this.synchObj)
				{
					return this.outputQueueSize;
				}
			}
		}

		/// <summary>
		/// Maximum output queue size, in bytes.
		/// </summary>
		public int MaxOutputQueueSize
		{
			get
			{
				lock (this.synchObj)
				{
					return this.maxOutputQueueSize;
				}
			}

			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException("Max output queue size must be positive.", nameof(value));

				lock (this.synchObj)
				{
					this.maxOutputQueueSize = value;
				}
			}
		}

		/// <summary>
		/// Connects to a host using TCP.
		/// </summary>
		/// <param name="Host">Host Name or IP Address in string format.</param>
		/// <param name="Port">Port number.</param>
		/// <returns>If connection was established. If false is returned, the object has been disposed during the connection attempt.</returns>
		public Task<bool> ConnectAsync(string Host, int Port)
		{
			return this.ConnectAsync(Host, Port, false);
		}

		/// <summary>
		/// Connects to a host using TCP.
		/// </summary>
		/// <param name="Host">Host Name or IP Address in string format.</param>
		/// <param name="Port">Port number.</param>
		/// <param name="Paused">If connection starts in a paused state (i.e. not waiting for incoming communication).</param>
		/// <returns>If connection was established. If false is returned, the object has been disposed during the connection attempt.</returns>
		public async Task<bool> ConnectAsync(string Host, int Port, bool Paused)
		{
			this.hostName = this.domainName = Host;
			this.PreConnect();
#if WINDOWS_UWP
			await this.client.ConnectAsync(new HostName(Host), Port.ToString(), SocketProtectionLevel.PlainSocket);
			return this.PostConnect(Paused);
#else
			await this.tcpClient.ConnectAsync(Host, Port);
			return this.PostConnect(Paused);
#endif
		}

		/// <summary>
		/// Connects to a host using TCP.
		/// </summary>
		/// <param name="Address">IP Address of the host.</param>
		/// <param name="Port">Port number.</param>
		/// <returns>If connection was established. If false is returned, the object has been disposed during the connection attempt.</returns>
		public Task<bool> ConnectAsync(IPAddress Address, int Port)
		{
			return this.ConnectAsync(Address, Port, false);
		}

		/// <summary>
		/// Connects to a host using TCP.
		/// </summary>
		/// <param name="Address">IP Address of the host.</param>
		/// <param name="Port">Port number.</param>
		/// <param name="Paused">If connection starts in a paused state (i.e. not waiting for incoming communication).</param>
		/// <returns>If connection was established. If false is returned, the object has been disposed during the connection attempt.</returns>
		public async Task<bool> ConnectAsync(IPAddress Address, int Port, bool Paused)
		{
			this.hostName = this.domainName = Address.ToString();
			this.PreConnect();
#if WINDOWS_UWP
			await this.client.ConnectAsync(new HostName(Address.ToString()), Port.ToString(), SocketProtectionLevel.PlainSocket);
			return this.PostConnect(Paused);
#else
			await this.tcpClient.ConnectAsync(Address, Port);
			return this.PostConnect(Paused);
#endif
		}

#if !WINDOWS_UWP
		/// <summary>
		/// Connects to a host using TCP.
		/// </summary>
		/// <param name="Addresses">IP Addresses of the host.</param>
		/// <param name="Port">Port number.</param>
		/// <returns>If connection was established. If false is returned, the object has been disposed during the connection attempt.</returns>
		public Task<bool> ConnectAsync(IPAddress[] Addresses, int Port)
		{
			return this.ConnectAsync(Addresses, Port, false);
		}

		/// <summary>
		/// Connects to a host using TCP.
		/// </summary>
		/// <param name="Addresses">IP Addresses of the host.</param>
		/// <param name="Port">Port number.</param>
		/// <param name="Paused">If connection starts in a paused state (i.e. not waiting for incoming communication).</param>
		/// <returns>If connection was established. If false is returned, the object has been disposed during the connection attempt.</returns>
		public async Task<bool> ConnectAsync(IPAddress[] Addresses, int Port, bool Paused)
		{
			this.hostName = this.domainName = null;
			this.PreConnect();
			await this.tcpClient.ConnectAsync(Addresses, Port);
			return this.PostConnect(Paused);
		}
#endif

		/// <summary>
		/// Binds to a <see cref="TcpClient"/> that was already connected when provided to the constructor.
		/// </summary>
		public void Bind()
		{
			this.Bind(false);
		}

		/// <summary>
		/// Binds to a <see cref="TcpClient"/> that was already connected when provided to the constructor.
		/// </summary>
		/// <param name="Paused">If connection starts in a paused state (i.e. not waiting for incoming communication).</param>
		public void Bind(bool Paused)
		{
			this.PreConnect();
			this.PostConnect(Paused);
		}

		/// <summary>
		/// If the connection is open.
		/// </summary>
		public bool Connected => this.connected && !this.disposing && !this.disposed;

#if WINDOWS_UWP
		/// <summary>
		/// Current Cancellation token for the current read operation.
		/// </summary>
		public CancellationToken CurrentCancellationToken => CancellationToken.None;
#else
		/// <summary>
		/// Current Cancellation token for the current read operation.
		/// </summary>
		public CancellationToken CurrentCancellationToken => this.currentCancelReading?.Token ?? CancellationToken.None;
#endif

		private void PreConnect()
		{
			lock (this.synchObj)
			{
				if (this.connecting)
					throw new InvalidOperationException("Connection attempt already underway.");

				if (this.connected)
					throw new InvalidOperationException("Already connected.");

				if (this.disposing || this.disposed)
					throw new ObjectDisposedException("Object has been disposed.");

				this.connecting = true;
			}

			this.remoteCertificate = null;
			this.remoteCertificateValid = false;
		}

		private bool PostConnect(bool Paused)
		{
			bool Disposing = false;

			lock (this.synchObj)
			{
				this.connecting = false;

				if (this.disposed)
					return false;

				if (this.disposing)
					Disposing = true;
				else
					this.connected = true;
			}

			if (Disposing)
			{
				Task.Run(async () =>
				{
					try
					{
						await this.DoDisposeAsyncLocked();
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				});

				return false;
			}

#if WINDOWS_UWP
			this.dataWriter = new DataWriter(this.client.OutputStream);
#else
			this.stream = this.tcpClient.GetStream();
#endif
			if (!Paused)
				this.BeginRead();

			return true;
		}

		/// <summary>
		/// Disposes the client when done sending all data.
		/// </summary>
		public void DisposeWhenDone()
		{
			lock (this.synchObj)
			{
				if (this.connected && this.sending)
				{
					this.disposeWhenDone = true;
					return;
				}
			}

			Task.Run(() =>
			{
				try
				{
					this.DisposeAsync();
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			});
		}

		/// <summary>
		/// Disposes of the object. The underlying <see cref="TcpClient"/> is either disposed directly, or when asynchronous
		/// operations have ceased.
		/// </summary>
		[Obsolete("Use DisposeAsync() instead.")]
		public void Dispose()
		{
			Task.Run(() => this.DisposeAsync());
		}

		/// <summary>
		/// Disposes of the object asynchronously. The underlying <see cref="TcpClient"/> is 
		/// either disposed directly, or when asynchronous operations have ceased.
		/// </summary>
		public virtual Task DisposeAsync()
		{
#if WINDOWS_UWP
			bool Cancel;
			bool DoDispose = false;

			lock (this.synchObj)
			{
				if (this.disposed || this.disposing)
					return Task.CompletedTask;

				if (Cancel = this.connecting || this.reading || this.sending)
					this.disposing = true;
				else
					DoDispose = true;
			}

			if (Cancel)
			{
				IAsyncAction _ = this.client.CancelIOAsync();
			}

			if (DoDispose)
				return this.DoDisposeAsyncLocked();
			else
				return Task.CompletedTask;
		}
#else
			bool DelayedDispose;

			lock (this.synchObj)
			{
				if (this.disposed || this.disposing)
					return Task.CompletedTask;

				if (this.connecting || this.reading || this.sending)
				{
					this.disposing = true;
#if !WINDOWS_UWP
					this.currentCancelReading?.Cancel();
#endif
					DelayedDispose = true;
				}
				else
					DelayedDispose = false;
			}

#if !WINDOWS_UWP
			NetworkingModule.UnregisterToken(this.id);
#endif
			if (DelayedDispose)
			{
				Task.Delay(1000).ContinueWith(this.AbortRead);  // Double-check socket gets cancelled. If not, forcefully close.
				return Task.CompletedTask;
			}
			else
				return this.DoDisposeAsyncLocked();
		}

		private Task AbortRead(object P)
		{
			if (this.disposed)
				return Task.CompletedTask;
			else
				return this.DoDisposeAsyncLocked();
		}
#endif

		private async Task DoDisposeAsyncLocked()
		{
			this.disposed = true;
			this.connecting = false;
			this.connected = false;

			await this.CancelOutputQueue();

			this.outputQueue.Clear();
			this.outputQueueSize = 0;

			lock (this.synchObj)
			{
				this.EmptyIdleQueueLocked();
				this.EmptyCancelQueueLocked();
			}

#if WINDOWS_UWP
			this.client.Dispose();
			this.memoryBuffer.Dispose();
			this.dataWriter.Dispose();
#else
			this.stream = null;
			this.tcpClient.Dispose();

			this.remoteCertificate?.Dispose();
			this.remoteCertificate = null;
#endif

			if (this.HasSniffers)
				await this.RemoveRange(this.Sniffers, true);
		}

		/// <summary>
		/// Continues reading from the socket, if paused in an event handler.
		/// </summary>
		public void Continue()
		{
			this.BeginRead();
		}

		/// <summary>
		/// If the reading is paused.
		/// </summary>
		public bool Paused
		{
			get
			{
				lock (this.synchObj)
				{
					return this.connected && !this.reading;
				}
			}
		}

		private async void BeginRead()  // Starts parallel task
		{
			lock (this.synchObj)
			{
				if (this.disposing || this.disposed || this.reading)
					return;

				this.reading = true;
			}

#if WINDOWS_UWP
			IInputStream InputStream;
#else
			Stream Stream;
#endif
			int NrRead;
			bool Continue = true;

			try
			{
				while (Continue)
				{
					lock (this.synchObj)
					{
						if (this.disposing || this.disposed || this.cancelRead || NetworkingModule.Stopping)
							break;

#if WINDOWS_UWP
						InputStream = this.client.InputStream;
						if (InputStream is null)
							break;
#else
						Stream = this.stream;
						if (Stream is null)
							break;
#endif
					}

#if WINDOWS_UWP
					IBuffer DataRead = await InputStream.ReadAsync(this.buffer, BufferSize, InputStreamOptions.Partial);
					if (DataRead.Length == 0)
						break;

					CryptographicBuffer.CopyToByteArray(DataRead, out byte[] Packet);
					if (Packet is null)
						break;

					NrRead = Packet.Length;
#else
					CancellationTokenSource PrevCancel = this.currentCancelReading;
					this.currentCancelReading = new CancellationTokenSource();
					NetworkingModule.RegisterToken(this.id, this.currentCancelReading);
					PrevCancel?.Dispose();

					NrRead = await Stream.ReadAsync(this.buffer, 0, BufferSize, this.currentCancelReading.Token);
#endif
					if (this.disposing || this.disposed || NetworkingModule.Stopping)
						break;

					if (NrRead <= 0)
					{
						lock (this.synchObj)
						{
							if (this.cancelRead)
								break;
						}

						await this.Disconnected();
						break;
					}

					try
					{
#if WINDOWS_UWP
						Continue = await this.BinaryDataReceived(false, Packet, 0, NrRead);
#else
						Continue = await this.BinaryDataReceived(false, this.buffer, 0, NrRead);
#endif
					}
					catch (Exception ex)
					{
						this.Exception(ex);
					}
				}
			}
			catch (Exception ex)
			{
				this.Exception(ex);
			}
			finally
			{
				bool Disposing = false;

#if !WINDOWS_UWP
				CancellationTokenSource Cancel = this.currentCancelReading;
				this.currentCancelReading = null;
				Cancel?.Dispose();

				NetworkingModule.UnregisterToken(this.id);
#endif

				lock (this.synchObj)
				{
					this.reading = false;

					if (this.cancelRead)
					{
						this.cancelRead = false;
						this.EmptyCancelQueueLocked();
					}

					if (this.disposing && !this.sending)
						Disposing = true;
				}

				if (Disposing)
					await this.DoDisposeAsyncLocked();
			}

			if (!Continue && !this.disposed && !this.disposing)
				await this.OnPaused.Raise(this, EventArgs.Empty, false);
		}

		/// <summary>
		/// Event raised when reading on the socked has been paused. Call <see cref="Continue"/> to resume reading.
		/// </summary>
		public event EventHandlerAsync OnPaused;

		/// <summary>
		/// Clears the <see cref="OnPaused"/> event handler.
		/// </summary>
		public void OnPausedClear() => this.OnPaused = null;

		/// <summary>
		/// Resets the <see cref="OnPaused"/> event handler.
		/// </summary>
		/// <param name="EventHandler">Event handler to set.</param>
		public void OnPausedReset(EventHandlerAsync EventHandler) => this.OnPaused = EventHandler;

		/// <summary>
		/// Method called when the connection has been disconnected.
		/// </summary>
		protected virtual Task Disconnected()
		{
			if (!(this.OnDisconnected is null))
				return this.OnDisconnected.Raise(this, EventArgs.Empty);
			else
				return Task.CompletedTask;
		}

		/// <summary>
		/// Method called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte read.</param>
		/// <param name="Count">Number of bytes read.</param>
		/// <returns>If the process should be continued.</returns>
		protected virtual async Task<bool> BinaryDataReceived(bool ConstantBuffer,
			byte[] Buffer, int Offset, int Count)
		{
			if (this.sniffBinary && this.HasSniffers)
				this.ReceiveBinary(ConstantBuffer, Buffer, Offset, Count);

			BinaryDataReadEventHandler h = this.OnReceived;
			if (h is null)
				return true;
			else
				return await h(this, ConstantBuffer, Buffer, Offset, Count);
		}

		/// <summary>
		/// Method called when an exception has been caught.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ex">Exception</param>
		public override void Exception(DateTime Timestamp, Exception ex)
		{
			try
			{
				base.Exception(Timestamp, ex);
			}
			catch (Exception ex2)
			{
				Log.Exception(ex2);
			}

			EventHandlerAsync<Exception> h = this.OnError;
			if (!(h is null))
			{
				Task.Run(async () =>
				{
					try
					{
						await h(this, ex);
					}
					catch (Exception ex2)
					{
						Log.Exception(ex2);
					}
				});
			}
		}

		/// <summary>
		/// Event received when binary data has been received.
		/// </summary>
		public event BinaryDataReadEventHandler OnReceived;

		/// <summary>
		/// Clears the <see cref="OnReceived"/> event handler.
		/// </summary>
		public void OnReceivedClear() => this.OnReceived = null;

		/// <summary>
		/// Resets the <see cref="OnReceived"/> event handler.
		/// </summary>
		/// <param name="EventHandler">Event handler to set.</param>
		public void OnReceivedReset(BinaryDataReadEventHandler EventHandler) => this.OnReceived = EventHandler;

		/// <summary>
		/// Event raised when an error has occurred.
		/// </summary>
		public event EventHandlerAsync<Exception> OnError;

		/// <summary>
		/// Clears the <see cref="OnError"/> event handler.
		/// </summary>
		public void OnErrorClear() => this.OnError = null;

		/// <summary>
		/// Resets the <see cref="OnError"/> event handler.
		/// </summary>
		/// <param name="EventHandler">Event handler to set.</param>
		public void OnErrorReset(EventHandlerAsync<Exception> EventHandler) => this.OnError = EventHandler;

		/// <summary>
		/// Event raised when the connection has been disconnected.
		/// </summary>
		public event EventHandlerAsync OnDisconnected;

		/// <summary>
		/// Clears the <see cref="OnDisconnected"/> event handler.
		/// </summary>
		public void OnDisconnectedClear() => this.OnDisconnected = null;

		/// <summary>
		/// Resets the <see cref="OnDisconnected"/> event handler.
		/// </summary>
		/// <param name="EventHandler">Event handler to set.</param>
		public void OnDisconnectedReset(EventHandlerAsync EventHandler) => this.OnDisconnected = EventHandler;

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Packet">Binary packet.</param>
		/// <returns>If data was sent.</returns>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task<bool> SendAsync(byte[] Packet)
		{
			return this.SendAsync(false, Packet, 0, Packet.Length, false, null, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Binary packet.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Packet)
		{
			return this.SendAsync(ConstantBuffer, Packet, 0, Packet.Length, false, null, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Packet">Binary packet.</param>
		/// <param name="Priority">If packet should be sent before any waiting in the queue.</param>
		/// <returns>If data was sent.</returns>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task<bool> SendAsync(byte[] Packet, bool Priority)
		{
			return this.SendAsync(false, Packet, 0, Packet.Length, Priority, null, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Binary packet.</param>
		/// <param name="Priority">If packet should be sent before any waiting in the queue.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Packet, bool Priority)
		{
			return this.SendAsync(ConstantBuffer, Packet, 0, Packet.Length, Priority, null, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Packet">Binary packet.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task<bool> SendAsync(byte[] Packet, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			return this.SendAsync(false, Packet, 0, Packet.Length, false, Callback, State);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Binary packet.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Packet, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			return this.SendAsync(ConstantBuffer, Packet, 0, Packet.Length, false, Callback, State);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Packet">Binary packet.</param>
		/// <param name="Priority">If packet should be sent before any waiting in the queue.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task<bool> SendAsync(byte[] Packet, bool Priority, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			return this.SendAsync(false, Packet, 0, Packet.Length, Priority, Callback, State);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Binary packet.</param>
		/// <param name="Priority">If packet should be sent before any waiting in the queue.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Packet, bool Priority, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			return this.SendAsync(ConstantBuffer, Packet, 0, Packet.Length, Priority, Callback, State);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <returns>If data was sent.</returns>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task<bool> SendAsync(byte[] Buffer, int Offset, int Count)
		{
			return this.SendAsync(false, Buffer, Offset, Count, false, null, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			return this.SendAsync(ConstantBuffer, Buffer, Offset, Count, false, null, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Priority">If packet should be sent before any waiting in the queue.</param>
		/// <returns>If data was sent.</returns>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task<bool> SendAsync(byte[] Buffer, int Offset, int Count, bool Priority)
		{
			return this.SendAsync(false, Buffer, Offset, Count, Priority, null, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Priority">If packet should be sent before any waiting in the queue.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Buffer, int Offset, int Count, bool Priority)
		{
			return this.SendAsync(ConstantBuffer, Buffer, Offset, Count, Priority, null, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task<bool> SendAsync(byte[] Buffer, int Offset, int Count, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			return this.SendAsync(false, Buffer, Offset, Count, false, Callback, State);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Buffer, int Offset, int Count, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			return this.SendAsync(ConstantBuffer, Buffer, Offset, Count, false, Callback, State);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Priority">If packet should be sent before any waiting in the queue.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task<bool> SendAsync(byte[] Buffer, int Offset, int Count, bool Priority,
			EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			return this.SendAsync(false, Buffer, Offset, Count, Priority, Callback, State);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Priority">If packet should be sent before any waiting in the queue.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		public async Task<bool> SendAsync(bool ConstantBuffer, byte[] Buffer, int Offset, int Count, bool Priority,
			EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
			await this.QueueForOutput(ConstantBuffer, Buffer, Offset, Count, Priority, Result, Callback, State);
			return await Result.Task;
		}

		private async Task QueueForOutput(bool ConstantBuffer, byte[] Buffer, int Offset, int Count, bool Priority,
			TaskCompletionSource<bool> Task, EventHandlerAsync<DeliveryEventArgs> Callback,
			object State)
		{
			if (Buffer is null)
			{
				Task.TrySetException(new ArgumentException("Cannot be null.", nameof(Buffer)));
				return;
			}

			if (Count < 0)
			{
				Task.TrySetException(new ArgumentException("Count cannot be negative.", nameof(Count)));
				return;
			}

			if (Count == 0)
			{
				Task.TrySetResult(true);

				if (!(Callback is null))
					await Callback.Raise(this, new DeliveryEventArgs(State, true));

				return;
			}

			int c = Buffer.Length;
			if (Offset < 0 || Offset >= c)
			{
				Task.TrySetException(new ArgumentOutOfRangeException("Invalid offset.", nameof(Offset)));
				return;
			}

			if (Count < 0 || Offset + Count > c)
			{
				Task.TrySetException(new ArgumentOutOfRangeException("Invalid number of bytes.", nameof(Count)));
				return;
			}

			Rec Item;

			if (ConstantBuffer)
			{
				Item = new Rec()
				{
					Data = Buffer,
					Offset = Offset,
					Count = Count,
					Callback = Callback,
					State = State,
					Task = Task
				};
			}
			else
			{
				Item = new Rec()
				{
					Data = new byte[Count],
					Offset = 0,
					Count = Count,
					Callback = Callback,
					State = State,
					Task = Task
				};

				Array.Copy(Buffer, Offset, Item.Data, 0, Count);
			}

			TaskCompletionSource<bool> Wait;

			while (true)
			{
				Wait = null;

				lock (this.synchObj)
				{
					if (this.disposing || this.disposed)
					{
						Task.TrySetResult(false);
						this.sending = false;
						break;
					}

					if (!this.connected)
					{
						Task.TrySetException(new InvalidOperationException("Not connected."));
						return;
					}

					if (this.sending)
					{
						if (this.outputQueueSize > 0 && this.outputQueueSize + Count > this.maxOutputQueueSize)
						{
							Wait = new TaskCompletionSource<bool>();

							if (this.waitingQueue is null)
								this.waitingQueue = new ChunkedList<TaskCompletionSource<bool>>();

							this.waitingQueue.AddLastItem(Wait);
						}
						else
						{
							if (Priority)
								this.outputQueue.AddFirstItem(Item);
							else
								this.outputQueue.Add(Item);

							this.outputQueueSize += Count;
							return;
						}
					}
					else
					{
						this.outputQueue.Add(Item);
						this.outputQueueSize += Count;
						this.sending = true;
					}
				}

				if (Wait is null)
				{
					this.SendTask();
					return;
				}
				else
				{
					this.Information("Limiting output...");

					await Wait.Task;
					continue;
				}
			}
		}

		private async void SendTask()
		{
			bool DoDispose = false;
			Rec Rec = null;
			TaskCompletionSource<bool>[] ReleaseWaiting = null;
			bool CancelOutput = false;

			try
			{
#if WINDOWS_UWP
				DataWriter DataWriter;
#else
				Stream Stream;
#endif
				while (true)
				{
					lock (this.synchObj)
					{
						if (this.disposing ||
							this.disposed ||
							!this.outputQueue.HasFirstItem)
						{
							this.sending = false;
							this.EmptyIdleQueueLocked();

							if (this.disposing && !this.reading)
								DoDispose = true;

							CancelOutput = true;
							break;
						}

						Rec = this.outputQueue.RemoveFirst();
						this.outputQueueSize -= Rec.Count;

						if (this.waitingQueue?.HasFirstItem ?? false)
						{
							ReleaseWaiting = this.waitingQueue.ToArray();
							this.waitingQueue.Clear();
						}
#if WINDOWS_UWP
						DataWriter = this.dataWriter;
						if (DataWriter is null)
#else
						Stream = this.stream;
						if (Stream is null)
#endif
						{
							this.sending = false;
							CancelOutput = true;
							break;
						}
					}
#if WINDOWS_UWP
					byte[] Buffer;

					if (Rec.Offset > 0 || Rec.Count < Rec.Data.Length)
						Buffer = SnifferBase.CloneSection(Rec.Data, Rec.Offset, Rec.Count);
					else
						Buffer = Rec.Data;

					DataWriter.WriteBytes(Buffer);
					await this.dataWriter.StoreAsync();

					Rec.Task.TrySetResult(true);

					if (!(Rec.Callback is null))
						await Rec.Callback.Raise(this, new DeliveryEventArgs(Rec.State, true));
#else
					if (this.tcpClient?.Client.Connected ?? false)
					{
						await Stream.WriteAsync(Rec.Data, Rec.Offset, Rec.Count);

						try
						{
							await this.BinaryDataSent(true, Rec.Data, Rec.Offset, Rec.Count);
						}
						catch (Exception ex)
						{
							this.Exception(ex);
						}

						Rec.Task.TrySetResult(true);

						if (!(Rec.Callback is null))
							await Rec.Callback.Raise(this, new DeliveryEventArgs(Rec.State, true));
					}
					else
					{
						this.Error("Connection closed.");
						Rec.Task?.TrySetResult(false);

						if (!(Rec.Callback is null))
							await Rec.Callback.Raise(this, new DeliveryEventArgs(Rec.State, false));

						lock (this.synchObj)
						{
							this.sending = false;
							this.EmptyIdleQueueLocked();

							DoDispose = this.disposing && !this.reading;
						}

						CancelOutput = true;
						break;
					}
#endif
					if (!(ReleaseWaiting is null))
					{
						foreach (TaskCompletionSource<bool> Wait in ReleaseWaiting)
							Wait.TrySetResult(true);

						ReleaseWaiting = null;
					}
				}

				if (!this.disposed)
				{
#if WINDOWS_UWP
					if (!(this.dataWriter is null))
						await this.dataWriter.FlushAsync();
#else
					if (!(this.stream is null))
						await this.stream?.FlushAsync();
#endif
					if (this.disposeWhenDone)
					{
						this.sending = false;
						await this.DisposeAsync();
						return;
					}

					if (!(this.OnWriteQueueEmpty is null))
						await this.OnWriteQueueEmpty.Raise(this, EventArgs.Empty);
				}
			}
			catch (Exception ex)
			{
				this.Exception(ex.Message);

				if (!(Rec is null))
				{
					Rec.Task.TrySetResult(false);

					if (!(Rec.Callback is null))
						await Rec.Callback.Raise(this, new DeliveryEventArgs(Rec.State, false));
				}

				lock (this.synchObj)
				{
					this.sending = false;
					this.EmptyIdleQueueLocked();

					DoDispose = this.disposing && !this.reading;
				}

				await this.CancelOutputQueue();
			}
			finally
			{
				if (CancelOutput)
					await this.CancelOutputQueue();

				if (!(ReleaseWaiting is null))
				{
					foreach (TaskCompletionSource<bool> Wait in ReleaseWaiting)
						Wait.TrySetResult(true);

					ReleaseWaiting = null;
				}

				if (DoDispose)
				{
					try
					{
						await this.DoDisposeAsyncLocked();
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			}
		}

		private void EmptyIdleQueueLocked()
		{
			if (!(this.idleQueue is null))
			{
				while (this.idleQueue?.HasFirstItem ?? false)
					this.idleQueue?.RemoveFirst().TrySetResult(true);

				this.idleQueue = null;
			}
		}

		private void EmptyCancelQueueLocked()
		{
			if (!(this.cancelledQueue is null))
			{
				while (this.cancelledQueue?.HasFirstItem ?? false)
					this.cancelledQueue?.RemoveFirst().TrySetResult(true);

				this.cancelledQueue = null;
			}
		}

		private async Task CancelOutputQueue()
		{
			Rec Rec;

			while (true)
			{
				lock (this.synchObj)
				{
					if (this.outputQueue is null)
						break;

					if (!this.outputQueue.HasFirstItem)
						break;

					Rec = this.outputQueue.RemoveFirst();
					this.outputQueueSize -= Rec.Count;
				}

				Rec.Task.TrySetResult(false);

				if (!(Rec.Callback is null))
					await Rec.Callback.Raise(this, new DeliveryEventArgs(Rec.State, false));
			}
		}

		/// <summary>
		/// Event raised when the write queue is empty.
		/// </summary>
		public event EventHandlerAsync OnWriteQueueEmpty = null;

		/// <summary>
		/// Clears the <see cref="OnWriteQueueEmpty"/> event handler.
		/// </summary>
		public void OnWriteQueueEmptyClear() => this.OnWriteQueueEmpty = null;

		/// <summary>
		/// Resets the <see cref="OnWriteQueueEmpty"/> event handler.
		/// </summary>
		/// <param name="EventHandler">Event handler to set.</param>
		public void OnWriteQueueEmptyReset(EventHandlerAsync EventHandler) => this.OnWriteQueueEmpty = EventHandler;

		/// <summary>
		/// Method called when binary data has been sent.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte written.</param>
		/// <param name="Count">Number of bytes written.</param>
		protected virtual async Task BinaryDataSent(bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			if (this.sniffBinary && this.HasSniffers)
				this.TransmitBinary(ConstantBuffer, Buffer, Offset, Count);

			BinaryDataWrittenEventHandler h = this.OnSent;
			if (!(h is null))
				await h(this, ConstantBuffer, Buffer, Offset, Count);
		}

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public event BinaryDataWrittenEventHandler OnSent;

		/// <summary>
		/// Clears the <see cref="OnSent"/> event handler.
		/// </summary>
		public void OnSentClear() => this.OnSent = null;

		/// <summary>
		/// Resets the <see cref="OnSent"/> event handler.
		/// </summary>
		/// <param name="EventHandler">Event handler to set.</param>
		public void OnSentReset(BinaryDataWrittenEventHandler EventHandler) => this.OnSent = EventHandler;

		/// <summary>
		/// Flushes any pending or intermediate data.
		/// </summary>
		/// <returns>If output has been flushed.</returns>
		public virtual Task<bool> FlushAsync()
		{
			lock (this.synchObj)
			{
				if (!this.connected || !this.sending)
					return Task.FromResult(true);

				if (this.idleQueue is null)
					this.idleQueue = new ChunkedList<TaskCompletionSource<bool>>();

				TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
				this.idleQueue.Add(Result);
				return Result.Task;
			}
		}

#if WINDOWS_UWP
		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		public Task UpgradeToTlsAsClient(SocketProtectionLevel Protocols)
		{
			return this.UpgradeToTlsAsClient(Protocols, false, this.hostName);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="TrustRemoteEndPoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		public Task UpgradeToTlsAsClient(SocketProtectionLevel Protocols, bool TrustRemoteEndPoint)
		{
			return this.UpgradeToTlsAsClient(Protocols, TrustRemoteEndPoint, this.hostName);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="TrustRemoteEndPoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		/// <param name="DomainName">The domain name to validate certifictes for. By default, this is the same as the host name.</param>
		public async Task UpgradeToTlsAsClient(SocketProtectionLevel Protocols, bool TrustRemoteEndPoint, string DomainName)
		{
			lock (this.synchObj)
			{
				if (this.reading)
					throw new InvalidOperationException("Connection cannot be upgraded to TLS while in a reading state.");

				if (this.upgrading)
					throw new InvalidOperationException("Upgrading connection.");

				if (this.upgraded)
					throw new InvalidOperationException("Connection already upgraded.");

				this.domainName = DomainName;
				this.upgrading = true;
				this.trustRemoteEndPoint = TrustRemoteEndPoint;
			}

			try
			{
				this.dataWriter.DetachStream();

				if (this.trustRemoteEndPoint)
				{
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.IncompleteChain);
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.WrongUsage);
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.RevocationInformationMissing);
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.RevocationFailure);
				}

				await this.client.UpgradeToSslAsync(Protocols, new HostName(this.domainName));
				this.remoteCertificate = this.client.Information.ServerCertificate;
				this.remoteCertificateValid = true;
				this.SetTcpEndPoints();

				this.dataWriter = new DataWriter(this.client.OutputStream);
				this.upgraded = true;
			}
			finally
			{
				this.trustRemoteEndPoint = false;
				this.upgrading = false;
			}
		}

		private Certificate remoteCertificate = null;
		private bool upgraded = false;

		/// <summary>
		/// Certificate used by the remote endpoint.
		/// </summary>
		public Certificate RemoteCertificate => this.remoteCertificate;
#else
		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		public Task UpgradeToTlsAsClient(SslProtocols Protocols)
		{
			return this.UpgradeToTlsAsClient(Protocols, Array.Empty<string>());
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="AlpnProtocols">TLS Application-Layer Protocol Negotiation (ALPN) Protocol IDs
		/// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml#alpn-protocol-ids</param>
		public Task UpgradeToTlsAsClient(SslProtocols Protocols, params string[] AlpnProtocols)
		{
			return this.UpgradeToTlsAsClient(null, Protocols, null, false, this.hostName, AlpnProtocols);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		public Task UpgradeToTlsAsClient(SslProtocols Protocols, RemoteCertificateValidationCallback CertificateValidationCheck)
		{
			return this.UpgradeToTlsAsClient(Protocols, CertificateValidationCheck, Array.Empty<string>());
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		/// <param name="AlpnProtocols">TLS Application-Layer Protocol Negotiation (ALPN) Protocol IDs
		/// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml#alpn-protocol-ids</param>
		public Task UpgradeToTlsAsClient(SslProtocols Protocols, RemoteCertificateValidationCallback CertificateValidationCheck,
			params string[] AlpnProtocols)
		{
			return this.UpgradeToTlsAsClient(null, Protocols, CertificateValidationCheck, false, this.hostName, AlpnProtocols);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, Array.Empty<string>());
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="AlpnProtocols">TLS Application-Layer Protocol Negotiation (ALPN) Protocol IDs
		/// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml#alpn-protocol-ids</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols,
			params string[] AlpnProtocols)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, null, false, this.hostName, AlpnProtocols);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols,
			RemoteCertificateValidationCallback CertificateValidationCheck)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, CertificateValidationCheck, Array.Empty<string>());
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		/// <param name="AlpnProtocols">TLS Application-Layer Protocol Negotiation (ALPN) Protocol IDs
		/// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml#alpn-protocol-ids</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols,
			RemoteCertificateValidationCallback CertificateValidationCheck,
			params string[] AlpnProtocols)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, CertificateValidationCheck, false, this.hostName, AlpnProtocols);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="TrustRemoteEndPoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols, bool TrustRemoteEndPoint)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, TrustRemoteEndPoint, Array.Empty<string>());
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="TrustRemoteEndPoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		/// <param name="AlpnProtocols">TLS Application-Layer Protocol Negotiation (ALPN) Protocol IDs
		/// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml#alpn-protocol-ids</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols, bool TrustRemoteEndPoint,
			params string[] AlpnProtocols)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, null, TrustRemoteEndPoint, this.hostName, AlpnProtocols);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		/// <param name="TrustRemoteEndPoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols,
			RemoteCertificateValidationCallback CertificateValidationCheck, bool TrustRemoteEndPoint)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, CertificateValidationCheck, TrustRemoteEndPoint, Array.Empty<string>());
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		/// <param name="TrustRemoteEndPoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		/// <param name="AlpnProtocols">TLS Application-Layer Protocol Negotiation (ALPN) Protocol IDs
		/// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml#alpn-protocol-ids</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols,
			RemoteCertificateValidationCallback CertificateValidationCheck, bool TrustRemoteEndPoint,
			params string[] AlpnProtocols)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, CertificateValidationCheck, TrustRemoteEndPoint, this.hostName, AlpnProtocols);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		/// <param name="TrustRemoteEndPoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		/// <param name="DomainName">The domain name to validate certifictes for. By default, this is the same as the host name.</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols,
			RemoteCertificateValidationCallback CertificateValidationCheck, bool TrustRemoteEndPoint, string DomainName)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols,
				CertificateValidationCheck, TrustRemoteEndPoint, DomainName,
				Array.Empty<string>());
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		/// <param name="TrustRemoteEndPoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		/// <param name="DomainName">The domain name to validate certifictes for. By default, this is the same as the host name.</param>
		/// <param name="AlpnProtocols">TLS Application-Layer Protocol Negotiation (ALPN) Protocol IDs
		/// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml#alpn-protocol-ids</param>
		public async Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols,
			RemoteCertificateValidationCallback CertificateValidationCheck, bool TrustRemoteEndPoint, string DomainName,
			params string[] AlpnProtocols)
		{
			lock (this.synchObj)
			{
				if (this.reading)
					throw new InvalidOperationException("Connection cannot be upgraded to TLS while in a reading state.");

				if (this.upgrading)
					throw new InvalidOperationException("Upgrading connection.");

				if (this.stream is SslStream)
					throw new InvalidOperationException("Connection already upgraded.");

				this.upgrading = true;
				this.domainName = DomainName;
				this.certValidation = CertificateValidationCheck;
				this.trustRemoteEndPoint = TrustRemoteEndPoint;
			}

			try
			{
				X509Certificate2Collection ClientCertificates = null;
				if (!(ClientCertificate is null))
				{
					ClientCertificates = new X509Certificate2Collection()
					{
						ClientCertificate
					};
				}

				SslClientAuthenticationOptions Options = new SslClientAuthenticationOptions()
				{
					AllowRenegotiation = false,
					ApplicationProtocols = null,
					CertificateRevocationCheckMode = X509RevocationMode.Online,
					ClientCertificates = ClientCertificates,
					EnabledSslProtocols = Protocols,
					EncryptionPolicy = EncryptionPolicy.RequireEncryption,
					TargetHost = this.domainName ?? ((IPEndPoint)this.Client.Client.RemoteEndPoint).Address.ToString()
				};

				if (!(AlpnProtocols is null) && AlpnProtocols.Length > 0)
				{
					Options.ApplicationProtocols = new List<SslApplicationProtocol>();

					foreach (string AlpnProtocol in AlpnProtocols)
						Options.ApplicationProtocols.Add(new SslApplicationProtocol(AlpnProtocol));
				}

				SslStream SslStream = new SslStream(this.stream, true, this.ValidateCertificateRequired);
				this.stream = SslStream;

				await SslStream.AuthenticateAsClientAsync(Options, CancellationToken.None);
			}
			finally
			{
				this.certValidation = null;
				this.trustRemoteEndPoint = false;
				this.upgrading = false;
			}
		}

		private RemoteCertificateValidationCallback certValidation = null;
		private X509Certificate remoteCertificate = null;

		private bool ValidateCertificateRequired(object Sender, X509Certificate Certificate, X509Chain Chain,
			SslPolicyErrors SslPolicyErrors)
		{
			return this.ValidateCertificate(Sender, Certificate, Chain, SslPolicyErrors, true);
		}

		private bool ValidateCertificateOptional(object Sender, X509Certificate Certificate, X509Chain Chain,
			SslPolicyErrors SslPolicyErrors)
		{
			return this.ValidateCertificate(Sender, Certificate, Chain, SslPolicyErrors, false);
		}

		private bool ValidateCertificate(object Sender, X509Certificate Certificate, X509Chain Chain, SslPolicyErrors SslPolicyErrors,
			bool RequireCertificate)
		{
			bool Result;

			this.remoteCertificate?.Dispose();
			this.remoteCertificate = null;

			if (SslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable || Certificate is null)
			{
				this.remoteCertificateValid = false;
				Result = !RequireCertificate;
			}
			else if (SslPolicyErrors == SslPolicyErrors.None)
				this.remoteCertificateValid = Result = true;
			else
			{
				this.remoteCertificateValid = false;
				Result = this.trustRemoteEndPoint;
			}

			if (!(this.certValidation is null) && !(Certificate is null))
			{
				try
				{
					Result = this.certValidation(Sender, Certificate, Chain, SslPolicyErrors);
				}
				catch (Exception ex)
				{
					try
					{
						this.Exception(ex);
					}
					catch (Exception ex2)
					{
						Log.Exception(ex2);
					}

					Result = false;
				}
			}

			byte[] Cert = Certificate?.Export(X509ContentType.Cert);    // Avoids SafeHandle exception when accessing certificate later.

			if (!Result)
			{
				if (RequireCertificate)
				{
					StringBuilder Base64 = new StringBuilder();
					string s;
					int c = Cert?.Length ?? 0;
					int i = 0;
					int j;

					while (i < c)
					{
						j = Math.Min(57, c - i);
						s = Convert.ToBase64String(Cert, i, j);
						i += j;

						Base64.Append(s);

						if (i < c)
							Base64.AppendLine();
					}

					KeyValuePair<string, object>[] Tags = new KeyValuePair<string, object>[]
					{
						new KeyValuePair<string, object>("SslPolicyErrors", SslPolicyErrors.ToString()),
						new KeyValuePair<string, object>("Subject", Certificate?.Subject),
						new KeyValuePair<string, object>("Issuer", Certificate?.Issuer),
						new KeyValuePair<string, object>("HostName", this.hostName),
						new KeyValuePair<string, object>("DomainName", this.domainName),
						new KeyValuePair<string, object>("Cert", Base64.ToString())
					};

					if (this.trustRemoteEndPoint)
					{
						Result = true;
						Log.Notice("Invalid certificate received. But server is trusted.", Certificate?.Subject, Certificate?.Issuer, Tags);
					}
					else
						Log.Warning("Invalid certificate received (and rejected)", Certificate?.Subject, Certificate?.Issuer, "CertError", Tags);

					if (this.HasSniffers)
					{
						StringBuilder SniffMsg = new StringBuilder();

						if (this.trustRemoteEndPoint)
							SniffMsg.AppendLine("Invalid certificate received. But server is trusted.");
						else
							SniffMsg.AppendLine("Invalid certificate received (and rejected).");

						SniffMsg.AppendLine();
						SniffMsg.Append("sslPolicyErrors: ");
						SniffMsg.AppendLine(SslPolicyErrors.ToString());
						SniffMsg.Append("Subject: ");
						SniffMsg.AppendLine(Certificate?.Subject);
						SniffMsg.Append("Issuer: ");
						SniffMsg.AppendLine(Certificate?.Issuer);
						SniffMsg.Append("BASE64(Cert): ");
						SniffMsg.Append(Base64);

						try
						{
							if (this.trustRemoteEndPoint)
								this.Information(SniffMsg.ToString());
							else
								this.Warning(SniffMsg.ToString());
						}
						catch (Exception ex2)
						{
							Log.Exception(ex2);
						}
					}
				}
				else
				{
					this.remoteCertificate = null;
					this.remoteCertificateValid = false;
					Result = true;
				}
			}

			if (!(Cert is null) && Result)
			{
				this.remoteCertificate = new X509Certificate(Cert);
				this.SetTcpEndPoints();
			}

			return Result;
		}

		/// <summary>
		/// Certificate used by the remote endpoint.
		/// </summary>
		public X509Certificate RemoteCertificate => this.remoteCertificate;
#endif

		/// <summary>
		/// If the remote certificate is valid.
		/// </summary>
		public bool RemoteCertificateValid => this.remoteCertificateValid;

#if !WINDOWS_UWP
		/// <summary>
		/// Upgrades a server connection to TLS.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate.</param>
		public Task UpgradeToTlsAsServer(X509Certificate ServerCertificate)
		{
			return this.UpgradeToTlsAsServer(ServerCertificate, Array.Empty<string>());
		}

		/// <summary>
		/// Upgrades a server connection to TLS.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate.</param>
		/// <param name="AlpnProtocols">TLS Application-Layer Protocol Negotiation (ALPN) Protocol IDs
		/// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml#alpn-protocol-ids</param>
		public Task UpgradeToTlsAsServer(X509Certificate ServerCertificate, params string[] AlpnProtocols)
		{
			return this.UpgradeToTlsAsServer(ServerCertificate, Crypto.SecureTls, ClientCertificates.Optional, null, false, AlpnProtocols);
		}

		/// <summary>
		/// Upgrades a server connection to TLS.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		public Task UpgradeToTlsAsServer(X509Certificate ServerCertificate, SslProtocols Protocols)
		{
			return this.UpgradeToTlsAsServer(ServerCertificate, Protocols, Array.Empty<string>());
		}

		/// <summary>
		/// Upgrades a server connection to TLS.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="AlpnProtocols">TLS Application-Layer Protocol Negotiation (ALPN) Protocol IDs
		/// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml#alpn-protocol-ids</param>
		public Task UpgradeToTlsAsServer(X509Certificate ServerCertificate, SslProtocols Protocols, params string[] AlpnProtocols)
		{
			return this.UpgradeToTlsAsServer(ServerCertificate, Protocols, ClientCertificates.Optional, null, false, AlpnProtocols);
		}

		/// <summary>
		/// Upgrades a server connection to TLS.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="ClientCertificates">If client certificates are requested from client.</param>
		public Task UpgradeToTlsAsServer(X509Certificate ServerCertificate, SslProtocols Protocols, ClientCertificates ClientCertificates)
		{
			return this.UpgradeToTlsAsServer(ServerCertificate, Protocols, ClientCertificates, Array.Empty<string>());
		}

		/// <summary>
		/// Upgrades a server connection to TLS.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="ClientCertificates">If client certificates are requested from client.</param>
		/// <param name="AlpnProtocols">TLS Application-Layer Protocol Negotiation (ALPN) Protocol IDs
		/// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml#alpn-protocol-ids</param>
		public Task UpgradeToTlsAsServer(X509Certificate ServerCertificate, SslProtocols Protocols, ClientCertificates ClientCertificates, params string[] AlpnProtocols)
		{
			return this.UpgradeToTlsAsServer(ServerCertificate, Protocols, ClientCertificates, null, false, AlpnProtocols);
		}

		/// <summary>
		/// Upgrades a server connection to TLS.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="ClientCertificates">If client certificates are requested from client.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		public Task UpgradeToTlsAsServer(X509Certificate ServerCertificate, SslProtocols Protocols, ClientCertificates ClientCertificates,
			RemoteCertificateValidationCallback CertificateValidationCheck)
		{
			return this.UpgradeToTlsAsServer(ServerCertificate, Protocols, ClientCertificates, CertificateValidationCheck, Array.Empty<string>());
		}

		/// <summary>
		/// Upgrades a server connection to TLS.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="ClientCertificates">If client certificates are requested from client.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		/// <param name="AlpnProtocols">TLS Application-Layer Protocol Negotiation (ALPN) Protocol IDs
		/// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml#alpn-protocol-ids</param>
		public Task UpgradeToTlsAsServer(X509Certificate ServerCertificate, SslProtocols Protocols, ClientCertificates ClientCertificates,
			RemoteCertificateValidationCallback CertificateValidationCheck, params string[] AlpnProtocols)
		{
			return this.UpgradeToTlsAsServer(ServerCertificate, Protocols, ClientCertificates, CertificateValidationCheck, false, AlpnProtocols);
		}

		/// <summary>
		/// Upgrades a server connection to TLS.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="ClientCertificates">If client certificates are requested from client.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		/// <param name="TrustRemoteEndPoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		public Task UpgradeToTlsAsServer(X509Certificate ServerCertificate, SslProtocols Protocols, ClientCertificates ClientCertificates,
			RemoteCertificateValidationCallback CertificateValidationCheck, bool TrustRemoteEndPoint)
		{
			return this.UpgradeToTlsAsServer(ServerCertificate, Protocols, ClientCertificates,
				CertificateValidationCheck, TrustRemoteEndPoint, Array.Empty<string>());
		}

		/// <summary>
		/// Upgrades a server connection to TLS.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="ClientCertificates">If client certificates are requested from client.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		/// <param name="TrustRemoteEndPoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		/// <param name="AlpnProtocols">TLS Application-Layer Protocol Negotiation (ALPN) Protocol IDs
		/// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml#alpn-protocol-ids</param>
		public async Task UpgradeToTlsAsServer(X509Certificate ServerCertificate, SslProtocols Protocols, ClientCertificates ClientCertificates,
			RemoteCertificateValidationCallback CertificateValidationCheck, bool TrustRemoteEndPoint, params string[] AlpnProtocols)
		{
			lock (this.synchObj)
			{
				if (this.disposed)
					throw new ObjectDisposedException(nameof(BinaryTcpClient));

				if (this.reading)
					throw new InvalidOperationException("Connection cannot be upgraded to TLS while in a reading state.");

				if (this.upgrading)
					throw new InvalidOperationException("Upgrading connection.");

				if (this.stream is SslStream)
					throw new InvalidOperationException("Connection already upgraded.");

				this.upgrading = true;
				this.certValidation = CertificateValidationCheck;
				this.trustRemoteEndPoint = TrustRemoteEndPoint;
			}

			try
			{
				SslServerAuthenticationOptions Options = new SslServerAuthenticationOptions()
				{
					AllowRenegotiation = false,
					ApplicationProtocols = null,
					CertificateRevocationCheckMode = X509RevocationMode.Online,
					ClientCertificateRequired = false,
					EnabledSslProtocols = Protocols,
					EncryptionPolicy = EncryptionPolicy.RequireEncryption,
					ServerCertificate = ServerCertificate
				};

				if (!(AlpnProtocols is null) && AlpnProtocols.Length > 0)
				{
					Options.ApplicationProtocols = new List<SslApplicationProtocol>();

					foreach (string AlpnProtocol in AlpnProtocols)
						Options.ApplicationProtocols.Add(new SslApplicationProtocol(AlpnProtocol));
				}

				switch (ClientCertificates)
				{
					case ClientCertificates.NotUsed:
						Options.RemoteCertificateValidationCallback = this.ValidateCertificateOptional;
						Options.CertificateRevocationCheckMode = X509RevocationMode.NoCheck;
						break;

					case ClientCertificates.Optional:
					default:
						Options.ClientCertificateRequired = true;
						Options.RemoteCertificateValidationCallback = this.ValidateCertificateOptional;
						break;

					case ClientCertificates.Required:
						Options.ClientCertificateRequired = true;
						Options.RemoteCertificateValidationCallback = this.ValidateCertificateRequired;
						break;
				}

				SslStream SslStream = new SslStream(this.stream, true, Options.RemoteCertificateValidationCallback);
				this.stream = SslStream;

				await SslStream.AuthenticateAsServerAsync(Options, CancellationToken.None);
			}
			finally
			{
				this.certValidation = null;
				this.trustRemoteEndPoint = false;
				this.upgrading = false;
			}
		}
#endif
		/// <summary>
		/// If connection is encrypted or not.
		/// </summary>
		public bool IsEncrypted
		{
#if WINDOWS_UWP
			get => this.client.Information.ProtectionLevel != SocketProtectionLevel.PlainSocket;
#else
			get => this.stream is SslStream SslStream && SslStream.IsEncrypted;
#endif
		}

#if !WINDOWS_UWP
		/// <summary>
		/// Cipher strength. (Nr bits of brute force complexity required to break algorithm).
		/// </summary>
		public int CipherStrength => this.stream is SslStream SslStream && SslStream.IsEncrypted ? SslStream.CipherStrength : 0;

		/// <summary>
		/// Hash algorithm strength. (Nr bits of brute force complexity required to break algorithm).
		/// </summary>
		public int HashStrength => this.stream is SslStream SslStream && SslStream.IsEncrypted ? SslStream.HashStrength : 0;

		/// <summary>
		/// Key Exchange strength. (Nr bits of brute force complexity required to break algorithm).
		/// </summary>
		public int KeyExchangeStrength => this.stream is SslStream SslStream && SslStream.IsEncrypted ? SslStream.KeyExchangeStrength : 0;
#endif

		/// <summary>
		/// Allows the caller to pause reading.
		/// </summary>
		public async Task<bool> PauseReading()
		{
			TaskCompletionSource<bool> Task = new TaskCompletionSource<bool>();

			lock (this.synchObj)
			{
				if (this.disposed || this.disposing)
					throw new ObjectDisposedException("Object already disposed.");

				if (this.reading)
				{
					this.cancelRead = true;

					if (this.cancelledQueue is null)
						this.cancelledQueue = new ChunkedList<TaskCompletionSource<bool>>();

					this.cancelledQueue.Add(Task);
#if WINDOWS_UWP
					IAsyncAction _ = this.client.CancelIOAsync();
#else
					this.currentCancelReading?.Cancel();

					NetworkingModule.UnregisterToken(this.id);
#endif
				}
				else
					return false;
			}

			await Task.Task;

			return Task.Task.Result;
		}

	}
}
