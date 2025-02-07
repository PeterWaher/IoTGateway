using System;
using System.Threading.Tasks;

namespace Waher.Networking
{
	/// <summary>
	/// Maintains information about a connection between an external client and
	/// a local server.
	/// </summary>
	public class ServerTcpConnection : IDisposable
	{
		private bool disposed = false;

		/// <summary>
		/// Maintains information about a connection between an external client and
		/// a local server.
		/// </summary>
		/// <param name="Server">Local server</param>
		/// <param name="Client">External client connection</param>
		public ServerTcpConnection(BinaryTcpServer Server, BinaryTcpClient Client)
		{
			this.Id = Guid.NewGuid();
			this.Server = Server;
			this.Client = Client;

			this.Client.OnDisconnected += this.Client_OnDisconnected;
			this.Client.OnError += this.Client_OnError;
			this.Client.OnReceived += this.Client_OnReceived;
		}

		/// <summary>
		/// ID of connection
		/// </summary>
		public Guid Id { get; }

		/// <summary>
		/// Local server
		/// </summary>
		public BinaryTcpServer Server { get; }

		/// <summary>
		/// External client connection
		/// </summary>
		public BinaryTcpClient Client { get; private set; }

		private async Task<bool> Client_OnReceived(object Sender, bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			await this.Server.DataReceived(this, ConstantBuffer, Buffer, Offset, Count);
			return true;
		}

		private Task Client_OnError(object Sender, Exception Exception)
		{
			this.Dispose();
			return Task.CompletedTask;
		}

		private Task Client_OnDisconnected(object Sender, EventArgs e)
		{
			this.Dispose();
			return Task.CompletedTask;
		}

		/// <summary>
		/// Disposes of the connection.
		/// </summary>
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;
				this.Client?.DisposeWhenDone();
				this.Client = null;
				this.Server.Remove(this);
			}
		}

		/// <summary>
		/// Sends data back to the client.
		/// </summary>
		/// <param name="Data">Data to send.</param>
		/// <returns>If data was sent.</returns>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task<bool> SendAsync(byte[] Data)
		{
			return this.SendAsync(false, Data);
		}

		/// <summary>
		/// Sends data back to the client.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Data to send.</param>
		/// <returns>If data was sent.</returns>
		public async Task<bool> SendAsync(bool ConstantBuffer, byte[] Data)
		{
			if (!await this.Client.SendAsync(ConstantBuffer, Data))
			{
				this.Dispose();
				return false;
			}

			this.Server.DataSent(ConstantBuffer, Data);

			return true;
		}

	}
}
