using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Things.Matter.Messaging
{
	/// <summary>
	/// Delegate for datagram reception events.
	/// </summary>
	/// <param name="Data">Datagram.</param>
	/// <param name="From">Remote endpoint.</param>
	public delegate Task DatagramReceivedHandler(byte[] Data, IPEndPoint From);

	/// <summary>
	/// UDP transport for Matter messages, supporting IPv6 and IPv4.
	/// </summary>
	public class UdpTransport : IDisposable
	{
		/// <summary>
		/// Default Matter UDP port.
		/// </summary>
		public const int DefaultPort = 5540;

		private UdpClient ipv4;
		private UdpClient ipv6;
		private bool disposed = false;

		/// <summary>
		/// UDP transport for Matter messages, supporting IPv6 and IPv4.
		/// </summary>
		/// <param name="Port">Local port to bind to (0 = ephemeral port).</param>
		public UdpTransport(int Port)
		{
			Exception Ipv6Error = null;
			Exception Ipv4Error = null;

			try
			{
				if (Socket.OSSupportsIPv6)
				{
					UdpClient Client = new UdpClient(AddressFamily.InterNetworkV6);
					Client.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, true);
					Client.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, Port));
					this.ipv6 = Client;
				}
			}
			catch (Exception ex)
			{
				Ipv6Error = ex;
				this.ipv6 = null;
			}

			try
			{
				int Port4 = Port;

				if (Port4 == 0 && !(this.ipv6 is null))
					Port4 = ((IPEndPoint)this.ipv6.Client.LocalEndPoint).Port;

				UdpClient Client = new UdpClient(AddressFamily.InterNetwork);

				try
				{
					Client.Client.Bind(new IPEndPoint(IPAddress.Any, Port4));
				}
				catch (SocketException) when (Port == 0)
				{
					Client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
				}

				this.ipv4 = Client;
			}
			catch (Exception ex)
			{
				Ipv4Error = ex;
				this.ipv4 = null;
			}

			if (this.ipv4 is null && this.ipv6 is null)
				throw new MatterException("Unable to open UDP sockets: " + (Ipv6Error ?? Ipv4Error)?.Message, Ipv6Error ?? Ipv4Error);

			if (!(this.ipv6 is null))
				this.StartReceiving(this.ipv6);

			if (!(this.ipv4 is null))
				this.StartReceiving(this.ipv4);
		}

		/// <summary>
		/// If IPv4 is supported.
		/// </summary>
		public bool SupportsIPv4 => !(this.ipv4 is null);

		/// <summary>
		/// If IPv6 is supported.
		/// </summary>
		public bool SupportsIPv6 => !(this.ipv6 is null);

		/// <summary>
		/// Local IPv4 port, or 0 if not available.
		/// </summary>
		public int Ipv4Port => this.ipv4 is null ? 0 : ((IPEndPoint)this.ipv4.Client.LocalEndPoint).Port;

		/// <summary>
		/// Local IPv6 port, or 0 if not available.
		/// </summary>
		public int Ipv6Port => this.ipv6 is null ? 0 : ((IPEndPoint)this.ipv6.Client.LocalEndPoint).Port;

		/// <summary>
		/// Event raised when a datagram has been received.
		/// </summary>
		public event DatagramReceivedHandler OnDatagramReceived = null;

		private async void StartReceiving(UdpClient Client)
		{
			try
			{
				while (!this.disposed)
				{
					UdpReceiveResult Result;

					try
					{
						Result = await Client.ReceiveAsync();
					}
					catch (ObjectDisposedException)
					{
						break;
					}
					catch (SocketException ex)
					{
						if (this.disposed)
							break;

						if (ex.SocketErrorCode == SocketError.ConnectionReset ||
							ex.SocketErrorCode == SocketError.NetworkReset ||
							ex.SocketErrorCode == SocketError.HostUnreachable ||
							ex.SocketErrorCode == SocketError.NetworkUnreachable)
						{
							continue;   // ICMP errors reported on UDP sockets.
						}

						Log.Exception(ex);
						await Task.Delay(100);
						continue;
					}

					DatagramReceivedHandler h = this.OnDatagramReceived;
					if (!(h is null))
					{
						try
						{
							await h(Result.Buffer, Result.RemoteEndPoint);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (!this.disposed)
					Log.Exception(ex);
			}
		}

		/// <summary>
		/// Sends a datagram.
		/// </summary>
		/// <param name="Data">Datagram.</param>
		/// <param name="Destination">Destination.</param>
		public async Task SendAsync(byte[] Data, IPEndPoint Destination)
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(UdpTransport));

			IPAddress Address = Destination.Address;

			if (Address.IsIPv4MappedToIPv6)
			{
				Address = Address.MapToIPv4();
				Destination = new IPEndPoint(Address, Destination.Port);
			}

			UdpClient Client;

			if (Address.AddressFamily == AddressFamily.InterNetworkV6)
				Client = this.ipv6 ?? throw new MatterException("IPv6 not available.");
			else
				Client = this.ipv4 ?? throw new MatterException("IPv4 not available.");

			await Client.SendAsync(Data, Data.Length, Destination);
		}

		/// <summary>
		/// If the transport can send to a given address.
		/// </summary>
		/// <param name="Address">Address.</param>
		/// <returns>If the address family is supported.</returns>
		public bool CanSendTo(IPAddress Address)
		{
			if (Address.IsIPv4MappedToIPv6 || Address.AddressFamily == AddressFamily.InterNetwork)
				return !(this.ipv4 is null);
			else
				return !(this.ipv6 is null);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.disposed = true;

			this.ipv4?.Dispose();
			this.ipv4 = null;

			this.ipv6?.Dispose();
			this.ipv6 = null;
		}
	}
}
