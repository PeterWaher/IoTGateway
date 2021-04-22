using System;
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

namespace Waher.Networking
{
	/// <summary>
	/// Asynchronous version of <see cref="EventArgs"/>.
	/// </summary>
	public delegate Task EventHandlerAsync(object Sender, EventArgs e);

	/// <summary>
	/// Connection error event handler delegate.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Exception">Information about error received.</param>
	public delegate Task ExceptionEventHandler(object Sender, Exception Exception);

	/// <summary>
	/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also makes the use of <see cref="TcpClient"/>
	/// safe, making sure it can be disposed, even during an active connection attempt. Outgoing data is queued and transmitted in the
	/// permitted pace.
	/// </summary>
	public class BinaryTcpClient : Sniffable, IDisposable, IBinaryTransportLayer
	{
		private const int BufferSize = 65536;

		private readonly LinkedList<Rec> queue = new LinkedList<Rec>();
		private LinkedList<TaskCompletionSource<bool>> idleQueue = null;
		private LinkedList<TaskCompletionSource<bool>> cancelledQueue = null;
#if WINDOWS_UWP
		private readonly MemoryBuffer memoryBuffer = new MemoryBuffer(BufferSize);
		private readonly StreamSocket client;
		private readonly IBuffer buffer = null;
		private DataWriter dataWriter = null;
#else
		private readonly byte[] buffer = new byte[BufferSize];
		private readonly TcpClient tcpClient;
		private Stream stream = null;
		private CancellationTokenSource cancelReading;
#endif
		private readonly object synchObj = new object();
		private readonly bool sniffBinary;
		private string hostName;
		private string domainName;
		private bool connecting = false;
		private bool connected = false;
		private bool disposing = false;
		private bool disposed = false;
		private bool sending = false;
		private bool reading = false;
		private bool upgrading = false;
		private bool trustRemoteEndpoint = false;
		private bool remoteCertificateValid = false;
		private bool disposeWhenDone = false;
		private bool cancelRead = false;

		private class Rec
		{
			public byte[] Data;
			public EventHandler Callback;
			public TaskCompletionSource<bool> Task;
		}

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
		public BinaryTcpClient(bool SniffBinary, params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.sniffBinary = SniffBinary;
#if WINDOWS_UWP
			this.buffer = Windows.Storage.Streams.Buffer.CreateCopyFromMemoryBuffer(this.memoryBuffer);
			this.client = new StreamSocket();
#else
			this.tcpClient = new TcpClient();
			this.cancelReading = new CancellationTokenSource();
#endif
		}

#if WINDOWS_UWP
		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public BinaryTcpClient(StreamSocket Client, params ISniffer[] Sniffers)
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
		public BinaryTcpClient(StreamSocket Client, bool SniffBinary, params ISniffer[] Sniffers)
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
		public BinaryTcpClient(TcpClient Client, bool SniffBinary, params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.sniffBinary = SniffBinary;
			this.tcpClient = Client;
			this.cancelReading = new CancellationTokenSource();
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

		/// <summary>
		/// Stream object currently being used.
		/// </summary>
		public Stream Stream => this.stream;
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
			lock (this.synchObj)
			{
				this.connecting = false;

				if (this.disposed)
					return false;

				if (this.disposing)
				{
					this.DoDisposeLocked();
					return false;
				}
				else
					this.connected = true;
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

			this.Dispose();
		}

		/// <summary>
		/// Disposes of the object. The underlying <see cref="TcpClient"/> is either disposed directly, or when asynchronous
		/// operations have ceased.
		/// </summary>
		public virtual void Dispose()
		{
#if WINDOWS_UWP
			bool Cancel;

			lock (this.synchObj)
			{
				if (this.disposed || this.disposing)
					return;

				if (Cancel = this.connecting || this.reading || this.sending)
					this.disposing = true;
				else
					this.DoDisposeLocked();
			}

			if (Cancel)
			{
				IAsyncAction _ = this.client.CancelIOAsync();
			}
		}
#else
			lock (this.synchObj)
			{
				if (this.disposed || this.disposing)
					return;

				if (this.connecting || this.reading || this.sending)
				{
					this.disposing = true;
					this.cancelReading.Cancel();
					Task.Delay(1000).ContinueWith(this.AbortRead);  // Double-check socket gets cancelled. If not, forcefully close.
					return;
				}

				this.DoDisposeLocked();
			}
		}

		private Task AbortRead(object P)
		{
			if (!this.disposed)
				this.DoDisposeLocked();

			return Task.CompletedTask;
		}
#endif

		private void DoDisposeLocked()
		{
			this.disposed = true;
			this.connecting = false;
			this.connected = false;

			LinkedListNode<Rec> Node = this.queue.First;

			while (!(Node is null))
			{
				Node.Value.Task.TrySetResult(false);
				Node = Node.Next;
			}

			this.queue.Clear();

			this.EmptyIdleQueueLocked();
			this.EmptyCancelQueueLocked();

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
			{
				foreach (ISniffer Sniffer in this.Sniffers)
				{
					this.Remove(Sniffer);

					if (Sniffer is IDisposable Disposable)
						Disposable.Dispose();
				}
			}
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

		private async void BeginRead()
		{
			lock (this.synchObj)
			{
				if (this.disposing || this.disposed)
					return;

				if (this.reading)
					throw new InvalidOperationException("Already in a reading state.");

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
						if (this.disposing || this.disposed || this.cancelRead)
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
					NrRead = await Stream.ReadAsync(this.buffer, 0, BufferSize, this.cancelReading.Token);
#endif
					if (this.disposing || this.disposed)
						break;

					if (NrRead <= 0)
					{
						lock (this.synchObj)
						{
							if (this.cancelRead)
								break;
						}

						this.Disconnected();
						break;
					}

					try
					{
#if WINDOWS_UWP
						Continue = await this.BinaryDataReceived(Packet, 0, NrRead);
#else
						Continue = await this.BinaryDataReceived(this.buffer, 0, NrRead);
#endif
					}
					catch (Exception ex)
					{
						this.Error(ex);
					}
				}
			}
			catch (Exception ex)
			{
				this.Error(ex);
			}
			finally
			{
				lock (this.synchObj)
				{
					this.reading = false;

					if (this.cancelRead)
					{
						this.cancelRead = false;
#if !WINDOWS_UWP
						this.cancelReading.Dispose();
						this.cancelReading = new CancellationTokenSource();
#endif
						this.EmptyCancelQueueLocked();
					}

					if (this.disposing && !this.sending)
						this.DoDisposeLocked();
				}
			}

			if (!Continue && !this.disposed && !this.disposing)
			{
				AsyncEventHandler h = this.OnPaused;
				if (!(h is null))
				{
					try
					{
						await h(this, new EventArgs());
					}
					catch (Exception ex)
					{
						this.Error(ex);
					}
				}
			}
		}

		/// <summary>
		/// Event raised when reading on the socked has been paused. Call <see cref="Continue"/> to resume reading.
		/// </summary>
		public event AsyncEventHandler OnPaused;

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
				this.Error(ex);
			}
		}

		/// <summary>
		/// Method called when binary data has been received.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte read.</param>
		/// <param name="Count">Number of bytes read.</param>
		/// <returns>If the process should be continued.</returns>
		protected virtual Task<bool> BinaryDataReceived(byte[] Buffer, int Offset, int Count)
		{
			if (this.sniffBinary && this.HasSniffers)
				this.ReceiveBinary(ToArray(Buffer, Offset, Count));

			return this.OnReceived?.Invoke(this, Buffer, Offset, Count) ?? Task.FromResult<bool>(true);
		}

		/// <summary>
		/// Converts a binary subset of a buffer into an array.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte read.</param>
		/// <param name="Count">Number of bytes read.</param>
		/// <returns>Array</returns>
		public static byte[] ToArray(byte[] Buffer, int Offset, int Count)
		{
			if (Offset == 0 && Count == Buffer.Length)
				return Buffer;

			byte[] Result = new byte[Count];
			Array.Copy(Buffer, Offset, Result, 0, Count);

			return Result;
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
					this.Exception(ex);

				OnError?.Invoke(this, ex);
			}
			catch (Exception ex2)
			{
				this.Error(ex2);
			}
		}

		/// <summary>
		/// Event received when binary data has been received.
		/// </summary>
		public event BinaryDataReadEventHandler OnReceived;

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
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(byte[] Packet)
		{
			return this.SendAsync(Packet, 0, Packet.Length, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Packet">Binary packet.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(byte[] Packet, EventHandler Callback)
		{
			return this.SendAsync(Packet, 0, Packet.Length, Callback);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(byte[] Buffer, int Offset, int Count)
		{
			return this.SendAsync(Buffer, Offset, Count, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(byte[] Buffer, int Offset, int Count, EventHandler Callback)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
			this.BeginSend(Buffer, Offset, Count, Result, Callback, true);
			return Result.Task;
		}

		private async void BeginSend(byte[] Buffer, int Offset, int Count, TaskCompletionSource<bool> Task,
			EventHandler Callback, bool CheckSending)
		{
			if (Buffer is null)
				throw new ArgumentException("Cannot be null.", nameof(Buffer));

			if (Count < 0)
				throw new ArgumentException("Count cannot be negative.", nameof(Count));

			if (Count == 0)
			{
				Task.TrySetResult(true);

				if (!(Callback is null))
				{
					try
					{
						Callback(this, new EventArgs());
					}
					catch (Exception ex)
					{
						this.Error(ex);
					}
				}

				return;
			}

			int c = Buffer.Length;
			if (Offset < 0 || Offset >= c)
				throw new ArgumentOutOfRangeException("Invalid offset.", nameof(Offset));

			if (Count < 0 || Offset + Count > c)
				throw new ArgumentOutOfRangeException("Invalid number of bytes.", nameof(Count));

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
							Task.TrySetResult(false);
							this.sending = false;
							break;
						}

						if (CheckSending)
						{
							if (!this.connected)
								throw new InvalidOperationException("Not connected.");

							if (this.sending)
							{
								byte[] Packet = new byte[Count];
								Array.Copy(Buffer, Offset, Packet, 0, Count);
								this.queue.AddLast(new Rec()
								{
									Data = Packet,
									Callback = Callback,
									Task = Task
								});
								return;
							}

							this.sending = true;
							CheckSending = false;
						}
						else
						{
							if (this.queue.First is null)
							{
								this.sending = false;
								this.EmptyIdleQueueLocked();

								if (this.disposing && !this.reading)
									this.DoDisposeLocked();

								break;
							}
							else
							{
								Rec Rec = this.queue.First.Value;
								this.queue.RemoveFirst();

								Buffer = Rec.Data;
								Offset = 0;
								Count = Buffer.Length;
								Callback = Rec.Callback;
								Task = Rec.Task;
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
					if (Offset > 0 || Count < c)
					{
						Buffer = ToArray(Buffer, Offset, Count);
						Offset = 0;
						Count = c;
					}

					DataWriter.WriteBytes(Buffer);
					await this.dataWriter.StoreAsync();
#else
					await Stream.WriteAsync(Buffer, Offset, Count);
#endif

					try
					{
						await this.BinaryDataSent(Buffer, Offset, Count);
					}
					catch (Exception ex)
					{
						this.Error(ex);
					}

					Task.TrySetResult(true);

					if (!(Callback is null))
					{
						try
						{
							Callback(this, new EventArgs());
						}
						catch (Exception ex)
						{
							this.Error(ex);
						}
					}
				}

				if (!this.disposed)
				{
#if WINDOWS_UWP
					await this.dataWriter.FlushAsync();
#else
					await this.stream?.FlushAsync();
#endif
					if (this.disposeWhenDone)
					{
						this.Dispose();
						return;
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
							this.Error(ex);
						}
					}
				}
			}
			catch (Exception ex)
			{
				bool DoDispose;

				lock (this.synchObj)
				{
					this.sending = false;
					Task?.TrySetResult(false);

					this.EmptyIdleQueueLocked();

					if (DoDispose = this.disposing && !this.reading)
						this.DoDisposeLocked();
				}

				if (!DoDispose)
					this.Error(ex);
			}
		}

		private void EmptyIdleQueueLocked()
		{
			if (!(this.idleQueue is null))
			{
				foreach (TaskCompletionSource<bool> T in this.idleQueue)
					T.TrySetResult(true);

				this.idleQueue = null;
			}
		}

		private void EmptyCancelQueueLocked()
		{
			if (!(this.cancelledQueue is null))
			{
				foreach (TaskCompletionSource<bool> T in this.cancelledQueue)
					T.TrySetResult(true);

				this.cancelledQueue = null;
			}
		}

		/// <summary>
		/// Event raised when the write queue is empty.
		/// </summary>
		public event EventHandler OnWriteQueueEmpty = null;

		/// <summary>
		/// Method called when binary data has been sent.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte written.</param>
		/// <param name="Count">Number of bytes written.</param>
		protected virtual Task BinaryDataSent(byte[] Buffer, int Offset, int Count)
		{
			if (this.sniffBinary && this.HasSniffers)
				this.TransmitBinary(ToArray(Buffer, Offset, Count));

			return this.OnSent?.Invoke(this, Buffer, Offset, Count) ?? Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public event BinaryDataWrittenEventHandler OnSent;

		/// <summary>
		/// Flushes any pending or intermediate data.
		/// </summary>
		/// <returns>If output has been flushed.</returns>
		public virtual Task<bool> FlushAsync()
		{
			lock (this.synchObj)
			{
				if (!this.connected || !this.sending)
					return Task.FromResult<bool>(true);

				if (this.idleQueue is null)
					this.idleQueue = new LinkedList<TaskCompletionSource<bool>>();

				TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
				this.idleQueue.AddLast(Result);
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
		/// <param name="TrustRemoteEndpoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		public Task UpgradeToTlsAsClient(SocketProtectionLevel Protocols, bool TrustRemoteEndpoint)
		{
			return this.UpgradeToTlsAsClient(Protocols, TrustRemoteEndpoint, this.hostName);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="TrustRemoteEndpoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		/// <param name="DomainName">The domain name to validate certifictes for. By default, this is the same as the host name.</param>
		public async Task UpgradeToTlsAsClient(SocketProtectionLevel Protocols, bool TrustRemoteEndpoint, string DomainName)
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
				this.trustRemoteEndpoint = TrustRemoteEndpoint;
			}

			try
			{
				this.dataWriter.DetachStream();

				if (this.trustRemoteEndpoint)
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

				this.dataWriter = new DataWriter(this.client.OutputStream);
				this.upgraded = true;
			}
			finally
			{
				this.trustRemoteEndpoint = false;
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
			return this.UpgradeToTlsAsClient(null, Protocols, null, false, this.hostName);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		public Task UpgradeToTlsAsClient(SslProtocols Protocols, RemoteCertificateValidationCallback CertificateValidationCheck)
		{
			return this.UpgradeToTlsAsClient(null, Protocols, CertificateValidationCheck, false, this.hostName);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, null, false, this.hostName);
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
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, CertificateValidationCheck, false, this.hostName);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="TrustRemoteEndpoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols, bool TrustRemoteEndpoint)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, null, TrustRemoteEndpoint, this.hostName);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		/// <param name="TrustRemoteEndpoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		public Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols,
			RemoteCertificateValidationCallback CertificateValidationCheck, bool TrustRemoteEndpoint)
		{
			return this.UpgradeToTlsAsClient(ClientCertificate, Protocols, CertificateValidationCheck, TrustRemoteEndpoint, this.hostName);
		}

		/// <summary>
		/// Upgrades a client connection to TLS.
		/// </summary>
		/// <param name="ClientCertificate">Optional client certificate. Can be null.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		/// <param name="TrustRemoteEndpoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		/// <param name="DomainName">The domain name to validate certifictes for. By default, this is the same as the host name.</param>
		public async Task UpgradeToTlsAsClient(X509Certificate ClientCertificate, SslProtocols Protocols,
			RemoteCertificateValidationCallback CertificateValidationCheck, bool TrustRemoteEndpoint, string DomainName)
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
				this.trustRemoteEndpoint = TrustRemoteEndpoint;
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

				SslStream SslStream = new SslStream(this.stream, true, this.ValidateCertificateRequired);
				this.stream = SslStream;

				await SslStream.AuthenticateAsClientAsync(this.domainName ?? ((IPEndPoint)this.Client.Client.RemoteEndPoint).Address.ToString(),
					ClientCertificates, Protocols, true);
			}
			finally
			{
				this.certValidation = null;
				this.trustRemoteEndpoint = false;
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

			if (SslPolicyErrors == SslPolicyErrors.None)
				this.remoteCertificateValid = Result = true;
			else if ((SslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable || Certificate is null))
			{
				this.remoteCertificateValid = false;
				Result = !RequireCertificate;
			}
			else
			{
				this.remoteCertificateValid = false;
				Result = this.trustRemoteEndpoint;
			}

			if (!(this.certValidation is null) && !(Certificate is null))
			{
				try
				{
					Result = this.certValidation(Sender, Certificate, Chain, SslPolicyErrors);
				}
				catch (Exception ex)
				{
					this.Error(ex);
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
					int c = Cert.Length;
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

					if (this.trustRemoteEndpoint)
					{
						Result = true;
						Log.Notice("Invalid certificate received. But server is trusted.", Certificate?.Subject, Certificate?.Issuer, Tags);
					}
					else
						Log.Warning("Invalid certificate received (and rejected)", Certificate?.Subject, Certificate?.Issuer, "CertError", Tags);

					if (this.HasSniffers)
					{
						StringBuilder SniffMsg = new StringBuilder();

						if (this.trustRemoteEndpoint)
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

						if (this.trustRemoteEndpoint)
							this.Information(SniffMsg.ToString());
						else
							this.Warning(SniffMsg.ToString());
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
				this.remoteCertificate = new X509Certificate(Cert);

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
			return this.UpgradeToTlsAsServer(ServerCertificate, SslProtocols.Tls12, ClientCertificates.Optional, null, false);
		}

		/// <summary>
		/// Upgrades a server connection to TLS.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		public Task UpgradeToTlsAsServer(X509Certificate ServerCertificate, SslProtocols Protocols)
		{
			return this.UpgradeToTlsAsServer(ServerCertificate, Protocols, ClientCertificates.Optional, null, false);
		}

		/// <summary>
		/// Upgrades a server connection to TLS.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="ClientCertificates">If client certificates are requested from client.</param>
		public Task UpgradeToTlsAsServer(X509Certificate ServerCertificate, SslProtocols Protocols, ClientCertificates ClientCertificates)
		{
			return this.UpgradeToTlsAsServer(ServerCertificate, Protocols, ClientCertificates, null, false);
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
			return this.UpgradeToTlsAsServer(ServerCertificate, Protocols, ClientCertificates, CertificateValidationCheck, false);
		}

		/// <summary>
		/// Upgrades a server connection to TLS.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate.</param>
		/// <param name="Protocols">Allowed SSL/TLS protocols.</param>
		/// <param name="ClientCertificates">If client certificates are requested from client.</param>
		/// <param name="CertificateValidationCheck">Method to call to check if a server certificate is valid.</param>
		/// <param name="TrustRemoteEndpoint">If the remote endpoint should be trusted, even if the certificate does not validate.</param>
		public async Task UpgradeToTlsAsServer(X509Certificate ServerCertificate, SslProtocols Protocols, ClientCertificates ClientCertificates,
			RemoteCertificateValidationCallback CertificateValidationCheck, bool TrustRemoteEndpoint)
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
				this.trustRemoteEndpoint = TrustRemoteEndpoint;
			}

			try
			{
				RemoteCertificateValidationCallback Callback;
				bool RequestCertificate;

				switch (ClientCertificates)
				{
					case ClientCertificates.NotUsed:
						RequestCertificate = false;
						Callback = this.ValidateCertificateOptional;
						break;

					case ClientCertificates.Optional:
					default:
						RequestCertificate = true;
						Callback = this.ValidateCertificateOptional;
						break;

					case ClientCertificates.Required:
						RequestCertificate = true;
						Callback = this.ValidateCertificateRequired;
						break;
				}

				SslStream SslStream = new SslStream(this.stream, true, Callback);
				this.stream = SslStream;

				await SslStream.AuthenticateAsServerAsync(ServerCertificate, RequestCertificate, Protocols, true);
			}
			finally
			{
				this.certValidation = null;
				this.trustRemoteEndpoint = false;
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
						this.cancelledQueue = new LinkedList<TaskCompletionSource<bool>>();

					this.cancelledQueue.AddLast(Task);
#if WINDOWS_UWP
					IAsyncAction _ = this.client.CancelIOAsync();
#else
					this.cancelReading.Cancel();
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
