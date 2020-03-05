using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
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

namespace Waher.Networking
{
	/// <summary>
	/// Connection error event handler delegate.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Exception">Information about error received.</param>
	public delegate void ExceptionEventHandler(object Sender, Exception Exception);

	/// <summary>
	/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also makes the use of <see cref="TcpClient"/>
	/// safe, making sure it can be disposed, even during an active connection attempt. Outgoing data is queued and tramitted in the
	/// permitted pace.
	/// </summary>
	public class BinaryTcpClient : Sniffable, IDisposable, IBinaryTransportLayer
	{
		private const int BufferSize = 65536;

		private readonly LinkedList<KeyValuePair<byte[], EventHandler>> queue = new LinkedList<KeyValuePair<byte[], EventHandler>>();
#if WINDOWS_UWP
		private readonly MemoryBuffer memoryBuffer = new MemoryBuffer(BufferSize);
		private readonly StreamSocket client;
		private readonly IBuffer buffer = null;
		private DataWriter dataWriter = null;
#else
		private readonly byte[] buffer = new byte[BufferSize];
		private readonly TcpClient tcpClient;
		private Stream stream = null;
		private readonly CancellationTokenSource source;
#endif
		private readonly object synchObj = new object();
		private readonly bool sniffBinary;
		private string hostName;
		private bool connecting = false;
		private bool connected = false;
		private bool disposing = false;
		private bool disposed = false;
		private bool sending = false;
		private bool reading = false;
		private bool upgrading = false;
		private bool trustServer = false;
		private bool serverCertificateValid = false;

		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		/// <param name="Sniffers">Sniffers.</param>
		public BinaryTcpClient(params ISniffer[] Sniffers)
			: this(true, Sniffers)
		{
		}

		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		/// <param name="SniffBinary">If binary communication is to be forwarded to registered sniffers.</param>
		/// <param name="Sniffers">Sniffers.</param>
		protected BinaryTcpClient(bool SniffBinary, params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.sniffBinary = SniffBinary;
#if WINDOWS_UWP
			this.buffer = Windows.Storage.Streams.Buffer.CreateCopyFromMemoryBuffer(this.memoryBuffer);
			this.client = new StreamSocket();
#else
			this.tcpClient = new TcpClient();
			this.source = new CancellationTokenSource();
#endif
		}

#if WINDOWS_UWP
		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="Sniffers">Sniffers.</param>
		protected BinaryTcpClient(StreamSocket Client, params ISniffer[] Sniffers)
			: this(Client, true, Sniffers)
		{
		}

		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="SniffBinary">If binary communication is to be forwarded to registered sniffers.</param>
		/// <param name="Sniffers">Sniffers.</param>
		protected BinaryTcpClient(StreamSocket Client, bool SniffBinary, params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.sniffBinary = SniffBinary;
			this.buffer = Windows.Storage.Streams.Buffer.CreateCopyFromMemoryBuffer(this.memoryBuffer);
			this.client = Client;
		}
#else
		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public BinaryTcpClient(TcpClient Client, params ISniffer[] Sniffers)
			: this(Client, true, Sniffers)
		{
		}

		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="SniffBinary">If binary communication is to be forwarded to registered sniffers.</param>
		/// <param name="Sniffers">Sniffers.</param>
		protected BinaryTcpClient(TcpClient Client, bool SniffBinary, params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.sniffBinary = SniffBinary;
			this.tcpClient = Client;
			this.source = new CancellationTokenSource();
		}
#endif

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
#endif

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
			this.hostName = Host;
			this.PreConnect();
#if WINDOWS_UWP
			await this.client.ConnectAsync(new HostName(Host), Port.ToString(), SocketProtectionLevel.PlainSocket);
			return await this.PostConnect(Paused);
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
			this.hostName = Address.ToString();
			this.PreConnect();
#if WINDOWS_UWP
			await this.client.ConnectAsync(new HostName(Address.ToString()), Port.ToString(), SocketProtectionLevel.PlainSocket);
			return await this.PostConnect(Paused);
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
			this.hostName = null;
			this.PreConnect();
			await this.tcpClient.ConnectAsync(Addresses, Port);
			return this.PostConnect(Paused);
		}
#endif

#if WINDOWS_UWP
		/// <summary>
		/// Binds to a <see cref="TcpClient"/> that was already connected when provided to the constructor.
		/// </summary>
		/// <returns>If connection was established. If false is returned, the object has been disposed during the connection attempt.</returns>
		public Task<bool> Bind()
		{
			return this.Bind(false);
		}

		/// <summary>
		/// Binds to a <see cref="TcpClient"/> that was already connected when provided to the constructor.
		/// </summary>
		/// <param name="Paused">If connection starts in a paused state (i.e. not waiting for incoming communication).</param>
		/// <returns>If connection was established. If false is returned, the object has been disposed during the connection attempt.</returns>
		public Task<bool> Bind(bool Paused)
		{
			this.PreConnect();
			return this.PostConnect(Paused);
		}
#else
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
#endif
		/// <summary>
		/// If the connection is open.
		/// </summary>
		public bool Connected => this.connected && !this.disposing && !this.disposed;

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

			this.serverCertificate = null;
			this.serverCertificateValid = false;
		}

#if WINDOWS_UWP
		private async Task<bool> PostConnect(bool Paused)
#else
		private bool PostConnect(bool Paused)
#endif
		{
#if WINDOWS_UWP
			bool DoDispose = false;
#endif

			lock (this.synchObj)
			{
				this.connecting = false;

				if (this.disposed)
					return false;

				if (this.disposing)
#if WINDOWS_UWP
					DoDispose = true;
#else
				{
					this.DoDisposeLocked();
					return false;
				}
#endif
				else
					this.connected = true;
			}

#if WINDOWS_UWP
			if (DoDispose)
			{
				await this.DoDispose();
				return false;
			}

			this.dataWriter = new DataWriter(this.client.OutputStream);
#else
			this.stream = this.tcpClient.GetStream();
#endif
			if (!Paused)
				this.BeginRead();

			return true;
		}

		/// <summary>
		/// Disposes of the object. The underlying <see cref="TcpClient"/> is either disposed directly, or when asynchronous
		/// operations have ceased.
		/// </summary>
		public void Dispose()
		{
			lock (this.synchObj)
			{
				if (this.disposed || this.disposing)
					throw new ObjectDisposedException("Object already disposed.");

				if (this.connecting)
				{
					this.disposing = true;
					return;
				}

#if !WINDOWS_UWP
				this.DoDisposeLocked();
#endif
			}

#if WINDOWS_UWP
			Task _ = this.DoDispose();
#endif
		}

#if WINDOWS_UWP
		private async Task DoDispose()
#else
		private void DoDisposeLocked()
#endif
		{
			this.disposed = true;
			this.connecting = false;
			this.connected = false;
			this.queue.Clear();
#if WINDOWS_UWP
			await this.client.CancelIOAsync();
			this.client.Dispose();
			this.memoryBuffer.Dispose();
			this.dataWriter.Dispose();
#else
			this.stream = null;
			this.source.Cancel();
			this.tcpClient.Dispose();
#endif
		}

		/// <summary>
		/// Continues reading from the socket, if paused in an event handler.
		/// </summary>
		public void Continue()
		{
			lock (this.synchObj)
			{
				if (this.reading)
					throw new InvalidOperationException("Already in a reading state.");

				this.reading = true;
			}

			this.BeginRead();
		}

		/// <summary>
		/// If the reading is paused.
		/// </summary>
		public bool Paused => this.connected && !this.reading;

		private async void BeginRead()
		{
			try
			{
#if WINDOWS_UWP
				IInputStream InputStream;
#else
				Stream Stream;
#endif
				int NrRead;
				bool Continue = true;

				while (Continue)
				{
					lock (this.synchObj)
					{
						if (this.disposing || this.disposed)
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
					if (this.disposed || NrRead <= 0)
					{
						this.Disconnected();
						break;
					}
#else
					NrRead = await Stream.ReadAsync(this.buffer, 0, BufferSize, this.source.Token);
					if (this.disposed || NrRead <= 0)
					{
						this.Disconnected();
						break;
					}

					byte[] Packet = new byte[NrRead];
					Array.Copy(this.buffer, 0, Packet, 0, NrRead);
#endif

					try
					{
						Continue = await this.BinaryDataReceived(Packet);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
			catch (Exception ex)
			{
				this.Error(ex);
			}
			finally
			{
				this.reading = false;
			}
		}

		/// <summary>
		/// Method called when the connection has been disconnected.
		/// </summary>
		protected virtual void Disconnected()
		{
			try
			{
				this.OnDisconnected?.Invoke(this, new EventArgs());
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Method called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary data received.</param>
		/// <returns>If the process should be continued.</returns>
		protected virtual Task<bool> BinaryDataReceived(byte[] Data)
		{
			if (this.sniffBinary && this.HasSniffers)
				this.ReceiveBinary(Data);

			return this.OnReceived?.Invoke(this, Data) ?? Task.FromResult<bool>(true);
		}

		/// <summary>
		/// Method called when an exception has been caught.
		/// </summary>
		/// <param name="ex">Exception</param>
		protected virtual void Error(Exception ex)
		{
			try
			{
				if (this.HasSniffers)
					this.Error(ex.Message);

				OnError?.Invoke(this, ex);
			}
			catch (Exception ex2)
			{
				Log.Critical(ex2);
			}
		}

		/// <summary>
		/// Event received when binary data has been received.
		/// </summary>
		public event BinaryEventHandler OnReceived;

		/// <summary>
		/// Event raised when an error has occurred.
		/// </summary>
		public event ExceptionEventHandler OnError;

		/// <summary>
		/// Event raised when the connection has been disconnected.
		/// </summary>
		public event EventHandler OnDisconnected;

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Packet">Binary packet.</param>
		public void Send(byte[] Packet)
		{
			this.BeginSend(Packet, null, true);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Packet">Binary packet.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		public void Send(byte[] Packet, EventHandler Callback)
		{
			this.BeginSend(Packet, Callback, true);
		}

		private async void BeginSend(byte[] Packet, EventHandler Callback, bool CheckSending)
		{
			if (Packet is null)
				throw new ArgumentException("Cannot be null.", nameof(Packet));

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
						if (this.disposing || this.disposed)
						{
							this.sending = false;
							break;
						}

						if (CheckSending)
						{
							if (!this.connected)
								throw new IOException("No connected.");

							if (this.sending)
							{
								this.queue.AddLast(new KeyValuePair<byte[], EventHandler>(Packet, Callback));
								return;
							}

							this.sending = true;
							CheckSending = false;
						}
						else if (Packet is null)
						{
							if (this.queue.First is null)
							{
								this.sending = false;
								break;
							}
							else
							{
								KeyValuePair<byte[], EventHandler> P = this.queue.First.Value;
								this.queue.RemoveFirst();

								Packet = P.Key;
								Callback = P.Value;
							}
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
							break;
						}
					}

#if WINDOWS_UWP
					DataWriter.WriteBytes(Packet);
					await this.dataWriter.StoreAsync();
#else
					await Stream.WriteAsync(Packet, 0, Packet.Length, this.source.Token);
#endif

					try
					{
						await this.BinaryDataSent(Packet);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}

					if (!(Callback is null))
					{
						try
						{
							Callback(this, new EventArgs());
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}

					Packet = null;
				}

				EventHandler h = this.OnWriteQueueEmpty;
				if (!(h is null))
				{
					try
					{
						h(this, new EventArgs());
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
			catch (Exception ex)
			{
				this.sending = false;
				this.Error(ex);
			}
		}

		/// <summary>
		/// Event raised when the write queue is empty.
		/// </summary>
		public event EventHandler OnWriteQueueEmpty = null;

		/// <summary>
		/// Method called when binary data has been sent.
		/// </summary>
		/// <param name="Data">Binary data sent.</param>
		protected virtual Task BinaryDataSent(byte[] Data)
		{
			if (this.sniffBinary && this.HasSniffers)
				this.TransmitBinary(Data);

			return this.OnSent?.Invoke(this, Data) ?? Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public event BinaryEventHandler OnSent;

#if WINDOWS_UWP
		/// <summary>
		/// Upgrades connection to TLS.
		/// </summary>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		public Task UpgradeToTlsAsClient(SocketProtectionLevel Protocols)
		{
			return this.UpgradeToTlsAsClient(null, Protocols, false);
		}

		/// <summary>
		/// Upgrades connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		public Task UpgradeToTlsAsClient(Certificate ClientCertificate, SocketProtectionLevel Protocols)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, false);
		}

		/// <summary>
		/// Upgrades connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="TrustServer">If the server should be trusted.</param>
		public async Task UpgradeToTlsAsClient(Certificate ClientCertificate, SocketProtectionLevel Protocols, bool TrustServer)
		{
			lock (this.synchObj)
			{
				if (this.reading)
					throw new InvalidOperationException("Connection cannot be upgraded to TLS while in a reading state.");

				if (this.upgrading)
					throw new InvalidOperationException("Upgrading connection.");

				if (this.upgraded)
					throw new InvalidOperationException("Connection already upgraded.");

				this.upgrading = true;
				this.trustServer = TrustServer;
			}

			try
			{
				this.dataWriter.DetachStream();

				if (this.trustServer)
				{
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.IncompleteChain);
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.WrongUsage);
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.RevocationInformationMissing);
					this.client.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.RevocationFailure);
				}

				await this.client.UpgradeToSslAsync(Protocols, new HostName(this.hostName));
				this.serverCertificate = this.client.Information.ServerCertificate;
				this.serverCertificateValid = true;

				this.dataWriter = new DataWriter(this.client.OutputStream);
				this.upgraded = true;
			}
			finally
			{
				this.trustServer = false;
				this.upgrading = false;
			}
		}

		private Certificate serverCertificate = null;
		private bool upgraded = false;

		/// <summary>
		/// Certificate used by the server.
		/// </summary>
		public Certificate ServerCertificate => this.serverCertificate;

#else
		/// <summary>
		/// Upgrades connection to TLS.
		/// </summary>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		public Task UpgradeToTlsAsClient(SslProtocols Protocols, RemoteCertificateValidationCallback CertificateValidationCheck)
		{
			return this.UpgradeToTlsAsClient(null, Protocols, CertificateValidationCheck, false);
		}

		/// <summary>
		/// Upgrades connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, null, false);
		}

		/// <summary>
		/// Upgrades connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols,
			RemoteCertificateValidationCallback CertificateValidationCheck)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, CertificateValidationCheck, false);
		}

		/// <summary>
		/// Upgrades connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="TrustServer">If the server should be trusted.</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols, bool TrustServer)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, null, TrustServer);
		}

		/// <summary>
		/// Upgrades connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		/// <param name="TrustServer">If the server should be trusted.</param>
		public async Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols,
			RemoteCertificateValidationCallback CertificateValidationCheck, bool TrustServer)
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
				this.certValidation = CertificateValidationCheck;
				this.trustServer = TrustServer;
			}

			try
			{
				X509Certificate2Collection ClientCertificates = null;
				SslStream SslStream = new SslStream(this.stream, true, this.ValidateCertificate);

				if (!(ClientCertificate is null))
				{
					ClientCertificates = new X509Certificate2Collection()
					{
						ClientCertificate
					};
				}

				this.stream = SslStream;
				await SslStream.AuthenticateAsClientAsync(this.hostName ?? ((IPEndPoint)this.Client.Client.LocalEndPoint).Address.ToString(),
					ClientCertificates, Protocols, true);
			}
			finally
			{
				this.certValidation = null;
				this.trustServer = false;
				this.upgrading = false;
			}
		}

		private RemoteCertificateValidationCallback certValidation = null;
		private X509Certificate serverCertificate = null;

		private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			this.serverCertificate = certificate;

			if (sslPolicyErrors == SslPolicyErrors.None)
				this.serverCertificateValid = true;
			else
			{
				this.serverCertificateValid = false;
				return this.trustServer;
			}

			if (this.certValidation is null)
				return true;
			else
			{
				try
				{
					return this.certValidation(sender, certificate, chain, sslPolicyErrors);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					return false;
				}
			}
		}

		/// <summary>
		/// Certificate used by the server.
		/// </summary>
		public X509Certificate ServerCertificate => this.serverCertificate;
#endif

		/// <summary>
		/// If the server certificate is valid.
		/// </summary>
		public bool ServerCertificateValid => this.serverCertificateValid;
	}
}
