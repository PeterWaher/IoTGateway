using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.DNS.Enumerations;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Networking.Sniffers;
using Waher.Runtime.Cache;
using Waher.Runtime.Timing;

namespace Waher.Networking.DNS.Communication
{
	/// <summary>
	/// Implements a DNS TCP-based client, as defined in:
	/// 
	/// RFC 1035: https://tools.ietf.org/html/rfc1035: DOMAIN NAMES - IMPLEMENTATION AND SPECIFICATION
	/// </summary>
	public class DnsUdpClient : Sniffable, IDisposable
	{
		/// <summary>
		/// Default Timeout, in milliseconds (30000 ms)
		/// </summary>
		public const int DefaultTimeout = 30000;

		private readonly Dictionary<ushort, Rec> outgoingMessages = new Dictionary<ushort, Rec>();
		private readonly Random gen = new Random();
		private readonly IPEndPoint dnsEndpoint;
		private readonly LinkedList<byte[]> outputQueue = new LinkedList<byte[]>();
		private Scheduler scheduler;
		private UdpClient udp = null;
		private bool isWriting = false;
		private bool disposed = false;

		private class Rec
		{
			public ushort ID;
			public byte[] Output;
			public DnsMessageEventHandler Callback;
			public object State;
		}

		/// <summary>
		/// Implements a DNS TCP-based client.
		/// </summary>
		public DnsUdpClient()
		{
			foreach (NetworkInterface Interface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (Interface.OperationalStatus != OperationalStatus.Up)
					continue;

				if (Interface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
					continue;

				IPInterfaceProperties Properties = Interface.GetIPProperties();
				int c;

				if ((c = Properties.DnsAddresses.Count) == 0)
					continue;

				this.dnsEndpoint = new IPEndPoint(Properties.DnsAddresses[0], DnsResolver.DefaultDnsPort);

				AddressFamily AddressFamily = this.dnsEndpoint.AddressFamily;

				foreach (UnicastIPAddressInformation UnicastAddress in Properties.UnicastAddresses)
				{
					if (UnicastAddress.Address.AddressFamily != AddressFamily)
						continue;

					IPAddress Address = UnicastAddress.Address;

					try
					{
						this.udp = new UdpClient(AddressFamily)
						{
							DontFragment = true,
							MulticastLoopback = false
						};
					}
					catch (NotSupportedException)
					{
						continue;
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
						continue;
					}

					this.udp.Ttl = 30;
					this.udp.Client.Bind(new IPEndPoint(Address, 0));

					this.BeginReceive();
					this.scheduler = new Scheduler();

					return;
				}
			}

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
				this.Error(ex.Message);
			}
		}

		private async void BeginTransmit(ushort ID, byte[] Message, DnsMessageEventHandler Callback, object State)
		{
			if (this.disposed)
				return;

			if (!(Callback is null))
			{
				Rec Rec = new Rec()
				{
					ID = ID,
					Output = Message,
					Callback = Callback,
					State = State
				};

				lock (this.outgoingMessages)
				{
					this.outgoingMessages[ID] = Rec;
				}

				this.scheduler.Add(DateTime.Now.AddSeconds(2), this.CheckRetry, Rec);
			}

			lock (this.outputQueue)
			{
				if (this.isWriting)
				{
					this.outputQueue.AddLast(Message);
					return;
				}
				else
					this.isWriting = true;
			}

			try
			{
				while (Message != null)
				{
					this.TransmitBinary(Message);

					await this.udp.SendAsync(Message, Message.Length, this.dnsEndpoint);

					if (this.disposed)
						return;

					lock (this.outputQueue)
					{
						if (this.outputQueue.First is null)
						{
							this.isWriting = false;
							Message = null;
						}
						else
						{
							Message = this.outputQueue.First.Value;
							this.outputQueue.RemoveFirst();
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.Error(ex.Message);
			}
		}

		private void CheckRetry(object P)
		{
			Rec Rec = (Rec)P;

			lock (this.outgoingMessages)
			{
				if (!this.outgoingMessages.ContainsKey(Rec.ID))
					return;
			}

			this.BeginTransmit(Rec.ID, Rec.Output, Rec.Callback, Rec.State);
		}

		private void ProcessIncomingMessage(DnsMessage Message)
		{
			if (Message.Response)
			{
				Rec Rec;

				lock (this.outgoingMessages)
				{
					if (this.outgoingMessages.TryGetValue(Message.ID, out Rec))
						this.outgoingMessages.Remove(Message.ID);
					else
						return;
				}

				try
				{
					Rec.Callback?.Invoke(this, new DnsMessageEventArgs(Message, Rec.State));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.disposed = true;

			this.udp?.Dispose();
			this.udp = null;

			this.scheduler?.Dispose();
			this.scheduler = null;
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="QNAME">Query Name</param>
		/// <param name="QTYPE">Query Type</param>
		/// <param name="QCLASS">Query Class</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Query(string QNAME, QTYPE QTYPE, QCLASS QCLASS, DnsMessageEventHandler Callback, object State)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				ushort ID = DnsResolver.NextID;

				DnsResolver.WriteUInt16(ID, ms);
				ms.WriteByte((byte)((int)OpCode.Query << 3));
				ms.WriteByte((byte)RCode.NoError);

				DnsResolver.WriteUInt16(1, ms);     // Query Count
				DnsResolver.WriteUInt16(0, ms);     // Answer Count
				DnsResolver.WriteUInt16(0, ms);     // Authoritative Count
				DnsResolver.WriteUInt16(0, ms);     // Additional Count
				DnsResolver.WriteName(QNAME, ms);
				DnsResolver.WriteUInt16((ushort)QTYPE, ms);
				DnsResolver.WriteUInt16((ushort)QCLASS, ms);

				byte[] Packet = ms.ToArray();

				this.BeginTransmit(ID, Packet, Callback, State);
			}
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="QNAME">Query Name</param>
		/// <param name="QTYPE">Query Type</param>
		/// <param name="QCLASS">Query Class</param>
		public Task<DnsMessage> QueryAsync(string QNAME, QTYPE QTYPE, QCLASS QCLASS)
		{
			return this.QueryAsync(QNAME, QTYPE, QCLASS, DefaultTimeout);
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="QNAME">Query Name</param>
		/// <param name="QTYPE">Query Type</param>
		/// <param name="QCLASS">Query Class</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public Task<DnsMessage> QueryAsync(string QNAME, QTYPE QTYPE, QCLASS QCLASS, int Timeout)
		{
			TaskCompletionSource<DnsMessage> Result = new TaskCompletionSource<DnsMessage>();
			DateTime TP;

			TP = this.scheduler.Add(DateTime.Now.AddMilliseconds(Timeout), (P) =>
			{
				((TaskCompletionSource<DnsMessage>)P).TrySetException(
					new TimeoutException("No response returned within the given time."));
			}, Result);


			this.Query(QNAME, QTYPE, QCLASS, (sender, e) =>
			{
				this.scheduler?.Remove(TP);

				((TaskCompletionSource<DnsMessage>)e.State).TrySetResult(e.Message);
			}, Result);

			return Result.Task;
		}

	}
}
