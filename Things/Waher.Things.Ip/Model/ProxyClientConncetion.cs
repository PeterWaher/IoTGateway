using System;
using System.Threading.Tasks;
using Waher.Networking;
using Waher.Networking.Sniffers;

namespace Waher.Things.Ip.Model
{
	/// <summary>
	/// Maintains one proxy connection
	/// </summary>
	public class ProxyClientConncetion : Sniffable, IDisposable
	{
		private readonly Guid id = Guid.NewGuid();
		private readonly BinaryTcpClient incoming;
		private readonly BinaryTcpClient outgoing;
		private readonly ProxyPort port;

		/// <summary>
		/// Maintains one proxy connection
		/// </summary>
		/// <param name="Port">Listening port object</param>
		/// <param name="Incoming">Incoming client connection</param>
		/// <param name="Outgoing">Outgoing client connection</param>
		/// <param name="Tls">If TLS is used</param>
		/// <param name="Sniffers">Sniffers</param>
		public ProxyClientConncetion(ProxyPort Port, BinaryTcpClient Incoming, BinaryTcpClient Outgoing, ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.port = Port;
			this.incoming = Incoming;
			this.outgoing = Outgoing;

			this.incoming.OnDisconnected += Incoming_OnDisconnected;
			this.incoming.OnError += Incoming_OnError;
			this.incoming.OnReceived += Incoming_OnReceived;

			this.outgoing.OnDisconnected += Outgoing_OnDisconnected;
			this.outgoing.OnError += Outgoing_OnError;
			this.outgoing.OnReceived += Outgoing_OnReceived;
		}

		/// <summary>
		/// ID of connection.
		/// </summary>
		public Guid Id => this.id;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			try
			{
				this.incoming.DisposeWhenDone();
			}
			catch (Exception)
			{
				// Ignore
			}

			try
			{
				this.outgoing.DisposeWhenDone();
			}
			catch (Exception)
			{
				// Ignore
			}
		}


		private async Task<bool> Outgoing_OnReceived(object Sender, byte[] Buffer, int Offset, int Count)
		{
			if (await this.incoming.SendAsync(Buffer, Offset, Count))
			{
				this.port.IncUplink(Count);
				return true;
			}
			else
				return false;
		}

		private Task Outgoing_OnError(object Sender, Exception Exception)
		{
			this.port.Remove(this);
			return Task.CompletedTask;
		}

		private void Outgoing_OnDisconnected(object sender, EventArgs e)
		{
			this.port.Remove(this);
		}

		private async Task<bool> Incoming_OnReceived(object Sender, byte[] Buffer, int Offset, int Count)
		{
			if (await this.outgoing.SendAsync(Buffer, Offset, Count))
			{
				this.port.IncDownlink(Count);
				return true;
			}
			else
				return false;
		}

		private Task Incoming_OnError(object Sender, Exception Exception)
		{
			this.port.Remove(this);
			return Task.CompletedTask;
		}

		private void Incoming_OnDisconnected(object sender, EventArgs e)
		{
			this.port.Remove(this);
		}

	}
}
