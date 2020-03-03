using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;

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
	public class BinaryTcpClient : IDisposable, IBinaryTransportLayer
	{
		private const int BufferSize = 65536;

		private readonly LinkedList<KeyValuePair<byte[], EventHandler>> queue = new LinkedList<KeyValuePair<byte[], EventHandler>>();
		private readonly byte[] buffer = new byte[BufferSize];
		private readonly object synchObj = new object();
		private readonly CancellationTokenSource source;
		private readonly TcpClient tcpClient;
		private Stream stream = null;
		private bool connecting = false;
		private bool connected = false;
		private bool disposing = false;
		private bool disposed = false;
		private bool sending = false;

		/// <summary>
		/// Implements a binary TCP Client, by encapsulating a <see cref="TcpClient"/>. It also maked the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt.
		/// </summary>
		public BinaryTcpClient()
		{
			this.tcpClient = new TcpClient();
			this.source = new CancellationTokenSource();
		}

		/// <summary>
		/// Underlying <see cref="TcpClient"/> object.
		/// </summary>
		public TcpClient Client => this.tcpClient;

		/// <summary>
		/// Connects to a host using TCP.
		/// </summary>
		/// <param name="Host">Host Name or IP Address in string format.</param>
		/// <param name="Port">Port number.</param>
		/// <returns>If connection was established. If false is returned, the object has been disposed during the connection attempt.</returns>
		public async Task<bool> ConnectAsync(string Host, int Port)
		{
			this.PreConnect();
			await this.tcpClient.ConnectAsync(Host, Port);
			return this.PostConnect();
		}

		/// <summary>
		/// Connects to a host using TCP.
		/// </summary>
		/// <param name="Address">IP Address of the host.</param>
		/// <param name="Port">Port number.</param>
		/// <returns>If connection was established. If false is returned, the object has been disposed during the connection attempt.</returns>
		public async Task<bool> ConnectAsync(IPAddress Address, int Port)
		{
			this.PreConnect();
			await this.tcpClient.ConnectAsync(Address, Port);
			return this.PostConnect();
		}

		/// <summary>
		/// Connects to a host using TCP.
		/// </summary>
		/// <param name="Addresses">IP Addresses of the host.</param>
		/// <param name="Port">Port number.</param>
		/// <returns>If connection was established. If false is returned, the object has been disposed during the connection attempt.</returns>
		public async Task<bool> ConnectAsync(IPAddress[] Addresses, int Port)
		{
			this.PreConnect();
			await this.tcpClient.ConnectAsync(Addresses, Port);
			return this.PostConnect();
		}

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
		}

		private bool PostConnect()
		{
			lock (this.synchObj)
			{
				this.connecting = false;

				if (this.disposed)
					return false;

				if (this.disposing)
				{
					this.stream = null;
					this.disposed = true;
					this.disposing = false;
					this.connected = false;
					this.queue.Clear();
					this.source.Cancel();
					this.tcpClient.Dispose();
					return false;
				}

				this.connected = true;
			}

			this.stream = this.tcpClient.GetStream();
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
					this.disposing = true;
				else
				{
					this.disposed = true;
					this.connecting = false;
					this.connected = false;
					this.stream = null;
					this.queue.Clear();
					this.source.Cancel();
					this.tcpClient.Dispose();
				}
			}
		}

		private async void BeginRead()
		{
			try
			{
				Stream Stream;
				int NrRead;

				while (true)
				{
					lock (this.synchObj)
					{
						if (this.disposing || this.disposed)
							break;

						Stream = this.stream;
						if (Stream is null)
							break;
					}

					NrRead = await Stream.ReadAsync(this.buffer, 0, BufferSize, this.source.Token);
					if (this.stream is null || NrRead <= 0)
						break;

					try
					{
						byte[] Packet = new byte[NrRead];
						Array.Copy(this.buffer, 0, Packet, 0, NrRead);

						this.BinaryDataReceived(Packet);
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
		}

		/// <summary>
		/// Method called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary data received.</param>
		protected virtual void BinaryDataReceived(byte[] Data)
		{
			this.OnReceived?.Invoke(this, Data);
		}

		/// <summary>
		/// Method called when an exception has been caught.
		/// </summary>
		/// <param name="ex">Exception</param>
		protected virtual void Error(Exception ex)
		{
			try
			{
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
				Stream Stream;

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

						Stream = this.stream;
						if (Stream is null)
						{
							this.sending = false;
							break;
						}
					}

					await Stream.WriteAsync(Packet, 0, Packet.Length, this.source.Token);

					try
					{
						this.BinaryDataSent(Packet);
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
			}
			catch (Exception ex)
			{
				this.sending = false;
				this.Error(ex);
			}
		}

		/// <summary>
		/// Method called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary data received.</param>
		protected virtual void BinaryDataSent(byte[] Data)
		{
			this.OnSent?.Invoke(this, Data);
		}

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public event BinaryEventHandler OnSent;
	}
}
