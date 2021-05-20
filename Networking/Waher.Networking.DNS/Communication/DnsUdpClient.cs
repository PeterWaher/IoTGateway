using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Networking.DNS.Communication
{
	/// <summary>
	/// Implements a DNS UDP-based client.
	/// </summary>
	public class DnsUdpClient : DnsClient
	{
		private static readonly IPAddress[] defaultDnsAddresses = new IPAddress[]
		{
			IPAddress.Parse("8.8.8.8"),                 // Use Google Public DNS IP addresses as default, if no DNS addresses found.
            IPAddress.Parse("8.8.4.4"),
			IPAddress.Parse("2001:4860:4860::8888"),
			IPAddress.Parse("2001:4860:4860::8844")
		};

		private readonly IPEndPoint dnsEndpoint;
		private UdpClient udp = null;

		/// <summary>
		/// Implements a DNS UDP-based client.
		/// </summary>
		public DnsUdpClient()
			: base()
		{
			KeyValuePair<IPAddress, IPAddress> P = this.FindDnsAddress();
			IPAddress Address = P.Value;
			AddressFamily Family = Address.AddressFamily;

			this.dnsEndpoint = new IPEndPoint(P.Key, DnsResolver.DefaultDnsPort);

			this.udp = new UdpClient(Family)
			{
				//DontFragment = true,
				MulticastLoopback = false
			};

			this.udp.Ttl = 30;
			this.udp.Client.Bind(new IPEndPoint(Address, 0));

			this.BeginReceive();
			this.Init();
		}

		private KeyValuePair<IPAddress, IPAddress> FindDnsAddress()
		{
			IPAddress Result = null;
			IPAddress Local = null;
			int Step;

			for (Step = 0; Step < 2; Step++)
			{
				foreach (NetworkInterface Interface in NetworkInterface.GetAllNetworkInterfaces())
				{
					if (Interface.OperationalStatus != OperationalStatus.Up)
						continue;

					if (Interface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
						continue;

					IPInterfaceProperties Properties = Interface.GetIPProperties();
					int c;

					if ((c = Properties.DnsAddresses?.Count ?? 0) == 0 && Step == 0)
						continue;

					foreach (UnicastIPAddressInformation UnicastAddress in Properties.UnicastAddresses)
					{
						IEnumerable<IPAddress> DnsAddresses = (IEnumerable<IPAddress>)Properties.DnsAddresses ?? defaultDnsAddresses;

						foreach (IPAddress DnsAddress in DnsAddresses)
						{
							if (UnicastAddress.Address.AddressFamily != DnsAddress.AddressFamily)
								continue;

							Result = DnsAddress;
							Local = UnicastAddress.Address;

							if (Result.AddressFamily == AddressFamily.InterNetwork)
								return new KeyValuePair<IPAddress, IPAddress>(Result, Local);
						}
					}
				}
			}

			if (!(Result is null))
				return new KeyValuePair<IPAddress, IPAddress>(Result, Local);

			throw new NotSupportedException("No route to DNS server found.");
		}

		private async void BeginReceive()
		{
			try
			{
				while (!this.disposed)
				{
					UdpReceiveResult Data = await this.udp.ReceiveAsync();
					if (this.disposed)
						return;

					this.ReceiveBinary(Data.Buffer);

					try
					{
						DnsMessage Message = new DnsMessage(Data.Buffer);
						this.ProcessIncomingMessage(Message);
					}
					catch (Exception ex)
					{
						Log.Error("Unable to process DNS packet: " + ex.Message);
					}
				}
			}
			catch (ObjectDisposedException)
			{
				// Closed.
			}
			catch (Exception ex)
			{
				this.Exception(ex);
			}
		}

		/// <summary>
		/// Sends a message to a destination.
		/// </summary>
		/// <param name="Message">Message</param>
		/// <param name="Destination">Destination. If null, default destination
		/// is assumed.</param>
		protected override Task SendAsync(byte[] Message, IPEndPoint Destination)
		{
			return this.udp.SendAsync(Message, Message.Length, Destination ?? this.dnsEndpoint);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.udp?.Dispose();
			this.udp = null;
		}

	}
}
